using System.Text.Json.Serialization;

namespace FJS.UnitTests.Customizations.JsonIgnore;

public class IgnoredPropertiesTests
{
    [Fact]
    public void Serialize()
    {
        TestData6 data = new();
        var output1 = SerializerHelper.SerializeType(writer =>
        {
            IgnoredPropertiesHost host = new();
            host.Write(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }
}

[GeneratedSerializer]
[RootType(typeof(TestData6))]
partial class IgnoredPropertiesHost
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