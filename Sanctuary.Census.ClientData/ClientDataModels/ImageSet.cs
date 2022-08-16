using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client image set data.
/// </summary>
/// <param name="ID">The ID of the image set.</param>
/// <param name="Description">The description of the image set.</param>
[Datasheet]
public partial record ImageSet
(
    uint ID,
    string Description
);
