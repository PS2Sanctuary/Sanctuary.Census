using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using System.Collections.Generic;

namespace Sanctuary.Census.Builder.Services;

/// <inheritdoc />
public class ImageSetHelperService : IImageSetHelperService
{
    private readonly IClientDataCacheService _clientDataCache;

    private IReadOnlyDictionary<uint, ImageSetMapping>? _defaultImageMappings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageSetHelperService"/>.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    public ImageSetHelperService(IClientDataCacheService clientDataCache)
    {
        _clientDataCache = clientDataCache;
    }

    /// <inheritdoc />
    public bool TryGetDefaultImage(uint imageSetID, out uint imageID)
    {
        imageID = 0;
        if (imageSetID == 0)
            return false;

        if (_defaultImageMappings is null)
        {
            if (_clientDataCache.ImageSetMappings is null)
                throw new MissingCacheDataException(typeof(ImageSetMapping));

            Dictionary<uint, ImageSetMapping> defaultMappings = new();
            foreach (ImageSetMapping set in _clientDataCache.ImageSetMappings)
            {
                if (!defaultMappings.TryGetValue(set.ImageSetID, out ImageSetMapping? existing))
                {
                    defaultMappings.Add(set.ImageSetID, set);
                    continue;
                }

                if (existing.ImageType == ImageSetType.Type.Large || existing.ImageType > set.ImageType)
                    continue;

                defaultMappings[set.ImageSetID] = set;
            }

            _defaultImageMappings = defaultMappings;
        }

        if (!_defaultImageMappings.TryGetValue(imageSetID, out ImageSetMapping? defaultMapping))
            return false;

        imageID = defaultMapping.ImageID;
        return true;
    }

    /// <inheritdoc />
    public string GetRelativeImagePath(uint imageID)
        => $"/files/ps2/images/static/{imageID}.png";
}
