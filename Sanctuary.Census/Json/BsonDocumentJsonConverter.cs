using MongoDB.Bson;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sanctuary.Census.Json;

/// <inheritdoc />
public class BsonDocumentJsonConverter : JsonConverter<BsonDocument>
{
    /// <inheritdoc />
    public override BsonDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotImplementedException();

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, BsonDocument value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (BsonElement element in value.Elements)
        {
            if (element.Value.IsBsonNull && options.DefaultIgnoreCondition is JsonIgnoreCondition.WhenWritingNull)
                continue;

            writer.WritePropertyName(element.Name);
            WriteBsonValue(writer, element.Value, options);
        }

        writer.WriteEndObject();
    }

    private void WriteBsonValue(Utf8JsonWriter writer, BsonValue value, JsonSerializerOptions options)
    {
        switch (value.BsonType)
        {
            case BsonType.Array:
            {
                writer.WriteStartArray();
                foreach (BsonValue arrayElement in value.AsBsonArray.Values)
                    WriteBsonValue(writer, arrayElement, options);
                writer.WriteEndArray();

                break;
            }
            case BsonType.Boolean:
            {
                writer.WriteBooleanValue(value.AsBoolean);
                break;
            }
            case BsonType.Document:
            {
                Write(writer, value.AsBsonDocument, options);
                break;
            }
            case BsonType.Double:
            {
                writer.WriteNumberValue(Math.Round((decimal)value.AsDouble, 3));
                break;
            }
            case BsonType.Int32:
            {
                writer.WriteNumberValue(value.AsInt32);
                break;
            }
            case BsonType.Int64:
            {
                writer.WriteNumberValue(value.AsInt64);
                break;
            }
            case BsonType.Null:
            {
                writer.WriteNullValue();
                break;
            }
            case BsonType.String:
            {
                writer.WriteStringValue(value.AsString);
                break;
            }
            case BsonType.Timestamp:
            {
                JsonSerializer.Serialize
                (
                    writer,
                    new TimeSpan(value.AsBsonTimestamp.Value),
                    options
                );
                break;
            }
            case BsonType.DateTime:
            {
                JsonSerializer.Serialize(writer, value.ToUniversalTime(), options);
                break;
            }
            default:
            {
                throw new JsonException($"The BSON type {value.BsonType} is not supported");
            }
        }
    }
}
