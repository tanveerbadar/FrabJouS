using FJS.Common.Metadata;
using System.Text;

namespace FJS.UnitTests.Collections.Dictionary;

public class SimplePropertiesTests
{
    [Fact]
    public void SerializeDictionaryWithStringKeys()
    {
        TestData2WithStringKeys data = new();
        var output1 = SerializerHelper.SerializeType(writer =>
        {
            SerializerHost9 host = new();
            host.WriteTestData2WithStringKeys(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }

    [Fact]
    public void SerializeDictionaryWithIntegralKeys()
    {
        TestData2WithIntegralKeys data = new();
        var output1 = SerializerHelper.SerializeType(writer =>
        {
            SerializerHost9 host = new();
            host.WriteTestData2WithIntegralKeys(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }
}

[GeneratedSerialier]
[RootType(typeof(TestData2WithStringKeys))]
[RootType(typeof(TestData2WithIntegralKeys))]
partial class SerializerHost9
{
}

class TestData2WithStringKeys
{
    public Dictionary<string, byte>? ByteProp { get; set; }

    public Dictionary<string, sbyte>? SByteProp { get; set; }

    public Dictionary<string, short>? ShortProp { get; set; }

    public Dictionary<string, ushort>? UShortProp { get; set; }

    public Dictionary<string, int>? IntProp { get; set; }

    public Dictionary<string, uint>? UIntProp { get; set; }

    public Dictionary<string, long>? LongProp { get; set; }

    public Dictionary<string, ulong>? ULongProp { get; set; }

    public Dictionary<string, float>? FloatProp { get; set; }

    public Dictionary<string, double>? DoubleProp { get; set; }

    public Dictionary<string, string>? StringProp { get; set; }

    public Dictionary<string, Version>? VersionProp { get; set; }

    public Dictionary<string, Guid>? GuidProp { get; set; }

    public Dictionary<string, TimeSpan>? TimeSpanProp { get; set; }

    public Dictionary<string, DateTime>? DateTimeProp { get; set; }

    public Dictionary<string, char>? CharProp { get; set; }

    public Dictionary<string, Rune>? RuneProp { get; set; }
}

class TestData2WithIntegralKeys
{
    public Dictionary<int, byte>? ByteProp { get; set; }

    public Dictionary<int, sbyte>? SByteProp { get; set; }

    public Dictionary<int, short>? ShortProp { get; set; }

    public Dictionary<int, ushort>? UShortProp { get; set; }

    public Dictionary<int, int>? IntProp { get; set; }

    public Dictionary<int, uint>? UIntProp { get; set; }

    public Dictionary<int, long>? LongProp { get; set; }

    public Dictionary<int, ulong>? ULongProp { get; set; }

    public Dictionary<int, float>? FloatProp { get; set; }

    public Dictionary<int, double>? DoubleProp { get; set; }

    public Dictionary<int, string>? StringProp { get; set; }

    public Dictionary<int, Version>? VersionProp { get; set; }

    public Dictionary<int, Guid>? GuidProp { get; set; }

    public Dictionary<int, TimeSpan>? TimeSpanProp { get; set; }

    public Dictionary<int, DateTime>? DateTimeProp { get; set; }

    public Dictionary<int, char>? CharProp { get; set; }

    public Dictionary<int, Rune>? RuneProp { get; set; }
}