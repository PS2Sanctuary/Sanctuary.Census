using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.PatchData.Abstractions.Services;
using Sanctuary.Census.PatchData.PatchDataModels;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.StaticFacilityInfo;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FacilityInfo = Sanctuary.Zone.Packets.StaticFacilityInfo.FacilityInfo;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MapRegion"/> collection.
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

        if (_serverDataCache.StaticFacilityInfos is null)
            throw new MissingCacheDataException(typeof(StaticFacilityInfoAllZones));

        Dictionary<uint, FacilityInfo> facilityInfos = new();
        foreach (FacilityInfo fac in _serverDataCache.StaticFacilityInfos.Facilities)
            facilityInfos.TryAdd(fac.FacilityID, fac);

        Dictionary<uint, MapRegion> builtRegions = new();
        foreach (MapRegionPatch mapRegion in _patchDataCache.MapRegions)
        {
            MapRegion built = new
            (
                mapRegion.RegionID,
                mapRegion.ZoneID,
                mapRegion.FacilityID,
                mapRegion.Name,
                mapRegion.TypeID,
                mapRegion.TypeName,
                mapRegion.LocationX,
                mapRegion.LocationY,
                mapRegion.LocationZ
            );

            if (facilityInfos.TryGetValue(built.FacilityId, out FacilityInfo? facility))
            {
                _localeDataCache.TryGetLocaleString(facility.FacilityNameID, out LocaleString? name);

                built = built with {
                    FacilityName = name!.En!,
                    FacilityTypeId = facility.FacilityType,
                    LocationX = facility.LocationX,
                    LocationY = facility.LocationY,
                    LocationZ = facility.LocationZ,
                    ZoneId = facility.ZoneDefinition
                };
            }

            builtRegions.TryAdd(built.MapRegionId, built);
        }

        await dbContext.UpsertMapRegionsAsync(builtRegions.Values, ct).ConfigureAwait(false);
    }
}
