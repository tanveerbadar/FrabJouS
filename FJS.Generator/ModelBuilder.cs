using System.Collections.Generic;
using System.Linq;
using FJS.Generator.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FJS.Generator;

static class ModelBuilder
{
    public static Host GatherSerializableTypes(SyntaxNode node, SemanticModel semanticModel)
    {
        var cls = (ClassDeclarationSyntax)node;
        Dictionary<string, TypeData> visited = new();
        var enabled = cls.AttributeLists
                        .SelectMany(list =>
                            list.Attributes.Where(attr => attr.Name.ToString() == "RootType"))
                        .Select(attr =>
                        {
                            var arg = (TypeOfExpressionSyntax)attr
                                .ArgumentList
                                .Arguments[0]
                                .Expression;
                            var enabledType = GetFQName(arg.Type);
                            return (EnabledType: enabledType,
                                Symbol: (INamedTypeSymbol)semanticModel.Compilation.GetSymbolsWithName(enabledType).FirstOrDefault());
                        })
                        .Where(sym => sym.Symbol != null)
                        .GroupBy(sym => sym.EnabledType)
                        .Select(g => GatherTypeData(g.First().Symbol, g.First().EnabledType, visited));

        var host = new Host
        {
            Name = cls.Identifier.Text,
            Namespace = GetNamespace(cls),
        };
        host.Types.AddRange(enabled);
        return host;
    }

    static string GetFQName(TypeSyntax type)
    {
        return type switch
        {
            SimpleNameSyntax sn => sn.Identifier.Text,
            QualifiedNameSyntax qn => string.Join(".", GetFQName(qn.Left), GetFQName(qn.Right)),
            _ => string.Empty,
        };
    }

    static string GetNamespace(ClassDeclarationSyntax member)
    {
        List<string> names = new();

        var parent = member.Parent;
        while (parent != null)
        {
            if (parent is NamespaceDeclarationSyntax ns)
            {
                names.Add(ns.Name.ToString());
            }
            if (parent is FileScopedNamespaceDeclarationSyntax fsns)
            {
                names.Add(fsns.Name.ToString());
            }
            parent = parent.Parent;
        }

        names.Reverse();

        return string.Join(".", names);
    }

    static string GetNamespace(INamedTypeSymbol member)
    {
        List<string> names = new();

        var parent = member.ContainingNamespace;
        while (parent != null)
        {
            if (!string.IsNullOrEmpty(parent.Name))
            {
                names.Add(parent.Name);
            }
            parent = parent.ContainingNamespace;
        }
        names.Reverse();
        return string.Join(".", names);
    }

    static TypeData GatherTypeData(INamedTypeSymbol sym, string typeName, Dictionary<string, TypeData> visited)
    {
        if (visited.TryGetValue(typeName, out var existing))
        {
            return existing;
        }

        var typeData = new TypeData
        {
            Name = typeName,
            Namespace = GetNamespace(sym),
        };

        visited[typeName] = typeData;

        var props = sym
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => !p.IsStatic && !p.IsIndexer && p.GetMethod?.DeclaredAccessibility == Accessibility.Public);

