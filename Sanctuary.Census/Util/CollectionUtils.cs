using Sanctuary.Census.Attributes;
using Sanctuary.Census.Json;
using Sanctuary.Census.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Sanctuary.Census.Util;

/// <summary>
/// Utilities for working with collections.
/// </summary>
public static class CollectionUtils
{
    private static readonly Dictionary<string, HashSet<string>> _collectionNamesAndFields;
    private static readonly Dictionary<string, string> _primaryJoinFields;
    // TODO: Lang fields

    static CollectionUtils()
    {
        _collectionNamesAndFields = new Dictionary<string, HashSet<string>>();
        _primaryJoinFields = new Dictionary<string, string>();

        IEnumerable<Type> collTypes = typeof(CollectionBuilderRepository).Assembly
            .GetTypes()
            .Where(t => t.IsDefined(typeof(CollectionAttribute)));

        foreach (Type collType in collTypes)
        {
            HashSet<string> propNames = new();
            string collName = SnakeCaseJsonNamingPolicy.Default.ConvertName(collType.Name);
            _collectionNamesAndFields.Add
            (
                collName,
                propNames
            );

            foreach (PropertyInfo prop in collType.GetProperties())
                propNames.Add(SnakeCaseJsonNamingPolicy.Default.ConvertName(prop.Name));
        }
    }

    /// <summary>
    /// Checks that a collection exists.
    /// </summary>
    /// <param name="webName">The name of the collection, formatted as it would be in a query.</param>
    /// <returns><c>True</c> if the collection exists, else <c>False</c>.</returns>
    public static bool CheckCollectionExists(string webName)
        => _collectionNamesAndFields.ContainsKey(webName);

    /// <summary>
    /// Checks that a field on a collection exists.
    /// </summary>
    /// <param name="collectionWebName">The name of the collection, formatted as it would be in a query.</param>
    /// <param name="fieldWebName">The name of the field, formatted as it would be in a query.</param>
    /// <returns><c>True</c> if the field exists, else <c>False</c>.</returns>
    public static bool CheckFieldExists(string collectionWebName, string fieldWebName)
        => _collectionNamesAndFields.TryGetValue(collectionWebName, out HashSet<string>? fields)
           && fields.Contains(fieldWebName);

    /// <summary>
    /// Gets the primary join field of a collection.
    /// </summary>
    /// <param name="collectionName">The name of the collection.</param>
    /// <param name="primaryJoinField">The primary join field.</param>
    /// <returns><c>True</c> if the collection has a primary join field, else <c>False</c>.</returns>
    public static bool TryGetPrimaryJoinField(string collectionName, [NotNullWhen(true)] out string? primaryJoinField)
        => _primaryJoinFields.TryGetValue(collectionName, out primaryJoinField);

    public static bool TryGetMatchingKeyFields
    (
        string collectionNameA,
        string collectionNameB,
        out string? matchingKeyFieldName
    )
    {
        throw new NotImplementedException();
    }
}
