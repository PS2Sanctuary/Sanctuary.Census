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
/// <param name="CameraHeight">The height of the player's camera.</param>
/// <param name="CrouchCameraHeight">The height of the player's camera while crouched.</param>
/// <param name="Description">The description of the profile.</param>
/// <param name="ImageSetId">The ID of the profile's image set.</param>
/// <param name="ImageId">The ID of the profile's default image.</param>
/// <param name="ImagePath">The relative path to the profile's default image.</param>
/// <param name="MovementSpeed">The walking speed of the profile. Valid only for infantry profiles.</param>
/// <param name="BackpedalSpeedMultiplier">Multiplied against the <see cref="MovementSpeed"/>. Valid only for infantry profiles.</param>
/// <param name="CrouchSpeedMultiplier">Multiplied against the <see cref="MovementSpeed"/>. Valid only for infantry profiles.</param>
/// <param name="SprintSpeedMultiplier">Multiplied against the <see cref="MovementSpeed"/>. Valid only for infantry profiles.</param>
/// <param name="StrafeSpeedMultiplier">Multiplied against the <see cref="MovementSpeed"/>. Valid only for infantry profiles.</param>
/// <param name="SprintAccelerationTimeSec">The time in seconds taken to accelerate into a sprint. Valid only for infantry profiles.</param>
/// <param name="SprintDecelerationTimeSec">The time in seconds taken to decelerate from a sprint. Valid only for infantry profiles.</param>
/// <param name="ForwardAccelerationTimeSec">The time in seconds taken to accelerate into a walk. Valid only for infantry profiles.</param>
/// <param name="ForwardDecelerationTimeSec">The time in seconds taken to decelerate from from a walk. Valid only for infantry profiles.</param>
/// <param name="BackAccelerationTimeSec">The time in seconds taken to accelerate into a backpedal. Valid only for infantry profiles.</param>
/// <param name="BackDecelerationTimeSec">The time in seconds taken to decelerate from from a backpedal. Valid only for infantry profiles.</param>
/// <param name="StrafeAccelerationTimeSec">The time in seconds taken to accelerate into a strafe. Valid only for infantry profiles.</param>
/// <param name="StrafeDecelerationTimeSec">The time in seconds taken to decelerate from from a strafe. Valid only for infantry profiles.</param>
/// <param name="MaxWaterSpeedMultiplier">Multiplied against the <see cref="MovementSpeed"/>. Valid only for infantry profiles.</param>
[Collection]
public record Profile
(
    [property: JoinKey] uint ProfileId,
    uint ProfileTypeId,
    [property: JoinKey] uint? FactionId,
    LocaleString? Name,
    LocaleString? Description,
    decimal? CameraHeight,
    decimal? CrouchCameraHeight,
    [property: JoinKey] uint? ImageSetId,
    [property: JoinKey] uint? ImageId,
    string? ImagePath,
    decimal? MovementSpeed,
    decimal? BackpedalSpeedMultiplier,
    decimal? CrouchSpeedMultiplier,
    decimal? SprintSpeedMultiplier,
    decimal? StrafeSpeedMultiplier,
    decimal? SprintAccelerationTimeSec,
    decimal? SprintDecelerationTimeSec,
    decimal? ForwardAccelerationTimeSec,
    decimal? ForwardDecelerationTimeSec,
    decimal? BackAccelerationTimeSec,
    decimal? BackDecelerationTimeSec,
    decimal? StrafeAccelerationTimeSec,
    decimal? StrafeDecelerationTimeSec,
    decimal? MaxWaterSpeedMultiplier
) : ISanctuaryCollection;