        foreach (var p in props)
        {
            MemberType memberType = default, elementWritingMethod = default;
            TypeData elementType = default;
            PrimitiveType primitiveType = default;
            CollectionType collectionType = default;
            bool isNullable = false;
            switch (p.Type)
            {
                case IArrayTypeSymbol at:
                    memberType = MemberType.Collection;
                    collectionType = CollectionType.Sequential;
                    elementWritingMethod = GetElementWritingMethod(at.ElementType);
                    elementType = GatherTypeData(at.ElementType as INamedTypeSymbol, at.ElementType.Name, visited);
                    isNullable = at.NullableAnnotation == NullableAnnotation.Annotated;
                    break;
                case INamedTypeSymbol { SpecialType: SpecialType.System_String }:
                    memberType = MemberType.Primitive;
                    primitiveType = PrimitiveType.String;
                    isNullable = p.Type.NullableAnnotation == NullableAnnotation.Annotated;
                    break;
                case INamedTypeSymbol { SpecialType: SpecialType.System_Boolean }:
                    memberType = MemberType.Primitive;
                    primitiveType = PrimitiveType.Boolean;
                    isNullable = false;
                    break;
                case INamedTypeSymbol nt when IsNumericType(nt.SpecialType):
                    memberType = MemberType.Primitive;
                    primitiveType = PrimitiveType.Number;
                    isNullable = false;
                    break;
                case INamedTypeSymbol { Name: "Dictionary", IsGenericType: true, Arity: 2 } dict:
                    memberType = MemberType.Collection;
                    collectionType = CollectionType.Associative;
                    elementWritingMethod = GetElementWritingMethod(dict.TypeArguments[1]);
                    elementType = GatherTypeData(dict.TypeArguments[1] as INamedTypeSymbol, dict.TypeArguments[1].Name, visited);
                    isNullable = dict.NullableAnnotation == NullableAnnotation.Annotated;
                    break;
                case INamedTypeSymbol { Name: "List", IsGenericType: true, Arity: 1 } list:
                    memberType = MemberType.Collection;
                    collectionType = CollectionType.Sequential;
                    elementWritingMethod = GetElementWritingMethod(list.TypeArguments[0]);
                    elementType = GatherTypeData(list.TypeArguments[0] as INamedTypeSymbol, list.TypeArguments[0].Name, visited);
                    isNullable = list.NullableAnnotation == NullableAnnotation.Annotated;
                    break;
                case INamedTypeSymbol { Name: "Nullable", IsGenericType: true, Arity: 1 } nullable:
                    memberType = MemberType.Nullable;
                    elementType = GatherTypeData(nullable.TypeArguments[0] as INamedTypeSymbol, nullable.TypeArguments[0].Name, visited);
                    elementWritingMethod = GetElementWritingMethod(nullable.TypeArguments[0]);
                    isNullable = true;
                    if (elementWritingMethod == MemberType.Primitive)
                    {
                        switch (nullable.TypeArguments[0])
                        {
                            case INamedTypeSymbol { SpecialType: SpecialType.System_String }:
                                primitiveType = PrimitiveType.String;
                                break;
                            case INamedTypeSymbol { SpecialType: SpecialType.System_Boolean }:
                                primitiveType = PrimitiveType.Boolean;
                                break;
                            case INamedTypeSymbol nt when IsNumericType(nt.SpecialType):
                                primitiveType = PrimitiveType.Number;
                                break;
                        }
                    }
                    break;
                default:
                    memberType = MemberType.ComplexObject;
                    elementType = GatherTypeData(p.Type as INamedTypeSymbol, p.Type.Name, visited);
                    elementWritingMethod = GetElementWritingMethod(p.Type);
                    isNullable = p.Type.IsReferenceType;
                    break;
            }

            var member = new MemberData
            {
                Name = p.Name,
                CanRead = !p.IsReadOnly,
                CanWrite = !p.IsWriteOnly,
                MemberType = memberType,
                PrimitiveType = primitiveType,
                CollectionType = collectionType,
                ElementWritingMethod = elementWritingMethod,
                ElementType = elementType,
                IsNullable = isNullable,
            };

            typeData.Members.Add(member);
        }

        return typeData;
    }

    static bool IsNumericType(SpecialType st) =>
        st is SpecialType.System_Byte or SpecialType.System_SByte or SpecialType.System_Int16 or SpecialType.System_UInt16 or SpecialType.System_Int32 or SpecialType.System_UInt32 or SpecialType.System_UInt64 or SpecialType.System_Int64 or SpecialType.System_Single or SpecialType.System_Double or SpecialType.System_Decimal;

    static MemberType GetElementWritingMethod(ITypeSymbol type)
    {
        switch (type)
        {
            case INamedTypeSymbol { SpecialType: SpecialType.System_String }:
            case INamedTypeSymbol { SpecialType: SpecialType.System_Boolean }:
            case INamedTypeSymbol { Name: "Nullable", IsGenericType: true, Arity: 1 }:
            case INamedTypeSymbol nt when IsNumericType(nt.SpecialType):
                return MemberType.Primitive;
            case IArrayTypeSymbol:
            case INamedTypeSymbol { Name: "Dictionary", IsGenericType: true, Arity: 2 }:
            case INamedTypeSymbol { Name: "List", IsGenericType: true, Arity: 1 }:
                return MemberType.Collection;
            case INamedTypeSymbol:
                return MemberType.ComplexObject;
            default:
                return MemberType.Unspecified;
        }
    }
}