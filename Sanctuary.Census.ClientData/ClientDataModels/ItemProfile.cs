﻿using Sanctuary.Census.ClientData.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents a client item profile
/// </summary>
/// <param name="ItemID">The ID of the item.</param>
/// <param name="ProfileID">The ID of the profile that applies to this item.</param>
/// <param name="ProfileNameID">The locale ID of the profile.</param>
/// <param name="FactionID">The faction that this item/profile combination may be used on.</param>
/// <param name="LoadoutID">The loadout that this item/profile combination may be used on.</param>
[Datasheet]
public partial record ItemProfile
(
    uint ItemID,
    uint ProfileID,
    uint ProfileNameID,
    FactionDefinition FactionID,
    uint LoadoutID
);
