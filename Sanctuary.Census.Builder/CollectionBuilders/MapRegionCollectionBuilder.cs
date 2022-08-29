using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.PatchData.Abstractions.Services;
using Sanctuary.Census.PatchData.PatchDataModels;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Common.Objects;
using Sanctuary.Zone.Packets.MapRegion;
using Sanctuary.Zone.Packets.StaticFacilityInfo;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FacilityInfo = Sanctuary.Zone.Packets.StaticFacilityInfo.FacilityInfo;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="Common.Objects.Collections.MapRegion"/> collection.
/// </summary>
public class MapRegionCollectionBuilder : ICollectionBuilder
{
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IPatchDataCacheService _patchDataCache;
    private readonly IServerDataCacheService _serverDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="MapRegionCollectionBuilder"/> class.
    /// </summary>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="patchDataCache">The patch data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    public MapRegionCollectionBuilder
    (
        ILocaleDataCacheService localeDataCache,
        IPatchDataCacheService patchDataCache,
        IServerDataCacheService serverDataCache
    )
    {
        _localeDataCache = localeDataCache;
        _patchDataCache = patchDataCache;
        _serverDataCache = serverDataCache;
    }

    /// <inheritdoc />
    public async Task BuildAsync(ICollectionsContext dbContext, CancellationToken ct = default)
    {
        if (_patchDataCache.MapRegions is null)
            throw new MissingCacheDataException(typeof(MapRegionPatch));

        if (_serverDataCache.MapRegionDatas.Count == 0)
            throw new MissingCacheDataException(typeof(MapRegionData));

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
                    region.FacilityID,
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

        await dbContext.UpsertMapRegionsAsync(builtRegions.Values, ct).ConfigureAwait(false);
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
}
