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
using CZoneSetMapping = Sanctuary.Census.ClientData.ClientDataModels.ZoneSetMapping;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="Zone"/> and <see cref="ZoneSetMapping"/> collection.
/// </summary>
public class ZoneCollectionsBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IServerDataCacheService _serverDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZoneCollectionsBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    public ZoneCollectionsBuilder
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
    public async Task BuildAsync(ICollectionsContext dbContext, CancellationToken ct)
    {
        await UpsertZonesAsync(dbContext, ct);
        await UpsertZoneSetMappingsAsync(dbContext, ct);
    }

    private async Task UpsertZonesAsync(ICollectionsContext dbContext, CancellationToken ct)
    {
        if (_serverDataCache.ContinentBattleInfos is null)
            throw new MissingCacheDataException(typeof(ContinentBattleInfo));

        Dictionary<uint, Common.Objects.Collections.Zone> builtZones = new();
        IEnumerable<ContinentBattleInfo_ZoneData> zones = _serverDataCache.ContinentBattleInfos
            .Values
            .SelectMany(x => x.Zones)
            .OrderBy(x => x.ZoneID);

        foreach (ContinentBattleInfo_ZoneData zone in zones)
        {
            _localeDataCache.TryGetLocaleString(zone.NameID, out LocaleString? name);
            _localeDataCache.TryGetLocaleString(zone.DescriptionID, out LocaleString? description);
            bool isDynamic = zone.ZoneType is not ZoneType.Static;

            Common.Objects.Collections.Zone built = new
            (
                (uint)zone.ZoneID,
                zone.Name,
                new decimal(zone.HexSize),
                name!,
                description!,
                zone.GeometryId,
                zone.ZoneType.ToString(),
                isDynamic
            );
            builtZones.TryAdd(built.ZoneID, built);
        }

        await dbContext.UpsertCollectionAsync(builtZones.Values, ct).ConfigureAwait(false);
    }

    private async Task UpsertZoneSetMappingsAsync(ICollectionsContext dbContext, CancellationToken ct)
    {
        if (_clientDataCache.ZoneSetMappings is null)
            throw new MissingCacheDataException(typeof(CZoneSetMapping));

        IEnumerable<ZoneSetMapping> mappings = _clientDataCache.ZoneSetMappings
            .Select(mapping => new ZoneSetMapping
            (
                mapping.Id,
                mapping.ZoneSet,
                mapping.ZoneType,
                mapping.ZoneId
            ));

        await dbContext.UpsertCollectionAsync(mappings, ct);
    }
}
