using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents fish data.
/// </summary>
/// <param name="ID">The ID of the fish.</param>
/// <param name="Name">The fish's name.</param>
/// <param name="Description">The fish's description.</param>
/// <param name="RarityID">The ID of the associated `fish_rarity`.</param>
/// <param name="ZoneSetID">The zone set that the fish can be found on.</param>
/// <param name="AverageSize">The average size of the fish when caught.</param>
/// <param name="SizeDeviation">The amount that the fish's size can deviate from the average.</param>
/// <param name="NormalSpeed">The standard speed of the fish.</param>
/// <param name="MaximumSpeed">The maximum speed that the fish can swim.</param>
/// <param name="Mobility">Unknown.</param>
/// <param name="ScanPointAmount">Unknown.</param>
/// <param name="SensitivityDistance">Unknown.</param>
/// <param name="Cost">The number of fish directive points granted by this fish.</param>
/// <param name="ImageSetID">The ID of the fish's image set.</param>
/// <param name="FishSizeType">The ID of the associated `fish_size_type`.</param>
[Collection]
public record Fish
(
    [property: JoinKey] uint ID,
    LocaleString Name,
    LocaleString Description,
    [property: JoinKey] uint RarityID,
    uint ZoneSetID,
    int AverageSize,
    int SizeDeviation,
    decimal NormalSpeed,
    decimal MaximumSpeed,
    int Mobility,
    int ScanPointAmount,
    decimal SensitivityDistance,
    int Cost,
    [property: JoinKey] uint ImageSetID,
    [property: JoinKey] uint? ImageID,
    string? ImagePath,
    [property: JoinKey] uint FishSizeType
) : ISanctuaryCollection;
