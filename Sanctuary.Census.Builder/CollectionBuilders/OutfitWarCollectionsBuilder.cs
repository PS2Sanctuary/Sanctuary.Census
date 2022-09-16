using Microsoft.Extensions.Logging;
using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Objects;
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
using MRegistration = Sanctuary.Census.Common.Objects.Collections.OutfitWarRegistration;
using MRound = Sanctuary.Census.Common.Objects.Collections.OutfitWarRound;
using MRounds = Sanctuary.Census.Common.Objects.Collections.OutfitWarRounds;
using MWar = Sanctuary.Census.Common.Objects.Collections.OutfitWar;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MWar"/>, <see cref="MRanking"/>, <see cref="MRound"/>, <see cref="MRounds"/>,
/// <see cref="MRegistration"/> and <see cref="MMatch"/> collections.
/// </summary>
public class OutfitWarCollectionsBuilder : ICollectionBuilder
{
    private readonly ILogger<OutfitWarCollectionsBuilder> _logger;
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IServerDataCacheService _serverDataCache;
    private readonly IImageSetHelperService _imageSetHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutfitWarCollectionsBuilder"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    /// <param name="imageSetHelper">The image set helper service.</param>
    public OutfitWarCollectionsBuilder
    (
        ILogger<OutfitWarCollectionsBuilder> logger,
        ILocaleDataCacheService localeDataCache,
        IServerDataCacheService serverDataCache,
        IImageSetHelperService imageSetHelper
    )
    {
        _logger = logger;
        _localeDataCache = localeDataCache;
        _serverDataCache = serverDataCache;
        _imageSetHelper = imageSetHelper;
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
        await BuildRegistrationsAsync(dbContext, ct).ConfigureAwait(false);
        await BuildSingleRoundsAsync(dbContext, ct).ConfigureAwait(false);
        await BuildRoundsAsync(dbContext, ct).ConfigureAwait(false);
        await BuildMatchesAsync(dbContext, ct).ConfigureAwait(false);

        // TODO: Remove after deployment
        await RetrospectivelyApplyMatchRounds(dbContext, ct).ConfigureAwait(false);
    }

