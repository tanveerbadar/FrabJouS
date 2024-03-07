using System.Collections.Generic;
using System.Diagnostics;
using FJS.Generator.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace FJS.Generator.Emit;

static partial class Emitter
{
    static void WriteArray(List<StatementSyntax> stmts, MemberType collectionElementWritingMethod, PrimitiveType primitiveType, string type, string name)
    {
        stmts.Add(
            ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(SimpleMemberAccessExpression,
                        IdentifierName("writer"),
                        IdentifierName("WritePropertyName")),
                    ArgumentList(SeparatedList(
                        [
                            Argument(LiteralExpression(StringLiteralExpression, Literal(name)))
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
                        IdentifierName(name)),
                    LiteralExpression(NullLiteralExpression)),
                ForEachStatement(
                    ParseTypeName("var"),
                    "iter",
                    MemberAccessExpression(SimpleMemberAccessExpression,
                        IdentifierName("obj"),
                        IdentifierName(name)),
                        Block(WriteCollectionValue(collectionElementWritingMethod, primitiveType, type, name)))));
        stmts.Add(
            ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(SimpleMemberAccessExpression,
                        IdentifierName("writer"),
                        IdentifierName("WriteEndArray")))));
    }

    static void WriteDictionary(List<StatementSyntax> stmts, MemberData member, string type, string name)
    {
        stmts.Add(
            ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(SimpleMemberAccessExpression,
                        IdentifierName("writer"),
                        IdentifierName("WritePropertyName")),
                    ArgumentList(SeparatedList(
                        [
                            Argument(LiteralExpression(StringLiteralExpression, Literal(name)))
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
                    IdentifierName(name)),
                    Block(WriteCollectionValue(member.CollectionElementWritingMethod, member.PrimitiveType, type, name))));
        stmts.Add(
            ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(SimpleMemberAccessExpression,
                        IdentifierName("writer"),
                        IdentifierName("WriteEndObject")))));
    }

    static List<StatementSyntax> WriteCollectionValue(MemberType collectionElementWritingMethod, PrimitiveType primitiveType, string type, string name)
    {
        List<StatementSyntax> bodyStatements;
        switch (collectionElementWritingMethod)
        {
            case MemberType.Collection:
                bodyStatements =
                [
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
                                                IdentifierName("ToString"))))
                                ])))),
                ];
                break;
            case MemberType.ComplexObject:
                bodyStatements =
                [
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
                                                IdentifierName("ToString"))))
                                ])))),
                        WriteValue(
                            name,
                            MemberType.ComplexObject,
                            MemberAccessExpression(SimpleMemberAccessExpression,
                                IdentifierName("kvp"),
                                IdentifierName("Value")))
                ];
                break;
            // case MemberType.Nullable:
            //     WriteNullable(stmts, member.ElementType);
            //     break;
            case MemberType.Primitive:
                bodyStatements = new();
                WritePrimitiveValue(
                    bodyStatements,
                    name,
                    primitiveType,
                    InvocationExpression(
                        MemberAccessExpression(SimpleMemberAccessExpression,
                            MemberAccessExpression(SimpleMemberAccessExpression,
                                IdentifierName("kvp"),
                                IdentifierName("Key")),
                            IdentifierName("ToString"))),
                    MemberAccessExpression(SimpleMemberAccessExpression,
                        IdentifierName("kvp"),
                        IdentifierName("Value")));
                break;
            default:
                bodyStatements = default;
                Debug.Fail($"We should never reach this. Member: {type}.{name}, type: {collectionElementWritingMethod} .");
                break;
        }
        return bodyStatements;
    }
}