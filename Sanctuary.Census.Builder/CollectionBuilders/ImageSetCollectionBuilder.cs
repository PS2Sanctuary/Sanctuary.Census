using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MImageSet = Sanctuary.Census.Common.Objects.Collections.ImageSet;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MImageSet"/> collection.
/// </summary>
public class ImageSetCollectionBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageSetCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    public ImageSetCollectionBuilder
    (
        IClientDataCacheService clientDataCache
    )
    {
        _clientDataCache = clientDataCache;
    }

    /// <inheritdoc />
    public async Task BuildAsync
    (
        ICollectionsContext dbContext,
        CancellationToken ct
    )
    {
        if (_clientDataCache.Images is null)
            throw new MissingCacheDataException(typeof(Image));

        if (_clientDataCache.ImageSets is null)
            throw new MissingCacheDataException(typeof(ImageSet));

        if (_clientDataCache.ImageSetMappings is null)
            throw new MissingCacheDataException(typeof(ImageSetMapping));

        if (_clientDataCache.ImageSetTypes is null)
            throw new MissingCacheDataException(typeof(ImageSetType));

        Dictionary<uint, Image> images = _clientDataCache.Images
            .ToDictionary(x => x.ID);

        Dictionary<uint, List<ImageSetMapping>> maps = new();
        foreach (ImageSetMapping map in _clientDataCache.ImageSetMappings)
        {
            if (!maps.ContainsKey(map.ImageSetID))
                maps.Add(map.ImageSetID, new List<ImageSetMapping>());
            maps[map.ImageSetID].Add(map);
        }

        Dictionary<ImageSetType.Type, ImageSetType> types = _clientDataCache.ImageSetTypes
            .ToDictionary(x => x.ID);

        List<MImageSet> builtSets = new();
        foreach (ImageSet set in _clientDataCache.ImageSets)
        {
            if (!maps.ContainsKey(set.ID))
                continue;

            foreach (ImageSetMapping map in maps[set.ID])
            {
                if (!images.ContainsKey(map.ImageID))
                    continue;

                if (string.IsNullOrEmpty(images[map.ImageID].FileName))
                    continue;

                builtSets.Add(new MImageSet
                (
                    set.ID,
                    map.ImageID,
                    set.Description,
                    (uint)map.ImageType,
                    types.TryGetValue(map.ImageType, out ImageSetType? st) ? st.Description : null,
                    $"/files/ps2/images/static/{map.ImageID}.png"
                ));
            }
        }

        await dbContext.UpsertImageSetsAsync(builtSets, ct).ConfigureAwait(false);
    }
}
