using Microsoft.Extensions.Logging;
using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Common.Objects;
using Sanctuary.Zone.Packets.MapRegion;
using Sanctuary.Zone.Packets.StaticFacilityInfo;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FacilityInfo = Sanctuary.Zone.Packets.StaticFacilityInfo.FacilityInfo;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="FacilityLink"/>, <see cref="MapHex"/> and <see cref="MapRegion"/> collections.
/// </summary>
public class MapRegionDatasCollectionBuilder : ICollectionBuilder
{
    private readonly ILogger<MapRegionDatasCollectionBuilder> _logger;
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IServerDataCacheService _serverDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="MapRegionDatasCollectionBuilder"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    public MapRegionDatasCollectionBuilder
    (
        ILogger<MapRegionDatasCollectionBuilder> logger,
        ILocaleDataCacheService localeDataCache,
        IServerDataCacheService serverDataCache
    )
    {
        _logger = logger;
        _localeDataCache = localeDataCache;
        _serverDataCache = serverDataCache;
    }

    /// <inheritdoc />
    public async Task BuildAsync(ICollectionsContext dbContext, CancellationToken ct = default)
    {
        if (_serverDataCache.MapRegionDatas.Count == 0)
            throw new MissingCacheDataException(typeof(MapRegionData));

        await UpsertHexesAsync(dbContext, ct).ConfigureAwait(false);

        Dictionary<uint, MapRegion>? regions = await UpsertRegionsAsync(dbContext, ct)
            .ConfigureAwait(false);

        if (regions is not null)
            await UpsertLatticeLinksAsync(dbContext, regions, ct).ConfigureAwait(false);
        else
            _logger.LogError("Cannot build FacilityLinks collection - map region build failed");

    }

    private async Task<Dictionary<uint, MapRegion>?> UpsertRegionsAsync
    (
        ICollectionsContext dbContext,
        CancellationToken ct
    )
    {
        try
        {
            if (_serverDataCache.StaticFacilityInfos is null)
                throw new MissingCacheDataException(typeof(StaticFacilityInfoAllZones));

            Dictionary<uint, FacilityInfo> facilityInfos = new();
            foreach (FacilityInfo fac in _serverDataCache.StaticFacilityInfos.Facilities)
                facilityInfos.TryAdd(fac.FacilityID, fac);

            Dictionary<uint, MapRegion> builtRegions = new();

            foreach ((ZoneDefinition zone, MapRegionData regionData) in _serverDataCache.MapRegionDatas)
            {
                foreach (MapRegionData_Region region in regionData.Regions)
                {
                    _localeDataCache.TryGetLocaleString(region.FacilityNameID, out LocaleString? name);
                    facilityInfos.TryGetValue(region.FacilityID, out FacilityInfo? facility);

                    builtRegions.TryAdd(region.MapRegionID_1, new MapRegion
                    (
                        region.MapRegionID_1,
                        (uint)zone,
                        region.FacilityID == 0 ? null : region.FacilityID,
                        name?.En,
                        name,
                        region.FacilityTypeID,
                        FacilityTypeToName(region.FacilityTypeID),
                        facility is null ? null : new decimal(facility.LocationX),
                        facility is null ? null : new decimal(facility.LocationY),
                        facility is null ? null : new decimal(facility.LocationZ)
                    ));
                }
            }

            await dbContext.UpsertCollectionAsync(builtRegions.Values, ct).ConfigureAwait(false);
            return builtRegions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert the MapRegion collection");
            return null;
        }
    }

    private async Task UpsertLatticeLinksAsync
    (
        ICollectionsContext dbContext,
        IReadOnlyDictionary<uint, MapRegion> regions,
        CancellationToken ct
    )
    {
        try
        {
            List<FacilityLink> builtLinks = new();

            foreach ((ZoneDefinition zone, MapRegionData regionData) in _serverDataCache.MapRegionDatas)
            {
                foreach (MapRegionData_LatticeLink link in regionData.LatticeLinks)
                {
                    MapRegion regionA = regions[link.MapRegionIDA];
                    MapRegion regionB = regions[link.MapRegionIDB];

                    if (regionA.FacilityId is null || regionB.FacilityId is null)
                        continue;

                    builtLinks.Add(new FacilityLink
                    (
                        (uint)zone,
                        (int)regionA.FacilityId,
                        (int)regionB.FacilityId,
                        $"{zone} - {regionA.FacilityName} to {regionB.FacilityName}"
                    ));
                }
            }

            await dbContext.UpsertCollectionAsync(builtLinks, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert the FacilityLink collection");
        }
    }

    private async Task UpsertHexesAsync
    (
        ICollectionsContext dbContext,
        CancellationToken ct
    )
    {
        try
        {
            List<MapHex> builtHexes = new();

            foreach ((ZoneDefinition zone, MapRegionData regionData) in _serverDataCache.MapRegionDatas)
            {
                foreach (MapRegionData_Hex hex in regionData.Hexes)
                {
                    builtHexes.Add(new MapHex
                    (
                        (uint)zone,
                        hex.MapRegionID,
                        hex.X,
                        hex.Y,
                        hex.HexType,
                        HexTypeToName(hex.HexType)
                    ));
                }
            }

            await dbContext.UpsertCollectionAsync(builtHexes, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert the MapHex collection");
        }
    }

    private static string FacilityTypeToName(byte facilityType)
        => facilityType switch
        {
            1 => "Default",
            2 => "Amp Station",
            3 => "Bio Lab",
            4 => "Tech Plant",
            5 => "Large Outpost",
            6 => "Small Outpost",
            7 => "Warpgate",
            8 => "Interlink Facility",
            9 => "Construction Outpost",
            10 => "Relic Outpost (Desolation)",
            11 => "Containment Site",
            12 => "Trident",
            13 => "Seapost",
            _ => "Unknown"
        };

    private static string HexTypeToName(byte hexType)
        => hexType switch
        {
            0 => "Unrestricted access",
            2 => "Restricted by faction",
            _ => "Unknown"
        };
}
