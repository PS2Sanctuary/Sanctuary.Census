using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.PatchData.Abstractions.Services;
using Sanctuary.Census.PatchData.PatchDataModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="FacilityLink"/> collection.
/// </summary>
public class FacilityLinkCollectionBuilder : ICollectionBuilder
{
    private readonly IPatchDataCacheService _patchDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="FacilityLinkCollectionBuilder"/> class.
    /// </summary>
    /// <param name="patchDataCache">The patch data cache.</param>
    public FacilityLinkCollectionBuilder(IPatchDataCacheService patchDataCache)
    {
        _patchDataCache = patchDataCache;
    }

    /// <inheritdoc />
    public async Task BuildAsync(ICollectionsContext dbContext, CancellationToken ct = default)
    {
        if (_patchDataCache.FacilityLinks is null)
            throw new MissingCacheDataException(typeof(FacilityLinkPatch));

        IEnumerable<FacilityLink> links = _patchDataCache.FacilityLinks
            .Select
            (
                p => new FacilityLink(p.ZoneID, p.FacilityA, p.FacilityB, p.Description)
            );

        await dbContext.UpsertFacilityLinksAsync(links, ct).ConfigureAwait(false);
    }
}
