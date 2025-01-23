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
using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Abstractions.Objects.RealtimeEvents;
using Sanctuary.Census.Common.Extensions;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.Common.Objects.RealtimeEvents;
using Sanctuary.Census.PatchData.Extensions;
using Sanctuary.Census.ServerData.Internal.Extensions;
using Sanctuary.Census.ServerData.Internal.Objects;
using Sanctuary.Common.Objects;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;

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
    public static void Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddSystemd();
        SetupLogger(builder);

        builder.Services.Configure<BuildOptions>(builder.Configuration.GetSection(nameof(BuildOptions)))
            .Configure<LoginClientOptions>(builder.Configuration.GetSection(nameof(LoginClientOptions)))
            .Configure<GatewayClientOptions>(builder.Configuration.GetSection(nameof(GatewayClientOptions)));

        builder.Services.AddCommonServices(builder.Environment)
            .AddClientDataServices(builder.Environment)
            .AddPatchDataServices(builder.Environment)
            .AddInternalServerDataServices(builder.Environment)
            .AddScoped<ICollectionsContext, CollectionsContext>()
            .AddScoped<ICollectionDiffService, GitCollectionDiffService>()
            .AddScoped<IImageSetHelperService, ImageSetHelperService>()
            .AddScoped<IRequirementsHelperService, RequirementsHelperService>()
            .RegisterCollectionConfigurations()
            .RegisterCollectionBuilders()
            .AddHostedService<CollectionBuildWorker>();

        builder.Build().Run();
    }

    private static IServiceCollection RegisterCollectionConfigurations(this IServiceCollection services)
    {
        CollectionConfigurationProvider configProvider = new();
        services.AddSingleton(configProvider);

        configProvider.Register<Ability>()
            .WithIndex(x => x.AbilityId, true)
            .WithEqualityKey(x => x.AbilityId);

        configProvider.Register<AbilitySet>()
            .WithIndex(x => x.AbilitySetId, false)
            .WithIndex(x => x.AbilityId, false)
            .WithEqualityKey(x => x.AbilitySetId)
            .WithEqualityKey(x => x.AbilityId);

        configProvider.Register<Currency>()
            .WithIndex(x => x.CurrencyID, true)
            .WithEqualityKey(x => x.CurrencyID);

        configProvider.Register<Directive>()
            .WithIndex(x => x.DirectiveID, true)
            .WithIndex(x => x.DirectiveTreeID, false)
            .WithEqualityKey(x => x.DirectiveID)
            .WithRemoveOldEntryTest(static _ => false);

        configProvider.Register<DirectiveTree>()
            .WithIndex(x => x.DirectiveTreeID, true)
            .WithIndex(x => x.DirectiveTreeCategoryID, false)
            .WithEqualityKey(x => x.DirectiveTreeID)
            .WithRemoveOldEntryTest(static _ => false);

        configProvider.Register<DirectiveTier>()
            .WithIndex(x => x.DirectiveTreeID, false)
            .WithEqualityKey(x => x.DirectiveTreeID)
            .WithEqualityKey(x => x.DirectiveTierID)
            .WithRemoveOldEntryTest(static _ => false);

        configProvider.Register<DirectiveTierReward>()
            .WithIndex(x => x.RewardSetID, false)
            .WithIndex(x => x.ItemID, false)
            .WithEqualityKey(x => x.RewardSetID)
            .WithEqualityKey(x => x.ItemID)
            .WithRemoveOldEntryTest(static _ => false);

        configProvider.Register<DirectiveTierRewardSet>()
            .WithIndex(x => x.RewardSetID, false)
            .WithEqualityKey(x => x.RewardSetID)
            .WithEqualityKey(x => x.FactionId)
            .WithRemoveOldEntryTest(static _ => false);

        configProvider.Register<DirectiveTreeCategory>()
            .WithIndex(x => x.DirectiveTreeCategoryID, true)
            .WithEqualityKey(x => x.DirectiveTreeCategoryID)
            .WithRemoveOldEntryTest(static _ => false);

        configProvider.Register<Experience>()
            .WithIndex(x => x.ExperienceID, true)
            .WithEqualityKey(x => x.ExperienceID);

        configProvider.Register<ExperienceRank>()
            .WithIndex(x => x.Id, true)
            .WithIndex(x => x.Rank, false)
            .WithIndex(x => x.PrestigeLevel, false)
            .WithEqualityKey(x => x.Id);

        configProvider.Register<FacilityInfo>()
            .WithIndex(x => x.FacilityID, true)
            .WithIndex(x => x.ZoneID, false)
            .WithEqualityKey(x => x.FacilityID)
            .WithRemoveOldEntryTest
            (
                static r => r.ZoneID is not (ushort)ZoneDefinition.Nexus
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
                static r => r.ZoneID is not (uint)ZoneDefinition.Nexus
                    && r.ZoneID is not (uint)ZoneDefinition.Tutorial2
            );

        configProvider.Register<FacilityType>()
            .WithIndex(x => x.FacilityTypeId, true)
            .WithEqualityKey(x => x.FacilityTypeId);

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

        configProvider.Register<Image>()
            .WithIndex(x => x.ImageID, true)
            .WithEqualityKey(x => x.ImageID);

        configProvider.Register<ImageSet>()
            .WithIndex(x => x.ImageSetID, false)
            .WithIndex(x => x.ImageID, false)
            .WithEqualityKey(x => x.ImageSetID)
            .WithEqualityKey(x => x.ImageID)
            .WithEqualityKey(x => x.TypeID);

        configProvider.Register<ImageSetDefault>()
            .WithIndex(x => x.ImageSetID, true)
            .WithIndex(x => x.ImageID, false)
            .WithEqualityKey(x => x.ImageSetID);

        configProvider.Register<Item>()
            .WithIndex(x => x.ItemID, true)
            .WithIndex(x => x.ItemCategoryID, false)
            .WithIndex(x => x.ItemClassId, false)
            .WithIndex(x => x.ItemTypeID, false)
            .WithIndex(x => x.Name!.En, false)
            .WithEqualityKey(x => x.ItemID);

        configProvider.Register<ItemAttachment>()
            .WithIndex(x => x.ItemId, false)
            .WithIndex(x => x.AttachmentItemId, false)
            .WithEqualityKey(x => x.ItemId)
            .WithEqualityKey(x => x.AttachmentItemId);

        configProvider.Register<ItemCategory>()
            .WithIndex(x => x.ItemCategoryID, true)
            .WithEqualityKey(x => x.ItemCategoryID);

        configProvider.Register<ItemToItemLine>()
            .WithIndex(x => x.ItemId, true)
            .WithIndex(x => x.ItemLineId, false)
            .WithEqualityKey(x => x.ItemId)
            .WithEqualityKey(x => x.ItemLineId);

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

        configProvider.Register<LoadoutSlotToItemClass>()
            .WithIndex(x => x.LoadoutId, false)
            .WithIndex(x => x.SlotId, false)
            .WithIndex(x => x.ItemClassId, false)
            .WithEqualityKey(x => x.LoadoutId)
            .WithEqualityKey(x => x.SlotId)
            .WithEqualityKey(x => x.ItemClassId);

        configProvider.Register<MapHex>()
            .WithIndex(x => x.MapRegionID, false)
            .WithIndex(x => x.ZoneID, false)
            .WithEqualityKey(x => x.ZoneID)
            .WithEqualityKey(x => x.X)
            .WithEqualityKey(x => x.Y)
            .WithRemoveOldEntryTest
            (
                static r => r.ZoneID is not (uint)ZoneDefinition.Nexus
                    && r.ZoneID is not (uint)ZoneDefinition.Tutorial2
            );

        configProvider.Register<MapRegion>()
            .WithIndex(x => x.MapRegionId, true)
            .WithIndex(x => x.FacilityId, false)
            .WithEqualityKey(x => x.MapRegionId)
            .WithRemoveOldEntryTest
            (
                static r => r.ZoneId is not (uint)ZoneDefinition.Nexus
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

        configProvider.Register<NoDeployArea>()
            .WithIndex(x => x.AreaID, true)
            .WithIndex(x => x.FacilityID, false)
            .WithEqualityKey(x => x.AreaID);

        configProvider.Register<Oops>()
            .WithEqualityKey(x => x.OopsId);

        configProvider.Register<OutfitWar>()
            .WithIndex(x => x.OutfitWarID, true)
            .WithEqualityKey(x => x.OutfitWarID)
            .WithRemoveOldEntryTest(static _ => false);

        configProvider.Register<OutfitWarMatch>()
            .WithIndex(x => x.OutfitWarID, false)
            .WithIndex(x => x.RoundID, false)
            .WithEqualityKey(x => x.MatchID)
            .WithRemoveOldEntryTest(static _ => false);

        configProvider.Register<OutfitWarRanking>()
            .WithIndex(x => x.RoundID, false)
            .WithIndex(x => x.OutfitID, false)
            .WithEqualityKey(x => x.RoundID)
            .WithEqualityKey(x => x.OutfitID)
            .WithRemoveOldEntryTest(static _ => false);

        configProvider.Register<OutfitWarRegistration>()
            .WithIndex(x => x.OutfitID, false)
            .WithIndex(x => x.WorldID, false)
            .WithEqualityKey(x => x.OutfitID)
            .WithRemoveOldEntryTest(static _ => false);

        configProvider.Register<OutfitWarRound>()
            .WithIndex(x => x.RoundID, true)
            .WithIndex(x => x.OutfitWarID, false)
            .WithEqualityKey(x => x.RoundID)
            .WithRemoveOldEntryTest(static _ => false);

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

        configProvider.Register<ResistInfo>()
            .WithIndex(x => x.ResistInfoId, true)
            .WithIndex(x => x.ResistTypeId, false)
            .WithEqualityKey(x => x.ResistInfoId);

        configProvider.Register<Resource>()
            .WithIndex(x => x.ResourceId, true)
            .WithIndex(x => x.ResourceTypeId, false)
            .WithEqualityKey(x => x.ResourceId);

        configProvider.Register<ResourceType>()
            .WithIndex(x => x.ResourceTypeId, true)
            .WithEqualityKey(x => x.ResourceTypeId);

        configProvider.Register<Skill>()
            .WithIndex(x => x.SkillID, true)
            .WithIndex(x => x.SkillSetID, false)
            .WithIndex(x => x.SkillCategoryID, false)
            .WithIndex(x => x.SkillLineID, false)
            .WithEqualityKey(x => x.SkillID);

        configProvider.Register<SkillCategory>()
            .WithIndex(x => x.SkillCategoryID, true)
            .WithIndex(x => x.SkillSetID, false)
            .WithEqualityKey(x => x.SkillCategoryID);

        configProvider.Register<SkillLine>()
            .WithIndex(x => x.SkillLineID, true)
            .WithIndex(x => x.SkillSetID, false)
            .WithIndex(x => x.SkillCategoryID, false)
            .WithEqualityKey(x => x.SkillLineID);

        configProvider.Register<SkillSet>()
            .WithIndex(x => x.SkillSetID, true)
            .WithEqualityKey(x => x.SkillSetID);

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

        configProvider.Register<VehicleLoadoutSlotToItemClass>()
            .WithIndex(x => x.LoadoutId, false)
            .WithIndex(x => x.SlotId, false)
            .WithIndex(x => x.ItemClassId, false)
            .WithEqualityKey(x => x.LoadoutId)
            .WithEqualityKey(x => x.SlotId)
            .WithEqualityKey(x => x.ItemClassId);

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
            .WithRemoveOldEntryTest(static _ => false);

        configProvider.Register<ZonePopulationLimits>()
            .WithIndex(x => x.WorldId, false)
            .WithIndex(x => x.ZoneId, false)
            .WithEqualityKey(x => x.WorldId)
            .WithEqualityKey(x => x.ZoneId)
            .WithRemoveOldEntryTest(static _ => false);

        configProvider.Register<ZoneSetMapping>()
            .WithIndex(x => x.ID, true)
            .WithEqualityKey(x => x.ID);

        // Realtime registration

        RegisterRealtime<MapState>(configProvider)
            .WithIndex(x => x.WorldId, false)
            .WithIndex(x => x.ZoneId, false)
            .WithIndex(x => x.MapRegionId, false)
            .WithEqualityKey(x => x.ZoneId)
            .WithEqualityKey(x => x.ZoneInstance)
            .WithEqualityKey(x => x.MapRegionId);

        RegisterRealtime<WorldPopulation>(configProvider)
            .WithIndex(x => x.WorldId, true);

        RegisterRealtime<ZonePopulation>(configProvider)
            .WithIndex(x => x.WorldId, false)
            .WithIndex(x => x.ZoneId, false)
            .WithEqualityKey(x => x.ZoneId)
            .WithEqualityKey(x => x.ZoneInstance);

        return services;
    }

    private static CollectionDbConfiguration<TCollection> RegisterRealtime<TCollection>
    (
        CollectionConfigurationProvider configProvider
    ) where TCollection : IRealtimeEvent, ISanctuaryCollection
    {
        return configProvider.Register<TCollection>()
            .WithEqualityKey(x => x.WorldId)
            .WithRemoveOldEntryTest
            (
                static x => DateTimeOffset.FromUnixTimeSeconds(x.Timestamp)
                    .Add(TimeSpan.FromMinutes(10)) < DateTimeOffset.UtcNow
            )
            .IsDynamic();
    }

    private static IServiceCollection RegisterCollectionBuilders(this IServiceCollection services)
    {
        services.AddSingleton<ICollectionBuilderRepository>
        (
            s => s.GetRequiredService<IOptions<CollectionBuilderRepository>>().Value
        );

        return services.RegisterCollectionBuilder<AbilityCollectionsBuilder>()
            .RegisterCollectionBuilder<AreaCollectionsBuilder>()
            .RegisterCollectionBuilder<CurrencyCollectionBuilder>()
            .RegisterCollectionBuilder<DirectiveCollectionsBuilder>()
            .RegisterCollectionBuilder<ExperienceCollectionBuilder>()
            .RegisterCollectionBuilder<ExperienceRankCollectionBuilder>()
            .RegisterCollectionBuilder<FacilityInfoCollectionBuilder>()
            .RegisterCollectionBuilder<FactionCollectionBuilder>()
            .RegisterCollectionBuilder<FireGroupCollectionBuilder>()
            .RegisterCollectionBuilder<FireGroupToFireModeCollectionBuilder>()
            .RegisterCollectionBuilder<FireModeCollectionBuilder>()
            .RegisterCollectionBuilder<FireModeToProjectileCollectionBuilder>()
            .RegisterCollectionBuilder<ImageCollectionsBuilder>()
            //.RegisterCollectionBuilder<ItemAttachmentCollectionBuilder>() Removed as the built data is not accurate
            .RegisterCollectionBuilder<ItemCollectionBuilder>()
            .RegisterCollectionBuilder<ItemCategoryCollectionBuilder>()
            .RegisterCollectionBuilder<ItemToItemLineCollectionBuilder>()
            .RegisterCollectionBuilder<ItemToWeaponCollectionBuilder>()
            .RegisterCollectionBuilder<LoadoutCollectionBuilder>()
            .RegisterCollectionBuilder<LoadoutSlotCollectionBuilder>()
            .RegisterCollectionBuilder<LoadoutSlotToItemClassCollectionsBuilder>()
            .RegisterCollectionBuilder<MapRegionDatasCollectionBuilder>()
            .RegisterCollectionBuilder<MarketingBundleCollectionBuilders>()
            .RegisterCollectionBuilder<OopsCollectionBuilder>()
            .RegisterCollectionBuilder<OutfitWarCollectionsBuilder>()
            .RegisterCollectionBuilder<PlayerStateGroup2CollectionBuilder>()
            .RegisterCollectionBuilder<ProfileCollectionBuilder>()
            .RegisterCollectionBuilder<ProjectileCollectionBuilder>()
            .RegisterCollectionBuilder<ResistInfoCollectionBuilder>()
            .RegisterCollectionBuilder<ResourceCollectionsBuilder>()
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
            .RegisterCollectionBuilder<ZoneCollectionBuilder>()
            .RegisterCollectionBuilder<ZonePopulationLimitCollectionBuilder>();
    }

    // ReSharper disable twice UnusedParameter.Local
    private static void SetupLogger(HostApplicationBuilder builder)
    {
        string? seqIngestionEndpoint = builder.Configuration["LoggingOptions:SeqIngestionEndpoint"];
        string? seqApiKey = builder.Configuration["LoggingOptions:SeqApiKey"];

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

        bool useSeq = builder.Environment.IsProduction()
            && !string.IsNullOrEmpty(seqIngestionEndpoint)
            && !string.IsNullOrEmpty(seqApiKey);
        if (useSeq)
        {
            LoggingLevelSwitch levelSwitch = new();
            loggerConfig.MinimumLevel.ControlledBy(levelSwitch)
                .WriteTo.Seq(seqIngestionEndpoint!, apiKey: seqApiKey, controlLevelSwitch: levelSwitch);
        }

        Log.Logger = loggerConfig.CreateLogger();
        builder.Services.AddSerilog();
    }

    private static IServiceCollection RegisterCollectionBuilder<TBuilder>(this IServiceCollection services)
        where TBuilder : class, ICollectionBuilder
    {
        services.AddScoped<TBuilder>();
        services.Configure<CollectionBuilderRepository>(x => x.Register<TBuilder>());
        return services;
    }
}
