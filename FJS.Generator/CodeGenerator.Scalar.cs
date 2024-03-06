using FJS.Generator.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace FJS.Generator;

static partial class CodeGenerator
{
    static StatementSyntax WritePrimitive(MemberData member)
        => ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(SimpleMemberAccessExpression,
                        IdentifierName("writer"),
                        IdentifierName(member.MemberType == MemberType.String ? "WriteString" : "WriteNumber")),
                    ArgumentList(SeparatedList(
                        [
                            Argument(LiteralExpression(StringLiteralExpression, Literal(member.Name))),
                                    Argument(MemberAccessExpression(SimpleMemberAccessExpression,
                                        IdentifierName("obj"),
                                        IdentifierName(member.Name))),
                        ]))));

    static StatementSyntax WriteValue(MemberType memberType, MemberType collectionElementType, string name, ExpressionSyntax valueExpression)
    {
        switch (memberType)
        {
            case MemberType.Number:
            case MemberType.String:
                return ExpressionStatement(
                            InvocationExpression(
                                MemberAccessExpression(SimpleMemberAccessExpression,
                                    IdentifierName("writer"),
                                    IdentifierName(memberType == MemberType.String ? "WriteString" : "WriteNumber")),
                                ArgumentList(SeparatedList(
                                    [
                                        Argument(LiteralExpression(StringLiteralExpression, Literal(name))),
                                        Argument(valueExpression),
                                    ]))));
            case MemberType.Nullable:
                return WriteNullable(collectionElementType, name);
            case MemberType.ComplexObject:
                return
                    ExpressionStatement(
                        InvocationExpression(IdentifierName("Write"),
                            ArgumentList(SeparatedList(
                                [
                                    Argument(IdentifierName("writer")),
                                    Argument(valueExpression),
                                ]))));
            case MemberType.Collection:
                break;
        }
        return ExpressionStatement(

                    InvocationExpression(
                        MemberAccessExpression(SimpleMemberAccessExpression,
                            IdentifierName("writer"),
                            IdentifierName(memberType == MemberType.String ? "WriteString" : "WriteNumber")),
                        ArgumentList(SeparatedList(
                            [
                                Argument(LiteralExpression(StringLiteralExpression, Literal(name))),
                                Argument(MemberAccessExpression(SimpleMemberAccessExpression,
                                    IdentifierName("obj"),
                                    IdentifierName(name))),
                            ]))));
    }

    static StatementSyntax WriteNullable(MemberType memberType, string name)
    {
        return IfStatement(
                MemberAccessExpression(SimpleMemberAccessExpression,
                    MemberAccessExpression(SimpleMemberAccessExpression,
                        IdentifierName("obj"),
                        IdentifierName(name)),
                    IdentifierName("HasValue")),
                Block(new StatementSyntax[]
                    {
                        ExpressionStatement(
                            InvocationExpression(
                                MemberAccessExpression(SimpleMemberAccessExpression,
                                    IdentifierName("writer"),
                                    IdentifierName("WritePropertyName")),
                                ArgumentList(SeparatedList(
                                    [
                                        Argument(LiteralExpression(StringLiteralExpression, Literal(name)))
                                    ])))),
                        WriteValue(memberType,
                            memberType,
                            name,
                            MemberAccessExpression(SimpleMemberAccessExpression,
                                IdentifierName("obj"),
                                IdentifierName(name))),
                    }),
                ElseClause(
                    Block(new StatementSyntax[]
                    {
                        ExpressionStatement(
                            InvocationExpression(
                                MemberAccessExpression(SimpleMemberAccessExpression,
                                    IdentifierName("writer"),
                                    IdentifierName("WriteNull")),
                                ArgumentList(SeparatedList(
                                    [
                                        Argument(LiteralExpression(StringLiteralExpression, Literal(name)))
                                    ]))))
                    })));
    }
}