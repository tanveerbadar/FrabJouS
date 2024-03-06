using System.Collections.Generic;
using System.Diagnostics;
using FJS.Generator.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace FJS.Generator;

static partial class CodeGenerator
{
    static void WriteCollection(string type, List<StatementSyntax> stmts, MemberData member)
    {
        switch (member.CollectionType)
        {
            case CollectionType.Sequential:
                WriteArray(type, stmts, member);
                break;
            case CollectionType.Associative:
                WriteDictionary(stmts, member);
                break;
            default:
                Debug.Fail($"We should never reach this. Member: {type}.{member.Name}.");
                break;
        }
    }

    static void WriteArray(string type, List<StatementSyntax> stmts, MemberData member)
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
                                WriteElement(type, stmts, member)
                            }))));
        stmts.Add(
            ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(SimpleMemberAccessExpression,
                        IdentifierName("writer"),
                        IdentifierName("WriteEndArray")))));
    }

    static StatementSyntax WriteElement(string type, List<StatementSyntax> stmts, MemberData member)
    {
        switch (member.CollectionElementWritingMethod)
        {
            case MemberType.Number:
            case MemberType.String:
                return WritePrimitive(member);
            case MemberType.ComplexObject:
                return
                    ExpressionStatement(
                        InvocationExpression(IdentifierName("Write"),
                            ArgumentList(SeparatedList(
                                [
                                    Argument(IdentifierName("writer")),
                                    Argument(IdentifierName("iter")),
                                ]))));
            case MemberType.Unspecified:
                Debug.Fail($"We should never reach this. Member: {type}.{member.Name}.");
                break;
        }
        return ExpressionStatement(InvocationExpression(
                                                MemberAccessExpression(SimpleMemberAccessExpression,
                                                    IdentifierName("writer"),
                                                    IdentifierName(GetMethodToCall(member))),
                                                ArgumentList(SeparatedList(
                                                    [
                                                        Argument(IdentifierName("iter")),
                                                    ]))));
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
                                                IdentifierName(member.MemberType == MemberType.String ? "WriteString" : "WriteNumber")),
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