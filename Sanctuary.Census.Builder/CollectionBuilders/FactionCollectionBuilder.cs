using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MFaction = Sanctuary.Census.Common.Objects.Collections.Faction;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MFaction"/> collection.
/// </summary>
public class FactionCollectionBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IImageSetHelperService _imageSetHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="FactionCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="imageSetHelper">The image set helper service.</param>
    public FactionCollectionBuilder
    (
        IClientDataCacheService clientDataCache,
        ILocaleDataCacheService localeDataCache,
        IImageSetHelperService imageSetHelper
    )
    {
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
        if (_clientDataCache.Factions is null)
            throw new MissingCacheDataException(typeof(Faction));

        Dictionary<uint, MFaction> builtItems = new();
        foreach (Faction faction in _clientDataCache.Factions)
        {
            _localeDataCache.TryGetLocaleString(faction.NameID, out LocaleString? name);
            _localeDataCache.TryGetLocaleString(faction.DescriptionTextID, out LocaleString? description);
            _localeDataCache.TryGetLocaleString(faction.ShortNameID, out LocaleString? shortName);

            bool hasDefaultImage = _imageSetHelper.TryGetDefaultImage(faction.IconID, out uint defaultImage);
            MFaction built = new
            (
                faction.ID,
                name!,
                shortName,
                description,
                faction.IconID > 0 ? faction.IconID : null,
                hasDefaultImage ? defaultImage : null,
                hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null,
                faction.HUDTintRGB,
                faction.CodeTag,
                faction.UserSelectable
            );
            builtItems.TryAdd(built.FactionID, built);
        }

        await dbContext.UpsertCollectionAsync(builtItems.Values, ct).ConfigureAwait(false);
    }
}
