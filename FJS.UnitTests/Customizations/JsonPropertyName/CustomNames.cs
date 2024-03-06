using System.Text.Json.Serialization;

namespace FJS.Customizations.JsonPropertyName;

public class CustomNameTests
{
    [Fact]
    public void NoCollisionTest()
    {
        NoCollision data = new();
        var output1 = SerializerHelper.SerializeType(writer =>
        {
            CustomPropertyNamesHost host = new();
            host.Write(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }

    [Fact]
    public void CollisionWithDefaultNameTest()
    {
        CollisionWithDefaultName data = new();
        var output1 = SerializerHelper.SerializeType(writer =>
        {
            CustomPropertyNamesHost host = new();
            host.Write(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }

    [Fact]
    public void CollisionThroughAttributeTest()
    {
        CollisionThroughAttribute data = new();
        var output1 = SerializerHelper.SerializeType(writer =>
        {
            CustomPropertyNamesHost host = new();
            host.Write(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }
}

[GeneratedSerialier]
[RootType(typeof(NoCollision))]
[RootType(typeof(CollisionWithDefaultName))]
[RootType(typeof(CollisionThroughAttribute))]
partial class CustomPropertyNamesHost
{
}

class NoCollision
{
    public int Prop1 { get; set; }

    [JsonPropertyName("abc")]
    public int Prop2 { get; set; }
}

class CollisionWithDefaultName
{
    public int Prop1 { get; set; }

    [JsonPropertyName("Prop1")]
    public int Prop2 { get; set; }
}

class CollisionThroughAttribute
{
    [JsonPropertyName("abc")]
    public int Prop1 { get; set; }

    [JsonPropertyName("abc")]
    public int Prop2 { get; set; }
}