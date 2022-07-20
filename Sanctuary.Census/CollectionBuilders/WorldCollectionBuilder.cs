using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.Models.Collections;
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
    /// <inheritdoc />
    public async Task BuildAsync
    (
        IClientDataCacheService clientDataCache,
        IServerDataCacheService serverDataCache,
        ILocaleDataCacheService localeDataCache,
        IMongoContext dbContext,
        CancellationToken ct
    )
    {
        if (serverDataCache.ServerListResponse is null)
            throw new MissingCacheDataException(typeof(ServerListResponse));

        Dictionary<uint, World> builtWorlds = new();
        foreach (ServerUpdate server in serverDataCache.ServerListResponse.Servers)
        {
            localeDataCache.TryGetLocaleString(server.NameID, out LocaleString? name);

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
