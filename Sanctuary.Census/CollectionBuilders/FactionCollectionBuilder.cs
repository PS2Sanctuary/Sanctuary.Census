using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using System.Collections.Generic;
using CFaction = Sanctuary.Census.ClientData.ClientDataModels.Faction;
using MFaction = Sanctuary.Census.CollectionModels.Faction;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MFaction"/> collection.
/// </summary>
public class FactionCollectionBuilder : ICollectionBuilder
{
    /// <inheritdoc />
    public void Build
    (
        IClientDataCacheService clientDataCache,
        IServerDataCacheService serverDataCache,
        ILocaleService localeService,
        CollectionsContext context
    )
    {
        if (clientDataCache.Factions.Count == 0)
            throw new MissingCacheDataException(typeof(CFaction));

        if (clientDataCache.ImageSetMappings.Count == 0)
            throw new MissingCacheDataException(typeof(ClientData.ClientDataModels.ImageSetMapping));

        Dictionary<uint, uint> imageSetToPrimaryImageMap = new();
        foreach (ClientData.ClientDataModels.ImageSetMapping mapping in clientDataCache.ImageSetMappings)
            imageSetToPrimaryImageMap[mapping.ImageSetID] = mapping.ImageID;

        Dictionary<uint, MFaction> builtItems = new();
        foreach (CFaction faction in clientDataCache.Factions)
        {
            localeService.TryGetLocaleString(faction.NameID, out LocaleString? name);
            localeService.TryGetLocaleString(faction.DescriptionTextID, out LocaleString? description);
            localeService.TryGetLocaleString(faction.ShortNameID, out LocaleString? shortName);

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

        context.Factions = builtItems;
    }
}
