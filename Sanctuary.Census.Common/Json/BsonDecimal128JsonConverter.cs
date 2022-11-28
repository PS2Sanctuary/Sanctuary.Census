using MongoDB.Bson;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sanctuary.Census.Common.Json;

/// <inheritdoc />
public class BsonDecimal128JsonConverter : JsonConverter<Decimal128>
{
    /// <inheritdoc />
    public override Decimal128 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => new(reader.GetDecimal());

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, Decimal128 value, JsonSerializerOptions options)
        => writer.WriteNumberValue(Convert.ToDecimal(value));
}
