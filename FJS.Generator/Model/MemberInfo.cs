using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FJS.Generator.Model
{
    [JsonPolymorphic]
    [JsonDerivedType(typeof(CollectionInfo))]
    [JsonDerivedType(typeof(ComplexObjectInfo))]
    [JsonDerivedType(typeof(PrimitiveInfo))]
    public abstract class MemberInfo
    {
        public required string Name { get; init; }

        public required bool CanRead { get; init; }

        public required bool CanWrite { get; init; }

        public required MemberType MemberType { get; init; }

        public void EmitWriteStatements(List<StatementSyntax> stmts)
        {
            if (CanRead)
            {
                EmitWriteStatementsCore(stmts);
            }
        }

        protected abstract void EmitWriteStatementsCore(List<StatementSyntax> stmts);
    }
}

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
    public sealed class CompilerFeatureRequiredAttribute : Attribute
    {
        public CompilerFeatureRequiredAttribute(string featureName)
        {
            FeatureName = featureName;
        }

        /// <summary>
        /// The name of the compiler feature.
        /// </summary>
        public string FeatureName { get; }

        /// <summary>
        /// If true, the compiler can choose to allow access to the location where this attribute is applied if it does not understand <see cref="FeatureName"/>.
        /// </summary>
        public bool IsOptional { get; init; }

        /// <summary>
        /// The <see cref="FeatureName"/> used for the ref structs C# feature.
        /// </summary>
        public const string RefStructs = nameof(RefStructs);

        /// <summary>
        /// The <see cref="FeatureName"/> used for the required members C# feature.
        /// </summary>
        public const string RequiredMembers = nameof(RequiredMembers);
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    internal sealed class RequiredMemberAttribute : Attribute
    {

    }
}