using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client fish rarity data.
/// </summary>
/// <param name="Id">The ID of the fish rarity level.</param>
/// <param name="NameId">The locale ID of the rarity level's name.</param>
[Datasheet]
public partial record FishRarity
(
    uint Id,
    uint NameId
);
