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

[GeneratedSerialier]
[RootType(typeof(NullableFields))]
partial class NullableFieldsHost
{
}

class NullableFields
{
    public byte? ByteProp;

    public sbyte? SByteProp;

    public short? ShortProp;

    public ushort? UShortProp;

    public int? IntProp;

    public uint? UIntProp;

    public long? LongProp;

    public ulong? ULongProp;

    public float? FloatProp;

    public double? DoubleProp;

    public Guid? GuidProp;

    public TimeSpan? TimeSpanProp;

    public DateTime? DateTimeProp;

    public char? CharProp;

    public Rune? RuneProp;
}
