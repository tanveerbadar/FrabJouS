using System.Collections.Generic;

namespace FJS.Generator.Model
{
    class TypeData
    {
        public string Name { get; set; }

        public string Namespace { get; set; }

        public List<MemberData> Members { get; } = new();
    }
}