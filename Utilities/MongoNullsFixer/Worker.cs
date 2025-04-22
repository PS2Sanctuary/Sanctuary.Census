using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Json;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Objects.CommonModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MongoNullsFixer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IMongoContext _mongoContext;
    private readonly SnakeCaseJsonNamingPolicy _namePolicy = SnakeCaseJsonNamingPolicy.Default;

    public Worker
    (
        ILogger<Worker> logger,
        IHostApplicationLifetime lifetime,
        IMongoContext mongoContext
    )
    {
        _logger = logger;
        _lifetime = lifetime;
        _mongoContext = mongoContext;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        Type[] collTypes = typeof(CollectionAttribute).Assembly
            .GetTypes()
            .Where(t => t.GetCustomAttribute<CollectionAttribute>()?.IsNestedType == false)
            .ToArray();

        foreach (PS2Environment env in Enum.GetValues<PS2Environment>())
        {
            _logger.LogInformation("Fixing nulls in environment '{CollName}'", env);
            IMongoDatabase db = _mongoContext.GetDatabase(env);

            foreach (Type collType in collTypes)
            {
                string collName = _namePolicy.ConvertName(collType.Name);
                _logger.LogInformation("Fixing nulls in collection '{CollName}'", collName);
                await FixNullsInCollection(collType, db.GetCollection<BsonDocument>(collName), ct);
            }
        }

        _lifetime.StopApplication();
    }

    private async Task FixNullsInCollection
    (
        Type collectionType,
        IMongoCollection<BsonDocument> collection,
        CancellationToken ct
    )
    {
        PropertyInfo[] allProps = collectionType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        await FixNullsOnField(string.Empty, allProps, collection, ct);

        PropertyInfo[] localeStringProps = typeof(LocaleString).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.PropertyType == typeof(string))
            .ToArray();
        PropertyInfo[] localeProps = collectionType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.PropertyType == typeof(LocaleString))
            .ToArray();
        foreach (PropertyInfo localeProp in localeProps)
            await FixNullsOnField(_namePolicy.ConvertName(localeProp.Name) + ".", localeStringProps, collection, ct);
    }

    private async Task FixNullsOnField
    (
        string namePrefix,
        IEnumerable<PropertyInfo> stringProps,
        IMongoCollection<BsonDocument> collection,
        CancellationToken ct
    )
    {
        foreach (PropertyInfo stringProp in stringProps)
        {
            string propName = namePrefix + _namePolicy.ConvertName(stringProp.Name);
            FieldDefinition<BsonDocument, string?> field = new StringFieldDefinition<BsonDocument, string?>(propName);

            FilterDefinition<BsonDocument> filter = new FilterDefinitionBuilder<BsonDocument>()
                .Eq(field, "NULL");
            UpdateDefinition<BsonDocument> updateBuilder = new UpdateDefinitionBuilder<BsonDocument>()
                .Set(field, null);

            await collection.UpdateManyAsync
            (
                filter,
                updateBuilder,
                null,
                ct
            );
        }
    }
}   
