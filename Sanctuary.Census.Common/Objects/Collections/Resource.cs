using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a resource.
/// </summary>
/// <param name="ResourceId">The ID of the resource.</param>
/// <param name="ResourceTypeId">The type of the resource. Links to <see cref="ResourceType"/>.</param>
/// <param name="Name">The name of the resource.</param>
/// <param name="Description">The description of the resource.</param>
/// <param name="ActivatedAbilityId"></param>
/// <param name="DepletedAbilityId"></param>
/// <param name="RemovedAbilityId"></param>
/// <param name="InitialValue">The initial amount of the resource available to use.</param>
/// <param name="InitialValueMax">The initial maximum amount of the resource that may be accumulated.</param>
/// <param name="UseInitialValueAsMax">
/// Indicates whether the <paramref name="InitialValue"/> should be used as the value of <paramref name="InitialValueMax"/>.
/// </param>
/// <param name="MaxValue">The maximum amount of the resource that may ever be accumulated.</param>
/// <param name="ValueMarkerLo">The amount of resource that must be left available to be considered in low quantity.</param>
/// <param name="ValueMarkerMed">The amount of resource that must be left available to be considered in medium quantity.</param>
/// <param name="ValueMarkerHi">The amount of resource that must be left available to be considered in high quantity.</param>
/// <param name="RegenPerMs">The amount of the resource that is regenerated per millisecond.</param>
/// <param name="RegenDelayMs">The delay in milliseconds before the resource begins regenerating.</param>
/// <param name="RegenDamageInterruptMs">The amount of time in milliseconds that taking damage will delay regeneration for.</param>
/// <param name="RegenTickMs">The delay in milliseconds between each regeneration tick.</param>
/// <param name="BurnPerMs">The amount of burn damage per millisecond that this resource causes.</param>
/// <param name="BurnTickMs">The delay in milliseconds between each tick of burn damage.</param>
/// <param name="BurnDelayMs"></param>
/// <param name="FlagProfileScope">Indicates whether the resource only applies to the current profile.</param>
/// <param name="FlagNotVehMountScope">Indicates whether the resource applies when not mounted in a vehicle.</param>
/// <param name="AllowBurnHealXp">Indicates whether any burn damage inflicted by the resource will grant XP when healed.</param>
/// <param name="FullAbilityId"></param>
/// <param name="ImageSetId">The ID of the image set linked to the resource.</param>
/// <param name="ImageId">The ID of the resource's default image.</param>
/// <param name="ImagePath">The relative path to the resource's default image.</param>
[Collection]
public record Resource
(
    [property: JoinKey] uint ResourceId,
    [property: JoinKey] uint ResourceTypeId,
    LocaleString? Name,
    LocaleString? Description,
    [property: JoinKey] uint? ActivatedAbilityId,
    [property: JoinKey] uint? DepletedAbilityId,
    [property: JoinKey] uint? RemovedAbilityId,
    decimal InitialValue,
    decimal? InitialValueMax,
    bool UseInitialValueAsMax,
    decimal? MaxValue,
    decimal? ValueMarkerLo,
    decimal? ValueMarkerMed,
    decimal? ValueMarkerHi,
    decimal? RegenPerMs,
    uint? RegenDelayMs,
    uint? RegenDamageInterruptMs,
    uint? RegenTickMs,
    decimal? BurnPerMs,
    uint? BurnTickMs,
    uint? BurnDelayMs,
    bool FlagProfileScope,
    bool FlagNotVehMountScope,
    bool AllowBurnHealXp,
    [property: JoinKey] uint? FullAbilityId,
    [property: JoinKey] uint? ImageSetId,
    [property: JoinKey] uint? ImageId,
    string? ImagePath
) : ISanctuaryCollection;
