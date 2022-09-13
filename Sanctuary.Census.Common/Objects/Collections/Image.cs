using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents information about an image.
/// </summary>
/// <param name="ImageID">The ID of the image.</param>
/// <param name="Description">The description of the image.</param>
/// <param name="Path">The relative path to the image.</param>
[Collection]
public record Image
(
    uint ImageID,
    string? Description,
    string Path
) : ISanctuaryCollection;
