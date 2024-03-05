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
            host.WriteTestDataWithFields(writer, data);
        });
        var output2 = SerializerHelper.SerializeUsingSTJ(data);
        Assert.Equal(output1, output2);
    }
}

[GeneratedSerialier]
[RootType(typeof(TestDataWithFields))]
partial class ScalarFieldsHost
{
}

class TestDataWithFields
{
    public byte ByteProp;

    public sbyte SByteProp;

    public short ShortProp;

    public ushort UShortProp;

    public int IntProp;

    public uint UIntProp;

    public long LongProp;

    public ulong ULongProp;

    public float FloatProp;

    public double DoubleProp;

    public string? StringProp;

    public Version? VersionProp;

    public Guid GuidProp;

    public TimeSpan TimeSpanProp;

    public DateTime DateTimeProp;

    public char CharProp;

    public Rune RuneProp;
}