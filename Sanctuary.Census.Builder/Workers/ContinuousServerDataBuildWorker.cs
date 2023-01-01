using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.Common.Services;
using Sanctuary.Census.ServerData.Internal.Objects;
using Sanctuary.Census.ServerData.Internal.Services;
using Sanctuary.Common.Objects;
using Sanctuary.Zone.Packets.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using FactionDefinition = Sanctuary.Census.Common.Objects.CommonModels.FactionDefinition;

namespace Sanctuary.Census.Builder.Workers;

/// <summary>
/// A service worker responsible for building collections that rely on continuous
/// server data.
/// </summary>
public sealed class ContinuousServerDataBuildWorker : BackgroundService
{
    private readonly ILogger<ContinuousServerDataBuildWorker> _logger;
    private readonly ContinuousServerDataService _continuousServiceService;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly string _factionLimitsStoragePath;

    private Dictionary<ServerDefinition, Dictionary<ZoneDefinition, ushort[]>> _factionLimits;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContinuousServerDataBuildWorker"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="commonOptions">The common options to use.</param>
    /// <param name="continuousServerService">The continuous server data service.</param>
    /// <param name="serviceScopeFactory">The service provider.</param>
    public ContinuousServerDataBuildWorker
    (
        ILogger<ContinuousServerDataBuildWorker> logger,
        IOptions<CommonOptions> commonOptions,
        ContinuousServerDataService continuousServerService,
        IServiceScopeFactory serviceScopeFactory
    )
    {
        _logger = logger;
        _continuousServiceService = continuousServerService;
        _serviceScopeFactory = serviceScopeFactory;

        _factionLimits = new Dictionary<ServerDefinition, Dictionary<ZoneDefinition, ushort[]>>();
        _factionLimitsStoragePath = Path.Combine
        (
            commonOptions.Value.AppDataDirectory,
            "continuousPopZoneFactionLimits.json"
        );
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await using FileStream fs = new(_factionLimitsStoragePath, FileMode.Open, FileAccess.Read);
        _factionLimits = await JsonSerializer.DeserializeAsync<Dictionary<ServerDefinition, Dictionary<ZoneDefinition, ushort[]>>>
        (
            fs,
            cancellationToken: ct
        ) ?? new Dictionary<ServerDefinition, Dictionary<ZoneDefinition, ushort[]>>();

        // Don't overload LaunchPad on first-time run
        await Task.Delay(TimeSpan.FromSeconds(30), ct);

        await Task.WhenAny
        (
            ProcessUpdates(_continuousServiceService.DataOutputReader, ct),
            _continuousServiceService.RunAsync(ct)
        );
    }

    /// <inheritdoc />
    public override async Task StopAsync(CancellationToken ct)
    {
        await using FileStream fs = new(_factionLimitsStoragePath, FileMode.Create, FileAccess.Write);
        await JsonSerializer.SerializeAsync(fs, _factionLimits, cancellationToken: ct);

        await base.StopAsync(ct);
    }

    private async Task ProcessUpdates(ChannelReader<ContinuousDataBundle> reader, CancellationToken ct)
    {
        try
        {
            await Task.Yield();

            await foreach (ContinuousDataBundle bundle in reader.ReadAllAsync(ct))
            {
                try
                {
                    _logger.LogDebug("Received continuous data bundle for {Server}", bundle.Server);

                    await using AsyncServiceScope serviceScope = _serviceScopeFactory.CreateAsyncScope();
                    IServiceProvider services = serviceScope.ServiceProvider;
                    services.GetRequiredService<EnvironmentContextProvider>().Environment = PS2Environment.PS2;
                    ICollectionsContext dbContext = services.GetRequiredService<ICollectionsContext>();

                    if (bundle.ServerPopulation is not null)
                    {
                        await ProcessWorldPopulation(bundle.Server, bundle.ServerPopulation, dbContext, ct)
                            .ConfigureAwait(false);
                    }

                    if (bundle.ContinentInfo is not null)
                    {
                        await ProcessZonePopulation(bundle.Server, bundle.ContinentInfo, dbContext, ct)
                            .ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    // This is fine
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process continuous data bundle");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // This is fine
        }
    }

    private static async Task ProcessWorldPopulation
    (
        ServerDefinition server,
        ServerPopulationInfo spi,
        ICollectionsContext dbContext,
        CancellationToken ct
    )
    {
        ValueEqualityDictionary<FactionDefinition, int> pops = new();
        int total = 0;

        for (int i = 0; i < spi.Population.Length; i++)
        {
            ushort value = spi.Population[i];
            pops[(FactionDefinition)i + 1] = value;
            total += value;
        }

        WorldPopulation worldPop = new
        (
            (uint)server,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            total,
            pops
        );

        await dbContext.UpsertCollectionAsync(new[] { worldPop }, ct).ConfigureAwait(false);
    }

    private async Task ProcessZonePopulation
    (
        ServerDefinition server,
        ContinentBattleInfo cbi,
        ICollectionsContext dbContext,
        CancellationToken ct
    )
    {
        List<ZonePopulation> zonePops = new(cbi.Zones.Length);

        foreach (ZonePopulationInfo zone in cbi.Zones)
        {
            if (zone.Population.All(x => x == 0))
            {
                _factionLimits.TryAdd(server, new Dictionary<ZoneDefinition, ushort[]>());
                _factionLimits[server][zone.ZoneID] = zone.RemainingCharacterLimit;
            }

            if (!_factionLimits.ContainsKey(server))
                continue;
            if (!_factionLimits[server].TryGetValue(zone.ZoneID, out ushort[]? zoneFactionLimits))
                continue;

            ValueEqualityDictionary<FactionDefinition, int> pops = new();
            int total = 0;

            for (int i = 0; i < 3; i++)
            {
                int value = zoneFactionLimits[i] - zone.RemainingCharacterLimit[i];
                pops[(FactionDefinition)i + 1] = value;
                total += value;
            }

            zonePops.Add(new ZonePopulation
            (
                (uint)server,
                (ushort)zone.ZoneID,
                zone.Instance,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                total,
                pops
            ));
        }

        await dbContext.UpsertCollectionAsync(zonePops, ct).ConfigureAwait(false);
    }
}
