using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Sanctuary.Census.Common.Util;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.Models;
using Sanctuary.Census.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;

namespace Sanctuary.Census.Database;

/// <summary>
/// Builds a MongoDB aggregate lookup stage from a query join string.
/// </summary>
public class JoinBuilder
{
    // TODO: Limit maximum joins
    private const char VALUE_DELIMITER = '\'';

    private static readonly char[] TypeKey = "type".ToCharArray();
    private static readonly char[] OnFieldKey = "on".ToCharArray();
    private static readonly char[] ToFieldKey = "to".ToCharArray();
    private static readonly char[] ListKey = "list".ToCharArray();
    private static readonly char[] OuterKey = "outer".ToCharArray();
    private static readonly char[] ShowKey = "show".ToCharArray();
    private static readonly char[] HideKey = "hide".ToCharArray();
    private static readonly char[] InjectAtKey = "inject_at".ToCharArray();
    private static readonly char[] TermsKey = "terms".ToCharArray();

    /// <summary>
    /// The collection that will be joined to.
    /// </summary>
    public string? ToCollection { get; private set; }

    /// <summary>
    /// The field on the local collection to join on.
    /// </summary>
    public string? OnField { get; private set; }

    /// <summary>
    /// The field on the foreign collection to join on.
    /// </summary>
    public string? ToField { get; private set; }

    /// <summary>
    /// Whether the joined results are a list.
    /// </summary>
    public bool IsList { get; private set; }

    /// <summary>
    /// Whether this is an outer join.
    /// </summary>
    public bool IsOuter { get; private set; } // TODO: Support

    /// <summary>
    /// The field to inject the joined results at.
    /// </summary>
    public string? InjectAt { get; private set; }

    /// <summary>
    /// Filters to apply to the foreign collection before joining to it.
    /// </summary>
    public List<string> Terms { get; }

    /// <summary>
    /// The projection to apply to the foreign collection.
    /// </summary>
    public ProjectionBuilder Projection { get; }

