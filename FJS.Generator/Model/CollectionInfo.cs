using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FJS.Generator.Model;

class CollectionInfo : MemberInfo
{
    public CollectionType CollectionType { get; set; }

    public TypeData ElementType { get; set; }

    public MemberType ElementWritingMethod { get; set; }

    protected override void EmitWriteStatementsCore(List<StatementSyntax> stmts)
    {
    }
}