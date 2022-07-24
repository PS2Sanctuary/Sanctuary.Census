using Sanctuary.Census.Common.Util;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.Models;
using Sanctuary.Census.Util;
using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;

namespace Sanctuary.Census.Database;

/// <summary>
/// Builds a MongoDB aggregate lookup stage from a query join string.
/// </summary>
public class JoinBuilder
{
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
    public string ToCollection { get; private set; }

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
    public bool IsOuter { get; private set; }

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
    public IReadOnlyList<JoinBuilder>? Children { get; private set; }

    public JoinBuilder()
    {
        Terms = new List<string>();
        IsOuter = true;
        Projection = new ProjectionBuilder();
        // _toCollection = new BsonDocument("from", toCollection);
        // _onField = new BsonDocument("localField", onField);
        // _toField = new BsonDocument("foreignField", toField);
        // _injectAt = new BsonDocument("as", injectAt);
    }

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

    public void Build(string onCollection)
    {
        if (!CollectionUtils.CheckCollectionExists(ToCollection))
        {
            throw new QueryException
            (
                QueryErrorCode.UnknownCollection,
                $"Cannot join to the {ToCollection} collection as it does not exist"
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

        if (OnField is not null && !CollectionUtils.CheckFieldExists(onCollection, OnField))
        {
            throw new QueryException
            (
                QueryErrorCode.UnknownField,
                $"The {OnField} field does not exist on the {onCollection} collection"
            );
        }

        // TODO: If ToField and OnField are null auto-fill them
    }

    private static JoinBuilder ParseIndividualJoin(ReadOnlySpan<char> value)
    {
        SpanReader<char> reader = new(value);
        JoinBuilder builder = new();

        while (reader.TryReadTo(out ReadOnlySpan<char> keyValue, '^'))
            ParseKeyValuePair(keyValue, builder);

        if (reader.TryReadTo(out ReadOnlySpan<char> finalKeyValue, '(', false))
            ParseKeyValuePair(finalKeyValue, builder);
        else if (reader.TryReadExact(out finalKeyValue, reader.Remaining))
            ParseKeyValuePair(finalKeyValue, builder);

        if (reader.IsNext('(', true))
        {
            if (reader.TryReadTo(out ReadOnlySpan<char> children, ')'))
                builder.Children = Parse(children);
            else
                throw new QueryException(QueryErrorCode.Malformed, "Missing a closing bracket on a child join");
        }

        return builder;
    }

    private static void ParseKeyValuePair(ReadOnlySpan<char> keyValuePair, JoinBuilder builder)
    {
        if (keyValuePair.Length == 0)
            throw new QueryException(QueryErrorCode.Malformed, "Zero-length keyvalues are not permitted");

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
            while (reader.TryReadTo(out ReadOnlySpan<char> show, VALUE_DELIMITER))
                builder.Projection.Include(show.ToString());

            if (reader.TryReadExact(out ReadOnlySpan<char> finalShow, VALUE_DELIMITER))
                builder.Projection.Include(finalShow.ToString());
        }
        else if (key.SequenceEqual(HideKey))
        {
            while (reader.TryReadTo(out ReadOnlySpan<char> hide, VALUE_DELIMITER))
                builder.Projection.Exclude(hide.ToString());

            if (reader.TryReadExact(out ReadOnlySpan<char> finalHide, VALUE_DELIMITER))
                builder.Projection.Exclude(finalHide.ToString());
        }
        else if (key.SequenceEqual(TermsKey))
        {
            while (reader.TryReadTo(out ReadOnlySpan<char> term, VALUE_DELIMITER))
                builder.Terms.Add(term.ToString());

            if (reader.TryReadExact(out ReadOnlySpan<char> finalTerm, VALUE_DELIMITER))
                builder.Terms.Add(finalTerm.ToString());
        }
    }
}
