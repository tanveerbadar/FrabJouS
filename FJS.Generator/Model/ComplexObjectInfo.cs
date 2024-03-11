using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace FJS.Generator.Model;

class ComplexObjectInfo : MemberInfo
{
    public required TypeData ElementType { get; init; }

    public required MemberType ElementWritingMethod { get; init; }

    protected override void EmitWriteStatementsCore(List<StatementSyntax> stmts)
    {
        stmts.Add(EmptyStatement().WithLeadingTrivia(Comment($"// Generating for type: '{Name}'.")));
        // foreach (var member in ElementType.Members)
        // {
        //     stmts.Add(EmptyStatement().WithLeadingTrivia(Comment($"Generating for member: {member.Name}.")));
        //     member.EmitWriteStatements(stmts);
        // }
    }
}