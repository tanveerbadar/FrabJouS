using System.Collections.Generic;
using System.Diagnostics;
using FJS.Generator.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace FJS.Generator.Emit;

static partial class Emitter
{
    static void WritePrimitiveValue(List<StatementSyntax> stmts, string name, PrimitiveType primitiveType, ExpressionSyntax nameExpression)
    {
        string methodName = null;
        switch (primitiveType)
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
                Debug.Fail($"We should never reach this. Member: {name}, type: {primitiveType}.");
                break;
        }
        stmts.Add(
            ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(SimpleMemberAccessExpression,
                        IdentifierName("writer"),
                        IdentifierName(methodName)),
                    ArgumentList(SeparatedList(
                        [
                            Argument(LiteralExpression(StringLiteralExpression, Literal(name))),
                            Argument(nameExpression),
                        ])))));
    }

    static StatementSyntax WriteNullableValue(CodeGeneratorState state, string type, MemberData member)
    {
        List<StatementSyntax> stmts = default;
        switch (member.ElementWritingMethod)
        {
            case MemberType.Primitive:
                stmts = new();
                WritePrimitiveValue(
                                stmts,
                                member.Name,
                                member.PrimitiveType,
                                MemberAccessExpression(SimpleMemberAccessExpression,
                                    MemberAccessExpression(SimpleMemberAccessExpression,
                                        IdentifierName("obj"),
                                        IdentifierName(member.Name)),
                                    IdentifierName("Value")));
                return stmts[0];
            case MemberType.ComplexObject:
                state.TypesToGenerate.Add(member.ElementType);
                return
                    ExpressionStatement(
                        InvocationExpression(IdentifierName("Write"),
                            ArgumentList(SeparatedList(
                                [
                                    Argument(IdentifierName("writer")),
                                    Argument(
                                        MemberAccessExpression(SimpleMemberAccessExpression,
                                        MemberAccessExpression(SimpleMemberAccessExpression,
                                            IdentifierName("obj"),
                                            IdentifierName(member.Name)),
                                        IdentifierName("Value"))),
                                ]))));
            default:
                Debug.Fail($"We should never reach this. Type: {type}, member: {member.Name}, writing method: {member.ElementWritingMethod}.");
                break;
        }
        return default;
    }

    static StatementSyntax WriteValue(string name, MemberType memberType, PrimitiveType primitiveType, ExpressionSyntax nameExpression)
    {
        switch (memberType)
        {
            case MemberType.Primitive:
                List<StatementSyntax> stmts = new();
                WritePrimitiveValue(
                                stmts,
                                name,
                                primitiveType,
                                nameExpression);
                return stmts[0];
            case MemberType.Nullable:
                return ExpressionStatement(
                            InvocationExpression(
                                MemberAccessExpression(SimpleMemberAccessExpression,
                                    IdentifierName("writer"),
                                    IdentifierName("WriteNumber")),
                                ArgumentList(SeparatedList(
                                    [
                                        Argument(LiteralExpression(StringLiteralExpression, Literal(name))),
                                        Argument(nameExpression),
                                    ]))));
            case MemberType.ComplexObject:
                return
                    ExpressionStatement(
                        InvocationExpression(IdentifierName("Write"),
                            ArgumentList(SeparatedList(
                                [
                                    Argument(IdentifierName("writer")),
                                    Argument(nameExpression),
                                ]))));
            case MemberType.Collection:
                break;
            default:
                Debug.Fail($"We should never reach this. Member: {name}, type: {memberType}.");
                break;
        }
        return EmptyStatement();
    }

    static void WriteNullable(CodeGeneratorState state, List<StatementSyntax> stmts, string type, MemberData member)
    {
        stmts.Add(
            IfStatement(
                MemberAccessExpression(SimpleMemberAccessExpression,
                    MemberAccessExpression(SimpleMemberAccessExpression,
                        IdentifierName("obj"),
                        IdentifierName(member.Name)),
                    IdentifierName("HasValue")),
                Block(new StatementSyntax[]
                    {
                        WriteNullableValue(state, type, member),
                    }),
                ElseClause(Block(new StatementSyntax[] { WriteNullValue(member), }))));
    }

    static ExpressionStatementSyntax WriteNullValue(MemberData member)
        => ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(SimpleMemberAccessExpression,
                        IdentifierName("writer"),
                        IdentifierName("WriteNull")),
                    ArgumentList(SeparatedList(
                        [
                            Argument(LiteralExpression(StringLiteralExpression, Literal(member.Name)))
                        ]))));

    static void WriteSubobject(CodeGeneratorState state, List<StatementSyntax> stmts, MemberData member)
    {
        StatementSyntax[] statements =
        [
            ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(SimpleMemberAccessExpression,
                        IdentifierName("writer"),
                        IdentifierName("WritePropertyName")),
                    ArgumentList(SeparatedList(
                        [
                            Argument(LiteralExpression(StringLiteralExpression, Literal(member.Name)))
                        ])))),
            WriteValue(
                member.Name,
                MemberType.ComplexObject,
                PrimitiveType.Unspecified,
                MemberAccessExpression(SimpleMemberAccessExpression,
                        IdentifierName("obj"),
                        IdentifierName(member.Name)))
        ];
        if (member.IsNullable)
        {
            stmts.Add(
                    IfStatement(
                        BinaryExpression(NotEqualsExpression,
                            MemberAccessExpression(SimpleMemberAccessExpression,
                                IdentifierName("obj"),
                                IdentifierName(member.Name)),
                            LiteralExpression(NullLiteralExpression)),
                        Block(statements),
                        ElseClause(Block(new StatementSyntax[] { WriteNullValue(member), }))));
        }
        else
        {
            stmts.AddRange(statements);
        }

        state.TypesToGenerate.Add(member.ElementType);
    }
}