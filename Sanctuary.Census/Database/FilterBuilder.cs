using MongoDB.Bson;
using MongoDB.Driver;
using Sanctuary.Census.Attributes;
using Sanctuary.Census.Common.Util;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.Json;
using Sanctuary.Census.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sanctuary.Census.Database;

/// <summary>
/// Builds a MongoDB aggregate filter operator from a query filter string.
/// </summary>
public class FilterBuilder
{
    private static readonly Dictionary<string, Dictionary<string, object>> QueryStringConverters = new();
    private static readonly SnakeCaseJsonNamingPolicy NameConv = SnakeCaseJsonNamingPolicy.Default;

    private readonly List<Filter> _filters;

    static FilterBuilder()
    {
        IEnumerable<Type> collTypes = typeof(ProjectionBuilder).Assembly
            .GetTypes()
            .Where(t => t.IsDefined(typeof(CollectionAttribute)));

        foreach (Type collType in collTypes)
        {
            Dictionary<string, object> propConverters = new();
            QueryStringConverters.Add
            (
                NameConv.ConvertName(collType.Name),
                propConverters
            );

            GetTypeConverters(propConverters, collType);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FilterBuilder"/> class.
    /// </summary>
    /// <param name="filters">The filters to build.</param>
    public FilterBuilder(List<Filter> filters)
    {
        _filters = filters;
    }

    /// <summary>
    /// Parses a filter string.
    /// </summary>
    /// <param name="collectionName">The name of the collection that the filter is being performed on.</param>
    /// <param name="fieldName">The field to perform the filter on.</param>
    /// <param name="value">The value of the filter.</param>
    /// <returns>A new <see cref="FilterBuilder"/>.</returns>
    /// <exception cref="QueryException">If the filter string was malformed.</exception>
    public static FilterBuilder Parse(string collectionName, ReadOnlySpan<char> fieldName, ReadOnlySpan<char> value)
    {
        if (!QueryStringConverters.TryGetValue(collectionName, out Dictionary<string, object>? propConverters))
            throw new QueryException(QueryErrorCode.UnknownCollection, $"The {collectionName} collection is not recognised");

        SpanReader<char> nameReader = new(fieldName);
        while (nameReader.TryReadTo(out ReadOnlySpan<char> nameComponent, '.'))
        {
            if (!propConverters.TryGetValue(nameComponent.ToString(), out object? subConverter))
                throw new QueryException(QueryErrorCode.UnknownField, $"The field path {fieldName} does not point to a valid field");

            if (subConverter is not Dictionary<string, object> subPropConverters)
                throw new QueryException(QueryErrorCode.UnknownField, $"The field path {fieldName} attempts to traverse through a non-existent object");

            propConverters = subPropConverters;
        }

        if (!nameReader.TryReadExact(out ReadOnlySpan<char> pathEnd, nameReader.Remaining))
            throw new QueryException(QueryErrorCode.Malformed, $"The field path {fieldName} ended in an accessor ('.')");

        if (!propConverters.TryGetValue(pathEnd.ToString(), out object? maybeConverter))
            throw new QueryException(QueryErrorCode.UnknownField, $"The field path {fieldName} does not point to a valid field");

        if (maybeConverter is not Func<string, object> converter)
            throw new QueryException(QueryErrorCode.UnknownField, $"The field path {fieldName} points to an object, rather than a field");

        SpanReader<char> reader = new(value);
        List<Filter> filterValues = new();

        while (reader.TryReadTo(out ReadOnlySpan<char> term, ','))
        {
            (FilterType type, object termValue) = GetTermValue(fieldName.ToString(), term, converter);
            filterValues.Add(new Filter(fieldName.ToString(), type, termValue));
        }

        if (reader.TryReadExact(out ReadOnlySpan<char> lastTerm, reader.Remaining))
        {
            (FilterType type, object termValue) = GetTermValue(fieldName.ToString(), lastTerm, converter);
            filterValues.Add(new Filter(fieldName.ToString(), type, termValue));
        }

        return new FilterBuilder(filterValues);
    }

    /// <summary>
    /// Parses a filter string.
    /// </summary>
    /// <param name="collectionName">The name of the collection that the filter is being performed on.</param>
    /// <param name="filter">The filter.</param>
    /// <returns>A new <see cref="FilterBuilder"/>.</returns>
    /// <exception cref="QueryException">If the filter string was malformed.</exception>
    public static FilterBuilder Parse(string collectionName, ReadOnlySpan<char> filter)
    {
        int splitIndex = filter.IndexOf('=');
        if (splitIndex == -1)
            throw new QueryException(QueryErrorCode.Malformed, "A filter term must have a be in the form '<field_name>=<value>'");

        string fieldName = filter[..splitIndex].ToString();
        return Parse(collectionName, fieldName, filter);
    }

    /// <summary>
    /// Builds the filter definition.
    /// </summary>
    /// <param name="appendTo">The filter to append the built filter to.</param>
    /// <param name="caseInsensitiveRegex">Whether to use case-insensitive regex searches.</param>
    /// <returns>The built filter definition.</returns>
    public void Build(ref FilterDefinition<BsonDocument> appendTo, bool caseInsensitiveRegex)
    {
        FilterDefinitionBuilder<BsonDocument> filterBuilder = Builders<BsonDocument>.Filter;

        Dictionary<string, Dictionary<FilterType, List<Filter>>> buckets = new();
        foreach (Filter f in _filters)
        {
            if (!buckets.ContainsKey(f.FieldName))
                buckets[f.FieldName] = new Dictionary<FilterType, List<Filter>>();

            if (!buckets[f.FieldName].ContainsKey(f.Type))
                buckets[f.FieldName][f.Type] = new List<Filter>();

            buckets[f.FieldName][f.Type].Add(f);
        }

        foreach ((string fieldName, Dictionary<FilterType, List<Filter>> bucket) in buckets)
        {
            foreach ((FilterType type, List<Filter> filters) in bucket)
            {
                FilterDefinition<BsonDocument> orFilter = GetFilter(filterBuilder, fieldName, type, filters[0].Value, caseInsensitiveRegex);
                for (int i = 1; i < filters.Count; i++)
                    orFilter |= GetFilter(filterBuilder, fieldName, type, filters[i].Value, caseInsensitiveRegex);

                appendTo &= orFilter;
            }
        }
    }

    private FilterDefinition<BsonDocument> GetFilter
    (
        FilterDefinitionBuilder<BsonDocument> filterBuilder,
        string fieldName,
        FilterType type,
        object filterValue,
        bool caseInsensitiveRegex
    ) => type switch
    {
        FilterType.LessThan => filterBuilder.Lt(fieldName, filterValue),
        FilterType.LessThanOrEqual => filterBuilder.Lte(fieldName, filterValue),
        FilterType.GreaterThan => filterBuilder.Gt(fieldName, filterValue),
        FilterType.GreaterThanOrEqual => filterBuilder.Gte(fieldName, filterValue),
        FilterType.StartsWith => filterBuilder.Regex
        (
            fieldName,
            caseInsensitiveRegex ? new BsonRegularExpression($"^{filterValue}", "i") : new BsonRegularExpression($"^{filterValue}")
        ),
        FilterType.Contains => filterBuilder.Regex
        (
            fieldName,
            caseInsensitiveRegex ? new BsonRegularExpression((string)filterValue, "i") : new BsonRegularExpression((string)filterValue)
        ),
        FilterType.NotEquals => filterBuilder.Ne(fieldName, filterValue),
        _ when filterValue is string value && caseInsensitiveRegex => filterBuilder.Regex
        (
            fieldName,
            new BsonRegularExpression(value, "i")
        ),
        _ => filterBuilder.Eq(fieldName, filterValue)
    };

    private static (FilterType, object) GetTermValue(string fieldName, ReadOnlySpan<char> term, Func<string, object> converter)
    {
        FilterType type = FilterType.Equals;
        if (term[0] is '<' or '[' or '>' or ']' or '^' or '*' or '!')
        {
            if (term.Length < 2)
            {
                throw new QueryException
                (
                    QueryErrorCode.Malformed,
                    $"A term with the comparison operator {term[0]} is expected to contain a value: on field {fieldName}"
                );
            }
            type = (FilterType)term[0];
            term = term[1..];
        }

        object value;
        try
        {
            value = converter(term.ToString());
        }
        catch
        {
            throw new QueryException
            (
                QueryErrorCode.InvalidFilterTerm,
                $"Invalid term ('{term}') for filter on {fieldName}"
            );
        }

        if (type is FilterType.Contains or FilterType.StartsWith && value is not string)
            throw new QueryException(QueryErrorCode.InvalidFilterTerm, "A regex query can only be performed on a string field");

        return (type, value);
    }

    private static void GetTypeConverters(IDictionary<string, object> converters, Type type)
    {
        foreach (PropertyInfo prop in type.GetProperties())
        {
            Func<string, object>? converter = GetConverter(prop.PropertyType);
            string name = NameConv.ConvertName(prop.Name);
            if (converter is null)
            {
                Dictionary<string, object> subConverters = new();
                GetTypeConverters(subConverters, prop.PropertyType);
                converters.Add(name, subConverters);
            }
            else
            {
                converters.Add(name, converter);
            }
        }
    }

    private static Func<string, object>? GetConverter(Type propertyType)
    {
        if (propertyType == typeof(bool))
            return ParseBoolean;
        if (propertyType == typeof(byte))
            return s => int.Parse(s);
        if (propertyType == typeof(sbyte))
            return s => int.Parse(s);
        if (propertyType == typeof(ushort))
            return s => int.Parse(s);
        if (propertyType == typeof(short))
            return s => int.Parse(s);
        if (propertyType == typeof(uint))
            return s => long.Parse(s);
        if (propertyType == typeof(int))
            return s => int.Parse(s);
        if (propertyType == typeof(ulong))
            return s => long.Parse(s); // EEEEK hope we don't have large numbers issues
        if (propertyType == typeof(long))
            return s => long.Parse(s);
        if (propertyType == typeof(float))
            return s => float.Parse(s);
        if (propertyType == typeof(double))
            return s => double.Parse(s);
        if (propertyType == typeof(decimal))
            return s => decimal.Parse(s);
        if (propertyType == typeof(string))
            return s => s;
        if (propertyType.IsEnum)
            return s => Enum.Parse(propertyType, s);
        if (Nullable.GetUnderlyingType(propertyType) != null)
            return GetConverter(Nullable.GetUnderlyingType(propertyType)!);

        return null;
    }

    private static object ParseBoolean(string value)
        => value switch
        {
            "0" => false,
            "1" => true,
            _ => bool.Parse(value)
        };

    /// <summary>
    /// Represents a filter.
    /// </summary>
    /// <param name="FieldName">The field to be filtered.</param>
    /// <param name="Type">The type of the filter.</param>
    /// <param name="Value">The value to filter by.</param>
    public record Filter(string FieldName, FilterType Type, object Value);

    /// <summary>
    /// Defines the possible filter types.
    /// </summary>
    public enum FilterType : short
    {
        /// <summary>
        /// Returns values that are equal to the value of the filter.
        /// </summary>
        Equals = 0,

        /// <summary>
        /// Returns values that are not equal to the value of the filter.
        /// </summary>
        NotEquals = (short)'!',

        /// <summary>
        /// Returns values that contain the value of the filter.
        /// </summary>
        Contains = (short)'*',

        /// <summary>
        /// Returns values that are less than the value of the filter.
        /// </summary>
        LessThan = (short)'<',

        /// <summary>
        /// Returns values that are greater than the value of the filter.
        /// </summary>
        GreaterThan = (short)'>',

        /// <summary>
        /// Returns values that are less than or equal to the value of the filter.
        /// </summary>
        LessThanOrEqual = (short)'[',

        /// <summary>
        /// Returns values that are greater than or equal to the value of the filter.
        /// </summary>
        GreaterThanOrEqual = (short)']',

        /// <summary>
        /// Returns values that start with the value of the filter.
        /// </summary>
        StartsWith = (short)'^'
    }
}
