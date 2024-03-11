using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http.Headers;
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

    static StatementSyntax WriteNullableValue(ComplexObjectInfo member)
    {
        return default;
    }

    static StatementSyntax WriteValue(string name, MemberType memberType, ExpressionSyntax nameExpression)
    {
        switch (memberType)
        {
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
            default:
                Debug.Fail($"We should never reach this. Member: {name}.");
                break;
        }
        // This is unreachable but the compiler doesn't seem to know it. Probably because there's no [DoesNotReturn] on netstandard.
        return default;
    }

    static void WriteNullable(List<StatementSyntax> stmts, ComplexObjectInfo member)
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
                        WriteNullableValue(member),
                    }),
                ElseClause(Block(new StatementSyntax[] { WriteNullValue(member), }))));
    }

    static ExpressionStatementSyntax WriteNullValue(MemberInfo member)
        => ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(SimpleMemberAccessExpression,
                        IdentifierName("writer"),
                        IdentifierName("WriteNull")),
                    ArgumentList(SeparatedList(
                        [
                            Argument(LiteralExpression(StringLiteralExpression, Literal(member.Name)))
                        ]))));

    static void WriteSubobject(CodeGeneratorState state, List<StatementSyntax> stmts, ComplexObjectInfo member)
    {
        stmts.Add(
            IfStatement(
                BinaryExpression(NotEqualsExpression,
                    IdentifierName("obj"),
                    LiteralExpression(NullLiteralExpression)),
                Block(new StatementSyntax[]
                    {
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
                                MemberAccessExpression(SimpleMemberAccessExpression,
                                        IdentifierName("obj"),
                                        IdentifierName(member.Name)))
                    }), 
                ElseClause(Block(new StatementSyntax[] { WriteNullValue(member), }))));

        state.TypesToGenerate.Add(member.ElementType);
    }
}