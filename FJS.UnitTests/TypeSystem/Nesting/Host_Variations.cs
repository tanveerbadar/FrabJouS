namespace FJS.UnitTests.TypeSystem.Nesting;

public class HostVariationTests
{
        [Fact]
        public void Serialize()
        {
            TestData data = new();
            var output1 = SerializerHelper.SerializeType(writer =>
            {
                HostParent.NestedHost2 host = new();
                host.Write(writer, data);
            });
            var output2 = SerializerHelper.SerializeUsingSTJ(data);
            Assert.Equal(output1, output2);
        }
}

public partial class HostParent
{
    [GeneratedSerialier]
    [RootType(typeof(TestData))]
    public partial class NestedHost2
    {
    }
}

class TestData
{
    public int Prop1 { get; set; }
}