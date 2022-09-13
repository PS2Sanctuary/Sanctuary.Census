using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.CollectionBuilders;
using Sanctuary.Census.Builder.Database;
using Sanctuary.Census.Builder.Objects;
using Sanctuary.Census.Builder.Services;
using Sanctuary.Census.Builder.Workers;
using Sanctuary.Census.ClientData.Extensions;
using Sanctuary.Census.Common.Extensions;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.PatchData.Extensions;
using Sanctuary.Census.ServerData.Internal.Extensions;
using Sanctuary.Census.ServerData.Internal.Objects;
using Sanctuary.Common.Objects;
using Serilog;
using Serilog.Events;
using System.Threading.Tasks;

namespace Sanctuary.Census.Builder;

/// <summary>
/// The main class of the application.
/// </summary>
public static class Program
{
    /// <summary>
    /// The entry point of the application.
    /// </summary>
    /// <param name="args">Runtime arguments to be passed to the application.</param>
    public static async Task Main(string[] args)
    {
        IHostBuilder builder = Host.CreateDefaultBuilder(args)
            .UseSystemd()
            .ConfigureServices((context, _) =>
            {
                string? seqIngestionEndpoint = context.Configuration["LoggingOptions:SeqIngestionEndpoint"];
                string? seqApiKey = context.Configuration["LoggingOptions:SeqApiKey"];
                SetupLogger(seqIngestionEndpoint, seqApiKey);
            })
            .UseSerilog()
            .ConfigureServices((context, services) =>
            {
                services.Configure<BuildOptions>(context.Configuration.GetSection(nameof(BuildOptions)))
                    .Configure<LoginClientOptions>(context.Configuration.GetSection(nameof(LoginClientOptions)))
                    .Configure<GatewayClientOptions>(context.Configuration.GetSection(nameof(GatewayClientOptions)));

                services.AddCommonServices()
                    .AddClientDataServices()
                    .AddPatchDataServices()
                    .AddInternalServerDataServices()
                    .AddScoped<ICollectionsContext, CollectionsContext>()
                    .AddScoped<ICollectionDiffService, CollectionDiffService>()
                    .RegisterCollectionConfigurations()
                    .RegisterCollectionBuilders()
                    .AddHostedService<CollectionBuildWorker>();
            });

        await builder.Build().RunAsync().ConfigureAwait(false);
    }

