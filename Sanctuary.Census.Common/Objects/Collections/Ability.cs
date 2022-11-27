using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents ability data.
/// </summary>
/// <param name="AbilityId">The ID of the ability.</param>
/// <param name="AbilityType">The type of the ability.</param>
/// <param name="Name">The name of the ability.</param>
/// <param name="Description">The description of the ability.</param>
/// <param name="CannotStartInFireBlock">Indicates whether the ability can be used while firing a weapon.</param>
/// <param name="ExpireMsec">The time in milliseconds before the ability expires after activation.</param>
/// <param name="FlagSurvivesProfileSwap">Indicates whether the ability remains active when swapping profiles.</param>
/// <param name="FlagToggle">Indicates whether the ability can be toggled.</param>
/// <param name="FlagUseTargetResource">Indicates whether the resource of the ability's target is consumed.</param>
/// <param name="FirstUseDelayMsec">The delay in milliseconds before the ability can be activated for the first time.</param>
/// <param name="NextUseDelayMsec"></param>
/// <param name="ReuseDelayMsec">The delay in milliseconds before the ability can be re-used after expiry.</param>
/// <param name="ResourceBurnInVr">Indicates whether the ability burns a resource in VR training.</param>
/// <param name="ResourceCostPerMsec">The amount of the resource drained each millisecond by the ability.</param>
/// <param name="ResourceFirstCost">The initial amount of resource drained when the ability is activated.</param>
/// <param name="ResourceThreshold">The minimum amount of resource that must be available before the ability can be activated.</param>
/// <param name="ResourceTypeId">The type of resource that the ability consumes.</param>
/// <param name="DistanceMax">The maximum distance to the target at which the ability can be activated?</param>
/// <param name="RadiusMax">The radius in which the ability applies its effect?</param>
/// <param name="Param1"></param>
/// <param name="Param2"></param>
/// <param name="Param3"></param>
/// <param name="Param4"></param>
/// <param name="Param5"></param>
/// <param name="Param6"></param>
/// <param name="Param7"></param>
/// <param name="Param8"></param>
/// <param name="Param9"></param>
/// <param name="Param10"></param>
/// <param name="Param11"></param>
/// <param name="Param12"></param>
/// <param name="Param13"></param>
/// <param name="Param14"></param>
/// <param name="String1"></param>
/// <param name="String2"></param>
/// <param name="String3"></param>
/// <param name="String4"></param>
/// <param name="UseWeaponCharge"></param>
/// <param name="ImageSetId">The ID of the image set linked to this ability.</param>
/// <param name="ImageId">The ID of the ability's default image.</param>
/// <param name="ImagePath">The relative path to the ability's default image.</param>
[Collection]
public record Ability
(
    [property: JoinKey] uint AbilityId,
    string AbilityType,
    LocaleString? Name,
    LocaleString? Description,
    bool CannotStartInFireBlock,
    uint? ExpireMsec,
    bool FlagSurvivesProfileSwap,
    bool FlagToggle,
    bool FlagUseTargetResource,
    uint? FirstUseDelayMsec,
    uint? NextUseDelayMsec,
    uint? ReuseDelayMsec,
    bool ResourceBurnInVr,
    decimal? ResourceCostPerMsec,
    decimal? ResourceFirstCost,
    uint? ResourceThreshold,
    [property: JoinKey] uint? ResourceTypeId,
    decimal? DistanceMax,
    decimal? RadiusMax,
    uint? Param1,
    uint? Param2,
    decimal? Param3,
    decimal? Param4,
    uint? Param5,
    uint? Param6,
    uint? Param7,
    uint? Param8,
    uint? Param9,
    uint? Param10,
    uint? Param11,
    uint? Param12,
    uint? Param13,
    uint? Param14,
    string? String1,
    string? String2,
    string? String3,
    string? String4,
    bool UseWeaponCharge,
    [property: JoinKey] uint? ImageSetId,
    [property: JoinKey] uint? ImageId,
    string? ImagePath
) : ISanctuaryCollection;
