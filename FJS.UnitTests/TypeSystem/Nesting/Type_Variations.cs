namespace FJS.UnitTests.TypeSystem.Nesting;

public class TypeVariationTests
{
    [Fact]
    public void SerializeNestedTestData2Test()
    {
        Parent.TestData data = new();
        var output1 = SerializerHelper.SerializeType(writer =>
        {
            NestedHost3 host = new();
            host.Write(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }
}

[GeneratedSerialier]
[RootType(typeof(Parent.TestData))]
partial class NestedHost3
{
}

public class Parent
{
    public class TestData
    {
        public int Prop1 { get; set; }
    }
}