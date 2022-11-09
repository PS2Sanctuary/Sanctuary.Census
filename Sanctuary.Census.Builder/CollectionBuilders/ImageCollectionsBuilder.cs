using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MImage = Sanctuary.Census.Common.Objects.Collections.Image;
using MImageSet = Sanctuary.Census.Common.Objects.Collections.ImageSet;
using MImageSetDefault = Sanctuary.Census.Common.Objects.Collections.ImageSetDefault;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MImage"/> and <see cref="MImageSet"/> collections.
/// </summary>
public class ImageCollectionsBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;
    private readonly IImageSetHelperService _imageSetHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageCollectionsBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="imageSetHelper">The image set helper service.</param>
    public ImageCollectionsBuilder
    (
        IClientDataCacheService clientDataCache,
        IImageSetHelperService imageSetHelper
    )
    {
        _clientDataCache = clientDataCache;
        _imageSetHelper = imageSetHelper;
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

        // We need to maintain a list of pairs that we've already processed,
        // as the ImageSetMapping collection can sometimes contain duplicates
        // with different ImageTypes
        Dictionary<uint, HashSet<uint>> setPairs = new();
        List<MImageSet> builtSets = new();
        foreach (ImageSet set in _clientDataCache.ImageSets)
        {
            if (!maps.ContainsKey(set.ID))
                continue;

            // Setup a pairing table for this set
            setPairs.TryAdd(set.ID, new HashSet<uint>());
            HashSet<uint> existingPairs = setPairs[set.ID];

            foreach (ImageSetMapping map in maps[set.ID])
            {
                if (!images.ContainsKey(map.ImageID))
                    continue;

                if (string.IsNullOrEmpty(images[map.ImageID].FileName))
                    continue;

                // Don't overwrite an existing pair
                if (!existingPairs.Add(map.ImageID))
                    continue;

                builtSets.Add(new MImageSet
                (
                    set.ID,
                    map.ImageID,
                    set.Description,
                    (uint)map.ImageType,
                    types.TryGetValue(map.ImageType, out ImageSetType? st) ? st.Description : null,
                    _imageSetHelper.GetRelativeImagePath(map.ImageID)
                ));
            }
        }

        Dictionary<uint, MImage> builtImages = new();
        foreach (Image image in _clientDataCache.Images)
        {
            builtImages.Add(image.ID, new MImage
            (
                image.ID,
                System.IO.Path.GetFileNameWithoutExtension(image.FileName),
                _imageSetHelper.GetRelativeImagePath(image.ID)
            ));
        }
        await dbContext.UpsertCollectionAsync(builtImages.Values, ct).ConfigureAwait(false);

        Dictionary<uint, MImageSetDefault> builtDefaultSets = new();
        foreach (MImageSet set in builtSets)
        {
            if (!builtDefaultSets.TryGetValue(set.ImageSetID, out MImageSetDefault? existing))
            {
                builtDefaultSets.Add(set.ImageSetID, SetToDefault(set));
                continue;
            }

            if (existing.TypeID == (uint)ImageSetType.Type.Large || existing.TypeID > set.TypeID)
                continue;

            builtDefaultSets[set.ImageSetID] = SetToDefault(set);
        }

        await dbContext.UpsertCollectionAsync(builtDefaultSets.Values, ct).ConfigureAwait(false);
        // This must occur after upserting the default sets, as the builder clears the collection
        await dbContext.UpsertCollectionAsync(builtSets, ct).ConfigureAwait(false);
    }

    private static MImageSetDefault SetToDefault(MImageSet set)
        => new
        (
            set.ImageSetID,
            set.ImageID,
            set.Description,
            set.TypeID,
            set.TypeDescription,
            set.ImagePath
        );
}
