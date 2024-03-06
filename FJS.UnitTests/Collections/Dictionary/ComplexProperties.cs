namespace FJS.UnitTests.Collections.Dictionary;

public class ComplexPropertiesTests
{
    [Fact]
    public void SerializeDictionaryWithStringKeys()
    {
        TestData3WithStringKeys data = new();
        var output1 = SerializerHelper.SerializeType(writer =>
        {
            DictionaryWithComplexValuesHost host = new();
            host.Write(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }

    [Fact]
    public void SerializeDictionaryWithIntegralKeys()
    {
        TestData3WithIntegralKeys data = new();
        var output1 = SerializerHelper.SerializeType(writer =>
        {
            DictionaryWithComplexValuesHost host = new();
            host.Write(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }

    [Fact]
    public void SerializeDictionaryWithArrayValues()
    {
        TestData3WithArrayValues data = new();
        var output1 = SerializerHelper.SerializeType(writer =>
        {
            DictionaryWithComplexValuesHost host = new();
            host.Write(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }

    [Fact]
    public void SerializeDictionaryWithDictionaryValues()
    {
        TestData3WithDictionaryValues data = new();
        var output1 = SerializerHelper.SerializeType(writer =>
        {
            DictionaryWithComplexValuesHost host = new();
            host.Write(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }
}

[GeneratedSerializer]
[RootType(typeof(TestData3WithStringKeys))]
[RootType(typeof(TestData3WithIntegralKeys))]
[RootType(typeof(TestData3WithArrayValues))]
[RootType(typeof(TestData3WithDictionaryValues))]
partial class DictionaryWithComplexValuesHost
{
}

class TestData3WithStringKeys
{
    public Dictionary<string, TestData2WithStringKeys>? Data { get; set; }
}

class TestData3WithIntegralKeys
{
    public Dictionary<int, TestData2WithStringKeys>? Data { get; set; }
}

class TestData3WithArrayValues
{
    public Dictionary<string, List<TestData2WithStringKeys>>? Data { get; set; }
}

class TestData3WithDictionaryValues
{
    public Dictionary<string, Dictionary<string, TestData2WithStringKeys>>? Data { get; set; }
}
