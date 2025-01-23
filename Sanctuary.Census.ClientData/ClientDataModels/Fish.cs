using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client fish data.
/// </summary>
/// <param name="Id">The ID of the fish.</param>
/// <param name="NameId">The locale ID of the fish's name.</param>
/// <param name="DescriptionId">The locale ID of the fish's description.</param>
/// <param name="RarityId">The ID of the fish's <see cref="FishRarity"/>.</param>
/// <param name="ZoneSetId">The zone set that the fish can be found on.</param>
/// <param name="AverageSize">The average size of the fish when caught.</param>
/// <param name="SizeDeviation">The amount that the fish's size can deviate from the average.</param>
/// <param name="NormalSpeed">The standard speed of the fish.</param>
/// <param name="MaximumSpeed">The maximum speed that the fish can swim.</param>
/// <param name="Mobility">Unknown.</param>
/// <param name="ScanPointAmount">Unknown.</param>
/// <param name="SensitivityDistance">Unknown.</param>
/// <param name="Cost">The number of fish directive points granted by this fish.</param>
/// <param name="ImageSetId">The ID of the fish's image set.</param>
/// <param name="FishSizeType">The ID of the fish's size type.</param>
[Datasheet]
public partial record Fish
(
    uint Id,
    uint NameId,
    uint DescriptionId,
    uint RarityId,
    uint ZoneSetId,
    int AverageSize,
    int SizeDeviation,
    decimal NormalSpeed,
    decimal MaximumSpeed,
    int Mobility,
    int ScanPointAmount,
    decimal SensitivityDistance,
    int Cost,
    uint ImageSetId,
    uint FishSizeType
);
