using Microsoft.Extensions.Logging;
using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.Directive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FactionDefinition = Sanctuary.Common.Objects.FactionDefinition;
using MCategory = Sanctuary.Census.Common.Objects.Collections.DirectiveTreeCategory;
using MDirective = Sanctuary.Census.Common.Objects.Collections.Directive;
using MReward = Sanctuary.Census.Common.Objects.Collections.DirectiveTierReward;
using MRewardSet = Sanctuary.Census.Common.Objects.Collections.DirectiveTierRewardSet;
using MTier = Sanctuary.Census.Common.Objects.Collections.DirectiveTier;
using MTree = Sanctuary.Census.Common.Objects.Collections.DirectiveTree;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MCategory"/>, <see cref="MTier"/> and <see cref="MTree"/> collections.
/// </summary>
public class DirectiveCollectionsBuilder : ICollectionBuilder
{
    private readonly ILogger<DirectiveCollectionsBuilder> _logger;
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IServerDataCacheService _serverDataCache;
    private readonly IImageSetHelperService _imageSetHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectiveCollectionsBuilder"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    /// <param name="imageSetHelper">The image set helper service.</param>
    public DirectiveCollectionsBuilder
    (
        ILogger<DirectiveCollectionsBuilder> logger,
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
        if (_serverDataCache.DirectiveData.Count == 0)
            throw new MissingCacheDataException(typeof(DirectiveInitialize));

        bool hasAllFactions = _serverDataCache.DirectiveData.ContainsKey(FactionDefinition.VS)
            && _serverDataCache.DirectiveData.ContainsKey(FactionDefinition.NC)
            && _serverDataCache.DirectiveData.ContainsKey(FactionDefinition.TR)
            && _serverDataCache.DirectiveData.ContainsKey(FactionDefinition.NSO);

        if (!hasAllFactions)
        {
            throw new Exception
            (
                "Missing a faction. Present: " +
                string.Join(", ", _serverDataCache.DirectiveData.Keys)
            );
        }

        await BuildCategoriesAsync(dbContext, ct).ConfigureAwait(false);
        await BuildTreesAsync(dbContext, ct).ConfigureAwait(false);
        await BuildTiersAsync(dbContext, ct).ConfigureAwait(false);
        await BuildDirectivesAsync(dbContext, ct).ConfigureAwait(false);
        await BuildRewardsAsync(dbContext, ct).ConfigureAwait(false);
    }

