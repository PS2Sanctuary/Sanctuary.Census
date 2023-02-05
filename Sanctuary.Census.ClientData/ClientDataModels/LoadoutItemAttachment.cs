using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client loadout item attachment data.
/// </summary>
/// <param name="ItemLineId">The ID of the item line that the attachment can be attached to.</param>
/// <param name="ItemFlagQuickUse">Unknown.</param>
/// <param name="ItemFlagCanEquip">Unknown.</param>
/// <param name="AttachmentId">The ID of the attachment.</param>
[Datasheet]
public partial record LoadoutItemAttachment
(
    uint ItemLineId,
    bool ItemFlagQuickUse,
    bool ItemFlagCanEquip,
    uint AttachmentId
);
