using MongoDB.Bson;
using MongoDB.Driver;
using Sanctuary.Census.Common.Util;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.Models;
using System;

namespace Sanctuary.Census.Database;

/// <summary>
/// Builds a MongoDB aggregate group stage from a query tree string.
/// </summary>
public class GroupBuilder
{
    private static readonly char[] FieldKey = "field".ToCharArray();
    private static readonly char[] ListKey = "list".ToCharArray();
    private static readonly char[] PrefixKey = "prefix".ToCharArray();
    private static readonly char[] StartKey = "start".ToCharArray();

    private readonly string _onField;
    private readonly bool _isList;
    private readonly string? _prefix;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupBuilder"/> class.
    /// </summary>
    /// <param name="onField">The field to group on.</param>
    /// <param name="isList">Whether the group field should contain a list of values.</param>
    /// <param name="prefix">A value to append the names of the group fields.</param>
    public GroupBuilder(string onField, bool isList, string? prefix)
    {
        _onField = onField;
        _isList = isList;
        _prefix = prefix;
    }

    /// <summary>
    /// Parses the given value of a c:tree command.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The parsed <see cref="GroupBuilder"/>.</returns>
    /// <exception cref="QueryException"></exception>
    public static GroupBuilder ParseFromTreeCommand(ReadOnlySpan<char> value)
    {
        string? field = null;
        bool isList = false;
        string? prefix = null;
        string? start = null;

        SpanReader<char> reader = new(value);
        while (!reader.End)
        {
            if (!reader.TryReadTo(out ReadOnlySpan<char> key, ':'))
                break;

            bool validValue = (reader.TryReadTo(out ReadOnlySpan<char> keyValue, '^')
                                || reader.TryReadExact(out keyValue, reader.Remaining))
                                && keyValue.Length > 0;
            if (!validValue)
                throw new QueryException(QueryErrorCode.Malformed, $"A value must be provided for the c:tree key '{key}'");

            if (key.SequenceEqual(FieldKey))
                field = keyValue.ToString();

            if (key.SequenceEqual(ListKey))
                isList = keyValue[0] == '1';

            if (key.SequenceEqual(PrefixKey))
                prefix = keyValue.ToString();

            if (key.SequenceEqual(StartKey))
                start = keyValue.ToString();
        }

        if (field is null)
            throw new QueryException(QueryErrorCode.MissingRequiredKey, "The 'field' key is required on a tree command");

        // TODO: Handle start
        return new GroupBuilder(field, isList, prefix);
    }

    /// <summary>
    /// Builds the <see cref="GroupBuilder"/> and appends it to an aggregate pipeline.
    /// </summary>
    /// <param name="aggregatePipeline">The aggregate pipeline to append the group stage to.</param>
    public void BuildAndAppendTo(ref IAggregateFluent<BsonDocument> aggregatePipeline)
    {
        BsonDocument group = new BsonDocument("_id", $"${_onField}")
            .Add
            (
                "group_key",
                new BsonDocument
                (
                    _isList ? "$push" : "$first",
                    "$$ROOT"
                )
            );

        ProjectionDefinition<BsonDocument> removeTreeKey =
            Builders<BsonDocument>.Projection.Exclude("group_key." + _onField);

        BsonDocument groupKeyNameConversion = new
        (
            "$convert",
            new BsonDocument("input", "$_id")
                .Add("to", "string")
                .Add("onNull", "null")
        );

        if (_prefix is not null)
        {
            groupKeyNameConversion = new BsonDocument
            (
                "$concat",
                new BsonArray(new BsonValue[] {
                    _prefix,
                    groupKeyNameConversion
                })
            );
        }

        BsonDocument rootReplacement = new
        (
            "$arrayToObject",
            new BsonArray(new BsonValue[] {
                new BsonArray(new BsonValue[] {
                    new BsonDocument("k",groupKeyNameConversion)
                        .Add("v", "$group_key")
                })
            })
        );

        aggregatePipeline = aggregatePipeline.Group(group)
            .Project(removeTreeKey)
            .ReplaceRoot(new BsonValueAggregateExpressionDefinition<BsonDocument, BsonDocument>(rootReplacement));
    }
}
