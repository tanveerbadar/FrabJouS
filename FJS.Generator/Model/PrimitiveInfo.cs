using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace FJS.Generator.Model;

class PrimitiveInfo : MemberInfo
{
    public required PrimitiveType PrimitiveType { get; init; }

    protected override void EmitWriteStatementsCore(List<StatementSyntax> stmts)
    {
        string methodName = null;
        switch (PrimitiveType)
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
                Debug.Fail($"We should never reach this. Member: {Name}, type: {PrimitiveType}.");
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
                            Argument(
                                LiteralExpression(StringLiteralExpression, Literal(Name))),
                            Argument(
                                MemberAccessExpression(SimpleMemberAccessExpression,
                                    IdentifierName("obj"),
                                    IdentifierName(Name))),
                        ])))));
    }
}