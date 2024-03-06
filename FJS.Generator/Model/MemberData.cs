namespace FJS.Generator.Model;

class MemberData
{
    public string Name { get; set; }

    public bool CanRead { get; set; }

    public bool CanWrite { get; set; }

    public MemberType MemberType { get; set; }

    public TypeData CollectionElementType { get; set; }
}