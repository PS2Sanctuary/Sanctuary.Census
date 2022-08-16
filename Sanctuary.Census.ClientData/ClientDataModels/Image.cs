using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client image data.
/// </summary>
/// <param name="ID">The ID of the image.</param>
/// <param name="FileName">The name of the image file.</param>
/// <param name="ContentID">Unused.</param>
[Datasheet]
public partial record Image
(
    uint ID,
    string FileName,
    uint ContentID
);
