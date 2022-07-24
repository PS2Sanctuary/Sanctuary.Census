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
    private static IReadOnlyDictionary<string, HashSet<string>> _collectionNamesAndFields;

    static CollectionUtils()
    {
        BuildCollectionNamesAndFields();
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

    [MemberNotNull(nameof(_collectionNamesAndFields))]
    private static void BuildCollectionNamesAndFields()
    {
        Dictionary<string, HashSet<string>> collectionNamesAndFields = new();

        IEnumerable<Type> collTypes = typeof(CollectionBuilderRepository).Assembly
            .GetTypes()
            .Where(t => t.IsDefined(typeof(CollectionAttribute)));

        foreach (Type collType in collTypes)
        {
            HashSet<string> propNames = new();
            collectionNamesAndFields.Add
            (
                SnakeCaseJsonNamingPolicy.Default.ConvertName(collType.Name),
                propNames
            );

            foreach (PropertyInfo prop in collType.GetProperties())
                propNames.Add(SnakeCaseJsonNamingPolicy.Default.ConvertName(prop.Name));
        }

        _collectionNamesAndFields = collectionNamesAndFields;
    }
}
