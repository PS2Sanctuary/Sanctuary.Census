using Sanctuary.Census.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Models.Collections;

/// <summary>
/// Represents a mapping between weapon and attachment items.
/// </summary>
/// <param name="WeaponGroupID">The item type group that the weapon belongs to.</param>
/// <param name="AttachmentID">The ID of the attachment item.</param>
/// <param name="ItemID">The ID of the item.</param>
[Collection]
public record WeaponToAttachment
(
    uint WeaponGroupID,
    [property: Key] uint AttachmentID,
    [property: Key] uint ItemID
);
