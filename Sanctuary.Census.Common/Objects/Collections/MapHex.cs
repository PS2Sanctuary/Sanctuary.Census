using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a map hex.
/// </summary>
/// <param name="ZoneID">The ID of the zone that the hex is on.</param>
/// <param name="MapRegionID">The ID of the map region that the hex is contained within.</param>
/// <param name="X">The X coordinate of the hex.</param>
/// <param name="Y">The Y coordinate of the hex.</param>
/// <param name="HexType">The type of the hex.</param>
/// <param name="TypeName">The name of the <paramref name="HexType"/>.</param>
[Collection]
public record MapHex
(
    [property: Key] uint ZoneID,
    [property: Key] uint MapRegionID,
    short X,
    short Y,
    byte HexType,
    string TypeName
) : ISanctuaryCollection;
