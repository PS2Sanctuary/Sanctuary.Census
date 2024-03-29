﻿using Microsoft.Extensions.Logging;
using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.PatchData.Abstractions.Services;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Common.Objects;
using Sanctuary.Zone.Packets.MapRegion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="FacilityLink"/>, <see cref="MapHex"/> and <see cref="MapRegion"/> collections.
/// </summary>
public class MapRegionDatasCollectionBuilder : ICollectionBuilder
{
    private readonly ILogger<MapRegionDatasCollectionBuilder> _logger;
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IServerDataCacheService _serverDataCache;
    private readonly IPatchDataCacheService _patchDataCache;
    private readonly IImageSetHelperService _imageSetHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="MapRegionDatasCollectionBuilder"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    /// <param name="patchDataCache">The patch data cache.</param>
    /// <param name="imageSetHelper">The image set helper service.</param>
    public MapRegionDatasCollectionBuilder
    (
        ILogger<MapRegionDatasCollectionBuilder> logger,
        ILocaleDataCacheService localeDataCache,
        IServerDataCacheService serverDataCache,
        IPatchDataCacheService patchDataCache,
        IImageSetHelperService imageSetHelper
    )
    {
        _logger = logger;
        _localeDataCache = localeDataCache;
        _serverDataCache = serverDataCache;
        _patchDataCache = patchDataCache;
        _imageSetHelper = imageSetHelper;
    }

    /// <inheritdoc />
    public async Task BuildAsync(ICollectionsContext dbContext, CancellationToken ct = default)
    {
        CheckForRequiredZonesHelper(_serverDataCache.MapRegionDatas);

        await UpsertHexesAsync(dbContext, ct).ConfigureAwait(false);

        Dictionary<uint, MapRegion>? regions = await UpsertRegionsAsync(dbContext, ct)
            .ConfigureAwait(false);

        if (regions is not null)
            await UpsertLatticeLinksAsync(dbContext, regions, ct).ConfigureAwait(false);
        else
            _logger.LogError("Cannot build FacilityLinks collection - map region build failed");

        await UpsertFacilityTypesAsync(dbContext, ct).ConfigureAwait(false);
    }

