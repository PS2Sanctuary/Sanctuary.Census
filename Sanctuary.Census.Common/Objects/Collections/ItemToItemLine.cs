using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using System.ComponentModel;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a mapping between items and item lines.
/// </summary>
/// <param name="ItemId">The ID of the item.</param>
/// <param name="ItemLineId">The ID of the item line that the item belongs to.</param>
/// <param name="ItemLineIndex">The position of the item within the line.</param>
[Collection]
[Description(
    "Contains mappings between items and the 'lines' they belong to. This can be used, for example, to link together " +
    "upgradable items such as vehicle attachments.")]
public record ItemToItemLine
(
    [property: JoinKey] uint ItemId,
    [property: JoinKey] uint ItemLineId,
    byte ItemLineIndex
) : ISanctuaryCollection;
