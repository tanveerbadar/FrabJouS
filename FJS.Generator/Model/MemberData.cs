namespace FJS.Generator.Model
{
    class MemberData
    {
        public string Name { get; set; }

        public string AttributeName { get; set; }

        public bool CanRead { get; set; }

        public bool CanWrite { get; set; }

        public MemberTypes MemberType { get; set; }

        public TypeData CollectionElementType { get; set; }
    }
}