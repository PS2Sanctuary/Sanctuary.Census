using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MResist = Sanctuary.Census.Common.Objects.Collections.ResistInfo;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MResist"/> collection.
/// </summary>
public class ResistInfoCollectionBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;
    private readonly IImageSetHelperService _imageSetHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResistInfoCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="imageSetHelper">The image set helper service.</param>
    public ResistInfoCollectionBuilder
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
        if (_clientDataCache.ResistInfos is null)
            throw new MissingCacheDataException(typeof(ResistInfo));

        List<MResist> builtItems = new();
        foreach (ResistInfo resist in _clientDataCache.ResistInfos)
        {

            bool hasDefaultImage = _imageSetHelper.TryGetDefaultImage(resist.ResistIcon, out uint defaultImage);
            MResist built = new
            (
                resist.ID,
                resist.ResistType,
                resist.ResistPercent,
                resist.ResistAmount.ToNullableInt(),
                resist.MultiplierWhenHeadshot,
                resist.ResistIcon.ToNullableUInt(),
                hasDefaultImage ? defaultImage : null,
                hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null
            );
            builtItems.Add(built);
        }

        await dbContext.UpsertCollectionAsync(builtItems, ct).ConfigureAwait(false);
    }
}
