using Sanctuary.Census.Api.Exceptions;
using Sanctuary.Census.Api.Models;
using Sanctuary.Census.Api.Util;
using Sanctuary.Census.Common;
using Sanctuary.Census.Common.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;

namespace Sanctuary.Census.Api.Services;

/// <summary>
/// A helper service to generate descriptions of a collection's structure.
/// </summary>
public class CollectionDescriptionService
{
    private const string DOCUMENTATION_FILE_NAME = "Sanctuary.Census.Common.xml";
    private const string COLLECTION_NAMESPACE = "P:Sanctuary.Census.Common.Objects.Collections.";

    private readonly NullabilityInfoContext _nullabilityContext;
    private readonly Dictionary<string, IReadOnlyList<CollectionFieldInformation>> _fields;

    private IReadOnlyDictionary<string, string?>? _fieldComments;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionDescriptionService"/>.
    /// </summary>
    public CollectionDescriptionService()
    {
        _nullabilityContext = new NullabilityInfoContext();
        _fields = new Dictionary<string, IReadOnlyList<CollectionFieldInformation>>();
    }

    /// <summary>
    /// Gets information about each field in a collection.
    /// </summary>
    /// <param name="collectionSnakeName">The snake_case representation of the collection's name.</param>
    /// <returns>Information about each field in the collection.</returns>
    /// <exception cref="QueryException">
    /// Thrown if a collection matching the <paramref name="collectionSnakeName"/> does not exist.
    /// </exception>
    public IReadOnlyList<CollectionFieldInformation> GetFieldInformation(string collectionSnakeName)
    {
        if (_fields.TryGetValue(collectionSnakeName, out IReadOnlyList<CollectionFieldInformation>? infos))
            return infos;

        if (!CollectionUtils.TryGetCollectionTypeFromSnakeName(collectionSnakeName, out Type? collectionType))
            throw new QueryException(QueryErrorCode.UnknownCollection, $"The {collectionSnakeName} collection does not exist");

        infos = BuildFieldInformation(collectionType);
        _fields.Add(collectionSnakeName, infos);

        return infos;
    }

    private IReadOnlyList<CollectionFieldInformation> BuildFieldInformation(Type collectionType)
    {
        List<CollectionFieldInformation> fieldInfos = new();
        IReadOnlyDictionary<string, string?> fieldComments = GetCollectionFieldDescriptions();

        foreach (PropertyInfo property in collectionType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            NullabilityInfo nullInfo = _nullabilityContext.Create(property);

            bool foundFieldComment = fieldComments.TryGetValue
            (
                $"{COLLECTION_NAMESPACE}{collectionType.Name}.{property.Name}",
                out string? fieldComment
            );
            if (!foundFieldComment)
            {
                throw new MissingFieldException
                (
                    "Failed to find XML documentation for " +
                    $"{COLLECTION_NAMESPACE}{collectionType.Name}.{property.Name}" +
                    $". Perhaps {DOCUMENTATION_FILE_NAME} is outdated?"
                );
            }

            string typeName = nullInfo.ReadState == NullabilityState.Nullable && property.PropertyType.IsValueType
                ? Nullable.GetUnderlyingType(property.PropertyType)!.Name
                : property.PropertyType.Name;

            fieldInfos.Add(new CollectionFieldInformation
            (
                SnakeCaseJsonNamingPolicy.Default.ConvertName(property.Name),
                typeName,
                nullInfo.ReadState == NullabilityState.Nullable,
                fieldComment
            ));
        }

        return fieldInfos;
    }

    private IReadOnlyDictionary<string, string?> GetCollectionFieldDescriptions()
    {
        if (_fieldComments is not null)
            return _fieldComments;

        Dictionary<string, string?> fieldComments = new();
        string? lastName = null;

        string? executionLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string path = executionLocation is null
            ? DOCUMENTATION_FILE_NAME
            : Path.Combine(executionLocation, DOCUMENTATION_FILE_NAME);
        using XmlReader reader = XmlReader.Create(path);

        while (reader.Read())
        {
            if (reader.NodeType != XmlNodeType.Element)
                continue;

            if (reader.Name == "member")
            {
                string name = reader["name"]!;
                if (!name.StartsWith(COLLECTION_NAMESPACE))
                    continue;
                lastName = name;
            }
            else if (reader.Name == "summary")
            {
                if (lastName != null)
                {
                    string inner = reader.ReadInnerXml();
                    fieldComments[lastName] = inner.Length == 0 ? null : inner.Trim();
                }
                lastName = null;
            }
        }

        _fieldComments = fieldComments;
        return fieldComments;
    }
}
