using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.ReferenceData;
using System.Collections.Generic;
using System.Linq;
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
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IServerDataCacheService _serverDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadoutCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    public LoadoutCollectionBuilder
    (
        IClientDataCacheService clientDataCache,
        ILocaleDataCacheService localeDataCache,
        IServerDataCacheService serverDataCache
    )
    {
        _clientDataCache = clientDataCache;
        _localeDataCache = localeDataCache;
        _serverDataCache = serverDataCache;
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

        if (_serverDataCache.ProfileDefinitions is null)
            throw new MissingCacheDataException(typeof(ProfileDefinitions));

        Dictionary<uint, string> profileNames = new();
        if (_serverDataCache.ProfileDefinitions is not null)
        {
            profileNames = _serverDataCache.ProfileDefinitions
                .Profiles
                .ToDictionary
                (
                    x => x.ProfileID,
                    x => _localeDataCache.TryGetLocaleString(x.NameID, out LocaleString? profileName)
                        ? profileName.En ?? string.Empty
                        : string.Empty
                );
        }

        List<MLoadout> builtLoadouts = new();
        foreach (Loadout loadout in _clientDataCache.Loadouts)
        {
            string? codeName = null;
            if (profileNames.TryGetValue(loadout.ProfileID, out string? profileName))
                codeName = $"{(FactionDefinition)loadout.FactionID} {profileName}";

            MLoadout built = new
            (
                loadout.LoadoutID,
                loadout.ProfileID,
                loadout.FactionID,
                codeName
            );
            builtLoadouts.Add(built);
        }

        await dbContext.UpsertCollectionAsync(builtLoadouts, ct).ConfigureAwait(false);
    }
}
