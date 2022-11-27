using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a resource type.
/// </summary>
/// <param name="ResourceTypeId">The ID of the type.</param>
/// <param name="Description">The description of the type.</param>
/// <param name="Name">The localized name of the resource type.</param>
/// <param name="ImageSetId">The ID of the image set linked to the resource type.</param>
/// <param name="ImageId">The ID of the resource type's default image.</param>
/// <param name="ImagePath">The relative path to the resource type's default image.</param>
[Collection]
[Description("Represents a resource type. Note that types without a name are excluded from this collection.")]
public record ResourceType
(
    [property: JoinKey] uint ResourceTypeId,
    string? Description,
    LocaleString? Name,
    [property: JoinKey] uint? ImageSetId,
    [property: JoinKey] uint? ImageId,
    string? ImagePath
) : ISanctuaryCollection;
