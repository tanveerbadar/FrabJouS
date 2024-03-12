using System.Collections.Generic;
using System.Linq;
using FJS.Generator.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
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

                            var sym1 = semanticModel.GetSymbolInfo(arg.Type);
                            var enabledType = GetFQName(arg.Type, semanticModel);
                            return (EnabledType: enabledType,
                                Symbol: (INamedTypeSymbol)sym1.Symbol);
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

    static string GetFQName(TypeSyntax type, SemanticModel semanticModel)
    {
        switch (type)
        {
            case SimpleNameSyntax sn:
                return sn.Identifier.Text;
            case QualifiedNameSyntax qn:
                var symbol = semanticModel.GetSymbolInfo(qn.Left);
                var left = string.Empty;
                if (symbol.Symbol is INamedTypeSymbol nt)
                {
                    left = nt.Name + ".";
                }
                return $"{left}{GetFQName(qn.Right, semanticModel)}";
            default:
                return string.Empty;
        }
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
            bool isNullable = false, isNullableElement = false;
            switch (p.Type)
            {
                case IArrayTypeSymbol array:
                    memberType = MemberType.Collection;
                    collectionType = CollectionType.Sequential;
                    elementWritingMethod = GetElementWritingMethod(array.ElementType);
                    elementType = GatherTypeData(array.ElementType as INamedTypeSymbol, array.ElementType.Name, visited);
                    isNullable = true;
                    isNullableElement = array.ElementType.IsReferenceType;
                    if (elementWritingMethod == MemberType.Primitive)
                    {
                        primitiveType = GetPrimitiveType(array.ElementType);
                    }
                    break;
                case INamedTypeSymbol { SpecialType: SpecialType.System_String }:
                    memberType = MemberType.Primitive;
                    primitiveType = PrimitiveType.String;
                    isNullable = true;
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
                    isNullable = true;
                    isNullableElement = dict.TypeArguments[1].IsReferenceType;
                    if (elementWritingMethod == MemberType.Primitive)
                    {
                        primitiveType = GetPrimitiveType(dict.TypeArguments[1]);
                    }
                    break;
                case INamedTypeSymbol { Name: "List", IsGenericType: true, Arity: 1 } list:
                    memberType = MemberType.Collection;
                    collectionType = CollectionType.Sequential;
                    elementWritingMethod = GetElementWritingMethod(list.TypeArguments[0]);
                    elementType = GatherTypeData(list.TypeArguments[0] as INamedTypeSymbol, list.TypeArguments[0].Name, visited);
                    isNullable = true;
                    isNullableElement = list.TypeArguments[0].IsReferenceType;
                    if (elementWritingMethod == MemberType.Primitive)
                    {
                        primitiveType = GetPrimitiveType(list.TypeArguments[0]);
                    }
                    break;
                case INamedTypeSymbol { Name: "Nullable", IsGenericType: true, Arity: 1 } nullable:
                    memberType = MemberType.Nullable;
                    elementType = GatherTypeData(nullable.TypeArguments[0] as INamedTypeSymbol, nullable.TypeArguments[0].Name, visited);
                    elementWritingMethod = GetElementWritingMethod(nullable.TypeArguments[0]);
                    isNullable = true;
                    if (elementWritingMethod == MemberType.Primitive)
                    {
                        primitiveType = GetPrimitiveType(nullable.TypeArguments[0]);
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
                IsNullableElement = isNullableElement,
            };

            typeData.Members.Add(member);
        }

        return typeData;
    }

    static PrimitiveType GetPrimitiveType(ITypeSymbol type)
       => type switch
       {
           INamedTypeSymbol { SpecialType: SpecialType.System_String } => PrimitiveType.String,
           INamedTypeSymbol { SpecialType: SpecialType.System_Boolean } => PrimitiveType.Boolean,
           INamedTypeSymbol nt when IsNumericType(nt.SpecialType) => PrimitiveType.Number,
           _ => PrimitiveType.Unspecified,
       };

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