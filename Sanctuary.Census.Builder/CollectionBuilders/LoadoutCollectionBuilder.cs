using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MLoadout = Sanctuary.Census.Common.Objects.Collections.Loadout;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MLoadout"/> collection.
/// </summary>
public class LoadoutCollectionBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadoutCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    public LoadoutCollectionBuilder
    (
        IClientDataCacheService clientDataCache
    )
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
        if (_clientDataCache.Loadouts is null)
            throw new MissingCacheDataException(typeof(LoadoutSlot));

        List<MLoadout> builtLoadouts = new();
        foreach (Loadout loadout in _clientDataCache.Loadouts)
        {
            MLoadout built = new
            (
                loadout.LoadoutID,
                loadout.ProfileID,
                loadout.FactionID
            );
            builtLoadouts.Add(built);
        }

        await dbContext.UpsertLoadoutsAsync(builtLoadouts, ct);
    }
}
