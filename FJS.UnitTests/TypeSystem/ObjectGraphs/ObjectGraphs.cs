namespace FJS.UnitTests.TypeSystems.ObjectGraphs;

public class ObjectGraphTests
{
    [Fact]
    public void SerializeSelfReferenceTest()
    {
        SelfReference data = new();
        var output1 = SerializerHelper.SerializeType(writer =>
        {
            SelfReferenceAndCycleHost host = new();
            host.Write(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }

    [Fact]
    public void SerializeCycle1Test()
    {
        Cycle1 data = new();
        var output1 = SerializerHelper.SerializeType(writer =>
        {
            SelfReferenceAndCycleHost host = new();
            host.Write(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }

    [Fact]
    public void SerializeCycle2SameHostTest()
    {
        Cycle2 data = new();
        var output1 = SerializerHelper.SerializeType(writer =>
        {
            TransitiveClosureHost host = new();
            host.Write(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }

    [Fact]
    public void SerializeCycle2DifferentHostTest()
    {
        Cycle2 data = new();
        var output1 = SerializerHelper.SerializeType(writer =>
        {
            SelfReferenceAndCycleHost host = new();
            host.Write(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }
}

class SelfReference
{
    public SelfReference? Parent { get; set; }
    public List<SelfReference>? Children { get; set; }
}

class Cycle1
{
    public Cycle2? Other { get; set; }
}

class Cycle2
{
    public Cycle1? Other { get; set; }
}

[GeneratedSerializer]
[RootType(typeof(SelfReference))]
[RootType(typeof(Cycle1))]
partial class SelfReferenceAndCycleHost
{
}

[GeneratedSerializer]
[RootType(typeof(Cycle2))]
partial class TransitiveClosureHost
{
}

[GeneratedSerializer]
[RootType(typeof(Cycle2))]
[RootType(typeof(Cycle2))]
partial class SameRootMultipleTimesHost
{
}
