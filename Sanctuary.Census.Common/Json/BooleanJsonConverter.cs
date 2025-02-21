using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sanctuary.Census.Common.Json;

/// <inheritdoc />
public class BooleanJsonConverter : JsonConverter<bool>
{
    private readonly bool _stringifyAll;
    private readonly bool _writeInTextForm;

    /// <summary>
    /// Initializes a new instance of the <see cref="BooleanJsonConverter"/> class.
    /// </summary>
    /// <param name="stringifyAll"><c>True</c> to stringify all values.</param>
    /// <param name="writeInTextForm">
    /// Whether, when <paramref name="stringifyAll"/> is <c>true</c>, tokens should be serialized as text
    /// rather than 1/0.
    /// </param>
    public BooleanJsonConverter(bool stringifyAll, bool writeInTextForm = false)
    {
        _stringifyAll = stringifyAll;
        _writeInTextForm = writeInTextForm;
    }

    /// <inheritdoc />
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is not JsonTokenType.String)
            return reader.GetBoolean();

        return reader.GetString() switch
        {
            "1" => true,
            "0" => false,
            _ => throw new JsonException()
        };

    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        if (_stringifyAll)
        {
            if (_writeInTextForm)
                writer.WriteStringValue(value ? "true" : "false");
            else
                writer.WriteStringValue(value ? "1" : "0");
        }
        else
        {
            writer.WriteBooleanValue(value);
        }
    }
}
