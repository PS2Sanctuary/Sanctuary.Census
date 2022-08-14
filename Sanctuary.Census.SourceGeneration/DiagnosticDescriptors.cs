using Microsoft.CodeAnalysis;

namespace Sanctuary.Census.SourceGeneration;

public class DiagnosticDescriptors
{
    public static DiagnosticDescriptor GetStringGenerationFailure(string description)
        => new
        (
            "SSG001",
            "String Generation Failure",
            description,
            "Generation",
            DiagnosticSeverity.Error,
            true
        );

    public static DiagnosticDescriptor SinglePositionalConstructor(string description)
        => new
        (
            "SSG002",
            "Expected Single Positional Constructor",
            description,
            "Generation",
            DiagnosticSeverity.Error,
            true
        );
}
