using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace FJS.Generator.Emit;

static partial class Emitter
{
    static ExpressionStatementSyntax InvokeMethod(string source, string methodName)
    =>
        ExpressionStatement(
            InvocationExpression(
                MemberAccessExpression(SimpleMemberAccessExpression,
                    IdentifierName(source),
                    IdentifierName(methodName))));

    static ExpressionStatementSyntax InvokeMethod(string source, string methodName, IEnumerable<ArgumentSyntax> arguments)
    =>
        ExpressionStatement(
            InvocationExpression(
                MemberAccessExpression(SimpleMemberAccessExpression,
                    IdentifierName(source),
                    IdentifierName(methodName)),
                    ArgumentList(SeparatedList(arguments))));

    static ExpressionStatementSyntax InvokeMethod(string methodName, IEnumerable<ArgumentSyntax> arguments)
    =>
        ExpressionStatement(
            InvocationExpression(IdentifierName(methodName),
                ArgumentList(SeparatedList(arguments))));
}