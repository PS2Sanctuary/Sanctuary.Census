using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.Common.Objects.Collections;

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
    [property: JoinKey] uint ProfileId,
    uint ProfileTypeId,
    [property: JoinKey] uint? FactionId,
    LocaleString? Name,
    LocaleString? Description,
    [property: JoinKey] uint? ImageSetId,
    [property: JoinKey] uint? ImageId,
    string? ImagePath
) : ISanctuaryCollection;
