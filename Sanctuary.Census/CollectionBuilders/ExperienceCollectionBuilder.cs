using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Exceptions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CExperience = Sanctuary.Census.ClientData.ClientDataModels.Experience;
using MExperience = Sanctuary.Census.Models.Collections.Experience;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MExperience"/> collection.
/// </summary>
public class ExperienceCollectionBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;
    private readonly ILocaleDataCacheService _localeDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExperienceCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    public ExperienceCollectionBuilder
    (
        IClientDataCacheService clientDataCache,
        ILocaleDataCacheService localeDataCache
    )
    {
        _clientDataCache = clientDataCache;
        _localeDataCache = localeDataCache;
    }

    /// <inheritdoc />
    public async Task BuildAsync
    (
        IMongoContext dbContext,
        CancellationToken ct
    )
    {
        if (_clientDataCache.Experiences is null)
            throw new MissingCacheDataException(typeof(CExperience));

        Dictionary<uint, MExperience> builtItems = new();
        foreach (CExperience experience in _clientDataCache.Experiences)
        {
            _localeDataCache.TryGetLocaleString(experience.StringID, out LocaleString? name);

            MExperience built = new
            (
                experience.ID,
                experience.AwardTypeID,
                name!,
                experience.XP,
                experience.NotableEvent
            );
            builtItems.TryAdd(built.ExperienceID, built);
        }

        await dbContext.UpsertExperiencesAsync(builtItems.Values, ct);
    }
}
