using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client faction data.
/// </summary>
/// <param name="ID">The ID of the faction.</param>
/// <param name="NameID">The locale string ID of the faction's name.</param>
/// <param name="IconID">The image set ID of the faction's icon.</param>
/// <param name="HUDTintRGB">The default HUD tint of this faction.</param>
/// <param name="TextureAlias">Unknown.</param>
/// <param name="EmpireTintAlias">Unknown.</param>
/// <param name="UserSelectable">Indicates whether this faction can be selected by the user.</param>
/// <param name="DescriptionTextID">The locale string ID of the faction's description.</param>
/// <param name="CodeTag">The code tag of this faction.</param>
/// <param name="OverseerSpeechPackID">The ID of this faction's overseer speech pack.</param>
/// <param name="ShortNameID">The locale string ID of the faction's short name.</param>
[Datasheet]
public partial record Faction
(
    uint ID,
    uint NameID,
    uint IconID,
    uint HUDTintRGB,
    string TextureAlias,
    string EmpireTintAlias,
    bool UserSelectable,
    uint DescriptionTextID,
    string CodeTag,
    uint OverseerSpeechPackID,
    uint ShortNameID
);
