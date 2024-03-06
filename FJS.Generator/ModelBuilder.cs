using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FJS.Generator.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FJS.Generator
{
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

            typeData.Members.AddRange(props.Select(p =>
            {
                MemberType memberType = default, collectionElementWritingMethod = default;
                TypeData collectionElementType = default;
                CollectionType collectionType = default;
                switch (p.Type)
                {
                    case INamedTypeSymbol { SpecialType: SpecialType.System_String } str:
                        memberType = MemberType.String;
                        break;
                    case INamedTypeSymbol { SpecialType: SpecialType.System_Int32 } i:
                        memberType = MemberType.Number;
                        break;
                    case IArrayTypeSymbol array:
                        memberType = MemberType.Collection;
                        collectionElementType = GatherTypeData(array.ElementType as INamedTypeSymbol, array.ElementType.Name, visited);
                        collectionElementWritingMethod = GetCollectionElementWritingMethod(array.ElementType);
                        collectionType = CollectionType.Sequential;
                        break;
                    case INamedTypeSymbol { Name: "Dictionary", IsGenericType: true, Arity: 2 } dict:
                        memberType = MemberType.Collection;
                        collectionElementType = GatherTypeData(dict.TypeArguments[1] as INamedTypeSymbol, dict.TypeArguments[1].Name, visited);
                        collectionElementWritingMethod = GetCollectionElementWritingMethod(dict.TypeArguments[1]);
                        collectionType = CollectionType.Associative;
                        break;
                    case INamedTypeSymbol { Name: "List", IsGenericType: true, Arity: 1 } list:
                        memberType = MemberType.Collection;
                        collectionElementType = GatherTypeData(list.TypeArguments[0] as INamedTypeSymbol, list.TypeArguments[0].Name, visited);
                        collectionElementWritingMethod = GetCollectionElementWritingMethod(list.TypeArguments[0]);
                        collectionType = CollectionType.Sequential;
                        break;
                    case INamedTypeSymbol { Name: "Nullable", IsGenericType: true, Arity: 1 } nullable:
                        memberType = MemberType.Nullable;
                        collectionElementType = GatherTypeData(nullable.TypeArguments[0] as INamedTypeSymbol, nullable.TypeArguments[0].Name, visited);
                        collectionElementWritingMethod = GetCollectionElementWritingMethod(nullable.TypeArguments[0]);
                        break;
                    case INamedTypeSymbol t:
                        memberType = MemberType.ComplexObject;
                        collectionElementType = GatherTypeData(t, t.Name, visited);
                        break;
                    default:
                        Debug.Fail($"Unhandled type in model builder: {p.Type.Name}.");
                        break;
                }

                var member = new MemberData
                {
                    Name = p.Name,
                    CanRead = !p.IsReadOnly,
                    CanWrite = !p.IsWriteOnly,
                    MemberType = memberType,
                    CollectionElementType = collectionElementType,
                    CollectionElementWritingMethod = collectionElementWritingMethod,
                    CollectionType = collectionType,
                };

                return member;
            }));

            return typeData;
        }

        static MemberType GetCollectionElementWritingMethod(ITypeSymbol type)
        {
            switch (type)
            {
                case INamedTypeSymbol { SpecialType: SpecialType.System_String }:
                    return MemberType.String;
                case INamedTypeSymbol { Name: "Nullable", IsGenericType: true, Arity: 1 }:
                case INamedTypeSymbol { SpecialType: SpecialType.System_Int32 }:
                    return MemberType.Number;
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
}