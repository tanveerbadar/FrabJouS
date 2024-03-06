using FJS.Generator.Emit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FJS.Generator;

[Generator]
public class JsonCodeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var hosts = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "FJS.Common.Metadata.GeneratedSerializerAttribute",
                static (node, token) => node is ClassDeclarationSyntax,
                static (ctx, ct) => ModelBuilder.GatherSerializableTypes(ctx.TargetNode, ctx.SemanticModel));

        context.RegisterSourceOutput(hosts, static (ctx, source) =>
        {
            ctx.AddSource($"{source.Name}.g.cs", Emitter.GetGeneratedSource(source));
        });
    }
}
