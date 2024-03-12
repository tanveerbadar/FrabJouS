using System.Text;

namespace FJS.UnitTests.Primitives;

public class NullablePropertiesTests
{
    [Fact]
    public void Serialize()
    {
        NullableProperties data = new();
        var output1 = SerializerHelper.SerializeType(writer =>
        {
            NullablePropertiesHost host = new();
            host.Write(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }
}

[GeneratedSerializer]
[RootType(typeof(NullableProperties))]
partial class NullablePropertiesHost
{
}

class NullableProperties
{
    public byte? ByteProp { get; set; }

    public sbyte? SByteProp { get; set; }

    public short? ShortProp { get; set; }

    public ushort? UShortProp { get; set; }

    public int? IntProp { get; set; }

    public uint? UIntProp { get; set; }

    public long? LongProp { get; set; }

    public ulong? ULongProp { get; set; }

    public float? FloatProp { get; set; }

    public double? DoubleProp { get; set; }

    public Guid? GuidProp { get; set; }

    public TimeSpan? TimeSpanProp { get; set; }

    public DateTime? DateTimeProp { get; set; }

    public char? CharProp { get; set; }

    public bool? BoolProp { get; set; }

    public Rune? RuneProp { get; set; }
}