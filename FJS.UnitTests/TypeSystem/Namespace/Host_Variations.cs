namespace FJS.UnitTests.TypeSystem.Namespace
{
    public class HostVariationTests
    {
        [Fact]
        public void SerializeUsingGlobalHost()
        {
            TestData data = new();
            var output1 = SerializerHelper.SerializeType(writer =>
            {
                GlobalHost1 host = new();
                host.Write(writer, data);
            });
            var output2 = SerializerHelper.SerializeUsingSTJ(data);
            Assert.Equal(output1, output2);
        }

        [Fact]
        public void SerializeUsingNestedHost()
        {
            TestData data = new();
            var output1 = SerializerHelper.SerializeType(writer =>
            {
                Nested.NestedHost1 host = new();
                host.Write(writer, data);
            });
            var output2 = SerializerHelper.SerializeUsingSTJ(data);
            Assert.Equal(output1, output2);
        }
    }

    [GeneratedSerialier]
    [RootType(typeof(TestData))]
    partial class GlobalHost1
    {
    }

    namespace Nested
    {
        [GeneratedSerialier]
        [RootType(typeof(TestData))]
        public partial class NestedHost1
        {
        }
    }

    class TestData
    {
        public int Prop1 { get; set; }
    }
}