    private async Task<Dictionary<uint, MapRegion>?> UpsertRegionsAsync
    (
        ICollectionsContext dbContext,
        CancellationToken ct
    )
    {
        try
        {
            CheckForRequiredZonesHelper(_serverDataCache.MapRegionExternalFacilityData);
            CheckForRequiredZonesHelper(_serverDataCache.OutfitWarFacilityRewards);

            Dictionary<uint, MapRegion_ExternalFacilityData_Facility> regionFacilityInfos = new();
            foreach (ExternalFacilityData externalFacilityData in _serverDataCache.MapRegionExternalFacilityData.Values)
            {
                foreach (MapRegion_ExternalFacilityData_Facility facility in externalFacilityData.Facilities)
                    regionFacilityInfos.TryAdd(facility.MapRegionId, facility);
            }

            Dictionary<byte, string> facilityTypeDescriptions = new();
            if (_patchDataCache.FacilityTypes is not null)
            {
                foreach (FacilityType facType in _patchDataCache.FacilityTypes)
                    facilityTypeDescriptions.Add(facType.FacilityTypeId, facType.Description);
            }

            Dictionary<uint, (OutfitResource, uint)> facilityOutfitRewards = _serverDataCache.OutfitWarFacilityRewards
                .SelectMany(x => x.Value.Facilities)
                .ToDictionary(x => x.FacilityId, x => (x.ResourceId, x.RewardAmount));

            Dictionary<uint, MapRegionData_Reward> facilityRewards = _serverDataCache.MapRegionDatas.Values
                .SelectMany(x => x.RegionRewards)
                .ToDictionary(x => x.MapRegionID, x => x);

            Dictionary<uint, MapRegion> builtRegions = new();

            foreach ((ZoneDefinition zone, MapRegionData regionData) in _serverDataCache.MapRegionDatas)
            {
                if (regionData.Regions.Length == 0)
                    throw new Exception("No region data present for " + zone);

                foreach (MapRegionData_Region region in regionData.Regions)
                {
                    ct.ThrowIfCancellationRequested();

                    _localeDataCache.TryGetLocaleString(region.FacilityNameID, out LocaleString? name);
                    regionFacilityInfos.TryGetValue(region.MapRegionID_1, out MapRegion_ExternalFacilityData_Facility? facility);
                    facilityTypeDescriptions.TryGetValue(region.FacilityTypeID, out string? facTypeDescription);

                    MapRegion.OutfitResourceReward? owReward = null;
                    if (facilityOutfitRewards.TryGetValue(region.FacilityID, out (OutfitResource, uint) reward))
                    {
                        owReward = new MapRegion.OutfitResourceReward
                        (
                            OutfitResourceToName(reward.Item1),
                            (int)reward.Item2
                        );
                    }

                    bool hasDefaultImage = false;
                    uint defaultImage = 0;
                    if (facility is not null)
                        hasDefaultImage = _imageSetHelper.TryGetDefaultImage(facility.IconImageSetId, out defaultImage);

                    facilityRewards.TryGetValue(region.MapRegionID_1, out MapRegionData_Reward? facilityReward);

                    builtRegions.TryAdd(region.MapRegionID_1, new MapRegion
                    (
                        region.MapRegionID_1,
                        (uint)zone,
                        region.FacilityID == 0 ? null : region.FacilityID,
                        name?.En,
                        name,
                        region.FacilityTypeID,
                        facTypeDescription,
                        facility is null ? null : new decimal(facility.LocationX),
                        facility is null ? null : new decimal(facility.LocationY),
                        facility is null ? null : new decimal(facility.LocationZ),
                        owReward,
                        facility?.IconImageSetId,
                        hasDefaultImage ? defaultImage : null,
                        hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null,
                        facilityReward?.RewardCurrencyID,
                        (int?)facilityReward?.RewardAmount
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
                    ct.ThrowIfCancellationRequested();

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
                    ct.ThrowIfCancellationRequested();

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

    private async Task UpsertFacilityTypesAsync
    (
        ICollectionsContext dbContext,
        CancellationToken ct
    )
    {
        try
        {
            if (_patchDataCache.FacilityTypes is null)
                throw new MissingCacheDataException(typeof(FacilityType));

            await dbContext.UpsertCollectionAsync(_patchDataCache.FacilityTypes, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upsert the FacilityType collection");
        }
    }

    private static void CheckForRequiredZonesHelper<T>(IReadOnlyDictionary<ZoneDefinition, T> zonesPresent)
    {
        if (zonesPresent.Count == 0)
            throw new MissingCacheDataException(typeof(T));

        bool hasAllStaticZones = zonesPresent.ContainsKey(ZoneDefinition.Indar)
            && zonesPresent.ContainsKey(ZoneDefinition.Esamir)
            && zonesPresent.ContainsKey(ZoneDefinition.Amerish)
            && zonesPresent.ContainsKey(ZoneDefinition.Hossin)
            && zonesPresent.ContainsKey(ZoneDefinition.Oshur);

        if (hasAllStaticZones)
            return;

        throw new Exception
        (
            "Missing a static zone. Present: " +
            string.Join(", ", zonesPresent.Keys)
        );
    }

    private static string HexTypeToName(byte hexType)
        => hexType switch
        {
            0 => "Unrestricted access",
            2 => "Restricted by faction",
            _ => "Unknown"
        };

    private static string OutfitResourceToName(OutfitResource resource)
        => resource switch
        {
            OutfitResource.Auraxium => "Auraxium - Common",
            OutfitResource.Synthium => "Synthium - Uncommon",
            OutfitResource.Polystellarite => "Polystellarite - Rare",
            _ => "Unknown"
        };
}
