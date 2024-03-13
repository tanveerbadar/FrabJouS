using System.Collections.Generic;
using System.Diagnostics;
using FJS.Generator.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace FJS.Generator.Emit;

static partial class Emitter
{
    static void WriteCollection(CodeGeneratorState state, List<StatementSyntax> stmts, MemberData member)
    {
        switch (member.CollectionType)
        {
            case CollectionType.Associative:
                WriteDictionary(state, stmts, member);
                break;
            case CollectionType.Sequential:
                WriteArray(state, stmts, member);
                break;
        }
    }

    static void WriteArray(CodeGeneratorState state, List<StatementSyntax> stmts, MemberData member)
    {
        if (member.ElementWritingMethod == MemberType.ComplexObject)
        {
            state.TypesToGenerate.Add(member.ElementType);
        }
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

        var body = WriteValue(member.Name, member.ElementWritingMethod, member.PrimitiveType, IdentifierName("iter"));

        if (member.IsNullableElement)
        {
            body =
                IfStatement(
                    BinaryExpression(NotEqualsExpression,
                        IdentifierName("iter"),
                        LiteralExpression(NullLiteralExpression)),
                    Block(new StatementSyntax[] { body }));
        }

        stmts.Add(
            IfStatement(
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
                        Block(new StatementSyntax[] { body }))));
        stmts.Add(
            ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(SimpleMemberAccessExpression,
                        IdentifierName("writer"),
                        IdentifierName("WriteEndArray")))));
    }

    static void WriteDictionary(CodeGeneratorState state, List<StatementSyntax> stmts, MemberData member)
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

        List<StatementSyntax> statements = new();

        switch (member.ElementWritingMethod)
        {
            case MemberType.Primitive:
                string methodName = null;
                switch (member.PrimitiveType)
                {
                    case PrimitiveType.String:
                        methodName = "WriteString";
                        break;
                    case PrimitiveType.Number:
                        methodName = "WriteNumber";
                        break;
                    case PrimitiveType.Boolean:
                        methodName = "WriteBoolean";
                        break;
                    default:
                        Debug.Fail($"We should never reach this. Member: {member.Name}, type: {member.PrimitiveType}.");
                        break;
                }
                statements.Add(
                    ExpressionStatement(
                        InvocationExpression(
                            MemberAccessExpression(SimpleMemberAccessExpression,
                                IdentifierName("writer"),
                                IdentifierName(methodName)),
                            ArgumentList(SeparatedList(
                                [
                                    Argument(
                                        InvocationExpression(
                                            MemberAccessExpression(SimpleMemberAccessExpression,
                                                MemberAccessExpression(SimpleMemberAccessExpression,
                                                    IdentifierName("kvp"),
                                                    IdentifierName("Key")),
                                                IdentifierName("ToString")))),
                                    Argument(
                                        MemberAccessExpression(SimpleMemberAccessExpression,
                                            IdentifierName("kvp"),
                                            IdentifierName("Value"))),
                                ])))));
                break;
            case MemberType.ComplexObject:
                state.TypesToGenerate.Add(member.ElementType);

                statements.Add(
                    ExpressionStatement(
                        InvocationExpression(
                            MemberAccessExpression(SimpleMemberAccessExpression,
                                IdentifierName("writer"),
                                IdentifierName("WritePropertyName")),
                            ArgumentList(SeparatedList(
                            [
                                Argument(
                                    InvocationExpression(
                                        MemberAccessExpression(SimpleMemberAccessExpression,
                                            MemberAccessExpression(SimpleMemberAccessExpression,
                                                IdentifierName("kvp"),
                                                IdentifierName("Key")),
                                            IdentifierName("ToString")))),
                            ])))));

                statements.Add(
                    ExpressionStatement(
                        InvocationExpression(IdentifierName("Write"),
                            ArgumentList(SeparatedList(
                                [
                                    Argument(IdentifierName("writer")),
                                    Argument(
                                        MemberAccessExpression(SimpleMemberAccessExpression,
                                            IdentifierName("kvp"),
                                            IdentifierName("Value"))),
                                ])))));
                break;
        }

        stmts.Add(
            ForEachStatement(
                ParseTypeName("var"),
                    "kvp",
                    MemberAccessExpression(SimpleMemberAccessExpression,
                        IdentifierName("obj"),
                        IdentifierName(member.Name)),
                        Block(statements)));
        stmts.Add(
            ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(SimpleMemberAccessExpression,
                        IdentifierName("writer"),
                        IdentifierName("WriteEndObject")))));
    }
}