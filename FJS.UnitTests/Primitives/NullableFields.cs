using System.Text;

namespace FJS.UnitTests.Primitives;

public class NullableFieldsTests
{
    [Fact]
    public void Serialize()
    {
        NullableFieldsHost data = new();
        var output1 = SerializerHelper.SerializeType(writer =>
        {
            NullableFieldsHost host = new();
            host.Write(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }
}

[GeneratedSerializer]
[RootType(typeof(NullableFields))]
partial class NullableFieldsHost
{
}

class NullableFields
{
    public byte? ByteProp = default;

    public sbyte? SByteProp = default;

    public short? ShortProp = default;

    public ushort? UShortProp = default;

    public int? IntProp = default;

    public uint? UIntProp = default;

    public long? LongProp = default;

    public ulong? ULongProp = default;

    public float? FloatProp = default;

    public double? DoubleProp = default;

    public Guid? GuidProp = default;

    public TimeSpan? TimeSpanProp = default;

    public DateTime? DateTimeProp = default;

    public char? CharProp = default;

    public bool BoolProp = default;

    public Rune? RuneProp = default;
}