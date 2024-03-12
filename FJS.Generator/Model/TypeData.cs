using System.Collections.Generic;

namespace FJS.Generator.Model;

class TypeData
{
    public string Namespace { get; set; }

    public string Name { get; set; }

    public string FullName => $"{Namespace}.{Name}";

    public List<MemberData> Members { get; } = new();
}