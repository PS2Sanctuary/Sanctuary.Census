using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using System.ComponentModel;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a mapping between an item and an attachment it may have.
/// </summary>
/// <param name="ItemId">The ID of the item.</param>
/// <param name="AttachmentItemId">The ID of the attachment item.</param>
/// <param name="IsDefaultAttachment">Indicates whether the attachment is equipped by default.</param>
/// <param name="IsRequiredAttachment">Indicates whether the attachment must always be equipped.</param>
[Collection]
[Description("Links items to any attachments they may have. Note that items which do not have any default attachments will not be included in this collection.")]
public record ItemAttachment
(
    [property: JoinKey] uint ItemId,
    [property: JoinKey] uint AttachmentItemId,
    bool IsDefaultAttachment,
    bool IsRequiredAttachment
) : ISanctuaryCollection;
