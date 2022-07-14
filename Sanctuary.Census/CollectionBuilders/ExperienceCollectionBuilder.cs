using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using System.Collections.Generic;
using CExperience = Sanctuary.Census.ClientData.ClientDataModels.Experience;
using MExperience = Sanctuary.Census.Models.Experience;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MExperience"/> collection.
/// </summary>
public class ExperienceCollectionBuilder : ICollectionBuilder
{
    /// <inheritdoc />
    public void Build
    (
        IClientDataCacheService clientDataCache,
        IServerDataCacheService serverDataCache,
        ILocaleService localeService,
        CollectionsContext context
    )
    {
        if (clientDataCache.Experiences.Count == 0)
            throw new MissingCacheDataException(typeof(CExperience));

        Dictionary<uint, MExperience> builtItems = new();
        foreach (CExperience experience in clientDataCache.Experiences)
        {
            localeService.TryGetLocaleString(experience.StringID, out LocaleString? name);

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

        context.Experiences = builtItems;
    }
}
