using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
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
        if (clientDataCache.Experiences.Count == 0)
            throw new MissingCacheDataException(typeof(CExperience));

        Dictionary<uint, MExperience> builtItems = new();
        foreach (CExperience experience in clientDataCache.Experiences)
        {
            localeDataCache.TryGetLocaleString(experience.StringID, out LocaleString? name);

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
