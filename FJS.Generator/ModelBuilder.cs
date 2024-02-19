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
            var enabled = cls.AttributeLists
                            .SelectMany(list =>
                                list.Attributes.Where(attr => attr.Name.ToString() == "RootType"))
                            .Select(attr =>
                            {
                                var arg = (TypeOfExpressionSyntax)attr
                                    .ArgumentList
                                    .Arguments[0]
                                    .Expression;
                                var enabledType = ((SimpleNameSyntax)arg.Type).Identifier.Text;
                                var sym = (INamedTypeSymbol)semanticModel.Compilation.GetSymbolsWithName(enabledType).First();
                                return GatherTypeData(sym, enabledType);
                            });

            var host = new Host
            {
                Name = cls.Identifier.Text,
            };
            host.Types.AddRange(enabled);
            return host;
        }

        static TypeData GatherTypeData(INamedTypeSymbol sym, string typeName)
        {
            var typeData = new TypeData
            {
                Name = typeName,
            };

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
                            GatherTypeData(at.ElementType as INamedTypeSymbol, at.ElementType.Name),
                        INamedTypeSymbol { Name: "Dictionary", IsGenericType: true, Arity: 2 } t =>
                            GatherTypeData(t.TypeArguments[1] as INamedTypeSymbol, t.TypeArguments[1].Name),
                        INamedTypeSymbol t => GatherTypeData(t, t.Name),
                        _ => null,
                    }
                };

                return member;
            }));

            return typeData;
        }

    }
}