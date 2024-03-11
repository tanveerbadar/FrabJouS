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
        Converters =
        {
            new JsonStringEnumConverter(),
        },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        ReferenceHandler = ReferenceHandler.Preserve,
        WriteIndented = true,
    };

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var hosts = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "FJS.Common.Metadata.GeneratedSerializerAttribute",
                static (node, token) => node is ClassDeclarationSyntax,
                static (ctx, ct) => ModelBuilder.GatherSerializableTypes(ctx.TargetNode, ctx.SemanticModel));

        context.RegisterSourceOutput(hosts, static (ctx, source) =>
        {
            ctx.AddSource($"{source.Name}.g", Emitter.GetGeneratedSource(source));
        });

        context.RegisterImplementationSourceOutput(hosts, static (ctx, source) =>
        {
            ctx.AddSource($"{source.Name}.g.json", $"/*{JsonSerializer.Serialize(source, options)}*/");
        });
    }
}
