using MongoDB.Bson;
using MongoDB.Driver;
using Sanctuary.Census.Common.Objects;
using System;

namespace Sanctuary.Census.Common.Abstractions.Services;

/// <summary>
/// Represents a MongoDB context.
/// </summary>
public interface IMongoContext
{
    /// <summary>
    /// Gets a connection to the database.
    /// </summary>
    /// <param name="environment">The environment to retrieve the database from.</param>
    /// <returns>The database.</returns>
    IMongoDatabase GetDatabase(PS2Environment? environment = null);

    /// <summary>
    /// Gets a collection of the given type.
    /// </summary>
    /// <typeparam name="T">The type of the collection.</typeparam>
    /// <param name="environment">The environment to retrieve the collection from.</param>
    /// <returns>The collection.</returns>
    IMongoCollection<T> GetCollection<T>(PS2Environment? environment = null);

    /// <summary>
    /// Gets a collection of the given type.
    /// </summary>
    /// <param name="collectionType">The type of the collection.</param>
    /// <param name="environment">The environment to retrieve the collection from.</param>
    /// <returns>The collection.</returns>
    IMongoCollection<BsonDocument> GetCollection(Type collectionType, PS2Environment? environment = null);
}
