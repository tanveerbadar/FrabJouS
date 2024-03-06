namespace FJS.UnitTests.Collections.List;

public class ComplexPropertiesTests
{
    [Fact]
    public void Serialize()
    {
        TestData3 data = new();
        var output1 = SerializerHelper.SerializeType(writer =>
        {
            ListWithComplexValuesHost host = new();
            host.Write(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }
}

[GeneratedSerializer]
[RootType(typeof(TestData3))]
partial class ListWithComplexValuesHost
{
}

class TestData3
{
    public List<TestData2>? Data { get; set; }
}