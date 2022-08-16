using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.Server;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="Zone"/> collection.
/// </summary>
public class ZoneCollectionBuilder : ICollectionBuilder
{
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IServerDataCacheService _serverDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZoneCollectionBuilder"/> class.
    /// </summary>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    public ZoneCollectionBuilder
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
        if (_serverDataCache.ContinentBattleInfos is null)
            throw new MissingCacheDataException(typeof(ContinentBattleInfo));

        Dictionary<uint, Common.Objects.Collections.Zone> builtZones = new();
        foreach (ZonePopulationInfo zone in _serverDataCache.ContinentBattleInfos.Zones.OrderBy(x => x.ZoneID))
        {
            _localeDataCache.TryGetLocaleString(zone.NameID, out LocaleString? name);
            _localeDataCache.TryGetLocaleString(zone.DescriptionID, out LocaleString? description);

            Common.Objects.Collections.Zone built = new
            (
                (uint)zone.ZoneID,
                zone.Name,
                zone.HexSize,
                name!,
                description!
            );
            builtZones.TryAdd(built.ZoneID, built);
        }

        await dbContext.UpsertZonesAsync(builtZones.Values, ct);
    }
}
