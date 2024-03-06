using System.Collections.Generic;
using System.Diagnostics;
using FJS.Generator.Model;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace FJS.Generator.Emit;

static partial class Emitter
{
    static StatementSyntax WriteValue(string name, MemberType memberType, ExpressionSyntax nameExpression)
    {
        switch (memberType)
        {
            case MemberType.Number:
            case MemberType.Nullable:
            case MemberType.String:
                return ExpressionStatement(
                            InvocationExpression(
                                MemberAccessExpression(SimpleMemberAccessExpression,
                                    IdentifierName("writer"),
                                    IdentifierName(memberType == MemberType.String ? "WriteString" : "WriteNumber")),
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

    static void WriteNullable(List<StatementSyntax> stmts, MemberData member)
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
                            member.MemberType,
                            MemberAccessExpression(SimpleMemberAccessExpression,
                                IdentifierName("obj"),
                                IdentifierName(member.Name))),
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
                                        Argument(LiteralExpression(StringLiteralExpression, Literal(member.Name)))
                                    ]))))
                    }))));
    }

    static void WriteSubobject(CodeGeneratorState state, List<StatementSyntax> stmts, MemberData member)
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
            WriteValue(
                member.Name,
                MemberType.ComplexObject,
                MemberAccessExpression(SimpleMemberAccessExpression,
                        IdentifierName("obj"),
                        IdentifierName(member.Name))));
        state.TypesToGenerate.Add(member.CollectionElementType);
    }
}