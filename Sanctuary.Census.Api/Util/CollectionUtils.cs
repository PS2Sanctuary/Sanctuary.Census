using Sanctuary.Census.Common;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Sanctuary.Census.Util;

/// <summary>
/// Utilities for working with collections.
/// </summary>
public static class CollectionUtils
{
    private static readonly Dictionary<string, HashSet<string>> _collectionNamesAndFields;
    private static readonly Dictionary<string, List<string>> _keyFields;
    private static readonly Dictionary<string, List<string>> _langFields;

    static CollectionUtils()
    {
        _collectionNamesAndFields = new Dictionary<string, HashSet<string>>();
        _keyFields = new Dictionary<string, List<string>>();
        _langFields = new Dictionary<string, List<string>>();

        IEnumerable<Type> collTypes = typeof(CollectionAttribute).Assembly
            .GetTypes()
            .Where(t => t.IsDefined(typeof(CollectionAttribute)));

        foreach (Type collType in collTypes)
        {
            HashSet<string> propNames = new();
            string collName = SnakeCaseJsonNamingPolicy.Default.ConvertName(collType.Name);

            _collectionNamesAndFields.Add(collName, propNames);
            _keyFields.Add(collName, new List<string>());
            _langFields.Add(collName, new List<string>());

            foreach (PropertyInfo prop in collType.GetProperties())
            {
                string propName = SnakeCaseJsonNamingPolicy.Default.ConvertName(prop.Name);
                propNames.Add(propName);

                if (prop.IsDefined(typeof(KeyAttribute)))
                    _keyFields[collName].Add(propName);

                if (prop.PropertyType == typeof(LocaleString))
                    _langFields[collName].Add(propName);
            }
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

        if (!_keyFields.TryGetValue(collectionNameA, out List<string>? keyFieldsA))
            return false;
        if (!_keyFields.TryGetValue(collectionNameB, out List<string>? keyFieldsB))
            return false;

        matchingKeyFieldName = keyFieldsA.FirstOrDefault(k => keyFieldsB.Contains(k));
        return matchingKeyFieldName is not null;
    }

    /// <summary>
    /// Gets any fields on a collection of the <see cref="LocaleString"/> type.
    /// </summary>
    /// <param name="collectionName">The name of the collection.</param>
    /// <returns></returns>
    public static List<string> GetLocaleFields(string collectionName)
        => _langFields[collectionName];
}
