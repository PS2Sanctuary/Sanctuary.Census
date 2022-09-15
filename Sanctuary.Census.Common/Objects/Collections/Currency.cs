using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents currency data.
/// </summary>
/// <param name="CurrencyID">The ID of the currency.</param>
/// <param name="Name">The currency's name.</param>
/// <param name="Description">The currency's description.</param>
/// <param name="InventoryCap">The maximum amount of this currency a character may have.</param>
/// <param name="IconID">The image set ID of the currency's icon.</param>
/// <param name="ImageSetID">The image set ID of the currency's icon.</param>
/// <param name="ImageID">The ID of the currency's default image.</param>
/// <param name="ImagePath">The relative path to the currency's default image.</param>
/// <param name="MapIconImageSetID">The image set ID of the currency's map icon.</param>
[Collection]
public record Currency
(
    [property:Key] uint CurrencyID,
    LocaleString Name,
    LocaleString? Description,
    uint? InventoryCap,
    uint IconID,
    [property: Key] uint ImageSetID,
    [property: Key] uint? ImageID,
    string? ImagePath,
    uint MapIconImageSetID
) : ISanctuaryCollection;
