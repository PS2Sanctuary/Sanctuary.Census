using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Common.Objects.DiffModels;
using Sanctuary.Census.Common.Services;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;

namespace Sanctuary.Census.Common.Extensions;

/// <summary>
/// Extension methods for the <see cref="IServiceCollection"/> type.
/// </summary>
public static class IServiceCollectionExtensions
{
    private static bool _registrationComplete;

    /// <summary>
    /// Adds common Sanctuary.Census services to the host.
    /// </summary>
    /// <param name="builder">The host builder.</param>
    /// <returns>The <see cref="IHostApplicationBuilder"/> instance, so that calls may be chained.</returns>
    public static IHostApplicationBuilder AddCommonServices(this IHostApplicationBuilder builder)
    {
        if (_registrationComplete)
            return builder;

        builder.Services.AddMemoryCache();

        builder.Services.TryAddSingleton<IFileSystem, FileSystem>();
        builder.Services.TryAddScoped<EnvironmentContextProvider>();

        // Register 
        DatabaseOptions dbOpts = new();
        builder.Configuration.GetSection(DatabaseOptions.CONFIG_KEY).Bind(dbOpts);
        MongoClientSettings mongoSettings = MongoClientSettings.FromConnectionString(dbOpts.ConnectionString);
        builder.Services.TryAddSingleton(new MongoClient(mongoSettings));
        builder.Services.TryAddScoped<IMongoContext, MongoContext>();
        RegisterCollectionClassMaps();

        builder.Services.AddHttpClient<Mandible.Abstractions.Manifest.IManifestService, Mandible.Manifest.ManifestService>();
        if (builder.Environment.IsProduction())
            builder.Services.TryAddScoped<IManifestService, CachingManifestService>();
        else
            builder.Services.TryAddScoped<IManifestService, DebugManifestService>();

        _registrationComplete = true;
        return builder;
    }

    private static void RegisterCollectionClassMaps()
    {
        BsonSerializer.RegisterSerializer(new DecimalSerializer(BsonType.Decimal128));
        BsonSerializer.RegisterSerializer(new EnumSerializer<FactionDefinition>(BsonType.String));

        IEnumerable<Type> collTypes = typeof(CollectionAttribute).Assembly
            .GetTypes()
            .Where(t => t.IsDefined(typeof(CollectionAttribute)));

        foreach (Type collType in collTypes)
            BsonClassMap.RegisterClassMap(MongoContext.AutoClassMap(collType));

        BsonClassMap.RegisterClassMap(MongoContext.AutoClassMap<CollectionDiffEntry>());
        BsonClassMap.RegisterClassMap(MongoContext.AutoClassMap<Datatype>());
        BsonClassMap.RegisterClassMap(MongoContext.AutoClassMap<DiffRecord>());
        BsonClassMap.RegisterClassMap(MongoContext.AutoClassMap<LocaleString>());
        BsonClassMap.RegisterClassMap(MongoContext.AutoClassMap<NewCollection>());
    }
}
