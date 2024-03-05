using System.Text.Json;

namespace FJS.UnitTests.Utils;

static class SerializerHelper
{
    public static string SerializeType(Action<Utf8JsonWriter> writeAction)
    {
        MemoryStream ms = new();
        Utf8JsonWriter writer = new(ms);

        writeAction(writer);
        writer.Flush();
        ms.Position = 0;

        StreamReader sr = new(ms);
        var output = sr.ReadToEnd();
        return output;
    }

    public static string SerializeUsingSTJ<T>(T obj)
    {
        MemoryStream ms = new();
        Utf8JsonWriter writer = new(ms);
        JsonSerializer.Serialize(obj);
        writer.Flush();
        ms.Position = 0;

        StreamReader sr = new(ms);
        var output = sr.ReadToEnd();
        return output;
    }
}