using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client loadout attachment data.
/// </summary>
/// <param name="Id">The ID of the attachment.</param>
/// <param name="GroupId">The ID of the group that the attachment belongs to.</param>
/// <param name="ItemLineId">The ID of the attachment's item line.</param>
/// <param name="FlagRequired">Indicates whether the attachment must be equipped.</param>
[Datasheet]
public partial record LoadoutAttachment
(
    uint Id,
    uint GroupId,
    uint ItemLineId,
    bool FlagRequired
);
