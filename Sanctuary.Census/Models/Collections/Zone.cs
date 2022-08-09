using Sanctuary.Census.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Models.Collections;

/// <summary>
/// Represents information about a zone.
/// </summary>
/// <param name="ZoneID">The ID of the zone.</param>
/// <param name="Code">The zone's internal name.</param>
/// <param name="HexSize">The size of the zone's hexes.</param>
/// <param name="Name">The localized name.</param>
/// <param name="Description">The localized description.</param>
[Collection]
public record Zone
(
    [property: Key] uint ZoneID,
    string Code,
    float HexSize,
    LocaleString Name,
    LocaleString Description
);
