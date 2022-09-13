using Microsoft.Extensions.Logging;
using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.CommonModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MSkill = Sanctuary.Census.Common.Objects.Collections.Skill;
using MSkillCategory = Sanctuary.Census.Common.Objects.Collections.SkillCategory;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="Skill"/>, <see cref="SkillCategory"/>,
/// <see cref="SkillLine"/> and <see cref="SkillSet"/> collections.
/// </summary>
public class SkillCollectionsBuilder : ICollectionBuilder
{
    private readonly ILogger<SkillCollectionsBuilder> _logger;
    private readonly IClientDataCacheService _clientDataCache;
    private readonly ILocaleDataCacheService _localeDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileCollectionBuilder"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    public SkillCollectionsBuilder
    (
        ILogger<SkillCollectionsBuilder> logger,
        IClientDataCacheService clientDataCache,
        ILocaleDataCacheService localeDataCache
    )
    {
        _logger = logger;
        _clientDataCache = clientDataCache;
        _localeDataCache = localeDataCache;
    }

    /// <inheritdoc />
    public async Task BuildAsync
    (
        ICollectionsContext dbContext,
        CancellationToken ct
    )
    {
        if (_clientDataCache.ImageSetMappings is null)
            throw new MissingCacheDataException(typeof(ImageSetMapping));

        Dictionary<uint, ImageSetMapping> defaultImages = new();
        foreach (ImageSetMapping mapping in _clientDataCache.ImageSetMappings)
        {
            if (!defaultImages.TryAdd(mapping.ImageSetID, mapping) && mapping.ImageType is ImageSetType.Type.Large)
                defaultImages[mapping.ImageSetID] = mapping;
        }

        await BuildSkillsAsync(dbContext, defaultImages, ct).ConfigureAwait(false);
        await BuildSkillCategoriesAsync(dbContext, defaultImages, ct).ConfigureAwait(false);
    }

    private async Task BuildSkillsAsync
    (
        ICollectionsContext dbContext,
        IReadOnlyDictionary<uint, ImageSetMapping> defaultImageMap,
        CancellationToken ct
    )
    {
        try
        {
            if (_clientDataCache.Skills is null)
                throw new MissingCacheDataException(typeof(Skill));

            Dictionary<uint, MSkill> builtSkills = new();
            foreach (Skill skill in _clientDataCache.Skills)
            {
                _localeDataCache.TryGetLocaleString(skill.NameID, out LocaleString? name);
                _localeDataCache.TryGetLocaleString(skill.DescriptionID, out LocaleString? description);
                defaultImageMap.TryGetValue(skill.IconID, out ImageSetMapping? defaultImage);

                builtSkills.Add(skill.ID, new MSkill
                (
                    skill.ID,
                    name,
                    description,
                    skill.SkillSetID == 0 ? null : skill.SkillSetID,
                    skill.SkillSetID == 0 ? null : skill.SkillSetIndex,
                    skill.SkillCategoryIdEx == 0 ? null : skill.SkillCategoryIdEx,
                    skill.SkillCategoryIdEx == 0 ? null : skill.SkillCategoryIndexEx,
                    skill.SkillLineID == 0 ? null : skill.SkillLineID,
                    skill.SkillLineID == 0 ? null : skill.SkillLineIndex,
                    skill.SkillPoints,
                    skill.GrantAbilityLineID == 0 ? null : skill.GrantAbilityLineID,
                    skill.GrantAbilityLineID == 0 ? null : skill.GrantAbilityLineIndex,
                    skill.GrantItemID == 0 ? null : skill.GrantItemID,
                    skill.GrantRewardSetID == 0 ? null : skill.GrantRewardSetID,
                    skill.FlagAutoGrant,
                    skill.FlagIsVisible,
                    skill.CurrencyID,
                    skill.IconID,
                    defaultImage?.ImageID ?? null,
                    defaultImage is null ? null : $"/files/ps2/images/static/{defaultImage.ImageID}.png"
                ));
            }

            await dbContext.UpsertCollectionAsync(builtSkills.Values, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build the Skill collection");
        }
    }

    private async Task BuildSkillCategoriesAsync
    (
        ICollectionsContext dbContext,
        IReadOnlyDictionary<uint, ImageSetMapping> defaultImageMap,
        CancellationToken ct
    )
    {
        try
        {
            if (_clientDataCache.SkillCategories is null)
                throw new MissingCacheDataException(typeof(Skill));

            Dictionary<uint, MSkillCategory> builtCategories = new();
            foreach (SkillCategory category in _clientDataCache.SkillCategories)
            {
                _localeDataCache.TryGetLocaleString(category.NameID, out LocaleString? name);
                _localeDataCache.TryGetLocaleString(category.DescriptionID, out LocaleString? description);
                defaultImageMap.TryGetValue(category.IconID, out ImageSetMapping? defaultImage);

                builtCategories.TryAdd(category.ID, new MSkillCategory
                (
                    category.ID,
                    category.SkillSetID,
                    category.SkillSetIndex,
                    category.SkillPoints,
                    name,
                    description,
                    category.IconID,
                    defaultImage?.ImageID ?? null,
                    defaultImage is null ? null : $"/files/ps2/images/static/{defaultImage.ImageID}.png"
                ));
            }

            await dbContext.UpsertCollectionAsync(builtCategories.Values, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build the SkillCategory collection");
        }
    }
}