    private static IServiceCollection RegisterCollectionConfigurations(this IServiceCollection services)
    {
        CollectionConfigurationProvider configProvider = new();
        services.AddSingleton(configProvider);

        configProvider.Register<Currency>()
            .WithIndex(x => x.CurrencyID, true)
            .WithEqualityKey(x => x.CurrencyID);

        configProvider.Register<Directive>()
            .WithIndex(x => x.DirectiveID, true)
            .WithIndex(x => x.DirectiveTreeID, false)
            .WithEqualityKey(x => x.DirectiveID);

        configProvider.Register<DirectiveTree>()
            .WithIndex(x => x.DirectiveTreeID, true)
            .WithIndex(x => x.DirectiveTreeCategoryID, false)
            .WithEqualityKey(x => x.DirectiveTreeID);

        configProvider.Register<DirectiveTier>()
            .WithIndex(x => x.DirectiveTreeID, false)
            .WithEqualityKey(x => x.DirectiveTreeID)
            .WithEqualityKey(x => x.DirectiveTierID);

        configProvider.Register<DirectiveTierReward>()
            .WithIndex(x => x.RewardSetID, false)
            .WithIndex(x => x.ItemID, false)
            .WithEqualityKey(x => x.RewardSetID)
            .WithEqualityKey(x => x.ItemID);

        configProvider.Register<DirectiveTierRewardSet>()
            .WithIndex(x => x.RewardSetID, true)
            .WithEqualityKey(x => x.RewardSetID);

        configProvider.Register<DirectiveTreeCategory>()
            .WithIndex(x => x.DirectiveTreeCategoryID, true)
            .WithEqualityKey(x => x.DirectiveTreeCategoryID);

        configProvider.Register<Experience>()
            .WithIndex(x => x.ExperienceID, true)
            .WithEqualityKey(x => x.ExperienceID);

        configProvider.Register<FacilityInfo>()
            .WithIndex(x => x.FacilityID, true)
            .WithIndex(x => x.ZoneID, false)
            .WithEqualityKey(x => x.FacilityID)
            .WithRemoveOldEntryTest
            (
                r => r.ZoneID is not (ushort)ZoneDefinition.Nexus
                    && r.ZoneID is not (ushort)ZoneDefinition.Tutorial2
            );

        configProvider.Register<FacilityLink>()
            .WithIndex(x => x.ZoneID, false)
            .WithIndex(x => x.FacilityIdA, false)
            .WithIndex(x => x.FacilityIdB, false)
            .WithEqualityKey(x => x.FacilityIdA)
            .WithEqualityKey(x => x.FacilityIdB)
            .WithRemoveOldEntryTest
            (
                r => r.ZoneID is not (uint)ZoneDefinition.Nexus
                    && r.ZoneID is not (uint)ZoneDefinition.Tutorial2
            );

        configProvider.Register<Faction>()
            .WithIndex(x => x.FactionID, true)
            .WithEqualityKey(x => x.FactionID);

        configProvider.Register<FireGroup>()
            .WithIndex(x => x.FireGroupID, true)
            .WithEqualityKey(x => x.FireGroupID);

        configProvider.Register<FireGroupToFireMode>()
            .WithIndex(x => x.FireGroupId, false)
            .WithIndex(x => x.FireModeId, false)
            .WithEqualityKey(x => x.FireGroupId)
            .WithEqualityKey(x => x.FireModeId)
            .WithEqualityKey(x => x.FireModeIndex);

        configProvider.Register<FireMode2>()
            .WithIndex(x => x.FireModeID, true)
            .WithEqualityKey(x => x.FireModeID);

        configProvider.Register<FireModeToProjectile>()
            .WithIndex(x => x.FireModeID, false)
            .WithIndex(x => x.ProjectileID, false)
            .WithEqualityKey(x => x.FireModeID)
            .WithEqualityKey(x => x.ProjectileID);

        configProvider.Register<ImageSet>()
            .WithIndex(x => x.ImageSetID, false)
            .WithIndex(x => x.ImageID, false)
            .WithEqualityKey(x => x.ImageSetID)
            .WithEqualityKey(x => x.ImageID);

        configProvider.Register<Item>()
            .WithIndex(x => x.ItemID, true)
            .WithIndex(x => x.Name!.En, false)
            .WithEqualityKey(x => x.ItemID);

        configProvider.Register<ItemCategory>()
            .WithIndex(x => x.ItemCategoryID, true)
            .WithEqualityKey(x => x.ItemCategoryID);

        configProvider.Register<ItemToWeapon>()
            .WithIndex(x => x.ItemId, false)
            .WithIndex(x => x.WeaponId, false)
            .WithEqualityKey(x => x.ItemId)
            .WithEqualityKey(x => x.WeaponId);

        configProvider.Register<Loadout>()
            .WithIndex(x => x.LoadoutID, true)
            .WithEqualityKey(x => x.LoadoutID);

        configProvider.Register<LoadoutSlot>()
            .WithIndex(x => x.LoadoutID, false)
            .WithIndex(x => x.SlotID, false)
            .WithEqualityKey(x => x.LoadoutID)
            .WithEqualityKey(x => x.SlotID);

        configProvider.Register<MapHex>()
            .WithIndex(x => x.MapRegionID, false)
            .WithIndex(x => x.ZoneID, false)
            .WithEqualityKey(x => x.ZoneID)
            .WithEqualityKey(x => x.X)
            .WithEqualityKey(x => x.Y)
            .WithRemoveOldEntryTest
            (
                r => r.ZoneID is not (uint)ZoneDefinition.Nexus
                    && r.ZoneID is not (uint)ZoneDefinition.Tutorial2
            );

        configProvider.Register<MapRegion>()
            .WithIndex(x => x.MapRegionId, true)
            .WithIndex(x => x.FacilityId, false)
            .WithEqualityKey(x => x.MapRegionId)
            .WithRemoveOldEntryTest
            (
                r => r.ZoneId is not (uint)ZoneDefinition.Nexus
                    && r.ZoneId is not (uint)ZoneDefinition.Tutorial2
            );

        configProvider.Register<MarketingBundle>()
            .WithIndex(x => x.MarketingBundleID, true)
            .WithIndex(x => x.MarketingBundleCategoryID, false)
            .WithIndex(x => x.Name.En, false)
            .WithIndex(x => x.IsOnSale, false)
            .WithEqualityKey(x => x.MarketingBundleID);

        configProvider.Register<MarketingBundleCategory>()
            .WithIndex(x => x.MarketingBundleCategoryID, true)
            .WithEqualityKey(x => x.MarketingBundleCategoryID);

        configProvider.Register<MarketingBundleItem>()
            .WithIndex(x => x.MarketingBundleID, false)
            .WithIndex(x => x.ItemID, false)
            .WithEqualityKey(x => x.MarketingBundleID)
            .WithEqualityKey(x => x.ItemID);

        configProvider.Register<OutfitWar>()
            .WithIndex(x => x.OutfitWarID, true)
            .WithEqualityKey(x => x.OutfitWarID)
            .WithRemoveOldEntryTest(_ => false);

        configProvider.Register<OutfitWarMatch>()
            .WithEqualityKey(x => x.MatchID)
            .WithRemoveOldEntryTest(_ => false);

        configProvider.Register<OutfitWarRanking>()
            .WithIndex(x => x.RoundID, false)
            .WithIndex(x => x.OutfitID, false)
            .WithEqualityKey(x => x.RoundID)
            .WithEqualityKey(x => x.OutfitID)
            .WithRemoveOldEntryTest(_ => false);

        configProvider.Register<OutfitWarRegistration>()
            .WithIndex(x => x.OutfitID, false)
            .WithIndex(x => x.WorldID, false)
            .WithEqualityKey(x => x.OutfitID)
            .WithRemoveOldEntryTest(_ => false);

        configProvider.Register<OutfitWarRounds>()
            .WithIndex(x => x.OutfitWarID, true)
            .WithIndex(x => x.PrimaryRoundID, true)
            .WithEqualityKey(x => x.OutfitWarID)
            .WithRemoveOldEntryTest(_ => false);

        configProvider.Register<PlayerStateGroup2>()
            .WithIndex(x => x.PlayerStateGroupId, false)
            .WithIndex(x => x.PlayerStateId, false)
            .WithEqualityKey(x => x.PlayerStateGroupId)
            .WithEqualityKey(x => x.PlayerStateId);

        configProvider.Register<Profile>()
            .WithIndex(x => x.ProfileId, true)
            .WithEqualityKey(x => x.ProfileId);

        configProvider.Register<Projectile>()
            .WithIndex(x => x.ProjectileId, true)
            .WithEqualityKey(x => x.ProjectileId);

        configProvider.Register<Skill>()
            .WithIndex(x => x.SkillID, true)
            .WithIndex(x => x.SkillSetID, false)
            .WithIndex(x => x.SkillCategoryID, false)
            .WithIndex(x => x.SkillLineID, false)
            .WithEqualityKey(x => x.SkillID);

        configProvider.Register<Vehicle>()
            .WithIndex(x => x.VehicleId, true)
            .WithEqualityKey(x => x.VehicleId);

        configProvider.Register<VehicleAttachment>()
            .WithIndex(x => x.ItemID, false)
            .WithIndex(x => x.VehicleID, false)
            .WithIndex(x => x.VehicleLoadoutID, false)
            .WithEqualityKey(x => x.ItemID)
            .WithEqualityKey(x => x.VehicleLoadoutID);

        configProvider.Register<VehicleLoadout>()
            .WithIndex(x => x.LoadoutID, true)
            .WithIndex(x => x.VehicleID, false)
            .WithEqualityKey(x => x.LoadoutID);

        configProvider.Register<VehicleLoadoutSlot>()
            .WithIndex(x => x.LoadoutID, false)
            .WithIndex(x => x.SlotID, false)
            .WithEqualityKey(x => x.LoadoutID)
            .WithEqualityKey(x => x.SlotID);

        configProvider.Register<VehicleSkillSet>()
            .WithIndex(x => x.SkillSetID, false)
            .WithIndex(x => x.VehicleID, false)
            .WithEqualityKey(x => x.FactionID)
            .WithEqualityKey(x => x.SkillSetID)
            .WithEqualityKey(x => x.VehicleID);

        configProvider.Register<Weapon>()
            .WithIndex(x => x.WeaponId, true)
            .WithEqualityKey(x => x.WeaponId);

        configProvider.Register<WeaponAmmoSlot>()
            .WithIndex(x => x.WeaponId, false)
            .WithEqualityKey(x => x.WeaponId)
            .WithEqualityKey(x => x.WeaponSlotIndex);

        configProvider.Register<WeaponToAttachment>()
            .WithIndex(x => x.AttachmentID, false)
            .WithIndex(x => x.ItemID, false)
            .WithEqualityKey(x => x.AttachmentID)
            .WithEqualityKey(x => x.ItemID);

        configProvider.Register<WeaponToFireGroup>()
            .WithIndex(x => x.FireGroupId, false)
            .WithIndex(x => x.WeaponId, false)
            .WithEqualityKey(x => x.FireGroupId)
            .WithEqualityKey(x => x.FireGroupIndex)
            .WithEqualityKey(x => x.WeaponId);

        configProvider.Register<World>()
            .WithIndex(x => x.WorldID, true)
            .WithEqualityKey(x => x.WorldID);

        configProvider.Register<Common.Objects.Collections.Zone>()
            .WithIndex(x => x.ZoneID, true)
            .WithEqualityKey(x => x.ZoneID)
            .WithRemoveOldEntryTest(_ => false);

        return services;
    }

