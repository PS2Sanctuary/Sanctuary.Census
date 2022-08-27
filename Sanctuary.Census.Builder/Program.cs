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
using Sanctuary.Census.PatchData.Extensions;
using Sanctuary.Census.ServerData.Internal.Extensions;
using Sanctuary.Census.ServerData.Internal.Objects;
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
                    .RegisterCollectionBuilders()
                    .AddHostedService<CollectionBuildWorker>();
            });

        await builder.Build().RunAsync();
    }

    private static IServiceCollection RegisterCollectionBuilders(this IServiceCollection services)
    {
        services.AddSingleton<ICollectionBuilderRepository>
        (
            s => s.GetRequiredService<IOptions<CollectionBuilderRepository>>().Value
        );

        return services.RegisterCollectionBuilder<CurrencyCollectionBuilder>()
            .RegisterCollectionBuilder<ExperienceCollectionBuilder>()
            .RegisterCollectionBuilder<FacilityInfoCollectionBuilder>()
            .RegisterCollectionBuilder<FacilityLinkCollectionBuilder>()
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
            .RegisterCollectionBuilder<MapRegionCollectionBuilder>()
            .RegisterCollectionBuilder<MarketingBundleCollectionBuilders>()
            .RegisterCollectionBuilder<OutfitWarCollectionsBuilder>()
            .RegisterCollectionBuilder<OutfitWarRegistrationCollectionBuilder>()
            .RegisterCollectionBuilder<PlayerStateGroup2CollectionBuilder>()
            .RegisterCollectionBuilder<ProfileCollectionBuilder>()
            .RegisterCollectionBuilder<ProjectileCollectionBuilder>()
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
