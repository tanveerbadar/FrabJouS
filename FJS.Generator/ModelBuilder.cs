using System.Collections.Generic;
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
                var member = new MemberData
                {
                    Name = p.Name,
                    CanRead = !p.IsReadOnly,
                    CanWrite = !p.IsWriteOnly,
                    MemberType =
                        p.Type switch
                        {
                            IArrayTypeSymbol => MemberTypes.SequentialCollection,
                            INamedTypeSymbol { SpecialType: SpecialType.System_String } t => MemberTypes.String,
                            INamedTypeSymbol { SpecialType: SpecialType.System_Int32 } t => MemberTypes.Number,
                            INamedTypeSymbol { Name: "Dictionary", IsGenericType: true, Arity: 2 } t => MemberTypes.AssociativeCollection,
                            _ => MemberTypes.ComplexObject,
                        },
                    CollectionElementType = p.Type switch
                    {
                        IArrayTypeSymbol at =>
                            GatherTypeData(at.ElementType as INamedTypeSymbol, at.ElementType.Name, visited),
                        INamedTypeSymbol { Name: "Dictionary", IsGenericType: true, Arity: 2 } t =>
                            GatherTypeData(t.TypeArguments[1] as INamedTypeSymbol, t.TypeArguments[1].Name, visited),
                        INamedTypeSymbol t => GatherTypeData(t, t.Name, visited),
                        _ => null,
                    }
                };

                return member;
            }));

            return typeData;
        }

    }
}