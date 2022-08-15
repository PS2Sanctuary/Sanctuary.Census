using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.Models.Collections;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Common.Objects;
using Sanctuary.Zone.Packets.OutfitWars;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="OutfitWar"/> and <see cref="OutfitWarRounds"/> collections.
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
                    DateTimeOffset.FromUnixTimeSeconds((long)phase.StartTime),
                    DateTimeOffset.FromUnixTimeSeconds((long)phase.EndTime)
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
                DateTimeOffset.FromUnixTimeSeconds((long)war.StartTime),
                DateTimeOffset.FromUnixTimeSeconds((long)war.EndTime),
                phases
            );
            builtWars.TryAdd(builtWar.OutfitWarID, builtWar);
        }
        await dbContext.UpsertOutfitWarsAsync(builtWars.Values, ct).ConfigureAwait(false);

        Dictionary<uint, OutfitWarRounds> builtRounds = new();
        foreach (OutfitWarsRounds sRounds in _serverDataCache.OutfitWarRounds.Values)
        {
            List<OutfitWarRounds.Round> rounds = new();
            foreach (OutfitWarsRounds_Round sRound in sRounds.Rounds)
            {
                rounds.Add(new OutfitWarRounds.Round
                (
                    sRound.Order,
                    sRound.Stage,
                    DateTimeOffset.FromUnixTimeSeconds((long)sRound.StartTime),
                    DateTimeOffset.FromUnixTimeSeconds((long)sRound.EndTime)
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
