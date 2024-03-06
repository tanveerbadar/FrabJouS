using System.Text;

namespace FJS.UnitTests.Collections.List;

public class SimplePropertiesTests
{
    [Fact]
    public void Serialize()
    {
        TestData2 data = new();
        var output1 = SerializerHelper.SerializeType(writer =>
        {
            ListWithSimpleValuesHost host = new();
            host.Write(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }
}

[GeneratedSerialier]
[RootType(typeof(TestData2))]
partial class ListWithSimpleValuesHost
{
}

class TestData2
{
    public List<byte>? ByteProp { get; set; }

    public List<sbyte>? SByteProp { get; set; }

    public List<short>? ShortProp { get; set; }

    public List<ushort>? UShortProp { get; set; }

    public List<int>? IntProp { get; set; }

    public List<uint>? UIntProp { get; set; }

    public List<long>? LongProp { get; set; }

    public List<ulong>? ULongProp { get; set; }

    public List<float>? FloatProp { get; set; }

    public List<double>? DoubleProp { get; set; }

    public List<string>? StringProp { get; set; }

    public List<Version>? VersionProp { get; set; }

    public List<Guid>? GuidProp { get; set; }

    public List<TimeSpan>? TimeSpanProp { get; set; }

    public List<DateTime>? DateTimeProp { get; set; }

    public List<char>? CharProp { get; set; }

    public List<Rune>? RuneProp { get; set; }
}