    private static IServiceCollection RegisterCollectionBuilders(this IServiceCollection services)
    {
        services.AddSingleton<ICollectionBuilderRepository>
        (
            s => s.GetRequiredService<IOptions<CollectionBuilderRepository>>().Value
        );

        return services.RegisterCollectionBuilder<CurrencyCollectionBuilder>()
            .RegisterCollectionBuilder<DirectiveCollectionsBuilder>()
            .RegisterCollectionBuilder<ExperienceCollectionBuilder>()
            .RegisterCollectionBuilder<FacilityInfoCollectionBuilder>()
            .RegisterCollectionBuilder<FactionCollectionBuilder>()
            .RegisterCollectionBuilder<FireGroupCollectionBuilder>()
            .RegisterCollectionBuilder<FireGroupToFireModeCollectionBuilder>()
            .RegisterCollectionBuilder<FireModeCollectionBuilder>()
            .RegisterCollectionBuilder<FireModeToProjectileCollectionBuilder>()
            .RegisterCollectionBuilder<ImageSetCollectionBuilder>()
            .RegisterCollectionBuilder<ItemCollectionBuilder>()
            .RegisterCollectionBuilder<ItemCategoryCollectionBuilder>()
            .RegisterCollectionBuilder<ItemToWeaponCollectionBuilder>()
            .RegisterCollectionBuilder<LoadoutCollectionBuilder>()
            .RegisterCollectionBuilder<LoadoutSlotCollectionBuilder>()
            .RegisterCollectionBuilder<MapRegionDatasCollectionBuilder>()
            .RegisterCollectionBuilder<MarketingBundleCollectionBuilders>()
            .RegisterCollectionBuilder<OutfitWarCollectionsBuilder>()
            .RegisterCollectionBuilder<OutfitWarRegistrationCollectionBuilder>()
            .RegisterCollectionBuilder<PlayerStateGroup2CollectionBuilder>()
            .RegisterCollectionBuilder<ProfileCollectionBuilder>()
            .RegisterCollectionBuilder<ProjectileCollectionBuilder>()
            .RegisterCollectionBuilder<SkillCollectionsBuilder>()
            .RegisterCollectionBuilder<VehicleCollectionBuilder>()
            .RegisterCollectionBuilder<VehicleAttachmentCollectionBuilder>()
            .RegisterCollectionBuilder<VehicleLoadoutCollectionBuilder>()
            .RegisterCollectionBuilder<VehicleLoadoutSlotCollectionBuilder>()
            .RegisterCollectionBuilder<VehicleSkillSetCollectionBuilder>()
            .RegisterCollectionBuilder<WeaponCollectionBuilder>()
            .RegisterCollectionBuilder<WeaponAmmoSlotCollectionBuilder>()
            .RegisterCollectionBuilder<WeaponToAttachmentCollectionBuilder>()
            .RegisterCollectionBuilder<WeaponToFireGroupCollectionBuilder>()
            .RegisterCollectionBuilder<WorldCollectionBuilder>()
            .RegisterCollectionBuilder<ZoneCollectionBuilder>();
    }

    // ReSharper disable twice UnusedParameter.Local
    private static void SetupLogger(string? seqIngestionEndpoint, string? seqApiKey)
    {
        LoggerConfiguration loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Serilog.AspNetCore.RequestLoggingMiddleware", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console
            (
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
            );

#if !DEBUG
        if (!string.IsNullOrEmpty(seqIngestionEndpoint) && !string.IsNullOrEmpty(seqApiKey))
        {
            Serilog.Core.LoggingLevelSwitch levelSwitch = new();
            loggerConfig.MinimumLevel.ControlledBy(levelSwitch)
                .WriteTo.Seq(seqIngestionEndpoint, apiKey: seqApiKey, controlLevelSwitch: levelSwitch);
        }
#endif

        Log.Logger = loggerConfig.CreateLogger();
    }

    private static IServiceCollection RegisterCollectionBuilder<TBuilder>(this IServiceCollection services)
        where TBuilder : class, ICollectionBuilder
    {
        services.AddScoped<TBuilder>();
        services.Configure<CollectionBuilderRepository>(x => x.Register<TBuilder>());
        return services;
    }
}
