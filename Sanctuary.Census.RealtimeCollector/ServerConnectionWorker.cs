using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sanctuary.Census.RealtimeHub;
using Sanctuary.Census.ServerData.Internal.Objects;
using Sanctuary.Census.ServerData.Internal.Services;
using Sanctuary.Common.Objects;
using Sanctuary.Zone.Packets.MapRegion;
using Sanctuary.Zone.Packets.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.RealtimeCollector;

/// <summary>
/// A background worker responsible for processing continuous server updates
/// and dispatching relevant realtime data to the realtime hub.
/// </summary>
public class ServerConnectionWorker : BackgroundService
{
    private readonly ILogger<ServerConnectionWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ContinuousServerDataService _continousServerService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerConnectionWorker"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="scopeFactory">The service scope factory.</param>
    /// <param name="continousServerService">The continuous server data retrieval service.</param>
    public ServerConnectionWorker
    (
        ILogger<ServerConnectionWorker> logger,
        IServiceScopeFactory scopeFactory,
        ContinuousServerDataService continousServerService
    )
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _continousServerService = continousServerService;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using CancellationTokenSource internalCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        using Task processTask = ProcessUpdates(internalCts.Token);
        using Task runTask = _continousServerService.RunAsync(internalCts.Token);

        await Task.WhenAny(processTask, runTask);
        internalCts.Cancel();

        try
        {
            await Task.WhenAll(processTask, runTask);
        }
        catch (AggregateException aex)
        {
            foreach (Exception ex in aex.InnerExceptions)
                _logger.LogCritical(ex, "Failed to process continuous server updates");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to process continuous server updates");
        }
    }

    private async Task ProcessUpdates(CancellationToken ct)
    {
        try
        {
            await Task.Yield();
            MapRegionPopulation? lastPopulation = null;

            await foreach (RealtimeDataWrapper data in _continousServerService.DataOutputReader.ReadAllAsync(ct))
            {
                try
                {
                    // NOTE: Population is always sent before capture data

                    switch (data.Data)
                    {
                        case MapRegionPopulation mrp:
                        {
                            lastPopulation = mrp;
                            break;
                        }
                        case CaptureDataUpdateAll cdua when lastPopulation is not null:
                        {
                            if (cdua.ZoneID == lastPopulation.ZoneID && cdua.InstanceID == lastPopulation.InstanceID)
                                await ProcessMapCaptureDataAsync(data.Server, cdua, lastPopulation, ct);
                            lastPopulation = null;
                            break;
                        }
                        case ServerPopulationInfo spi:
                        {
                            await ProcessWorldPopulation(data.Server, spi, ct);
                            break;
                        }
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

    private async Task ProcessMapCaptureDataAsync
    (
        ServerDefinition server,
        CaptureDataUpdateAll captureData,
        MapRegionPopulation populationData,
        CancellationToken ct
    )
    {
        if (captureData.Regions.Length is 0)
            return;

        await using AsyncServiceScope serviceScope = _scopeFactory.CreateAsyncScope();
        RealtimeIngress.RealtimeIngressClient ingressClient = serviceScope.ServiceProvider
            .GetRequiredService<RealtimeIngress.RealtimeIngressClient>();

        MapStateUpdate mapStateUpdate = new()
        {
            Timestamp = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow),
            WorldId = (uint)server,
            ZoneId = (int)captureData.ZoneID,
            ZoneInstance = captureData.InstanceID
        };

        Dictionary<uint, MapRegionPopulation_Region> popRegions = populationData.Regions
            .ToDictionary(x => x.RegionID, x => x);

        foreach (CaptureDataUpdateAll_Region region in captureData.Regions)
        {
            MapRegionState regionState = new()
            {
                CaptureTimeMs = region.CaptureTimeMs,
                ContestingFactionId = (int)region.ContestingFactionId,
                CtfFlags = region.CtfFlags,
                MapRegionId = region.MapRegionId,
                OwningFactionId = (int)region.OwningFactionId,
                RemainingCaptureTimeMs = region.RemainingCaptureTimeMs,
                RemainingCtfFlags = region.RemainingCtfFlags
            };

            mapStateUpdate.Regions.Add(regionState);

            if (!popRegions.TryGetValue(region.MapRegionId, out MapRegionPopulation_Region? population))
                continue;

            for (int i = 0; i < population.FactionPopTiers.Length; i++)
            {
                int faction = i + 1;
                byte tier = population.FactionPopTiers[i];

                if (tier is 0)
                    regionState.FactionPopulationUpperBound.Add(faction, 0);
                else
                    regionState.FactionPopulationUpperBound.Add(faction, (int)(12 * Math.Pow(2, tier - 1)));
            }

            for (int i = 0; i < population.FactionPercentages.Length; i++)
            {
                float percentage = population.FactionPercentages[i] / (float)byte.MaxValue;
                regionState.FactionPopulationPercentage.Add(i + 1, percentage * 100);
            }
        }

        await ingressClient.SubmitMapStateUpdateAsync
        (
            mapStateUpdate,
            deadline: DateTime.UtcNow.AddSeconds(10),
            cancellationToken: ct
        );
    }

    private async Task ProcessWorldPopulation
    (
        ServerDefinition server,
        ServerPopulationInfo spi,
        CancellationToken ct
    )
    {
        await using AsyncServiceScope serviceScope = _scopeFactory.CreateAsyncScope();
        RealtimeIngress.RealtimeIngressClient ingressClient = serviceScope.ServiceProvider
            .GetRequiredService<RealtimeIngress.RealtimeIngressClient>();

        WorldPopulationUpdate worldPopUpdate = new()
        {
            Timestamp = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow),
            WorldId = (uint)server
        };

        for (int i = 0; i < spi.Population.Length; i++)
            worldPopUpdate.FactionPopulations.Add(i + 1, spi.Population[i]);

        await ingressClient.SubmitWorldPopulationUpdateAsync
        (
            worldPopUpdate,
            deadline: DateTime.UtcNow.AddSeconds(10),
            cancellationToken: ct
        );
    }
}
