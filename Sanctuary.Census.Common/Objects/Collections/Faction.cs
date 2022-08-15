using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents faction data.
/// </summary>
/// <param name="FactionID">The ID of the faction.</param>
/// <param name="Name">The faction's name.</param>
/// <param name="ShortName">The faction's abbreviated name.</param>
/// <param name="Description">The faction's description.</param>
/// <param name="ImageSetID">The image set ID of the faction's image set.</param>
/// <param name="ImageID">The ID of the faction's default icon image.</param>
/// <param name="ImagePath">The relative path to the faction's default icon image.</param>
/// <param name="HUDTintRGB">The default HUD tint of the faction.</param>
/// <param name="CodeTag">The code tag of the faction.</param>
/// <param name="UserSelectable">Indicates whether a user can select to play as this faction.</param>
[Collection]
public record Faction
(
    [property:Key] uint FactionID,
    LocaleString Name,
    LocaleString? ShortName,
    LocaleString? Description,
    uint? ImageSetID,
    uint? ImageID,
    string? ImagePath,
    uint HUDTintRGB,
    string CodeTag,
    bool UserSelectable
);
