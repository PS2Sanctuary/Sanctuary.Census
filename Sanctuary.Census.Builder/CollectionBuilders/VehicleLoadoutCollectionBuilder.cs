using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MVehicleLoadout = Sanctuary.Census.Common.Objects.Collections.VehicleLoadout;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MVehicleLoadout"/> collection.
/// </summary>
public class VehicleLoadoutCollectionBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleLoadoutCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    public VehicleLoadoutCollectionBuilder(IClientDataCacheService clientDataCache)
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
        if (_clientDataCache.VehicleLoadouts is null)
            throw new MissingCacheDataException(typeof(VehicleLoadout));

        List<MVehicleLoadout> builtLoadouts = new();
        foreach (VehicleLoadout loadout in _clientDataCache.VehicleLoadouts)
        {
            MVehicleLoadout built = new
            (
                loadout.ID,
                loadout.VehicleID,
                loadout.FactionID,
                loadout.FlagCanCustomize,
                loadout.HideFromLoadoutScreen
            );
            builtLoadouts.Add(built);
        }

        await dbContext.UpsertVehicleLoadoutsAsync(builtLoadouts, ct);
    }
}
