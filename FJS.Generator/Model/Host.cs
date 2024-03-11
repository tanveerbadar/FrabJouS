using System.Collections.Generic;

namespace FJS.Generator.Model;

public class Host
{
    public string Name { get; set; }

    public string Namespace { get; set; }

    public List<TypeData> Types { get; } = new();
}