    /// <summary>
    /// The children of this join.
    /// </summary>
    public List<JoinBuilder> Children { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JoinBuilder"/> class.
    /// </summary>
    public JoinBuilder()
    {
        Terms = new List<string>();
        IsOuter = true;
        Projection = new ProjectionBuilder();
        Children = new List<JoinBuilder>();
    }

    /// <summary>
    /// Parses a <see cref="JoinBuilder"/> from a query join command.
    /// </summary>
    /// <param name="value">The values of the join command.</param>
    /// <returns>The parsed <see cref="JoinBuilder"/>s.</returns>
    public static List<JoinBuilder> Parse(ReadOnlySpan<char> value)
    {
        List<JoinBuilder> builders = new();

        SpanReader<char> reader = new(value);
        while (reader.TryReadTo(out ReadOnlySpan<char> joinValue, ','))
            builders.Add(ParseIndividualJoin(joinValue));

        if (reader.TryReadExact(out ReadOnlySpan<char> finalJoinValue, reader.Remaining))
            builders.Add(ParseIndividualJoin(finalJoinValue));

        return builders;
    }

    /// <summary>
    /// Builds the lookup stage.
    /// </summary>
    /// <param name="aggregatePipeline">The aggregate pipeline to append the join to.</param>
    /// <param name="onCollection">The collection that the join is being performed on.</param>
    /// <param name="documentSerializer">The document serializer.</param>
    /// <param name="serializerRegistry">The serializer registry.</param>
    /// <param name="useCaseInsensitiveRegex">Whether regex matches should be case-insensitive.</param>
    public void Build
    (
        ref IAggregateFluent<BsonDocument> aggregatePipeline,
        string onCollection,
        IBsonSerializer<BsonDocument> documentSerializer,
        IBsonSerializerRegistry serializerRegistry,
        bool useCaseInsensitiveRegex
    )
    {
        BsonDocument built = BuildInternal(this, onCollection, documentSerializer, serializerRegistry, useCaseInsensitiveRegex);
        BsonDocumentPipelineStageDefinition<BsonDocument, BsonDocument> stageDef = new(built);
        aggregatePipeline = aggregatePipeline.AppendStage(stageDef);

        if (!IsOuter)
            aggregatePipeline.Match(Builders<BsonDocument>.Filter.Ne(InjectAt, Array.Empty<BsonDocument>()));

        if (!IsList)
            aggregatePipeline = aggregatePipeline.Unwind(InjectAt);
    }

    private static JoinBuilder ParseIndividualJoin(ReadOnlySpan<char> value)
    {
        SpanReader<char> reader = new(value);
        JoinBuilder builder = new();

        while (reader.TryReadToAny(out ReadOnlySpan<char> keyValue, new[] { '^', '(' }, false))
        {
            ParseKeyValuePair(keyValue, builder);

            if (reader.IsNext('(', true))
            {
                builder.Children = Parse(reader.Span[^reader.Remaining..]);
                return builder;
            }

            reader.Advance(1);
        }

        if (!reader.IsNext(')'))
            ParseKeyValuePair(reader.Span[^reader.Remaining..], builder);

        return builder;
    }

    private static void ParseKeyValuePair(ReadOnlySpan<char> keyValuePair, JoinBuilder builder)
    {
        if (keyValuePair.Length == 0)
            throw new QueryException(QueryErrorCode.Malformed, "Zero-length keyvalues are not permitted");

        // We're naughty and don't validate closing joins properly
        // which means we have to ignore them here
        int closingJoinIndex = keyValuePair.IndexOf(')');
        if (closingJoinIndex != -1)
            keyValuePair = keyValuePair[..closingJoinIndex];

        SpanReader<char> reader = new(keyValuePair);

        if (!reader.TryReadTo(out ReadOnlySpan<char> key, ':') || key.SequenceEqual(TypeKey))
        {
            builder.ToCollection = keyValuePair.ToString();
            return;
        }

        if (!reader.TryReadExact(out ReadOnlySpan<char> value, reader.Remaining))
            throw new QueryException(QueryErrorCode.ServerError, string.Empty);

        if (value.Length == 0)
            throw new QueryException(QueryErrorCode.Malformed, $"The {key} key requires a value");
        SpanReader<char> valueReader = new(value);

        if (key.SequenceEqual(OnFieldKey))
        {
            builder.OnField = value.ToString();
        }
        else if (key.SequenceEqual(ToFieldKey))
        {
            builder.ToField = value.ToString();
        }
        else if (key.SequenceEqual(ListKey))
        {
            builder.IsList = value[0] == '1';
        }
        else if (key.SequenceEqual(InjectAtKey))
        {
            builder.InjectAt = JavaScriptEncoder.Default.Encode(value.ToString());
        }
        else if (key.SequenceEqual(OuterKey))
        {
            builder.IsOuter = value[0] == '1';
        }
        else if (key.SequenceEqual(ShowKey))
        {
            while (valueReader.TryReadTo(out ReadOnlySpan<char> show, VALUE_DELIMITER))
                builder.Projection.Include(show.ToString());

            if (valueReader.TryReadExact(out ReadOnlySpan<char> finalShow, valueReader.Remaining))
                builder.Projection.Include(finalShow.ToString());
        }
        else if (key.SequenceEqual(HideKey))
        {
            while (valueReader.TryReadTo(out ReadOnlySpan<char> hide, VALUE_DELIMITER))
                builder.Projection.Exclude(hide.ToString());

            if (valueReader.TryReadExact(out ReadOnlySpan<char> finalHide, valueReader.Remaining))
                builder.Projection.Exclude(finalHide.ToString());
        }
        else if (key.SequenceEqual(TermsKey))
        {
            while (valueReader.TryReadTo(out ReadOnlySpan<char> term, VALUE_DELIMITER))
                builder.Terms.Add(term.ToString());

            if (valueReader.TryReadExact(out ReadOnlySpan<char> finalTerm, valueReader.Remaining))
                builder.Terms.Add(finalTerm.ToString());
        }
    }

    private BsonDocument BuildInternal
    (
        JoinBuilder builder,
        string onCollection,
        IBsonSerializer<BsonDocument> documentSerializer,
        IBsonSerializerRegistry serializerRegistry,
        bool useCaseInsensitiveRegex
    )
    {
        List<FilterBuilder> filterBuilders = builder.PerformPreBuildChecks(onCollection);

        BsonArray subPipeline = new();

        FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Empty;
        foreach (FilterBuilder f in filterBuilders)
            f.Build(ref filter, useCaseInsensitiveRegex);

        if (filterBuilders.Count > 0)
        {
            BsonDocument filterStage = new
            (
                "$match",
                filter.Render(documentSerializer, serializerRegistry)
            );
            subPipeline.Add(filterStage);
        }

        BsonDocument projectStage = new
        (
            "$project",
            builder.Projection.Build().Render(documentSerializer, serializerRegistry)
        );
        subPipeline.Add(projectStage);

        foreach (JoinBuilder child in builder.Children)
        {
            BsonDocument builtChild = child.BuildInternal
            (
                child,
                builder.ToCollection,
                documentSerializer,
                serializerRegistry,
                useCaseInsensitiveRegex
            );
            subPipeline.Add(builtChild);

            if (!child.IsList)
            {
                subPipeline.Add(new BsonDocument
                (
                    "$unwind",
                    new BsonDocument("path", $"${child.InjectAt}")
                ));
            }

            if (!child.IsOuter)
            {
                subPipeline.Add(new BsonDocument
                (
                    "$match",
                    Builders<BsonDocument>.Filter.Ne(InjectAt, Array.Empty<BsonDocument>())
                        .Render(documentSerializer, serializerRegistry)
                ));
            }
        }

        BsonDocument lookup = new
        (
            "$lookup",
            new BsonDocument("from", ToCollection)
                .Add("localField", OnField)
                .Add("foreignField", ToField)
                .Add("pipeline", subPipeline)
                .Add("as", InjectAt)
        );

        return lookup;
    }

    [MemberNotNull(nameof(ToCollection))]
    [MemberNotNull(nameof(OnField))]
    [MemberNotNull(nameof(ToField))]
    private List<FilterBuilder> PerformPreBuildChecks(string onCollection)
    {
        List<FilterBuilder> filters = new();

        if (ToCollection is null)
        {
            throw new QueryException
            (
                QueryErrorCode.JoinCollectionRequired,
                "Specify the 'type' key by providing the name of the collection you wish to join to"
            );
        }

        if (!CollectionUtils.CheckCollectionExists(ToCollection))
        {
            throw new QueryException
            (
                QueryErrorCode.UnknownCollection,
                $"Cannot join to the '{ToCollection}' collection as it does not exist"
            );
        }

        if (OnField is not null && !CollectionUtils.CheckFieldExists(onCollection, OnField))
        {
            throw new QueryException
            (
                QueryErrorCode.UnknownField,
                $"The {OnField} field does not exist on the {onCollection} collection"
            );
        }

        if (ToField is not null && !CollectionUtils.CheckFieldExists(ToCollection, ToField))
        {
            throw new QueryException
            (
                QueryErrorCode.UnknownField,
                $"The {ToField} field does not exist on the {ToCollection} collection"
            );
        }

        CollectionUtils.TryGetMatchingKeyFields(onCollection, ToCollection, out string? matchingKeyField);
        if (OnField is null)
        {
            OnField = matchingKeyField
                ?? throw new QueryException(QueryErrorCode.JoinFieldRequired, $"The {onCollection} requires the 'on' key to be specified.");
        }
        if (ToField is null)
        {
            ToField = matchingKeyField
                ?? throw new QueryException(QueryErrorCode.JoinFieldRequired, $"The {ToCollection} requires the 'to' key to be specified.");
        }

        if (InjectAt is null)
            InjectAt = $"{OnField}_join_{ToCollection}";

        foreach (string term in Terms)
            filters.Add(FilterBuilder.Parse(ToCollection, term));

        return filters;
    }
}
