using System.Collections.Generic;
using System.Linq;
using FJS.Generator.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace FJS.Generator.Emit;

static partial class Emitter
{
    public static string GetGeneratedSource(Host host)
    {
        CodeGeneratorState state = new();
        var surrogateType = ClassDeclaration(host.Name)
            .AddModifiers(Token(PartialKeyword))
            .AddMembers(AddMethods(host.Types, state));

        var types = state.TypesToGenerate;

        while (types is { Count: > 0 })
        {
            var type = types[types.Count - 1];
            if (state.Processed.Add(type.FullName))
            {
                surrogateType = surrogateType.AddMembers(GenerateMethodForType(type, state));
            }
            types.Remove(type);
        }

        surrogateType = surrogateType.AddMembers(AddCatchAllWriteMethod());
        MemberDeclarationSyntax member = surrogateType;

        if (!string.IsNullOrEmpty(host.Namespace))
        {
            member = NamespaceDeclaration(ParseName(host.Namespace))
                .AddMembers(surrogateType);
        }

        var code = CompilationUnit()
            .AddUsings(UsingDirective(ParseName("System.Text.Json")))
            .AddMembers(member)
            .NormalizeWhitespace()
            .ToFullString();
        return code;
    }

    static MethodDeclarationSyntax[] AddMethods(List<TypeData> types, CodeGeneratorState state) =>
        types.Select(t => GenerateMethodForType(t, state)).ToArray();

    static MethodDeclarationSyntax AddCatchAllWriteMethod()
        => MethodDeclaration(ParseTypeName("void"), "Write")
                             .AddModifiers(Token(PublicKeyword))
                             .AddParameterListParameters(
                                 [
                                    Parameter(Identifier("writer")).WithType(ParseTypeName("Utf8JsonWriter")),
                                    Parameter(Identifier("obj")).WithType(ParseTypeName("object")),
                                 ]
                             )
                             .AddBodyStatements(
                                ThrowStatement(
                                    ObjectCreationExpression(
                                        ParseTypeName("NotImplementedException"),
                                        ArgumentList(SeparatedList(
                                            [
                                                Argument(LiteralExpression(StringLiteralExpression, Literal("This indicates a bug in generator.")))
                                            ])),
                                        null)));

    static MethodDeclarationSyntax GenerateMethodForType(TypeData type, CodeGeneratorState state)
    {
        state.Processed.Add(type.FullName);
        return MethodDeclaration(ParseTypeName("void"), "Write")
                             .AddModifiers(Token(PublicKeyword))
                             .AddParameterListParameters(
                                 [
                                    Parameter(Identifier("writer")).WithType(ParseTypeName("Utf8JsonWriter")),
                                    Parameter(Identifier("obj")).WithType(ParseTypeName(type.FullName)),
                                 ]
                             )
                             .AddBodyStatements(AddStatements(type.Members, type.FullName, state));
    }

    static StatementSyntax[] AddStatements(List<MemberData> members, string type, CodeGeneratorState state)
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
            switch (member.MemberType)
            {
                case MemberType.Collection:
                    WriteCollection(state, stmts, member);
                    break;
                case MemberType.ComplexObject:
                    WriteSubobject(state, stmts, member);
                    break;
                case MemberType.Nullable:
                    WriteNullable(state, stmts, type, member);
                    break;
                case MemberType.Primitive:
                    WritePrimitiveValue(
                        stmts,
                        member.Name,
                        member.PrimitiveType,
                        MemberAccessExpression(SimpleMemberAccessExpression,
                                IdentifierName("obj"),
                                IdentifierName(member.Name)));
                    break;
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
}