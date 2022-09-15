using Microsoft.Extensions.Logging;
using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
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
using MSkillLine = Sanctuary.Census.Common.Objects.Collections.SkillLine;
using MSkillSet = Sanctuary.Census.Common.Objects.Collections.SkillSet;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MSkill"/>, <see cref="MSkillCategory"/>,
/// <see cref="MSkillLine"/> and <see cref="MSkillSet"/> collections.
/// </summary>
public class SkillCollectionsBuilder : ICollectionBuilder
{
    private readonly ILogger<SkillCollectionsBuilder> _logger;
    private readonly IClientDataCacheService _clientDataCache;
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IImageSetHelperService _imageSetHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileCollectionBuilder"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="imageSetHelper">The image set helper service.</param>
    public SkillCollectionsBuilder
    (
        ILogger<SkillCollectionsBuilder> logger,
        IClientDataCacheService clientDataCache,
        ILocaleDataCacheService localeDataCache,
        IImageSetHelperService imageSetHelper
    )
    {
        _logger = logger;
        _clientDataCache = clientDataCache;
        _localeDataCache = localeDataCache;
        _imageSetHelper = imageSetHelper;
    }

    /// <inheritdoc />
    public async Task BuildAsync
    (
        ICollectionsContext dbContext,
        CancellationToken ct
    )
    {
        await BuildSkillsAsync(dbContext, ct).ConfigureAwait(false);
        await BuildSkillCategoriesAsync(dbContext, ct).ConfigureAwait(false);
        await BuildSkillLinesAsync(dbContext, ct).ConfigureAwait(false);
        await BuildSkillSetsAsync(dbContext, ct).ConfigureAwait(false);
    }

    private async Task BuildSkillsAsync(ICollectionsContext dbContext, CancellationToken ct)
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
                bool hasDefaultImage = _imageSetHelper.TryGetDefaultImage(skill.IconID, out uint defaultImage);

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
                    hasDefaultImage ? defaultImage : null,
                    hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null
                ));
            }

            await dbContext.UpsertCollectionAsync(builtSkills.Values, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build the Skill collection");
        }
    }

    private async Task BuildSkillCategoriesAsync(ICollectionsContext dbContext, CancellationToken ct)
    {
        try
        {
            if (_clientDataCache.SkillCategories is null)
                throw new MissingCacheDataException(typeof(SkillCategory));

            Dictionary<uint, MSkillCategory> builtCategories = new();
            foreach (SkillCategory category in _clientDataCache.SkillCategories)
            {
                _localeDataCache.TryGetLocaleString(category.NameID, out LocaleString? name);
                _localeDataCache.TryGetLocaleString(category.DescriptionID, out LocaleString? description);
                bool hasDefaultImage = _imageSetHelper.TryGetDefaultImage(category.IconID, out uint defaultImage);

                builtCategories.TryAdd(category.ID, new MSkillCategory
                (
                    category.ID,
                    category.SkillSetID,
                    category.SkillSetIndex,
                    category.SkillPoints,
                    name,
                    description,
                    category.IconID,
                    hasDefaultImage ? defaultImage : null,
                    hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null
                ));
            }

            await dbContext.UpsertCollectionAsync(builtCategories.Values, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build the SkillCategory collection");
        }
    }

    private async Task BuildSkillLinesAsync(ICollectionsContext dbContext, CancellationToken ct)
    {
        try
        {
            if (_clientDataCache.SkillLines is null)
                throw new MissingCacheDataException(typeof(SkillLine));

            Dictionary<uint, MSkillLine> builtLines = new();
            foreach (SkillLine line in _clientDataCache.SkillLines)
            {
                _localeDataCache.TryGetLocaleString(line.NameID, out LocaleString? name);
                _localeDataCache.TryGetLocaleString(line.DescriptionID, out LocaleString? description);
                bool hasDefaultImage = _imageSetHelper.TryGetDefaultImage(line.IconID, out uint defaultImage);

                builtLines.Add(line.ID, new MSkillLine
                (
                    line.ID,
                    line.SkillSetID == 0 ? null : line.SkillSetID,
                    line.SkillSetID == 0 ? null : line.SkillSetIndex,
                    line.SkillCategoryIdEx == 0 ? null : line.SkillCategoryIdEx,
                    line.SkillCategoryIdEx == 0 ? null : line.SkillCategoryIndexEx,
                    line.SkillPoints,
                    name,
                    description,
                    line.IconID,
                    hasDefaultImage ? defaultImage : null,
                    hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null
                ));
            }

            await dbContext.UpsertCollectionAsync(builtLines.Values, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build the SkillLine collection");
        }
    }

    private async Task BuildSkillSetsAsync(ICollectionsContext dbContext, CancellationToken ct)
    {
        try
        {
            if (_clientDataCache.SkillSets is null)
                throw new MissingCacheDataException(typeof(SkillSet));

            Dictionary<uint, MSkillSet> builtSets = new();
            foreach (SkillSet set in _clientDataCache.SkillSets)
            {
                _localeDataCache.TryGetLocaleString(set.NameID, out LocaleString? name);
                _localeDataCache.TryGetLocaleString(set.DescriptionID, out LocaleString? description);
                bool hasDefaultImage = _imageSetHelper.TryGetDefaultImage(set.IconID, out uint defaultImage);

                builtSets.TryAdd(set.ID, new MSkillSet
                (
                    set.ID,
                    set.SkillPoints,
                    set.RequiredItemID == 0 ? null : set.RequiredItemID,
                    name,
                    description,
                    set.IconID,
                    hasDefaultImage ? defaultImage : null,
                    hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null
                ));
            }

            await dbContext.UpsertCollectionAsync(builtSets.Values, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build the SkillSet collection");
        }
    }
}
