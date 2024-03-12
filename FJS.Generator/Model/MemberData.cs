namespace FJS.Generator.Model;

class MemberData
{
    public string Name { get; set; }

    public bool CanRead { get; set; }

    public bool CanWrite { get; set; }

    public MemberType MemberType { get; set; }

    public PrimitiveType PrimitiveType { get; set; }

    public CollectionType CollectionType { get; set; }

    public MemberType ElementWritingMethod { get; set; }

    public bool IsNullable { get; set; }

    public bool IsNullableElement { get; set; }

    public TypeData ElementType { get; set; }
}