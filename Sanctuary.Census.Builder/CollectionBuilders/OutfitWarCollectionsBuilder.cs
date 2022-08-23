using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Common.Objects;
using Sanctuary.Zone.Packets.OutfitWars;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="OutfitWar"/>, <see cref="OutfitWarRanking"/> and <see cref="OutfitWarRounds"/> collections.
/// </summary>
public class OutfitWarCollectionsBuilder : ICollectionBuilder
{
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IServerDataCacheService _serverDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutfitWarCollectionsBuilder"/> class.
    /// </summary>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    public OutfitWarCollectionsBuilder
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
        if (_serverDataCache.OutfitWars.Count == 0)
            throw new MissingCacheDataException(typeof(OutfitWarsWar));

        if (_serverDataCache.OutfitWarRankings.Count == 0)
            throw new MissingCacheDataException(typeof(OutfitWarsRankings));

        if (_serverDataCache.OutfitWarRounds.Count == 0)
            throw new MissingCacheDataException(typeof(OutfitWarsRounds));

        Dictionary<uint, OutfitWar> builtWars = new();
        foreach ((ServerDefinition server, OutfitWarsWar war) in _serverDataCache.OutfitWars)
        {
            List<OutfitWar.Phase> phases = new();
            foreach (OutfitWarsWar_Phase phase in war.Phases)
            {
                _localeDataCache.TryGetLocaleString(phase.NameID, out LocaleString? phaseName);
                _localeDataCache.TryGetLocaleString(phase.DescriptionID, out LocaleString? phaseDesc);

                phases.Add(new OutfitWar.Phase
                (
                    phase.Order,
                    phaseName!,
                    phaseDesc!,
                    phase.StartTime,
                    phase.EndTime
                ));
            }

            _localeDataCache.TryGetLocaleString(war.NameID, out LocaleString? name);

            OutfitWar builtWar = new
            (
                war.OutfitWarID,
                (uint)server,
                name!,
                war.ImageSetID,
                war.MaybePlayerSize,
                war.MaybePlayerSignupRequirement,
                war.StartTime,
                war.EndTime,
                phases
            );
            builtWars.TryAdd(builtWar.OutfitWarID, builtWar);
        }
        await dbContext.UpsertOutfitWarsAsync(builtWars.Values, ct).ConfigureAwait(false);

        List<OutfitWarRanking> builtRankings = new();
        foreach (OutfitWarsRankings rankingGroup in _serverDataCache.OutfitWarRankings.Values)
        {
            foreach (Outfit rankedOutfit in rankingGroup.Outfits)
            {
                OutfitWarRanking ranking = new
                (
                    rankingGroup.RoundID,
                    rankedOutfit.OutfitID_1,
                    rankedOutfit.FactionID,
                    rankedOutfit.Order,
                    rankedOutfit.UIParams.ToDictionary(x => x.Name, x => x.Value)
                );
                builtRankings.Add(ranking);
            }
        }
        await dbContext.UpsertOutfitWarRankingsAsync(builtRankings, ct).ConfigureAwait(false);

        Dictionary<uint, OutfitWarRounds> builtRounds = new();
        foreach (OutfitWarsRounds sRounds in _serverDataCache.OutfitWarRounds.Values)
        {
            List<OutfitWarRounds.Round> rounds = new();
            foreach (OutfitWarsRounds_Round sRound in sRounds.Rounds)
            {
                rounds.Add(new OutfitWarRounds.Round
                (
                    sRound.Order,
                    (OutfitWarRounds.RoundStage)sRound.Stage,
                    sRound.StartTime,
                    sRound.EndTime
                ));
            }

            OutfitWarRounds built = new
            (
                sRounds.OutfitWarID,
                sRounds.ActiveWarInfo.RoundID,
                rounds
            );
            builtRounds.TryAdd(built.OutfitWarID, built);
        }
        await dbContext.UpsertOutfitWarRoundsAsync(builtRounds.Values, ct);
    }
}
