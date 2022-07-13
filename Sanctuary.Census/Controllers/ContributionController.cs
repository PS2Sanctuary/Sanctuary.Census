using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.Objects.ClientDataModels;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Common.Objects.DtoModels;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Sanctuary.Census.Controllers;

/// <summary>
/// Returns data built through the contribution factory.
/// </summary>
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class ContributionController
{
    private readonly ILogger<ContributionController> _logger;
    private readonly IClientDataCacheService _clientDataCache;
    private readonly ILocaleService _localeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContributionController"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    public ContributionController
    (
        ILogger<ContributionController> logger,
        IClientDataCacheService clientDataCache,
        ILocaleService localeService
    )
    {
        _logger = logger;
        _clientDataCache = clientDataCache;
        _localeService = localeService;
    }

    /// <summary>
    /// Builds the Item collection using the <see cref="IClientDataCacheService"/>.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The built collection.</returns>
    [HttpGet("client-cache")]
    public async IAsyncEnumerable<Item> GetFromCacheServiceAsync([EnumeratorCancellation] CancellationToken ct)
    {
        int counter = 0;

        if (_clientDataCache.LastPopulated < DateTimeOffset.UtcNow.AddMinutes(5))
            await _clientDataCache.Repopulate(ct).ConfigureAwait(false);

        Dictionary<uint, FactionDefinition> itemFactionMap = new();
        foreach (ItemProfile profile in _clientDataCache.ItemProfiles)
        {
            itemFactionMap.TryAdd(profile.ItemID, profile.FactionID);
            if (itemFactionMap[profile.ItemID] != profile.FactionID)
                itemFactionMap[profile.ItemID] = FactionDefinition.All;
        }

        Dictionary<uint, uint> imageSetToPrimaryImageMap = new();
        foreach (ImageSetMapping mapping in _clientDataCache.ImageSetMappings)
        {
            if (mapping.ImageType is not ImageSetType.Large)
                continue;

            imageSetToPrimaryImageMap[mapping.ImageSetID] = mapping.ImageID;
        }

        foreach (ClientItemDefinition definition in _clientDataCache.ClientItemDefinitions)
        {
            LocaleString? name = definition.NameID == -1
                ? null
                : await _localeService.GetLocaleStringAsync((uint)definition.NameID, ct).ConfigureAwait(false);

            LocaleString? description = definition.DescriptionID == -1
                ? null
                : await _localeService.GetLocaleStringAsync((uint)definition.DescriptionID, ct).ConfigureAwait(false);

            if (!itemFactionMap.TryGetValue(definition.ID, out FactionDefinition faction))
                faction = FactionDefinition.All;

            uint? imageID = null;
            string? imagePath = null;
            if (definition.ImageSetID > 0)
            {
                if (imageSetToPrimaryImageMap.ContainsKey((uint)definition.ImageSetID))
                    imageID = imageSetToPrimaryImageMap[(uint)definition.ImageSetID];
                imagePath = $"/files/ps2/images/static/{imageID}.png";
            }

            yield return new Item
            (
                definition.ID,
                definition.ItemType,
                definition.CategoryID,
                faction,
                name,
                description,
                definition.ActivatableAbilityID == 0 ? null : definition.ActivatableAbilityID,
                definition.PassiveAbilityID == 0 ? null : definition.PassiveAbilityID,
                definition.PassiveAbilitySetID == 0 ? null: definition.PassiveAbilitySetID,
                definition.SkillSetID == 0 ? null : definition.SkillSetID,
                definition.ImageSetID <= 0 ? null : (uint)definition.ImageSetID,
                imageID,
                imagePath,
                definition.HudImageSetID == 0 ? null : definition.HudImageSetID,
                definition.MaxStackSize,
                definition.FlagAccountScope
            );

            if (counter++ >= 100)
                break;
        }
    }
}
