using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Exceptions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MFaction = Sanctuary.Census.Models.Collections.Faction;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MFaction"/> collection.
/// </summary>
public class FactionCollectionBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;
    private readonly ILocaleDataCacheService _localeDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="FactionCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    public FactionCollectionBuilder
    (
        IClientDataCacheService clientDataCache,
        ILocaleDataCacheService localeDataCache
    )
    {
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
        if (_clientDataCache.Factions is null)
            throw new MissingCacheDataException(typeof(Faction));

        if (_clientDataCache.ImageSetMappings is null)
            throw new MissingCacheDataException(typeof(ImageSetMapping));

        Dictionary<uint, uint> imageSetToPrimaryImageMap = new();
        foreach (ImageSetMapping mapping in _clientDataCache.ImageSetMappings)
        {
            if (mapping.ImageType is not ImageSetType.Massive)
                continue;

            imageSetToPrimaryImageMap[mapping.ImageSetID] = mapping.ImageID;
        }

        Dictionary<uint, MFaction> builtItems = new();
        foreach (Faction faction in _clientDataCache.Factions)
        {
            _localeDataCache.TryGetLocaleString(faction.NameID, out LocaleString? name);
            _localeDataCache.TryGetLocaleString(faction.DescriptionTextID, out LocaleString? description);
            _localeDataCache.TryGetLocaleString(faction.ShortNameID, out LocaleString? shortName);

            uint? imageID = null;
            string? imagePath = null;
            if (faction.IconID > 0)
            {
                if (imageSetToPrimaryImageMap.ContainsKey(faction.IconID))
                {
                    imageID = imageSetToPrimaryImageMap[faction.IconID];
                    imagePath = $"/files/ps2/images/static/{imageID}.png";
                }
            }

            MFaction built = new
            (
                faction.ID,
                name!,
                shortName,
                description,
                faction.IconID > 0 ? faction.IconID : null,
                imageID,
                imagePath,
                faction.HUDTintRGB,
                faction.CodeTag,
                faction.UserSelectable
            );
            builtItems.TryAdd(built.FactionID, built);
        }

        await dbContext.UpsertFactionsAsync(builtItems.Values, ct);
    }
}
