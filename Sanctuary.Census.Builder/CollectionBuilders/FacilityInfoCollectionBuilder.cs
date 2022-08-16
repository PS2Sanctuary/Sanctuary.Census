using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.PatchData.Abstractions.Services;
using Sanctuary.Census.PatchData.PatchDataModels;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.StaticFacilityInfo;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MFacilityInfo = Sanctuary.Census.Common.Objects.Collections.FacilityInfo;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="Common.Objects.Collections.FacilityInfo"/> collection.
/// </summary>
public class FacilityInfoCollectionBuilder : ICollectionBuilder
{
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IServerDataCacheService _serverDataCache;
    private readonly IPatchDataCacheService _patchDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="FacilityInfoCollectionBuilder"/> class.
    /// </summary>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    /// <param name="patchDataCache">The patch data cache.</param>
    public FacilityInfoCollectionBuilder
    (
        ILocaleDataCacheService localeDataCache,
        IServerDataCacheService serverDataCache,
        IPatchDataCacheService patchDataCache
    )
    {
        _localeDataCache = localeDataCache;
        _serverDataCache = serverDataCache;
        _patchDataCache = patchDataCache;
    }

    /// <inheritdoc />
    public async Task BuildAsync(ICollectionsContext dbContext, CancellationToken ct = default)
    {
        if (_serverDataCache.StaticFacilityInfos is null)
            throw new MissingCacheDataException(typeof(StaticFacilityInfoAllZones));

        if (_patchDataCache.StaticFacilityInfos is null)
            throw new MissingCacheDataException(typeof(StaticFacilityInfo));

        Dictionary<uint, MFacilityInfo> builtFacilities = new();

        foreach (FacilityInfo fi in _serverDataCache.StaticFacilityInfos.Facilities)
        {
            _localeDataCache.TryGetLocaleString(fi.FacilityNameID, out LocaleString? name);
            MFacilityInfo built = new
            (
                fi.ZoneDefinition,
                fi.FacilityID,
                name!,
                fi.FacilityType,
                new decimal(fi.LocationX),
                new decimal(fi.LocationY),
                new decimal(fi.LocationZ)
            );
            builtFacilities.TryAdd(built.FacilityID, built);
        }

        foreach (StaticFacilityInfo sfi in _patchDataCache.StaticFacilityInfos)
        {
            _localeDataCache.TryGetLocaleString(sfi.FacilityNameID, out LocaleString? name);

            MFacilityInfo built = new
            (
                sfi.ZoneDefinition,
                sfi.FacilityID,
                name!,
                sfi.FacilityType,
                new decimal(sfi.LocationX),
                new decimal(sfi.LocationY),
                new decimal(sfi.LocationZ)
            );
            builtFacilities.TryAdd(built.FacilityID, built);
        }

        await dbContext.UpsertFacilityInfosAsync(builtFacilities.Values, ct).ConfigureAwait(false);
    }
}
