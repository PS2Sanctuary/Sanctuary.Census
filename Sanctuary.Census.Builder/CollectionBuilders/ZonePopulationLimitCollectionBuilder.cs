using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Common.Objects;
using Sanctuary.Zone.Packets.Server;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FactionDefinition = Sanctuary.Census.Common.Objects.CommonModels.FactionDefinition;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="ZonePopulation"/> collection.
/// </summary>
public class ZonePopulationLimitCollectionBuilder : ICollectionBuilder
{
    private readonly IServerDataCacheService _serverDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ZonePopulationLimitCollectionBuilder"/> class.
    /// </summary>
    /// <param name="serverDataCache">The server data cache.</param>
    public ZonePopulationLimitCollectionBuilder(IServerDataCacheService serverDataCache)
    {
        _serverDataCache = serverDataCache;
    }

    /// <inheritdoc />
    public async Task BuildAsync(ICollectionsContext dbContext, CancellationToken ct = default)
    {
        if (_serverDataCache.ContinentBattleInfos is null)
            throw new MissingCacheDataException(typeof(ContinentBattleInfo));

        List<ZonePopulationLimits> builtLimits = new();

        foreach ((ServerDefinition world, ContinentBattleInfo cbi) in _serverDataCache.ContinentBattleInfos)
        {
            foreach (ContinentBattleInfo_ZoneData zone in cbi.Zones)
            {
                // Simple check to see if the zone is active
                int unpopulatedFactions = zone.PercentOfTotalLimit.Count(x => x is 0);
                if (unpopulatedFactions < 2)
                    continue;

                ushort max = zone.RemainingCharacterLimit.Max();

                ValueEqualityDictionary<FactionDefinition, int> limits = new();
                for (int i = 0; i < zone.RemainingCharacterLimit.Length; i++)
                {
                    ushort limit = zone.RemainingCharacterLimit[i];

                    // If the limit is within 5 of the maximum, there's probably just some lingering
                    // players on the continent. We'll hence ignore this and save the max
                    if (limit >= max - 5)
                        limit = max;

                    limits[(FactionDefinition)(i + 1)] = limit;
                }

                builtLimits.Add(new ZonePopulationLimits
                (
                    (uint)world,
                    (ushort)zone.ZoneID,
                    limits.Values.Sum(),
                    limits
                ));
            }
        }

        await dbContext.UpsertCollectionAsync(builtLimits, ct);
    }
}
