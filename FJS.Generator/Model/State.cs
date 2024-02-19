using System.Collections.Generic;

namespace FJS.Generator.Model
{
    class State
    {
        public List<TypeData> TypesToGenerate { get; } = new();
        public HashSet<string> Processed { get; } = new();
    }
}