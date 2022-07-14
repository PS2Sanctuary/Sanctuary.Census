namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client currency data.
/// </summary>
/// <param name="ID">The ID of the currency.</param>
/// <param name="NameID">The locale string ID of the currency's name.</param>
/// <param name="DescriptionID">The locale string ID of the currency's description.</param>
/// <param name="IconID">The image set ID of the currency's icon.</param>
/// <param name="MapIconID">The image set ID of the currency's map icon.</param>
/// <param name="ValueMax">The maximum amount of this currency a character may have.</param>
/// <param name="ScarcityValue">Unknown.</param>
/// <param name="ContentID">Unknown.</param>
/// <param name="ExemptFromBuffs">Unknown.</param>
/// <param name="PriorityUpdate">Unknown.</param>
/// <param name="BuffCharacterStatID">The ID of the stat that this currency buffs.</param>
public record Currency
(
    uint ID,
    uint NameID,
    uint DescriptionID,
    uint IconID,
    uint MapIconID,
    uint ValueMax,
    uint ScarcityValue,
    uint ContentID,
    bool ExemptFromBuffs,
    bool PriorityUpdate,
    uint BuffCharacterStatID
);
