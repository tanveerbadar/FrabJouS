using FJS.Common.Metadata;
using System.Text;

namespace FJS.UnitTests.Primitives;

public class ScalarFieldsTests
{
    [Fact]
    public void Serialize()
    {
        TestDataWithFields data = new();
        var output1 = SerializerHelper.SerializeType(writer =>
        {
            ScalarFieldsHost host = new();
            host.Write(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }
}

[GeneratedSerializer]
[RootType(typeof(TestDataWithFields))]
partial class ScalarFieldsHost
{
}

class TestDataWithFields
{
    public byte ByteProp = default;

    public sbyte SByteProp = default;

    public short ShortProp = default;

    public ushort UShortProp = default;

    public int IntProp = default;

    public uint UIntProp = default;

    public long LongProp = default;

    public ulong ULongProp = default;

    public float FloatProp = default;

    public double DoubleProp = default;

    public string? StringProp = default;

    public Version? VersionProp = default;

    public Guid GuidProp = default;

    public TimeSpan TimeSpanProp = default;

    public DateTime DateTimeProp = default;

    public char CharProp = default;

    public bool BoolProp = default;
    
    public Rune RuneProp = default;
}