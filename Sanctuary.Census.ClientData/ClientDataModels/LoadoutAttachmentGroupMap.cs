using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents loadout attachment group mappings (weapon attachments).
/// </summary>
/// <param name="ItemType">The type of the item.</param>
/// <param name="ItemTypeGroupID">The group that the item type belongs to.</param>
/// <param name="AttachmentID">The ID of the attachment item.</param>
/// <param name="ItemID">The ID of the item.</param>
[Datasheet]
public partial record LoadoutAttachmentGroupMap
(
    uint ItemType,
    uint ItemTypeGroupID,
    uint AttachmentID,
    uint ItemID
);
