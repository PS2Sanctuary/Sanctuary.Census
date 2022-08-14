using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Sanctuary.Census.SourceGeneration.Objects;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sanctuary.Census.SourceGeneration.Generators;

[Generator]
public class DatasheetGenerator : IIncrementalGenerator
{
    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<RecordDeclarationSyntax> classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider
            (
                static (s, _) => IsSyntaxTargetForGeneration(s), // Selects classes with attributes
                static (ctx, _) => GetSemanticTargetForGeneration(ctx) // Selects classes with the chosen attribute
            )
            .Where(static m => m is not null)!; // Filter out attributed classes that we don't care about

        // Combine the selected classes with the `Compilation`
        IncrementalValueProvider<(Compilation, ImmutableArray<RecordDeclarationSyntax>)> compilationAndClasses
            = context.CompilationProvider.Combine(classDeclarations.Collect());

        // Generate the source using the compilation and classes
        context.RegisterSourceOutput
        (
            compilationAndClasses,
            (spc, source) => Execute(source.Item1, source.Item2, spc)
        );
    }

    /// <summary>
    /// Determines whether a syntax node represents a class with at least one attribute.
    /// </summary>
    /// <param name="node">The node to investigate.</param>
    /// <returns>A value indicating whether the node met the requirements.</returns>
    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        => node is RecordDeclarationSyntax { AttributeLists.Count: > 0 };

    /// <summary>
    /// Determines whether the current node represents a class with the given marker attribute.
    /// </summary>
    /// <param name="context">The generator context.</param>
    /// <returns>The class syntax, or null if the node did not meet the requirements.</returns>
    private static RecordDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        // We know the node is a RecordDeclarationSyntax thanks to IsSyntaxTargetForGeneration
        RecordDeclarationSyntax classDeclaration = (RecordDeclarationSyntax)context.Node;

        // Now we check each attribute, looking for the given attribute
        foreach (AttributeListSyntax attributeListSyntax in classDeclaration.AttributeLists)
        {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol)
                    continue;

                INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                string fullName = attributeContainingTypeSymbol.ToDisplayString();

                if (fullName is Constants.DatasheetAttributeTypeName)
                    return classDeclaration;
            }
        }

        return null;
    }

    private void Execute
    (
        Compilation compilation,
        ImmutableArray<RecordDeclarationSyntax> classes,
        SourceProductionContext context
    )
    {
        if (classes.IsDefaultOrEmpty)
            return;

        IEnumerable<RecordDeclarationSyntax> distinctClasses = classes.Distinct();

        List<ClassToAugment> classesToGenerate = GetTypesToGenerate
        (
            compilation,
            distinctClasses,
            context.CancellationToken
        );

        foreach (ClassToAugment @class in classesToGenerate)
        {
            string result = Generate(@class, context.ReportDiagnostic);
            context.AddSource($"Datasheet_{@class.Name}.g.cs", SourceText.From(result, Encoding.UTF8));
        }
    }

    private static List<ClassToAugment> GetTypesToGenerate
    (
        Compilation compilation,
        IEnumerable<RecordDeclarationSyntax> classes,
        CancellationToken ct
    )
    {
        List<ClassToAugment> classesToGenerate = new();

        // Get the semantic representation of our marker attribute
        INamedTypeSymbol? loginDataPacketAttribute = compilation.GetTypeByMetadataName(Constants.DatasheetAttributeTypeName);

        if (loginDataPacketAttribute is null)
        {
            // If this is null, the compilation couldn't find the marker attribute type
            // which suggests there's something very wrong! Bail out..
            return classesToGenerate;
        }

        foreach (RecordDeclarationSyntax classDeclaration in classes)
        {
            ct.ThrowIfCancellationRequested();

            // Get the semantic representation of the class syntax
            SemanticModel semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            if (semanticModel.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol classSymbol)
                continue;

            classesToGenerate.Add(ClassToAugment.FromNamedTypeSymbol(classSymbol));
        }

        return classesToGenerate;
    }

    private static string Generate(ClassToAugment @class, Action<Diagnostic> reportDiagnostic)
    {
        IMethodSymbol ctor;
        try
        {
            ctor = @class.Constructors.Single
            (
                c => c.Parameters.Length > 0
                     && c.DeclaredAccessibility is Accessibility.Public
            );
        }
        catch
        {
            Diagnostic d = Diagnostic.Create
            (
                DiagnosticDescriptors.SinglePositionalConstructor
                (
                    $"Expected the {@class.Name} type to have a positional constructor, " +
                    "and only one constructor with parameters."
                ),
                @class.Locations[0]
            );
            reportDiagnostic(d);
            return $"// Must have a positional constructor, and only one constructor with parameters";
        }

        StringBuilder ctorParamNames = new();
        foreach (IParameterSymbol paramName in ctor.Parameters.OrderBy(p => p.Ordinal))
        {
            if (ctorParamNames.Length > 0)
                ctorParamNames.Append(", ");

            ctorParamNames.Append("\"")
                .Append(paramName.Name.ToUpperInvariant())
                .Append("\"");
        }

        return $@"using Sanctuary.Census.Common.Util;
using System;
using System.Buffers.Text;
using System.Collections.Generic;

namespace {@class.Namespace};

public partial record {@class.Name}
{{
    /// <inheritdoc />
    public static List<{@class.Name}> Deserialize(ReadOnlySpan<byte> buffer)
    {{
        SpanReader<byte> reader = new(buffer);
        ValidateHeader(ref reader);

        List<{@class.Name}> objects = new();
        while (!reader.End)
            objects.Add(DeserializeSingleObject(ref reader));

        return objects;
    }}

    private static void ValidateHeader(ref SpanReader<byte> reader)
    {{
        string[] ctorParamNames = new string[] {{ {ctorParamNames} }};

        if (!reader.IsNext({(byte)'#'}, true)) // '#'
            throw new Exception($""Header indicator not present"");

        for (int i = 0; i < ctorParamNames.Length; i++)
        {{
            // Skip past the key indicator ('*')
            reader.IsNext({(byte)'*'}, true);

            if (!reader.TryReadTo(out ReadOnlySpan<byte> element, {(byte)'^'})) // '^'
                throw new Exception($""The '{{ctorParamNames[i]}}' property is not present in the datasheet header"");

            string name = System.Text.Encoding.UTF8.GetString(element)
                .Replace(""_"", string.Empty);

            if (!ctorParamNames[i].Equals(name, StringComparison.OrdinalIgnoreCase))
                throw new Exception($""The '{{ctorParamNames[i]}}' param is either wrongly positioned or invalid"");
        }}

        reader.IsNext({(byte)'\r'}, true); // '\r'
        if (!reader.IsNext({(byte)'\n'}, true)) // '\n'
            throw new Exception($""Header contains more fields than constructor"");
    }}

    private static {@class.Name} DeserializeSingleObject(ref SpanReader<byte> reader)
    {{
        {GenerateDeserializeString(ctor, @class, reportDiagnostic)}
    }}
}}
";
    }

    private static string GenerateDeserializeString(IMethodSymbol ctor, ClassToAugment c, Action<Diagnostic> reportDiagnostic)
    {
        StringBuilder sb = new();
        StringBuilder ctorSb = new();

        foreach (IParameterSymbol param in ctor.Parameters.OrderBy(p => p.Ordinal))
        {
            string paramName = param.Name.ToSafeLowerCamel();
            if (ctorSb.Length > 0)
                ctorSb.Append(", ");
            ctorSb.Append(paramName);

            sb.Append
            (
                $@"if (!reader.TryReadTo(out ReadOnlySpan<byte> {paramName}Span, {(byte)'^'})) // '^'
            throw new Exception($""Failed to read value span for '{param}' parameter"");
        "
            );

            switch (param.Type.SpecialType)
            {
                case SpecialType.System_Boolean:
                {
                    sb.Append
                    (
                        $@"bool {paramName} = {paramName}Span[0] switch
            {{
                {(byte)'0'} => false, // '0'
                {(byte)'1'} => true, // '1'
                _ => Utf8Parser.TryParse({paramName}Span, out bool {paramName}Value, out _) ? {paramName}Value : throw new Exception(""Failed to parse '{param}' parameter"")
            }};

        "
                    );
                    break;
                }
                case SpecialType.System_Byte:
                {
                    sb.Append
                    (
                        $@"byte {paramName} = Utf8Parser.TryParse({paramName}Span, out byte {paramName}Value, out _)
            ? {paramName}Value
            : throw new Exception(""Failed to parse '{param}' parameter"");

        "
                    );
                    break;
                }
                case SpecialType.System_SByte:
                {
                    sb.Append
                    (
                        $@"sbyte {paramName} = Utf8Parser.TryParse({paramName}Span, out sbyte {paramName}Value, out _)
            ? {paramName}Value
            : throw new Exception(""Failed to parse '{param}' parameter"");

        "
                    );
                    break;
                }
                case SpecialType.System_UInt16:
                {
                    sb.Append
                    (
                        $@"ushort {paramName} = Utf8Parser.TryParse({paramName}Span, out ushort {paramName}Value, out _)
            ? {paramName}Value
            : throw new Exception(""Failed to parse '{param}' parameter"");

        "
                    );
                    break;
                }
                case SpecialType.System_Int16:
                {
                    sb.Append
                    (
                        $@"short {paramName} = Utf8Parser.TryParse({paramName}Span, out short {paramName}Value, out _)
            ? {paramName}Value
            : throw new Exception(""Failed to parse '{param}' parameter"");

        "
                    );
                    break;
                }
                case SpecialType.System_UInt32:
                {
                    sb.Append
                    (
                        $@"uint {paramName} = Utf8Parser.TryParse({paramName}Span, out uint {paramName}Value, out _)
            ? {paramName}Value
            : throw new Exception(""Failed to parse '{param}' parameter"");

        "
                    );
                    break;
                }
                case SpecialType.System_Int32:
                {
                    sb.Append
                    (
                        $@"int {paramName} = Utf8Parser.TryParse({paramName}Span, out int {paramName}Value, out _)
            ? {paramName}Value
            : throw new Exception(""Failed to parse '{param}' parameter"");

        "
                    );
                    break;
                }
                case SpecialType.System_UInt64:
                {
                    sb.Append
                    (
                        $@"ulong {paramName} = Utf8Parser.TryParse({paramName}Span, out ulong {paramName}Value, out _)
            ? {paramName}Value
            : throw new Exception(""Failed to parse '{param}' parameter"");

        "
                    );
                    break;
                }
                case SpecialType.System_Int64:
                {
                    sb.Append
                    (
                        $@"long {paramName} = Utf8Parser.TryParse({paramName}Span, out long {paramName}Value, out _)
            ? {paramName}Value
            : throw new Exception(""Failed to parse '{param}' parameter"");

        "
                    );
                    break;
                }
                case SpecialType.System_Single:
                {
                    sb.Append
                    (
                        $@"float {paramName} = Utf8Parser.TryParse({paramName}Span, out float {paramName}Value, out _)
            ? {paramName}Value
            : throw new Exception(""Failed to parse '{param}' parameter"");

        "
                    );
                    break;
                }
                case SpecialType.System_Double:
                {
                    sb.Append
                    (
                        $@"double {paramName} = Utf8Parser.TryParse({paramName}Span, out double {paramName}Value, out _)
            ? {paramName}Value
            : throw new Exception(""Failed to parse '{param}' parameter"");

        "
                    );
                    break;
                }
                case SpecialType.System_String:
                {
                    sb.Append
                    (
                        $@"string {paramName} = System.Text.Encoding.UTF8.GetString({paramName}Span);

        "
                    );
                    break;
                }
                default:
                {
                    Diagnostic d = Diagnostic.Create
                    (
                        DiagnosticDescriptors.GetStringGenerationFailure
                        (
                            $"{c.Name}.{param.Name}: {param.Type} types are not yet supported for datasheet deserialization"
                        ),
                        c.Locations[0]
                    );
                    reportDiagnostic(d);
                    sb.Append
                    (
                        $@"throw new Exception(""{param.Type} types are not yet supported (property: {param.Name})"");

        "
                    );
                    break;
                }
            }
        }

        sb.Append("return new ").Append(c.Name).Append("(").Append(ctorSb).Append(");");
        return sb.ToString();
    }
}
