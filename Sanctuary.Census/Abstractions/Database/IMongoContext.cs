using MongoDB.Driver;
using Sanctuary.Census.Common.Objects;

namespace Sanctuary.Census.Abstractions.Database;

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
}
