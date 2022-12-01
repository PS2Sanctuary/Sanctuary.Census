using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Json;
using Sanctuary.Census.Common.Objects;
using System;
using System.Reflection;

namespace Sanctuary.Census.Common.Services;

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
    /// Sets up a <see cref="BsonClassMap"/> for the given
    /// type, automatically converting property names using the
    /// <see cref="SnakeCaseJsonNamingPolicy"/>.
    /// </summary>
    /// <typeparam name="T">The type to map.</typeparam>
    /// <returns>The class map.</returns>
    public static BsonClassMap AutoClassMap<T>()
        => AutoClassMap(typeof(T));

    /// <summary>
    /// Sets up a <see cref="BsonClassMap"/> for the given
    /// type, automatically converting property names using the
    /// <see cref="SnakeCaseJsonNamingPolicy"/>.
    /// </summary>
    /// <param name="type">The type to map.</param>
    /// <returns>The class map.</returns>
    public static BsonClassMap AutoClassMap(Type type)
    {
        BsonClassMap cm = new(type);

        foreach (PropertyInfo prop in type.GetProperties())
        {
            BsonMemberMap map = cm.MapProperty(prop.Name);
            map.SetElementName(SnakeCaseJsonNamingPolicy.Default.ConvertName(prop.Name));

            if (!prop.PropertyType.IsEnum)
                continue;

            Type serializerType = typeof(EnumSerializer<>).MakeGenericType(prop.PropertyType);
            object? serializer = Activator.CreateInstance(serializerType, BsonType.String);
            if (serializer is null)
                throw new Exception("Couldn't create instance of enum serializer");
            map.SetSerializer((IBsonSerializer)serializer);
        }

        return cm;
    }

    /// <inheritdoc />
    public IMongoDatabase GetDatabase(PS2Environment? environment = null)
        => _mongoClient.GetDatabase((environment ?? _environmentContextProvider.Environment) + "-collections");

    /// <inheritdoc />
    public IMongoCollection<T> GetCollection<T>(PS2Environment? environment = null)
        => GetDatabase(environment).GetCollection<T>(SnakeCaseJsonNamingPolicy.Default.ConvertName(typeof(T).Name));

    /// <inheritdoc />
    public IMongoCollection<BsonDocument> GetCollection(Type collectionType, PS2Environment? environment = null)
        => GetDatabase(environment).GetCollection<BsonDocument>(SnakeCaseJsonNamingPolicy.Default.ConvertName(collectionType.Name));
}
