using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Login.Packets;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="World"/> collection.
/// </summary>
public class WorldCollectionBuilder : ICollectionBuilder
{
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IServerDataCacheService _serverDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldCollectionBuilder"/> class.
    /// </summary>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    public WorldCollectionBuilder
    (
        ILocaleDataCacheService localeDataCache,
        IServerDataCacheService serverDataCache
    )
    {
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
        if (_serverDataCache.ServerListResponse is null)
            throw new MissingCacheDataException(typeof(ServerListResponse));

        Dictionary<uint, World> builtWorlds = new();
        foreach (ServerUpdate server in _serverDataCache.ServerListResponse.Servers)
        {
            _localeDataCache.TryGetLocaleString(server.NameID, out LocaleString? name);

            World built = new
            (
                (uint)server.ServerID,
                name!,
                server.Islocked,
                server.AllowedAccess
            );
            builtWorlds.TryAdd(built.WorldID, built);
        }

        await dbContext.UpsertWorldsAsync(builtWorlds.Values, ct);
    }
}
