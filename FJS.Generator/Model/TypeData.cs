using System.Collections.Generic;

namespace FJS.Generator.Model;

public class TypeData
{
    public string Name { get; set; }

    public string Namespace { get; set; }

    public List<MemberInfo> Members { get; } = new();
}