using System.Collections.Generic;
using System.Linq;
using FJS.Generator.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace FJS.Generator
{
    static class CodeGenerator
    {
        public static string GetGeneratedSource(Host host)
        {
            State state = new();
            var surrogateType = ClassDeclaration(host.Name)
                        .AddModifiers(Token(PartialKeyword))
                        .AddMembers(AddMethods(host.Types, state));

            var types = state.TypesToGenerate;

            while (types is { Count: > 0 })
            {
                var type = types[types.Count - 1];
                if (state.Processed.Add(type.Name))
                {
                    surrogateType = surrogateType.AddMembers(GenerateMethodForType(type, state));
                }
                types.Remove(type);
            }

            var code = CompilationUnit()
                .AddUsings(UsingDirective(ParseName("System.Text.Json")))
                .AddMembers(surrogateType)
                .NormalizeWhitespace()
                .ToFullString();
            return code;
        }

        static MethodDeclarationSyntax[] AddMethods(List<TypeData> types, State state) =>
            types.Select(t => GenerateMethodForType(t, state)).ToArray();

        static MethodDeclarationSyntax GenerateMethodForType(TypeData t, State state)
        {
            state.Processed.Add(t.Name);
            return MethodDeclaration(ParseTypeName("void"), $"Write")
                                 .AddModifiers(Token(PublicKeyword))
                                 .AddParameterListParameters(
                                     [
                                         Parameter(Identifier("writer")).WithType(ParseTypeName("Utf8JsonWriter")),
                                        Parameter(Identifier("obj")).WithType(ParseTypeName(t.Name)),
                                     ]
                                 )
                                 .AddBodyStatements(AddStatements(t.Members, state));
        }

        static StatementSyntax[] AddStatements(List<MemberData> members, State state)
        {
            List<StatementSyntax> stmts = new()
            {
                ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(SimpleMemberAccessExpression,
                            IdentifierName("writer"),
                            IdentifierName("WriteStartObject")))),
            };
            foreach (var member in members)
            {
                if (!member.CanRead)
                {
                    continue;
                }
                if (member.MemberType == MemberTypes.SequentialCollection)
                {
                    WriteArray(stmts, member);
                }
                else if (member.MemberType == MemberTypes.ComplexObject)
                {
                    WriteSubobject(state, stmts, member);
                }
                else if (member.MemberType == MemberTypes.AssociativeCollection)
                {
                    WriteDictionary(stmts, member);
                }
                else
                {
                    WritePrimitive(stmts, member);
                }
            }
            stmts.Add(
                ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(SimpleMemberAccessExpression,
                            IdentifierName("writer"),
                                IdentifierName("WriteEndObject")))));

            return stmts.ToArray();
        }

        static void WritePrimitive(List<StatementSyntax> stmts, MemberData member)
        {
            stmts.Add(
                ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(SimpleMemberAccessExpression,
                            IdentifierName("writer"),
                            IdentifierName(member.MemberType == MemberTypes.String ? "WriteString" : "WriteNumber")),
                        ArgumentList(SeparatedList(
                            [
                                Argument(LiteralExpression(StringLiteralExpression, Literal(member.Name))),
                                        Argument(MemberAccessExpression(SimpleMemberAccessExpression,
                                            IdentifierName("obj"),
                                            IdentifierName(member.Name))),
                            ])))));
        }

        static void WriteDictionary(List<StatementSyntax> stmts, MemberData member)
        {
            stmts.Add(
                ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(SimpleMemberAccessExpression,
                            IdentifierName("writer"),
                            IdentifierName("WritePropertyName")),
                        ArgumentList(SeparatedList(
                            [
                                Argument(LiteralExpression(StringLiteralExpression, Literal(member.Name)))
                            ])))));
            stmts.Add(
                ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(SimpleMemberAccessExpression,
                            IdentifierName("writer"),
                            IdentifierName("WriteStartObject")))));
            stmts.Add(
                ForEachStatement(
                    ParseTypeName("var"),
                    "kvp",
                    MemberAccessExpression(SimpleMemberAccessExpression,
                        IdentifierName("obj"),
                        IdentifierName(member.Name)),
                        Block(
                            new StatementSyntax[]
                            {
                                        ExpressionStatement(
                                            InvocationExpression(
                                                MemberAccessExpression(SimpleMemberAccessExpression,
                                                    IdentifierName("writer"),
                                                    IdentifierName(member.MemberType == MemberTypes.String ? "WriteString" : "WriteNumber")),
                                                ArgumentList(SeparatedList(
                                                    [
                                                        Argument(MemberAccessExpression(SimpleMemberAccessExpression,
                                                                    IdentifierName("kvp"),
                                                                    IdentifierName("Key"))),
                                                        Argument(MemberAccessExpression(SimpleMemberAccessExpression,
                                                                    IdentifierName("kvp"),
                                                                    IdentifierName("Value"))),
                                                    ])))),
                            })));
            stmts.Add(
                ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(SimpleMemberAccessExpression,
                            IdentifierName("writer"),
                            IdentifierName("WriteEndObject")))));
        }

        static void WriteSubobject(State state, List<StatementSyntax> stmts, MemberData member)
        {
            stmts.Add(
                ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(SimpleMemberAccessExpression,
                            IdentifierName("writer"),
                            IdentifierName("WritePropertyName")),
                        ArgumentList(SeparatedList(
                            [
                                Argument(LiteralExpression(StringLiteralExpression, Literal(member.Name)))
                            ])))));
            stmts.Add(
                ExpressionStatement(
                    InvocationExpression(IdentifierName($"Write{member.CollectionElementType.Name}"),
                        ArgumentList(SeparatedList(
                            [
                                Argument(IdentifierName("writer")),
                                        Argument(MemberAccessExpression(SimpleMemberAccessExpression,
                                                    IdentifierName("obj"),
                                                    IdentifierName(member.Name)))
                            ])))));
            state.TypesToGenerate.Add(member.CollectionElementType);
        }

        static void WriteArray(List<StatementSyntax> stmts, MemberData member)
        {
            stmts.Add(
                ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(SimpleMemberAccessExpression,
                            IdentifierName("writer"),
                            IdentifierName("WritePropertyName")),
                        ArgumentList(SeparatedList(
                            [
                                Argument(LiteralExpression(StringLiteralExpression, Literal(member.Name)))
                            ])))));
            stmts.Add(
                ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(SimpleMemberAccessExpression,
                            IdentifierName("writer"),
                            IdentifierName("WriteStartArray")))));
            stmts.Add(WriteCollection(member));
            stmts.Add(
                ExpressionStatement(
                    InvocationExpression(
                        MemberAccessExpression(SimpleMemberAccessExpression,
                            IdentifierName("writer"),
                            IdentifierName("WriteEndArray")))));
        }

        static StatementSyntax WriteCollection(MemberData member)
        {
            return IfStatement(
                    BinaryExpression(NotEqualsExpression,
                        MemberAccessExpression(SimpleMemberAccessExpression,
                            IdentifierName("obj"),
                            IdentifierName(member.Name)),
                        LiteralExpression(NullLiteralExpression)),
                    ForEachStatement(
                        ParseTypeName("var"),
                        "iter",
                        MemberAccessExpression(SimpleMemberAccessExpression,
                            IdentifierName("obj"),
                            IdentifierName(member.Name)),
                            Block(
                                new StatementSyntax[]
                                {
                                    ExpressionStatement(
                                        InvocationExpression(
                                            MemberAccessExpression(SimpleMemberAccessExpression,
                                                IdentifierName("writer"),
                                                IdentifierName(GetMethodToCall(member))),
                                            ArgumentList(SeparatedList(
                                                [
                                                    Argument(IdentifierName("iter")),
                                                ]))))
                                })));
        }

        static string GetMethodToCall(MemberData member)
        {
            if (member is { MemberType: MemberTypes.SequentialCollection, CollectionElementType.Name: "String" }
                || member.MemberType == MemberTypes.String)
                return "WriteStringValue";
            else
                return "WriteNumberValue";
        }
    }
}