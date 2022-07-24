using Sanctuary.Census.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.ComponentModel.DataAnnotations;

namespace Sanctuary.Census.Models.Collections;

/// <summary>
/// Represents vehicle data.
/// </summary>
/// <param name="VehicleId">The ID of the vehicle.</param>
/// <param name="Name">The name of the vehicle.</param>
/// <param name="Description">The description of the vehicle.</param>
/// <param name="TypeId">The type of the vehicle.</param>
/// <param name="DecaySec">The time in seconds that the vehicle will take to decay if no one is occupying a seat.</param>
/// <param name="PurchaseCooldown">Possibly the re-pull cooldown time.</param>
/// <param name="ImageSetId">The image set ID of the vehicle.</param>
/// <param name="ImageId">The ID of the vehicle's default image.</param>
/// <param name="ImagePath">The relative path to the vehicle's default image.</param>
/// <param name="Cost">The cost of the vehicle.</param>
/// <param name="CostResourceId">The ID of the currency required to unlock the vehicle.</param>
/// <param name="LandingHeight">The height at which the vehicle enters landing mode.</param>
/// <param name="ImpactDamageBlocked">The magnitude of impact damage that the vehicle ignores.</param>
/// <param name="ImpactDamageMultiplier">The multiplier that the vehicle has against impact damage.</param>
/// <param name="ImpactDamageInflictedMultiplier">
/// The multiplier on impact damage applied to the vehicle, when more than the amount of blocked impact damage is received.
/// </param>
/// <param name="PropulsionType">The propulsion type of the vehicle.</param>
/// <param name="SchematicImageSetId">The ID of the vehicle's schematic HUD image.</param>
/// <param name="HealthImageSetId">The ID of the vehicle's full health HUD image.</param>
/// <param name="MinimapRange">The range at which the vehicle will show on the minimap.</param>
/// <param name="AutoDetectRadius">The radius in which this vehicle will be auto-detected.</param>
/// <param name="LockonTimeAdd">The additional time that lock-ons require to achieve a lock on the vehicle.</param>
/// <param name="LockonTimeMult">The multiplier applied to lock-on time.</param>
[Collection]
public record Vehicle
(
    [property:Key] int VehicleId,
    LocaleString Name,
    LocaleString Description,
    byte TypeId,
    ushort DecaySec,
    ushort PurchaseCooldown,
    uint? ImageSetId,
    uint? ImageId,
    string? ImagePath,
    uint Cost,
    uint CostResourceId,
    uint? LandingHeight,
    uint ImpactDamageBlocked,
    float? ImpactDamageMultiplier,
    float? ImpactDamageInflictedMultiplier,
    byte PropulsionType,
    uint? SchematicImageSetId,
    uint? HealthImageSetId,
    uint MinimapRange,
    uint AutoDetectRadius,
    ushort? LockonTimeAdd,
    float? LockonTimeMult
);
