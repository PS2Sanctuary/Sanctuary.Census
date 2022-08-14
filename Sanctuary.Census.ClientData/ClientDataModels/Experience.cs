using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client experience data.
/// </summary>
/// <param name="ID">The ID of the experience point.</param>
/// <param name="AwardTypeID">The ID of the award that this experience point grants.</param>
/// <param name="StringID">The locale string ID of this experience point.</param>
/// <param name="StringIDSecondary">The secondary locale string ID of this experience point.</param>
/// <param name="XP">The amount of XP granted by this experience point.</param>
/// <param name="NotableEvent">Indicates whether this experience point denotes an important player action.</param>
[Datasheet]
public partial record Experience
(
    uint ID,
    uint AwardTypeID,
    uint StringID,
    uint StringIDSecondary,
    float XP,
    bool NotableEvent
);
