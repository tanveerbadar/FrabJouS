namespace FJS.UnitTests.Collections.Array;

public class ComplexPropertiesTests
{
    [Fact]
    public void SerializeComplexValuesTest()
    {
        TestData3 data = new();
        var output1 = SerializerHelper.SerializeType(writer =>
        {
            ComplexArrayPropertiesHost host = new();
            host.Write(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }

    [Fact]
    public void SerializeDictionaryValuesTest()
    {
        TestData3WithDictionaryValues data = new();
        var output1 = SerializerHelper.SerializeType(writer =>
        {
            ComplexArrayPropertiesHost host = new();
            host.Write(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }
}

[GeneratedSerializer]
[RootType(typeof(TestData3))]
[RootType(typeof(TestData3WithDictionaryValues))]
partial class ComplexArrayPropertiesHost
{
}

class TestData3
{
    public TestData2[]? Data { get; set; }
}

class TestData3WithDictionaryValues
{
    public Dictionary<string, TestData2>[]? Data { get; set; }
}
