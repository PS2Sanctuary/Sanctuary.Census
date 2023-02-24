using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Json;
using Sanctuary.Census.Common.Objects.CommonModels;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Sanctuary.Census.Api.Util;

/// <summary>
/// Utilities for working with collections.
/// </summary>
public static class CollectionUtils
{
    private static readonly HashSet<string> _knownCollections;
    private static readonly Dictionary<string, Type> _collectionSnakeNameToType;
    private static readonly Dictionary<string, HashSet<string>> _collectionNamesAndFields;
    private static readonly Dictionary<string, HashSet<string>> _joinKeyFields;
    private static readonly Dictionary<string, List<string>> _langFields;

    static CollectionUtils()
    {
        _knownCollections = new HashSet<string>();
        _collectionSnakeNameToType = new Dictionary<string, Type>();
        _collectionNamesAndFields = new Dictionary<string, HashSet<string>>();
        _joinKeyFields = new Dictionary<string, HashSet<string>>();
        _langFields = new Dictionary<string, List<string>>();

        IEnumerable<Type> collTypes = typeof(CollectionAttribute).Assembly
            .GetTypes()
            .Where
            (
                t =>
                {
                    CollectionAttribute? collAttr = t.GetCustomAttribute<CollectionAttribute>();
                    return collAttr is { IsHidden: false, IsNestedType: false };
                }
            );

        foreach (Type collType in collTypes)
        {
            HashSet<string> propNames = new();
            string collName = SnakeCaseJsonNamingPolicy.Default.ConvertName(collType.Name);

            _knownCollections.Add(collName);
            _collectionSnakeNameToType.Add(collName, collType);
            _collectionNamesAndFields.Add(collName, propNames);
            _joinKeyFields.Add(collName, new HashSet<string>());
            _langFields.Add(collName, new List<string>());

            void AddProps(string? prefix, Type t)
            {
                foreach (PropertyInfo prop in t.GetProperties())
                {
                    string propName = SnakeCaseJsonNamingPolicy.Default.ConvertName(prop.Name);
                    if (prefix is not null)
                        propName = $"{prefix}.{propName}";
                    propNames.Add(propName);

                    if (prop.IsDefined(typeof(JoinKeyAttribute)))
                        _joinKeyFields[collName].Add(propName);

                    if (prop.PropertyType == typeof(LocaleString))
                        _langFields[collName].Add(propName);

                    if (prop.PropertyType.GetCustomAttribute<CollectionAttribute>()?.IsNestedType is true)
                        AddProps(propName, prop.PropertyType);
                }
            }

            AddProps(null, collType);
        }
    }

    /// <summary>
    /// Attempts to match a snake_case collection name to its associated
    /// POCO type.
    /// </summary>
    /// <param name="snakeName">The snake_case name of the collection.</param>
    /// <param name="collectionType">The type of the collection if found, else <c>null</c>.</param>
    /// <returns>
    /// <c>True</c> if a collection with the given snake_case name representation exists, otherwise <c>false</c>.
    /// </returns>
    public static bool TryGetCollectionTypeFromSnakeName
    (
        string snakeName,
        [NotNullWhen(true)] out Type? collectionType
    ) => _collectionSnakeNameToType.TryGetValue(snakeName, out collectionType);

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
    /// Tries to find key fields on two different collections that share the same name.
    /// </summary>
    /// <param name="collectionNameA">The first collection.</param>
    /// <param name="collectionNameB">The second collection.</param>
    /// <param name="matchingKeyFieldName">A common key field, or <c>null</c> if none exists.</param>
    /// <returns><c>True</c> if a common key field could be found, otherwise <c>False</c>.</returns>
    public static bool TryGetMatchingKeyFields
    (
        string collectionNameA,
        string collectionNameB,
        out string? matchingKeyFieldName
    )
    {
        matchingKeyFieldName = null;

        if (!_joinKeyFields.TryGetValue(collectionNameA, out HashSet<string>? keyFieldsA))
            return false;
        if (!_joinKeyFields.TryGetValue(collectionNameB, out HashSet<string>? keyFieldsB))
            return false;

        matchingKeyFieldName = keyFieldsA.FirstOrDefault(k => keyFieldsB.Contains(k));
        return matchingKeyFieldName is not null;
    }

    /// <summary>
    /// Gets any fields on a collection of the <see cref="LocaleString"/> type.
    /// </summary>
    /// <param name="collectionName">The name of the collection.</param>
    /// <returns></returns>
    public static IReadOnlyList<string> GetLocaleFields(string collectionName)
        => _langFields[collectionName];

    /// <summary>
    /// Gets a value indicating whether a collection is hidden.
    /// </summary>
    /// <param name="collectionName">The name of the collection.</param>
    /// <returns><c>True</c> if the collection is hidden, otherwise <c>false</c>.</returns>
    public static bool IsCollectionHidden(string collectionName)
        => !_knownCollections.Contains(collectionName);
}
