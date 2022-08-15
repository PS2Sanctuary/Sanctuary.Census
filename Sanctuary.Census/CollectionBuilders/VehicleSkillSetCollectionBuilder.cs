using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Exceptions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MVehicleSkillSet = Sanctuary.Census.Common.Objects.Collections.VehicleSkillSet;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MVehicleSkillSet"/> collection.
/// </summary>
public class VehicleSkillSetCollectionBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleSkillSetCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    public VehicleSkillSetCollectionBuilder(IClientDataCacheService clientDataCache)
    {
        _clientDataCache = clientDataCache;
    }

    /// <inheritdoc />
    public async Task BuildAsync
    (
        ICollectionsContext dbContext,
        CancellationToken ct
    )
    {
        if (_clientDataCache.VehicleSkillSets is null)
            throw new MissingCacheDataException(typeof(VehicleSkillSet));

        List<MVehicleSkillSet> builtSets = new();
        foreach (VehicleSkillSet skillSet in _clientDataCache.VehicleSkillSets)
        {
            MVehicleSkillSet built = new
            (
                skillSet.VehicleID,
                skillSet.SkillSetID,
                skillSet.FactionID,
                skillSet.DisplayIndex
            );
            builtSets.Add(built);
        }

        await dbContext.UpsertVehicleSkillSetsAsync(builtSets, ct);
    }
}
