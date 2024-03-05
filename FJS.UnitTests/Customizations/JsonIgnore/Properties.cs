using System.Text.Json.Serialization;
using FJS.Common.Metadata;

namespace FJS.UnitTests.Customizations.JsonIgnore;

public class IgnoredPropertiesTests
{
    [Fact]
    public void Serialize()
    {
        TestData6 data = new();
        var output1 = SerializerHelper.SerializeType(writer =>
        {
            SerializerHost3 host = new();
            host.WriteTestData6(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }
}

[GeneratedSerialier]
[RootType(typeof(TestData6))]
partial class SerializerHost3
{
}

class TestData6
{
    public int AvailableProp { get; set; }

    [JsonIgnore]
    public int IgnoredProp { get; set; }

    [JsonIgnore]
    public int AlwaysIgnoredProp { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int IgnoredWhenDefault { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IgnoredWhenNul { get; set; }
}