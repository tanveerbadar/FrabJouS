using System.Text.Json;
using System.Text.Json.Serialization;
using FJS.Generator.Emit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FJS.Generator;

[Generator]
public class JsonCodeGenerator : IIncrementalGenerator
{
    static JsonSerializerOptions options = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    };

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var hosts = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "FJS.Common.Metadata.GeneratedSerializerAttribute",
                static (node, token) => node is ClassDeclarationSyntax,
                static (ctx, ct) => ModelBuilder.GatherSerializableTypes(ctx.TargetNode, ctx.SemanticModel));

        context.RegisterSourceOutput(hosts, static (ctx, host) =>
        {
            ctx.AddSource($"{host.Name}.g", Emitter.GetGeneratedSource(host));
        });

        context.RegisterImplementationSourceOutput(hosts, static (ctx, host) =>
        {
    //        ctx.AddSource($"{host.Name}.g.json", $"/*{JsonSerializer.Serialize(host, options)}*/");
        });
    }
}
