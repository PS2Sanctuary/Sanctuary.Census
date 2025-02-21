using OneOf;
using Sanctuary.Census.RealtimeHub.Objects;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sanctuary.Census.RealtimeHub.Json;

/// <summary>
/// A JSON converter for the subscribe command world list.
/// </summary>
public sealed class SubscribeWorldListJsonConverter : JsonConverter<OneOf<All, IEnumerable<uint>>?>
{
    private readonly string _allString = new All().ToString();

    /// <inheritdoc />
    public override OneOf<All, IEnumerable<uint>>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null)
            return null;

        if (reader.TokenType is not JsonTokenType.StartArray)
            throw new JsonException("Expected a StartArray token.");

        reader.Read();

        if (reader.TokenType is JsonTokenType.EndArray)
            return null;

        if (reader.TokenType is JsonTokenType.String && reader.GetString()?.Equals(_allString, StringComparison.Ordinal) is true)
        {
            reader.Read(); // Read past the end of the array
            return new All();
        }

        List<uint> list = new();
        while (reader.TokenType is not JsonTokenType.EndArray)
        {
            string? value = reader.GetString();
            if (value is not null)
                list.Add(uint.Parse(value));

            reader.Read();
        }

        return list;
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, OneOf<All, IEnumerable<uint>>? value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        if (value is null)
        {
            writer.WriteEndArray();
            return;
        }

        if (value.Value.IsT0)
        {
            writer.WriteStringValue(_allString);
        }
        else
        {
            foreach (int item in value.Value.AsT1)
                writer.WriteStringValue(item.ToString());
        }

        writer.WriteEndArray();
    }
}
