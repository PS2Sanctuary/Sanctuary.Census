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

        return $@"#nullable enable

using Sanctuary.Census.ClientData.Abstractions.ClientDataModels;
using Sanctuary.Census.Common.Util;
using System;
using System.Buffers.Text;
using System.Collections.Generic;

namespace {@class.Namespace};

public partial record {@class.Name} : IDatasheet<{@class.Name}>
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

            sb.Append
            (
                $@"{param.Type} {paramName};
        "
            );

            if (param.Type.NullableAnnotation is NullableAnnotation.Annotated)
            {
                sb.Append
                (
                    $@"if ({paramName}Span.Length != 0)
                    {{
            "
                );
            }

            GenerateFieldDeserializationString(sb, param.Type, paramName, paramName + "Span", param, c, reportDiagnostic);

            if (param.Type.NullableAnnotation is NullableAnnotation.Annotated)
            {
                sb.Append
                (
                    $@"}}
        else
        {{
            {paramName} = null;
        }}
            "
                );
            }
        }

        sb.Append
        (
            $@"reader.IsNext({(byte)'\r'}, true); // '\r'
        reader.IsNext({(byte)'\n'}, true); // '\n'

        return new {c.Name}({ctorSb});"
        );
        return sb.ToString();
    }

    private static void GenerateFieldDeserializationString
    (
        StringBuilder sb,
        ITypeSymbol paramType,
        string paramName,
        string paramValueSpanName,
        IParameterSymbol originalParam,
        ClassToAugment c,
        Action<Diagnostic> reportDiagnostic
    )
    {
        switch (paramType.SpecialType)
        {
            case SpecialType.System_Boolean:
            {
                sb.Append
                (
                    $@"{paramName} = {paramValueSpanName}[0] switch
            {{
                {(byte)'0'} => false, // '0'
                {(byte)'1'} => true, // '1'
                _ => Utf8Parser.TryParse({paramValueSpanName}, out bool {paramName}Value, out _) ? {paramName}Value : throw new Exception(""Failed to parse '{originalParam}' parameter"")
            }};

        "
                );
                break;
            }
            case SpecialType.System_Byte:
            {
                sb.Append
                (
                    $@"{paramName} = Utf8Parser.TryParse({paramValueSpanName}, out byte {paramName}Value, out _)
            ? {paramName}Value
            : throw new Exception(""Failed to parse a '{paramName}' field as a {paramType}"");

        "
                );
                break;
            }
            case SpecialType.System_SByte:
            {
                sb.Append
                (
                    $@"{paramName} = Utf8Parser.TryParse({paramValueSpanName}, out sbyte {paramName}Value, out _)
            ? {paramName}Value
            : throw new Exception(""Failed to parse a '{paramName}' field as a {paramType}"");

        "
                );
                break;
            }
            case SpecialType.System_UInt16:
            {
                sb.Append
                (
                    $@"{paramName} = Utf8Parser.TryParse({paramValueSpanName}, out ushort {paramName}Value, out _)
            ? {paramName}Value
            : throw new Exception(""Failed to parse a '{paramName}' field as a {paramType}"");

        "
                );
                break;
            }
            case SpecialType.System_Int16:
            {
                sb.Append
                (
                    $@"{paramName} = Utf8Parser.TryParse({paramValueSpanName}, out short {paramName}Value, out _)
            ? {paramName}Value
            : throw new Exception(""Failed to parse a '{paramName}' field as a {paramType}"");

        "
                );
                break;
            }
            case SpecialType.System_UInt32:
            {
                sb.Append
                (
                    $@"{paramName} = Utf8Parser.TryParse({paramValueSpanName}, out uint {paramName}Value, out _)
            ? {paramName}Value
            : throw new Exception(""Failed to parse a '{paramName}' field as a {paramType}"");

        "
                );
                break;
            }
            case SpecialType.System_Int32:
            {
                sb.Append
                (
                    $@"{paramName} = Utf8Parser.TryParse({paramValueSpanName}, out int {paramName}Value, out _)
            ? {paramName}Value
            : throw new Exception(""Failed to parse a '{paramName}' field as a {paramType}"");

        "
                );
                break;
            }
            case SpecialType.System_UInt64:
            {
                sb.Append
                (
                    $@"{paramName} = Utf8Parser.TryParse({paramValueSpanName}, out ulong {paramName}Value, out _)
            ? {paramName}Value
            : throw new Exception(""Failed to parse a '{paramName}' field as a {paramType}"");

        "
                );
                break;
            }
            case SpecialType.System_Int64:
            {
                sb.Append
                (
                    $@"{paramName} = Utf8Parser.TryParse({paramValueSpanName}, out long {paramName}Value, out _)
            ? {paramName}Value
            : throw new Exception(""Failed to parse a '{paramName}' field as a {paramType}"");

        "
                );
                break;
            }
            case SpecialType.System_Single:
            {
                sb.Append
                (
                    $@"{paramName} = Utf8Parser.TryParse({paramValueSpanName}, out float {paramName}Value, out _)
            ? {paramName}Value
            : throw new Exception(""Failed to parse a '{paramName}' field as a {paramType}"");

        "
                );
                break;
            }
            case SpecialType.System_Double:
            {
                sb.Append
                (
                    $@"{paramName} = Utf8Parser.TryParse({paramValueSpanName}, out double {paramName}Value, out _)
            ? {paramName}Value
            : throw new Exception(""Failed to parse a '{paramName}' field as a {paramType}"");

        "
                );
                break;
            }
            case SpecialType.System_String:
            {
                sb.Append
                (
                    $@"{paramName} = System.Text.Encoding.UTF8.GetString({paramValueSpanName});

        "
                );
                break;
            }
            case SpecialType.None when paramType is INamedTypeSymbol { EnumUnderlyingType: {} } enumType:
            {
                string enumField = paramName + "EnumValue";
                sb.Append
                (
                    $@"{enumType.EnumUnderlyingType} {enumField};
        "
                );
                GenerateFieldDeserializationString
                (
                    sb,
                    enumType.EnumUnderlyingType,
                    enumField,
                    paramValueSpanName,
                    originalParam,
                    c,
                    reportDiagnostic
                );
                sb.Append
                (
                    $@"{paramName} = ({paramType}){enumField};

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
                        $"{c.Name}.{originalParam}: {originalParam.Type} types are not yet supported for datasheet deserialization"
                    ),
                    c.Locations[0]
                );
                reportDiagnostic(d);
                sb.Append
                (
                    $@"throw new Exception(""{originalParam.Type} types are not yet supported (property: {originalParam})"");

    "
                );
                break;
            }
        }
    }
}
