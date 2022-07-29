using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.Models.Collections;
using Sanctuary.Census.PatchData.Abstractions.Services;
using Sanctuary.Census.PatchData.PatchDataModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="FacilityInfo"/> collection.
/// </summary>
public class FacilityInfoCollectionBuilder : ICollectionBuilder
{
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IPatchDataCacheService _patchDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="FacilityInfoCollectionBuilder"/> class.
    /// </summary>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="patchDataCache">The patch data cache.</param>
    public FacilityInfoCollectionBuilder
    (
        ILocaleDataCacheService localeDataCache,
        IPatchDataCacheService patchDataCache
    )
    {
        _localeDataCache = localeDataCache;
        _patchDataCache = patchDataCache;
    }

    /// <inheritdoc />
    public async Task BuildAsync(ICollectionsContext dbContext, CancellationToken ct = default)
    {
        if (_patchDataCache.StaticFacilityInfos is null)
            throw new MissingCacheDataException(typeof(StaticFacilityInfo));

        Dictionary<uint, FacilityInfo> builtFacilities = new();
        foreach (StaticFacilityInfo sfi in _patchDataCache.StaticFacilityInfos)
        {
            _localeDataCache.TryGetLocaleString(sfi.FacilityNameID, out LocaleString? name);

            FacilityInfo built = new
            (
                sfi.ZoneDefinition,
                sfi.FacilityID,
                name!,
                sfi.FacilityType,
                sfi.LocationX,
                sfi.LocationY,
                sfi.LocationZ
            );
            builtFacilities.TryAdd(built.FacilityID, built);
        }

        await dbContext.UpsertFacilityInfosAsync(builtFacilities.Values, ct).ConfigureAwait(false);
    }
}
