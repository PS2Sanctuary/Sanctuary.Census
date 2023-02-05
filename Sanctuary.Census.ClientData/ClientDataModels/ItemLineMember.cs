using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client item line member data.
/// </summary>
/// <param name="ItemId">The ID of the item in the relationship.</param>
/// <param name="ItemLineId">The ID of the line the item belongs to.</param>
/// <param name="ItemLineIndex">The position in the line of the item.</param>
[Datasheet]
public partial record ItemLineMember
(
    uint ItemId,
    uint ItemLineId,
    byte ItemLineIndex
);
