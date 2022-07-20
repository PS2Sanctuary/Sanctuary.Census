using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.Models.Collections;
using Sanctuary.Census.PatchData.Abstractions.Services;
using Sanctuary.Census.PatchData.PatchDataModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MapRegion"/> collection.
/// </summary>
public class MapRegionCollectionBuilder : ICollectionBuilder
{
    private readonly IPatchDataCacheService _patchDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="MapRegionCollectionBuilder"/> class.
    /// </summary>
    /// <param name="patchDataCache">The patch data cache.</param>
    public MapRegionCollectionBuilder(IPatchDataCacheService patchDataCache)
    {
        _patchDataCache = patchDataCache;
    }

    /// <inheritdoc />
    public async Task BuildAsync(IMongoContext dbContext, CancellationToken ct = default)
    {
        if (_patchDataCache.MapRegions.Count == 0)
            throw new MissingCacheDataException(typeof(MapRegionPatch));

        IEnumerable<MapRegion> links = _patchDataCache.MapRegions
            .Select
            (
                p => new MapRegion
                (
                    p.RegionID,
                    p.ZoneID,
                    p.FacilityID,
                    p.Name,
                    p.TypeID,
                    p.TypeName,
                    p.LocationX,
                    p.LocationY,
                    p.LocationZ
                )
            );

        await dbContext.UpsertMapRegionsAsync(links, ct).ConfigureAwait(false);
    }
}
