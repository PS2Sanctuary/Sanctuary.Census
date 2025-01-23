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
/// <param name="AverageSize"></param>
/// <param name="SizeDeviation"></param>
/// <param name="NormalSpeed"></param>
/// <param name="MaximumSpeed"></param>
/// <param name="Mobility"></param>
/// <param name="ScanPointAmount"></param>
/// <param name="SensitivityDistance"></param>
/// <param name="Cost"></param>
/// <param name="ImageSetId"></param>
/// <param name="FishSizetype"></param>
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
    uint FishSizetype
);
