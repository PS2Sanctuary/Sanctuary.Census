using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a resource type.
/// </summary>
/// <param name="ResourceTypeId">The ID of the type.</param>
/// <param name="CodeName">The code name of the type.</param>
/// <param name="Name">The localized name of the resource type.</param>
/// <param name="ImageSetId">The ID of the image set linked to the resource type.</param>
/// <param name="ImageId">The ID of the resource type's default image.</param>
/// <param name="ImagePath">The relative path to the resource type's default image.</param>
[Collection]
[Description("Represents a resource type. Note that types without a name are excluded from this collection.")]
public record ResourceType
(
    [property: Key] uint ResourceTypeId,
    string? CodeName,
    LocaleString? Name,
    [property: Key] uint? ImageSetId,
    [property: Key] uint? ImageId,
    string? ImagePath
) : ISanctuaryCollection;
