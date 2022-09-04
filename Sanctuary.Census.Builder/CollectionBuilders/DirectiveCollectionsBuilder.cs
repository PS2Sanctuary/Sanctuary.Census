using Microsoft.Extensions.Logging;
using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.Directive;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MCategory = Sanctuary.Census.Common.Objects.Collections.DirectiveTreeCategory;
using MTree = Sanctuary.Census.Common.Objects.Collections.DirectiveTree;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MCategory"/> and <see cref="MTree"/> collections.
/// </summary>
public class DirectiveCollectionsBuilder : ICollectionBuilder
{
    private readonly ILogger<DirectiveCollectionsBuilder> _logger;
    private readonly IClientDataCacheService _clientDataCache;
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IServerDataCacheService _serverDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="DirectiveCollectionsBuilder"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    public DirectiveCollectionsBuilder
    (
        ILogger<DirectiveCollectionsBuilder> logger,
        IClientDataCacheService clientDataCache,
        ILocaleDataCacheService localeDataCache,
        IServerDataCacheService serverDataCache
    )
    {
        _logger = logger;
        _clientDataCache = clientDataCache;
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
        if (_clientDataCache.ImageSetMappings is null)
            throw new MissingCacheDataException(typeof(ImageSetMapping));

        Dictionary<uint, ImageSetMapping> defaultImages = new();
        foreach (ImageSetMapping mapping in _clientDataCache.ImageSetMappings)
        {
            if (!defaultImages.TryAdd(mapping.ImageSetID, mapping) && mapping.ImageType is ImageSetType.Type.Large)
                defaultImages[mapping.ImageSetID] = mapping;
        }

        await BuildCategoriesAsync(dbContext, ct).ConfigureAwait(false);
        await BuildTreesAsync(dbContext, defaultImages, ct).ConfigureAwait(false);
    }

    private async Task BuildCategoriesAsync(ICollectionsContext dbContext, CancellationToken ct)
    {
        try
        {
            if (_serverDataCache.DirectiveData is null)
                throw new MissingCacheDataException(typeof(DirectiveInitialize));

            Dictionary<uint, MCategory> builtCategories = new();
            foreach ((Sanctuary.Common.Objects.FactionDefinition faction, DirectiveInitialize data) in _serverDataCache.DirectiveData)
            {
                foreach (DirectiveTreeCategory category in data.Categories)
                {
                    _localeDataCache.TryGetLocaleString(category.NameID, out LocaleString? name);

                    if (builtCategories.TryGetValue(category.DirectiveCategoryID, out MCategory? prevBuilt))
                    {
                        prevBuilt.FactionIds.Add((uint)faction);
                    }
                    else
                    {
                        builtCategories.Add(category.DirectiveCategoryID, new MCategory
                        (
                            category.DirectiveCategoryID,
                            new List<uint> { (uint)faction },
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

    private async Task BuildTreesAsync
    (
        ICollectionsContext dbContext,
        IReadOnlyDictionary<uint, ImageSetMapping> defaultImages,
        CancellationToken ct
    )
    {
        try
        {
            if (_serverDataCache.DirectiveData is null)
                throw new MissingCacheDataException(typeof(DirectiveInitialize));

            Dictionary<uint, MTree> builtTrees = new();
            foreach ((Sanctuary.Common.Objects.FactionDefinition faction, DirectiveInitialize data) in _serverDataCache.DirectiveData)
            {
                foreach (DirectiveTree tree in data.Trees)
                {
                    _localeDataCache.TryGetLocaleString(tree.NameID, out LocaleString? name);
                    _localeDataCache.TryGetLocaleString(tree.DescriptionID, out LocaleString? description);
                    defaultImages.TryGetValue(tree.ImageSetID, out ImageSetMapping? defaultImage);

                    if (builtTrees.TryGetValue(tree.DirectiveTreeID_1, out MTree? prevBuilt))
                    {
                        prevBuilt.FactionIds.Add((uint)faction);
                    }
                    else
                    {
                        builtTrees.Add(tree.DirectiveTreeID_1, new MTree
                        (
                            tree.DirectiveTreeID_1,
                            tree.DirectiveTreeCategoryID,
                            new List<uint> { (uint)faction },
                            name!,
                            description,
                            tree.ImageSetID,
                            defaultImage?.ImageID ?? null,
                            defaultImage is null ? null : $"/files/ps2/images/static/{defaultImage.ImageID}.png"
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
}
