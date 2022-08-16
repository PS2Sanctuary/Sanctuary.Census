using Sanctuary.Census.Api.Models;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sanctuary.Census.Api.Json;

/// <summary>
/// Defines a JSON converter for the <see cref="DataResponse{TDataType}"/> type.
/// </summary>
public class DataResponseJsonConverter : JsonConverterFactory
{
    /// <inheritdoc />
    public override bool CanConvert(Type typeToConvert)
    {
        if (!typeToConvert.IsGenericType)
            return false;

        return typeToConvert.GetGenericTypeDefinition() == typeof(DataResponse<>);
    }

    /// <inheritdoc />
    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        Type valueType = typeToConvert.GenericTypeArguments[0];

        return (JsonConverter)Activator.CreateInstance
        (
            typeof(DataResponseJsonConverterInner<>).MakeGenericType(valueType),
            options
        )!;
    }

    private class DataResponseJsonConverterInner<TDataType> : JsonConverter<DataResponse<TDataType>>
    {
        private readonly JsonEncodedText _returnedPropName;
        private readonly JsonEncodedText _timingName;

        public DataResponseJsonConverterInner(JsonSerializerOptions options)
        {
            string returnedPropName = nameof(DataResponse<TDataType>.Returned);
            string timingName = nameof(DataResponse<TDataType>.Timing);

            if (options.PropertyNamingPolicy is not null)
            {
                returnedPropName = options.PropertyNamingPolicy.ConvertName(returnedPropName);
                timingName = options.PropertyNamingPolicy.ConvertName(timingName);
            }

            _returnedPropName = JsonEncodedText.Encode(returnedPropName);
            _timingName = JsonEncodedText.Encode(timingName);
        }

        /// <inheritdoc />
        public override DataResponse<TDataType> Read
        (
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options
        ) => throw new NotImplementedException();

        /// <inheritdoc />
        public override void Write
        (
            Utf8JsonWriter writer,
            DataResponse<TDataType> value,
            JsonSerializerOptions options
        )
        {
            string name = value.DataTypeName + "_list";

            writer.WriteStartObject();

            if (value.Timing is not null)
            {
                writer.WritePropertyName(_timingName);
                JsonSerializer.Serialize(writer, value.Timing, options);
            }

            writer.WritePropertyName(name);
            writer.WriteStartArray();
            foreach (TDataType data in value.Data)
                JsonSerializer.Serialize(writer, data, options);
            writer.WriteEndArray();

            writer.WriteNumber(_returnedPropName, value.Returned);
            writer.WriteEndObject();
        }
    }
}
