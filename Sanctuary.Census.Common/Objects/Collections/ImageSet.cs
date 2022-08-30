using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a mapping between an image set and an image.
/// </summary>
/// <param name="ImageSetID">The ID of the image set.</param>
/// <param name="ImageID">The ID of the image.</param>
/// <param name="Description">The description of the image set.</param>
/// <param name="TypeID">The type of the image.</param>
/// <param name="TypeDescription">The description of the <paramref name="TypeID"/>.</param>
/// <param name="ImagePath">The relative path to the image.</param>
[Collection]
public record ImageSet
(
    [property: Key] uint ImageSetID,
    [property: Key] uint ImageID,
    string Description,
    uint TypeID,
    string? TypeDescription,
    string ImagePath
) : ISanctuaryCollection;