    private async Task RetrospectivelyApplyMatchRounds(ICollectionsContext dbContext, CancellationToken ct)
    {
        List<MMatch> fixedMatches = new();

        await foreach (MMatch match in dbContext.GetCollectionDocumentsAsync<MMatch>(ct).ConfigureAwait(false))
        {
            bool hasRounds = _serverDataCache.OutfitWarRounds.TryGetValue((ServerDefinition)match.WorldID, out OutfitWarRounds? rounds);
            MMatch toAppend = match;

            if (hasRounds)
            {
                IEnumerable<OutfitWarRounds_Round> round = rounds!.Rounds.Where
                (
                    r => r.StartTime < match.StartTime && r.EndTime > match.StartTime
                );
                try
                {
                    ulong roundID = round.Single().RoundID;
                    toAppend = match with { RoundID = roundID };
                }
                catch
                {
                    // This is fine, we must be able to conclusively assign
                    // a round ID to the match so we will just skip it
                }
            }

            fixedMatches.Add(toAppend);
        }

        await dbContext.UpsertCollectionAsync(fixedMatches, ct).ConfigureAwait(false);
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
                if (!_serverDataCache.OutfitWarRounds.ContainsKey(server))
                    throw new MissingCacheDataException(typeof(OutfitWarRounds), $"Missing the OutfitWarRounds value for {server}");

                ValueEqualityList<MWar.Phase> phases = new();
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
                bool hasDefaultImage = _imageSetHelper.TryGetDefaultImage(war.ImageSetID, out uint defaultImage);

                MWar builtWar = new
                (
                    war.OutfitWarID,
                    (uint)server,
                    _serverDataCache.OutfitWarRounds[server].ActiveWarInfo.RoundID,
                    name,
                    war.MaybeOutfitSizeRequirement,
                    war.MaybePlayerSignupRequirement,
                    war.StartTime,
                    war.EndTime,
                    war.ImageSetID,
                    hasDefaultImage ? defaultImage : null,
                    hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null,
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
            if (_serverDataCache.OutfitWars.Count == 0)
                throw new MissingCacheDataException(typeof(OutfitWarWar));

            if (_serverDataCache.OutfitWarRankings.Count == 0)
                throw new MissingCacheDataException(typeof(OutfitWarRankings));

            List<MRanking> builtRankings = new();

            foreach ((ServerDefinition server, OutfitWarRankings rankingGroup) in _serverDataCache.OutfitWarRankings)
            {
                if (!_serverDataCache.OutfitWars.TryGetValue(server, out OutfitWarWar? activeWar))
                    throw new MissingCacheDataException(typeof(OutfitWarWar), $"Missing the {nameof(OutfitWarWar)} data for the {server} server");

                foreach (OutfitWarRankings_Outfit rankedOutfit in rankingGroup.Outfits)
                {
                    ValueEqualityDictionary<string, int> rankingParams = new
                    (
                        rankedOutfit.UIParams.ToDictionary(x => x.Name, x => x.Value)
                    );

                    MRanking ranking = new
                    (
                        rankingGroup.RoundID,
                        rankedOutfit.OutfitID_1,
                        rankedOutfit.FactionID,
                        activeWar.OutfitWarID,
                        rankedOutfit.Order,
                        rankingParams
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

    private async Task BuildRegistrationsAsync(ICollectionsContext dbContext, CancellationToken ct)
    {
        try
        {
            if (_serverDataCache.OutfitWars.Count == 0)
                throw new MissingCacheDataException(typeof(OutfitWarWar));

            if (_serverDataCache.RegisteredOutfits.Count == 0)
                throw new MissingCacheDataException(typeof(RegisteredOutfit));

            Dictionary<ulong, MRegistration> builtRegistrations = new();
            foreach ((ServerDefinition server, RegisteredOutfits outfits) in _serverDataCache.RegisteredOutfits)
            {
                if (!_serverDataCache.OutfitWars.TryGetValue(server, out OutfitWarWar? activeWar))
                    throw new MissingCacheDataException(typeof(OutfitWarWar), $"Missing the {nameof(OutfitWarWar)} data for the {server} server");

                foreach (RegisteredOutfit outfit in outfits.Outfits)
                {
                    MRegistration built = new
                    (
                        outfit.OutfitID,
                        (uint)outfit.FactionID,
                        (uint)server,
                        activeWar.OutfitWarID,
                        outfit.RegistrationOrder,
                        (MRegistration.RegistrationStatus)outfit.Status,
                        outfit.Status is RegistrationStatus.Full or RegistrationStatus.WaitingOnNextFullReg
                            ? activeWar.MaybePlayerSignupRequirement
                            : outfit.MemberSignupCount
                    );
                    builtRegistrations.Add(built.OutfitID, built);
                }
            }

            await dbContext.UpsertCollectionAsync(builtRegistrations.Values, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build the OutfitWar collection");
        }
    }

    private async Task BuildSingleRoundsAsync(ICollectionsContext dbContext, CancellationToken ct)
    {
        try
        {
            if (_serverDataCache.OutfitWarRounds.Count == 0)
                throw new MissingCacheDataException(typeof(OutfitWarRounds));

            Dictionary<ulong, MRound> builtRounds = new();

            foreach (OutfitWarRounds sRounds in _serverDataCache.OutfitWarRounds.Values)
            {
                foreach (OutfitWarRounds_Round sRound in sRounds.Rounds)
                {
                    builtRounds.Add(sRound.RoundID, new MRound
                    (
                        sRound.RoundID,
                        sRounds.OutfitWarID,
                        sRounds.ActiveWarInfo.RoundID,
                        sRound.Order,
                        (MRound.RoundStage)sRound.Stage,
                        sRound.StartTime,
                        sRound.EndTime
                    ));
                }
            }

            await dbContext.UpsertCollectionAsync(builtRounds.Values, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build the OutfitWarRounds collection");
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
                ValueEqualityList<MRounds.Round> rounds = new();
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
                bool hasRounds = _serverDataCache.OutfitWarRounds.TryGetValue(world, out OutfitWarRounds? rounds);

                foreach (OutfitWarMatchTime time in times)
                {
                    ulong? roundID = null;
                    if (hasRounds)
                    {
                        IEnumerable<OutfitWarRounds_Round> round = rounds!.Rounds.Where
                        (
                            r => r.StartTime < time.StartTime && r.EndTime > time.StartTime
                        );
                        try
                        {
                            roundID = round.Single().RoundID;
                        }
                        catch
                        {
                            // This is fine, we must be able to conclusively assign
                            // a round ID to the match so we will just skip it
                        }
                    }

                    builtMatches.TryAdd(time.MatchID, new MMatch
                    (
                        time.MatchID,
                        roundID,
                        time.OutfitWarID,
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
