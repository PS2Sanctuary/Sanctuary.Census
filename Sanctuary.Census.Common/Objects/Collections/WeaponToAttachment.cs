using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a mapping between weapon and attachment items.
/// </summary>
/// <param name="WeaponGroupID">The item type group that the weapon belongs to.</param>
/// <param name="AttachmentID">The ID of the attachment item.</param>
/// <param name="ItemID">The ID of the item.</param>
[Collection]
public record WeaponToAttachment
(
    [property: Key] uint WeaponGroupID,
    [property: Key] uint AttachmentID,
    [property: Key] uint ItemID
) : ISanctuaryCollection;
