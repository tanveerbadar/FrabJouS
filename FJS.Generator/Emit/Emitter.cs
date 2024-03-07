using System.Collections.Generic;
using System.Diagnostics;
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
            .AddMembers(AddMethods(host.Types, state))
            .AddMembers(AddCatchAllWriteMethod());

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

    static MethodDeclarationSyntax GenerateMethodForType(TypeData t, CodeGeneratorState state)
    {
        state.Processed.Add(t.Name);
        return MethodDeclaration(
                    ParseTypeName("void"), "Write")
                        .AddModifiers(Token(PublicKeyword))
                        .AddParameterListParameters(
                        [
                            Parameter(Identifier("writer")).WithType(ParseTypeName("Utf8JsonWriter")),
                            Parameter(Identifier("obj")).WithType(ParseTypeName(string.Join(".", t.Namespace, t.Name))),
                        ])
                        .AddBodyStatements(AddStatements($"{t.Namespace}.{t.Name}", t.Members, state));
    }

    static StatementSyntax[] AddStatements(string type, List<MemberData> members, CodeGeneratorState state)
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
                    state.TypesToGenerate.Add(member.ElementType);
                    switch (member.CollectionType)
                    {
                        case CollectionType.Array:
                            WriteArray(stmts, member.CollectionElementWritingMethod, member.PrimitiveType, $"{member.ElementType.Namespace}.{member.ElementType.Name}", member.Name);
                            break;
                        case CollectionType.Associative:
                            WriteDictionary(stmts, member, $"{member.ElementType.Namespace}.{member.ElementType.Name}", member.Name);
                            break;
                        default:
                            Debug.Fail($"We should never reach this. Member: {type}.{member.Name}.");
                            break;
                    }
                    break;
                case MemberType.ComplexObject:
                    WriteSubobject(state, stmts, member);
                    break;
                case MemberType.Nullable:
                    WriteNullable(stmts, member);
                    break;
                case MemberType.Primitive:
                    WritePrimitiveValue(
                        stmts,
                        member.Name,
                        member.PrimitiveType,
                        LiteralExpression(StringLiteralExpression, Literal(member.Name)),
                        MemberAccessExpression(SimpleMemberAccessExpression,
                                IdentifierName("obj"),
                                IdentifierName(member.Name)));
                    break;
                default:
                    Debug.Fail($"We should never reach this. Member: {type}.{member.Name}.");
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