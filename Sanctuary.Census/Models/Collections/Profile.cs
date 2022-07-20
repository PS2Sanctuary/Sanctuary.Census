using Sanctuary.Census.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.Models.Collections;

/// <summary>
/// Represents profile data.
/// </summary>
/// <param name="ProfileId">The ID of the profile.</param>
/// <param name="ProfileTypeId">The ID of the profile's type.</param>
/// <param name="FactionId">The faction that this profile is built for.</param>
/// <param name="Name">The name of the profile.</param>
/// <param name="Description">The description of the profile.</param>
/// <param name="ImageSetId">The ID of the profile's image set.</param>
/// <param name="ImageId">The ID of the profile's default image.</param>
/// <param name="ImagePath">The relative path to the profile's default image.</param>
[Collection]
public record Profile
(
    uint ProfileId,
    uint ProfileTypeId,
    uint? FactionId,
    LocaleString? Name,
    LocaleString? Description,
    uint? ImageSetId,
    uint? ImageId,
    string? ImagePath
);
