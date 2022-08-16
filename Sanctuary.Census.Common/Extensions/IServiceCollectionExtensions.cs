using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Common.Objects.DiffModels;
using Sanctuary.Census.Common.Services;
using System.IO.Abstractions;

namespace Sanctuary.Census.Common.Extensions;

/// <summary>
/// Extension methods for the <see cref="IServiceCollection"/> type.
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Adds services common Sanctuary.Census services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance, so that calls may be chained.</returns>
    public static IServiceCollection AddCommonServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IFileSystem, FileSystem>();
        services.TryAddScoped<EnvironmentContextProvider>();

        services.AddSingleton(new MongoClient("mongodb://localhost:27017"))
            .AddScoped<IMongoContext, MongoContext>();
        RegisterCollectionClassMaps();

        services.AddHttpClient(nameof(ManifestService));
#if DEBUG
        services.TryAddSingleton<IManifestService, DebugManifestService>();
#else
        services.TryAddSingleton<IManifestService, CachingManifestService>();
#endif

        return services;
    }

    private static void RegisterCollectionClassMaps()
    {
        BsonClassMap.RegisterClassMap<Currency>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<Experience>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<FacilityInfo>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<FacilityLink>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<Faction>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<FireGroup>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<FireGroupToFireMode>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<FireMode2>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<FireModeToProjectile>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<Item>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<ItemCategory>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<ItemToWeapon>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<Loadout>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<LoadoutSlot>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<MapRegion>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<OutfitWar>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<OutfitWarRegistration>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<OutfitWarRounds>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<PlayerStateGroup2>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<Profile>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<Projectile>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<Vehicle>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<VehicleAttachment>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<VehicleLoadout>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<VehicleLoadoutSlot>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<VehicleSkillSet>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<Weapon>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<WeaponAmmoSlot>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<WeaponToAttachment>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<WeaponToFireGroup>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<World>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<Zone>(MongoContext.AutoClassMap);

        BsonClassMap.RegisterClassMap<Datatype>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<LocaleString>(MongoContext.AutoClassMap);
        BsonClassMap.RegisterClassMap<NewCollection>(MongoContext.AutoClassMap);
    }
}
