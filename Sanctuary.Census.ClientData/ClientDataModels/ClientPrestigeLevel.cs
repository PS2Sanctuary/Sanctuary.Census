using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client prestige level data.
/// </summary>
/// <param name="Id">The ID of the prestige level.</param>
/// <param name="PrestigeLevel">The in-game/'true' character level.</param>
[Datasheet]
public partial record ClientPrestigeLevel
(
    uint Id,
    int PrestigeLevel
);
