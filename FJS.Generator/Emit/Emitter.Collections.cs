using System.Collections.Generic;
using FJS.Generator.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace FJS.Generator.Emit;

static partial class Emitter
{
    static void WriteArray(List<StatementSyntax> stmts, MemberInfo member)
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
                        Block(
                            new StatementSyntax[]
                            {
                                WriteValue("iter", MemberType.ComplexObject, IdentifierName("iter"))
                            }))));
        stmts.Add(
            ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(SimpleMemberAccessExpression,
                        IdentifierName("writer"),
                        IdentifierName("WriteEndArray")))));
    }

    static void WriteDictionary(List<StatementSyntax> stmts, MemberInfo member)
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
                                                    IdentifierName("WriteNumber")),
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
}