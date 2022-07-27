using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Services;
using Sanctuary.Census.Json;
using System.Reflection;

namespace Sanctuary.Census.Database;

/// <inheritdoc />
public class MongoContext : IMongoContext
{
    private readonly MongoClient _mongoClient;
    private readonly EnvironmentContextProvider _environmentContextProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoContext"/> class.
    /// </summary>
    /// <param name="mongoClient">The mongo client to use.</param>
    /// <param name="environmentContextProvider">The environment context provider.</param>
    public MongoContext
    (
        MongoClient mongoClient,
        EnvironmentContextProvider environmentContextProvider
    )
    {
        _mongoClient = mongoClient;
        _environmentContextProvider = environmentContextProvider;
    }

    /// <summary>
    /// Sets up a <see cref="BsonClassMap{TClass}"/> for the given
    /// type, automatically converting property names using the
    /// <see cref="SnakeCaseJsonNamingPolicy"/>.
    /// </summary>
    /// <typeparam name="T">The type to map.</typeparam>
    /// <param name="cm">The class map to setup.</param>
    public static void AutoClassMap<T>(BsonClassMap<T> cm)
    {
        foreach (PropertyInfo prop in typeof(T).GetProperties())
            cm.MapProperty(prop.Name).SetElementName(SnakeCaseJsonNamingPolicy.Default.ConvertName(prop.Name));
    }

    /// <inheritdoc />
    public IMongoDatabase GetDatabase(PS2Environment? environment = null)
        => _mongoClient.GetDatabase((environment ?? _environmentContextProvider.Environment) + "-collections");
}
