﻿using System.Text.Json;

MemoryStream ms = new();
Utf8JsonWriter writer = new(ms, new() { Indented = true });

ToShutupTheCompiler.TestData data = new()
{
    Prop1 = 5,
    Prop4 = 0.9,
    Nums = new[] { 5, 10, 15, 18, -33 },
    KVPs = {
        ["x"] = 1,
        ["y"] = 0,
    },
    Child = new()
    {
        Names = ["a", "b", "", "\t"]
    }
};

SerializerHost host = new();
host.Write(writer, data);
writer.Flush();
ms.Position = 0;

StreamReader sr = new(ms);
Console.WriteLine(sr.ReadToEnd());

namespace ToShutupTheCompiler
{
    class TestData
    {
        public int Prop1 { get; set; }
        public string? Prop2 { get; set; }
        decimal Prop3 { get; set; }
        public double Prop4 { private get; set; }
        public int[]? Nums { get; set; }
        public int[]? Nums2 { get; set; }
        public AnotherType? Child { get; set; }
        public Dictionary<string, int> KVPs { get; set; } = new();
    }

    class AnotherType
    {
        public string[]? Names { get; set; }
    }
}