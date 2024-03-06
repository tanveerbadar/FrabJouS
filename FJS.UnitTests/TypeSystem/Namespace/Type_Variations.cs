namespace FJS.UnitTests.TypeSystem.Namespace
{
    public class TypeVariationTests
    {
        [Fact]
        public void SerializeTestDataTest()
        {
            TestData1 data = new();
            var output1 = SerializerHelper.SerializeType(writer =>
            {
                GlobalHost2 host = new();
                host.Write(writer, data);
            });
            var output2 = SerializerHelper.SerializeUsingSTJ(data);
            Assert.Equal(output1, output2);
        }

        [Fact]
        public void SerializeNestedTestData2Test()
        {
            Nested.TestData2 data = new();
            var output1 = SerializerHelper.SerializeType(writer =>
            {
                GlobalHost2 host = new();
                host.Write(writer, data);
            });
            var output2 = SerializerHelper.SerializeUsingSTJ(data);
            Assert.Equal(output1, output2);
        }
    }

    [GeneratedSerializer]
    [RootType(typeof(TestData1))]
    [RootType(typeof(Nested.TestData2))]
    partial class GlobalHost2
    {
    }

    namespace Nested
    {
        class TestData2
        {
            public int Prop1 { get; set; }
        }
    }

    class TestData1
    {
        public int Prop1 { get; set; }
    }
}