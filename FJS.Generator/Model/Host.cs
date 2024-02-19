using System.Collections.Generic;

namespace FJS.Generator.Model
{
    class Host
    {
        public string Name { get; set; }

        public List<TypeData> Types { get; } = new();
    }
}