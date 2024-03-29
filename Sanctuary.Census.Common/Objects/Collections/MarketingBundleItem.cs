﻿using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using System.ComponentModel;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents an item from a marketing_bundle
/// </summary>
/// <param name="MarketingBundleID">The ID of the bundle that this item belongs to.</param>
/// <param name="ItemID">The ID of the item.</param>
/// <param name="Quantity">The amount of the item that is granted when purchasing the bundle.</param>
/// <param name="ReleaseTime">The time that the bundle was released.</param>
[Collection]
[Description("Represents an item from a marketing_bundle")]
public record MarketingBundleItem
(
    [property: JoinKey] uint MarketingBundleID,
    [property: JoinKey] uint ItemID,
    uint Quantity,
    ulong ReleaseTime
) : ISanctuaryCollection;
