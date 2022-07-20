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
    private static readonly Dictionary<string, Dictionary<string, Func<string, object>>> QueryStringConverters = new();
    private static readonly SnakeCaseJsonNamingPolicy NameConv = SnakeCaseJsonNamingPolicy.Default;

    private readonly List<Filter> _filters;

    static FilterBuilder()
    {
        IEnumerable<Type> collTypes = typeof(ProjectionBuilder).Assembly
            .GetTypes()
            .Where(t => t.IsDefined(typeof(CollectionAttribute)));

        foreach (Type collType in collTypes)
        {
            Dictionary<string, Func<string, object>> propConverters = new();
            QueryStringConverters.Add
            (
                NameConv.ConvertName(collType.Name),
                propConverters
            );

            foreach (PropertyInfo prop in collType.GetProperties())
            {
                propConverters.Add
                (
                    NameConv.ConvertName(prop.Name),
                    GetConverter(prop.PropertyType)
                );
            }
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
    public static FilterBuilder Parse(string collectionName, string fieldName, ReadOnlySpan<char> value)
    {
        if (!QueryStringConverters.TryGetValue(collectionName, out Dictionary<string, Func<string, object>>? propConverters))
            throw new QueryException(QueryErrorCode.UnknownCollection, $"The {collectionName} collection is not recognised");

        if (!propConverters.TryGetValue(fieldName, out Func<string, object>? converter))
            throw new QueryException(QueryErrorCode.UnknownField, $"The {fieldName} does not exist on the {collectionName} collection");

        SpanReader<char> reader = new(value);
        List<Filter> filterValues = new();

        while (reader.TryReadTo(out ReadOnlySpan<char> term, ','))
        {
            (FilterType type, object termValue) = GetTermValue(fieldName, term, converter);
            filterValues.Add(new Filter(fieldName, type, termValue));
        }

        if (reader.TryReadExact(out ReadOnlySpan<char> lastTerm, reader.Remaining))
        {
            (FilterType type, object termValue) = GetTermValue(fieldName, lastTerm, converter);
            filterValues.Add(new Filter(fieldName, type, termValue));
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

        foreach (Filter f in _filters)
        {
            appendTo &= f.Type switch {
                FilterType.LessThan => filterBuilder.Lt(f.FieldName, f.Value),
                FilterType.LessThanOrEqual => filterBuilder.Lte(f.FieldName, f.Value),
                FilterType.GreaterThan => filterBuilder.Gt(f.FieldName, f.Value),
                FilterType.GreaterThanOrEqual => filterBuilder.Gte(f.FieldName, f.Value),
                FilterType.StartsWith => filterBuilder.Regex
                    (
                        f.FieldName,
                        caseInsensitiveRegex ? new BsonRegularExpression($"^{f.Value}", "i") : new BsonRegularExpression($"^{f.Value}")
                    ),
                FilterType.Contains => filterBuilder.Regex
                    (
                        f.FieldName,
                        caseInsensitiveRegex ? new BsonRegularExpression((string)f.Value, "i") : new BsonRegularExpression((string)f.Value)
                    ),
                FilterType.NotEquals => filterBuilder.Ne(f.FieldName, f.Value),
                _ when f.Value is string value && caseInsensitiveRegex => filterBuilder.Regex
                    (
                        f.FieldName,
                        new BsonRegularExpression(value, "i")
                    ),
                _ => filterBuilder.Eq(f.FieldName, f.Value)
            };
        }
    }

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

    private static Func<string, object> GetConverter(Type propertyType)
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

        // Naughty! Our way of getting around nested types
        return s => s.ToString();
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
