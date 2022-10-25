﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sanctuary.Census.Api.Json;

/// <inheritdoc />
public class BooleanJsonConverter : JsonConverter<bool>
{
    private readonly bool _stringifyAll;

    /// <summary>
    /// Initializes a new instance of the <see cref="BooleanJsonConverter"/> class.
    /// </summary>
    /// <param name="stringifyAll"><c>True</c> to stringify all values.</param>
    public BooleanJsonConverter(bool stringifyAll)
    {
        _stringifyAll = stringifyAll;
    }

    /// <inheritdoc />
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotImplementedException();

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        if (_stringifyAll)
            writer.WriteStringValue(value ? "1" : "0");
        else
            writer.WriteBooleanValue(value);
    }
}
