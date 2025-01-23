using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client fish size data.
/// </summary>
/// <param name="Id">The ID of the fish size level.</param>
/// <param name="NameId">The locale ID of the size level's name.</param>
[Datasheet]
public partial record FishSizeType
(
    uint Id,
    uint NameId
);
