namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client image set maps.
/// </summary>
/// <param name="ImageSetID">The image set ID.</param>
/// <param name="ImageType">The type of the mapped image.</param>
/// <param name="ImageID">The ID of the mapped image.</param>
public record ImageSetMapping
(
    uint ImageSetID,
    ImageSetType ImageType,
    uint ImageID
);
