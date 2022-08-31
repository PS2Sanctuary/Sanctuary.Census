using Microsoft.Extensions.Logging;
using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Common.Objects;
using Sanctuary.Zone.Packets.OutfitWars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MMatch = Sanctuary.Census.Common.Objects.Collections.OutfitWarMatch;
using MRanking = Sanctuary.Census.Common.Objects.Collections.OutfitWarRanking;
using MRounds = Sanctuary.Census.Common.Objects.Collections.OutfitWarRounds;
using MWar = Sanctuary.Census.Common.Objects.Collections.OutfitWar;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MWar"/>, <see cref="MRanking"/>, <see cref="MRounds"/> and <see cref="MMatch"/> collections.
/// </summary>
public class OutfitWarCollectionsBuilder : ICollectionBuilder
{
    private readonly ILogger<OutfitWarCollectionsBuilder> _logger;
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IServerDataCacheService _serverDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutfitWarCollectionsBuilder"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    public OutfitWarCollectionsBuilder
    (
        ILogger<OutfitWarCollectionsBuilder> logger,
        ILocaleDataCacheService localeDataCache,
        IServerDataCacheService serverDataCache
    )
    {
        _logger = logger;
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
        await BuildWarsAsync(dbContext, ct).ConfigureAwait(false);
        await BuildRankingsAsync(dbContext, ct).ConfigureAwait(false);
        await BuildRoundsAsync(dbContext, ct).ConfigureAwait(false);
        await BuildMatchesAsync(dbContext, ct).ConfigureAwait(false);
    }

    private async Task BuildWarsAsync(ICollectionsContext dbContext, CancellationToken ct)
    {
        try
        {
            if (_serverDataCache.OutfitWars.Count == 0)
                throw new MissingCacheDataException(typeof(OutfitWarWar));

            Dictionary<uint, MWar> builtWars = new();

            foreach ((ServerDefinition server, OutfitWarWar war) in _serverDataCache.OutfitWars)
            {
                List<MWar.Phase> phases = new();
                foreach (OutfitWarWar_Phase phase in war.Phases)
                {
                    _localeDataCache.TryGetLocaleString(phase.NameID, out LocaleString? phaseName);
                    _localeDataCache.TryGetLocaleString(phase.DescriptionID, out LocaleString? phaseDesc);

                    phases.Add(new MWar.Phase
                    (
                        phase.Order,
                        phaseName!,
                        phaseDesc!,
                        phase.StartTime,
                        phase.EndTime
                    ));
                }

                _localeDataCache.TryGetLocaleString(war.NameID, out LocaleString? name);

                MWar builtWar = new
                (
                    war.OutfitWarID,
                    (uint)server,
                    name,
                    war.ImageSetID,
                    war.MaybeOutfitSizeRequirement,
                    war.MaybePlayerSignupRequirement,
                    war.StartTime,
                    war.EndTime,
                    phases
                );
                builtWars.TryAdd(builtWar.OutfitWarID, builtWar);
            }

            await dbContext.UpsertCollectionAsync(builtWars.Values, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build the OutfitWar collection");
        }
    }

    private async Task BuildRankingsAsync(ICollectionsContext dbContext, CancellationToken ct)
    {
        try
        {
            if (_serverDataCache.OutfitWarRankings.Count == 0)
                throw new MissingCacheDataException(typeof(OutfitWarRankings));

            List<MRanking> builtRankings = new();

            foreach (OutfitWarRankings rankingGroup in _serverDataCache.OutfitWarRankings.Values)
            {
                foreach (OutfitWarRankings_Outfit rankedOutfit in rankingGroup.Outfits)
                {
                    MRanking ranking = new
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

            await dbContext.UpsertCollectionAsync(builtRankings, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build the OutfitWarRanking collection");
        }
    }

    private async Task BuildRoundsAsync(ICollectionsContext dbContext, CancellationToken ct)
    {
        try
        {
            if (_serverDataCache.OutfitWarRounds.Count == 0)
                throw new MissingCacheDataException(typeof(OutfitWarRounds));

            Dictionary<uint, MRounds> builtRounds = new();

            foreach (OutfitWarRounds sRounds in _serverDataCache.OutfitWarRounds.Values)
            {
                List<MRounds.Round> rounds = new();
                foreach (OutfitWarRounds_Round sRound in sRounds.Rounds)
                {
                    rounds.Add(new MRounds.Round
                    (
                        sRound.Order,
                        (MRounds.RoundStage)sRound.Stage,
                        sRound.StartTime,
                        sRound.EndTime
                    ));
                }

                MRounds built = new
                (
                    sRounds.OutfitWarID,
                    sRounds.ActiveWarInfo.RoundID,
                    rounds
                );
                builtRounds.TryAdd(built.OutfitWarID, built);
            }

            await dbContext.UpsertCollectionAsync(builtRounds.Values, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build the OutfitWarRounds collection");
        }
    }

    private async Task BuildMatchesAsync(ICollectionsContext dbContext, CancellationToken ct)
    {
        try
        {
            if (_serverDataCache.OutfitWarMatchParticipants.Count == 0)
                throw new MissingCacheDataException(typeof(OutfitWarMatchParticipant));

            if (_serverDataCache.OutfitWarMatchTimes.Count == 0)
                throw new MissingCacheDataException(typeof(OutfitWarMatchTime));

            Dictionary<ulong, MMatch> builtMatches = new();

            foreach ((ServerDefinition world, List<OutfitWarMatchTime> times) in _serverDataCache .OutfitWarMatchTimes)
            {
                foreach (OutfitWarMatchTime time in times)
                {
                    builtMatches.TryAdd(time.MatchID, new MMatch
                    (
                        time.OutfitWarID,
                        time.MatchID,
                        0,
                        0,
                        time.StartTime,
                        time.Order,
                        (uint)world,
                        0,
                        0
                    ));
                }
            }

            foreach (List<OutfitWarMatchParticipant> participants in _serverDataCache.OutfitWarMatchParticipants.Values)
            {
                foreach (OutfitWarMatchParticipant participant in participants)
                {
                    MMatch match = builtMatches[participant.MatchID];
                    if (match.OutfitAId == 0)
                    {
                        match = match with
                        {
                            OutfitAId = participant.OutfitID,
                            OutfitAFactionId = (uint)participant.FactionID
                        };
                    }
                    else
                    {
                        match = match with
                        {
                            OutfitBId = participant.OutfitID,
                            OutfitBFactionId = (uint)participant.FactionID
                        };
                    }
                    builtMatches[participant.MatchID] = match;
                }
            }

            await dbContext.UpsertCollectionAsync(builtMatches.Values, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build the OutfitWarMatch collection");
        }
    }
}