    private async Task BuildCategoriesAsync(ICollectionsContext dbContext, CancellationToken ct)
    {
        try
        {
            Dictionary<uint, MCategory> builtCategories = new();
            foreach ((FactionDefinition faction, DirectiveInitialize data) in _serverDataCache.DirectiveData)
            {
                foreach (DirectiveTreeCategory category in data.Categories)
                {
                    ct.ThrowIfCancellationRequested();

                    _localeDataCache.TryGetLocaleString(category.NameID, out LocaleString? name);

                    if (builtCategories.TryGetValue(category.DirectiveCategoryID, out MCategory? prevBuilt))
                    {
                        prevBuilt.FactionIds.Add((uint)faction);
                        prevBuilt.FactionIds.Sort();
                    }
                    else
                    {
                        builtCategories.Add(category.DirectiveCategoryID, new MCategory
                        (
                            category.DirectiveCategoryID,
                            new ValueEqualityList<uint> { (uint)faction },
                            name!,
                            category.DisplayOrder
                        ));
                    }
                }
            }

            await dbContext.UpsertCollectionAsync(builtCategories.Values, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build the DirectiveTreeCategory collection");
        }
    }

    private async Task BuildTreesAsync(ICollectionsContext dbContext, CancellationToken ct)
    {
        try
        {
            Dictionary<uint, MTree> builtTrees = new();
            foreach ((FactionDefinition faction, DirectiveInitialize data) in _serverDataCache.DirectiveData)
            {
                foreach (DirectiveTree tree in data.Trees)
                {
                    ct.ThrowIfCancellationRequested();

                    // Plenty of null trees. No idea why
                    if (tree.DirectiveTreeID_2 == 0)
                        continue;

                    _localeDataCache.TryGetLocaleString(tree.NameID, out LocaleString? name);
                    _localeDataCache.TryGetLocaleString(tree.DescriptionID, out LocaleString? description);
                    bool hasDefaultImage = _imageSetHelper.TryGetDefaultImage(tree.ImageSetID, out uint defaultImage);

                    if (builtTrees.TryGetValue(tree.DirectiveTreeID_1, out MTree? prevBuilt))
                    {
                        prevBuilt.FactionIds.Add((uint)faction);
                        prevBuilt.FactionIds.Sort();
                    }
                    else
                    {
                        builtTrees.Add(tree.DirectiveTreeID_1, new MTree
                        (
                            tree.DirectiveTreeID_1,
                            tree.DirectiveTreeCategoryID,
                            new ValueEqualityList<uint> { (uint)faction },
                            name!,
                            description,
                            tree.ImageSetID,
                            hasDefaultImage ? defaultImage : null,
                            hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null
                        ));
                    }
                }
            }

            await dbContext.UpsertCollectionAsync(builtTrees.Values, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build the DirectiveTree collection");
        }
    }

    private async Task BuildTiersAsync(ICollectionsContext dbContext, CancellationToken ct)
    {
        try
        {
            Dictionary<uint, List<MTier>> builtTiers = new();
            foreach (DirectiveInitialize data in _serverDataCache.DirectiveData.Values)
            {
                foreach (DirectiveTree tree in data.Trees)
                {
                    ct.ThrowIfCancellationRequested();

                    // Plenty of null trees. No idea why
                    if (tree.DirectiveTreeID_2 == 0)
                        continue;

                    if (builtTiers.ContainsKey(tree.DirectiveTreeID_1))
                        continue;

                    List<MTier> treeTiers = new();
                    foreach (DirectiveTreeTier tier in tree.Tiers)
                    {
                        _localeDataCache.TryGetLocaleString(tier.NameID, out LocaleString? name);
                        bool hasDefaultImage = _imageSetHelper.TryGetDefaultImage(tree.ImageSetID, out uint defaultImage);

                        treeTiers.Add(new MTier
                        (
                            tree.DirectiveTreeID_1,
                            tier.TierID,
                            name!,
                            tier.Reward.RewardSetID == 0 ? null : tier.Reward.RewardSetID,
                            tier.DirectivePoints,
                            tier.RequiredObjectiveCompletionCount,
                            tier.ImageSetID,
                            hasDefaultImage ? defaultImage : null,
                            hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null
                        ));
                    }

                    builtTiers.Add(tree.DirectiveTreeID_1, treeTiers);
                }
            }

            await dbContext.UpsertCollectionAsync(builtTiers.Values.SelectMany(x => x), ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build the DirectiveTree collection");
        }
    }

    private async Task BuildDirectivesAsync(ICollectionsContext dbContext, CancellationToken ct)
    {
        try
        {
            Dictionary<uint, MDirective> builtDirectives = new();
            foreach ((FactionDefinition faction, DirectiveInitialize data) in _serverDataCache.DirectiveData)
            {
                foreach (DirectiveTree tree in data.Trees)
                {
                    // Plenty of null trees. No idea why
                    if (tree.DirectiveTreeID_2 == 0)
                        continue;

                    foreach (DirectiveTreeTier tier in tree.Tiers)
                    {
                        foreach (DirectiveTreeTierObjective objective in tier.Objectives)
                        {
                            ct.ThrowIfCancellationRequested();
                            if (objective.ObjectiveID_2 == 0)
                                continue;

                            _localeDataCache.TryGetLocaleString(objective.NameID, out LocaleString? name);
                            _localeDataCache.TryGetLocaleString(objective.DescriptionID, out LocaleString? description);
                            bool hasDefaultImage = _imageSetHelper.TryGetDefaultImage(tree.ImageSetID, out uint defaultImage);

                            if (builtDirectives.TryGetValue(objective.ObjectiveID_1, out MDirective? prevBuilt))
                            {
                                prevBuilt.Factions.Add((byte)faction);
                                prevBuilt.Factions.Sort();
                            }
                            else
                            {
                                builtDirectives.Add(objective.ObjectiveID_1, new MDirective
                                (
                                    objective.ObjectiveID_1,
                                    tree.DirectiveTreeID_1,
                                    tier.TierID,
                                    new ValueEqualityList<byte> { (byte)faction },
                                    name,
                                    description,
                                    objective.ImageSetID,
                                    hasDefaultImage ? defaultImage : null,
                                    hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null
                                ));
                            }
                        }
                    }
                }
            }

            await dbContext.UpsertCollectionAsync(builtDirectives.Values, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build the DirectiveTree collection");
        }
    }

    private async Task BuildRewardsAsync(ICollectionsContext dbContext, CancellationToken ct)
    {
        try
        {
            Dictionary<uint, MRewardSet> builtSets = new();
            List<MReward> builtRewards = new();

            foreach (DirectiveInitialize data in _serverDataCache.DirectiveData.Values)
            {
                foreach (DirectiveTree tree in data.Trees)
                {
                    // Plenty of null trees. No idea why
                    if (tree.DirectiveTreeID_2 == 0)
                        continue;

                    foreach (RewardBundleData reward in tree.Tiers.Select(x => x.Reward))
                    {
                        ct.ThrowIfCancellationRequested();

                        if (reward.RewardSetID == 0 || builtSets.ContainsKey(reward.RewardSetID))
                            continue;

                        _localeDataCache.TryGetLocaleString(reward.NameID, out LocaleString? setName);
                        bool setHasDefaultImage = _imageSetHelper.TryGetDefaultImage(tree.ImageSetID, out uint setDefaultImage);

                        builtSets.Add(reward.RewardSetID, new MRewardSet
                        (
                            reward.RewardSetID,
                            setName,
                            reward.ImageSetID,
                            setHasDefaultImage ? setDefaultImage : null,
                            setHasDefaultImage ? _imageSetHelper.GetRelativeImagePath(setDefaultImage) : null
                        ));

                        foreach (RewardItem item in reward.Items)
                        {
                            _localeDataCache.TryGetLocaleString(item.NameID, out LocaleString? rewardName);
                            bool hasDefaultImage = _imageSetHelper.TryGetDefaultImage(tree.ImageSetID, out uint defaultImage);

                            builtRewards.Add(new MReward
                            (
                                reward.RewardSetID,
                                item.ItemID,
                                rewardName,
                                item.Quantity,
                                item.ImageSetID,
                                hasDefaultImage ? defaultImage : null,
                                hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null
                            ));
                        }
                    }
                }
            }

            await dbContext.UpsertCollectionAsync(builtSets.Values, ct).ConfigureAwait(false);
            await dbContext.UpsertCollectionAsync(builtRewards, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build the DirectiveTree collection");
        }
    }
}
