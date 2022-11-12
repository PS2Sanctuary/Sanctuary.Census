using MemoryReaders;
using MongoDB.Bson;
using MongoDB.Driver;
using Sanctuary.Census.Common;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Api.Exceptions;
using Sanctuary.Census.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sanctuary.Census.Api.Database;

/// <summary>
/// Builds a MongoDB aggregate filter operator from a query filter string.
/// </summary>
public class FilterBuilder
{
    private static readonly Dictionary<string, Dictionary<string, object>> QueryStringConverters = new();
    private static readonly SnakeCaseJsonNamingPolicy NameConv = SnakeCaseJsonNamingPolicy.Default;

    private readonly string _field;
    private readonly FilterType _type;
    private readonly List<object> _values;

    static FilterBuilder()
    {
        IEnumerable<Type> collTypes = typeof(CollectionAttribute).Assembly
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
    /// <param name="field">The field to filter on.</param>
    /// <param name="type">The type of the filter.</param>
    /// <param name="values">The values to filter by.</param>
    public FilterBuilder(string field, FilterType type, List<object> values)
    {
        _field = field;
        _type = type;
        _values = values;
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

        FilterType type = FilterType.Equals;
        if (value[0] is '<' or '[' or '>' or ']' or '^' or '*' or '!')
        {
            if (value.Length < 2)
            {
                throw new QueryException
                (
                    QueryErrorCode.Malformed,
                    $"A term with the comparison operator {value[0]} is expected to contain a value: on field {fieldName}"
                );
            }
            type = (FilterType)value[0];
            value = value[1..];
        }

        SpanReader<char> reader = new(value);
        List<object> filterValues = new();

        while (reader.TryReadTo(out ReadOnlySpan<char> term, ','))
        {
            object termValue = GetTermValue(fieldName.ToString(), type, term, converter);
            filterValues.Add(termValue);
        }

        if (reader.TryReadExact(out ReadOnlySpan<char> lastTerm, reader.Remaining))
        {
            object termValue = GetTermValue(fieldName.ToString(), type, lastTerm, converter);
            filterValues.Add(termValue);
        }

        return new FilterBuilder(fieldName.ToString(), type, filterValues);
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

        string fieldName = filter[..splitIndex++].ToString();
        return Parse(collectionName, fieldName, filter[splitIndex..]);
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

        FilterDefinition<BsonDocument> orFilter = GetFilter(filterBuilder, _field, _type, _values[0], caseInsensitiveRegex);
        for (int i = 1; i < _values.Count; i++)
            orFilter |= GetFilter(filterBuilder, _field, _type, _values[i], caseInsensitiveRegex);

        appendTo &= orFilter;
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

    private static object GetTermValue(string fieldName, FilterType type, ReadOnlySpan<char> term, Func<string, object> converter)
    {
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
                $"Invalid term ('{term}') for filter on {fieldName}. Please check the type matches that of the field you are filtering on"
            );
        }

        if (type is FilterType.Contains or FilterType.StartsWith && value is not string)
            throw new QueryException(QueryErrorCode.InvalidFilterTerm, "A regex query can only be performed on a string field");

        return value;
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
            return s => double.Parse(s);
        if (propertyType == typeof(double))
            return s => double.Parse(s);
        if (propertyType == typeof(decimal))
            return s => Decimal128.Parse(s);
        if (propertyType == typeof(string))
            return s => s;
        if (propertyType.IsEnum)
            return s => s; // Enums are stored as strings
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
