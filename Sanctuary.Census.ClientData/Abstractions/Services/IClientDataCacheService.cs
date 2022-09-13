using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Abstractions.Services;
using System.Collections.Generic;

namespace Sanctuary.Census.ClientData.Abstractions.Services;

/// <summary>
/// Represents a cache of client data.
/// </summary>
public interface IClientDataCacheService : IDataCacheService
{
    /// <summary>
    /// Gets the cached <see cref="AreaDefinition"/> objects.
    /// </summary>
    IReadOnlyDictionary<AssetZone, IReadOnlyList<AreaDefinition>>? AreaDefinitions { get; }

    /// <summary>
    /// Gets the cached <see cref="ClientItemDatasheetData"/> objects.
    /// </summary>
    IReadOnlyList<ClientItemDatasheetData>? ClientItemDatasheetDatas { get; }

    /// <summary>
    /// Gets the cached <see cref="ClientItemDefinition"/> objects.
    /// </summary>
    IReadOnlyList<ClientItemDefinition>? ClientItemDefinitions { get; }

    /// <summary>
    /// Gets the cached <see cref="Currency"/> objects.
    /// </summary>
    IReadOnlyList<Currency>? Currencies { get; }

    /// <summary>
    /// Gets the cached <see cref="Experience"/> objects.
    /// </summary>
    IReadOnlyList<Experience>? Experiences { get; }

    /// <summary>
    /// Gets the cached <see cref="Faction"/> objects.
    /// </summary>
    IReadOnlyList<Faction>? Factions { get; }

    /// <summary>
    /// Gets the cached <see cref="FireModeDisplayStats"/> objects.
    /// </summary>
    IReadOnlyList<FireModeDisplayStat>? FireModeDisplayStats { get; }

    /// <summary>
    /// Gets the cached <see cref="Image"/> objects.
    /// </summary>
    IReadOnlyList<Image>? Images { get; }

    /// <summary>
    /// Gets the cached <see cref="ImageSet"/> objects.
    /// </summary>
    IReadOnlyList<ImageSet>? ImageSets { get; }

    /// <summary>
    /// Gets the cached <see cref="ImageSetMapping"/> objects.
    /// </summary>
    IReadOnlyList<ImageSetMapping>? ImageSetMappings { get; }

    /// <summary>
    /// Gets the cached <see cref="ImageSetType"/> objects.
    /// </summary>
    IReadOnlyList<ImageSetType>? ImageSetTypes { get; }

    /// <summary>
    /// Gets the cached <see cref="ItemProfile"/> objects.
    /// </summary>
    IReadOnlyList<ItemProfile>? ItemProfiles { get; }

    /// <summary>
    /// Gets the cached <see cref="ItemVehicle"/> objects.
    /// </summary>
    IReadOnlyList<ItemVehicle>? ItemVehicles { get; }

    /// <summary>
    /// Gets the cached <see cref="Loadout"/> objects.
    /// </summary>
    IReadOnlyList<Loadout>? Loadouts { get; }

    /// <summary>
    /// Gets the cached <see cref="LoadoutAttachmentGroupMap"/> objects.
    /// </summary>
    IReadOnlyList<LoadoutAttachmentGroupMap>? LoadoutAttachmentGroupMaps { get; }

    /// <summary>
    /// Gets the cached <see cref="LoadoutSlot"/> objects.
    /// </summary>
    IReadOnlyList<LoadoutSlot>? LoadoutSlots { get; }

    /// <summary>
    /// Gets the cached <see cref="ResistInfo"/> objects.
    /// </summary>
    IReadOnlyList<ResistInfo>? ResistInfos { get; }

    /// <summary>
    /// Gets the cached <see cref="ResourceType"/> objects.
    /// </summary>
    IReadOnlyList<ResourceType>? ResourceTypes { get; }

    /// <summary>
    /// Gets the cached <see cref="Skill"/> objects.
    /// </summary>
    IReadOnlyList<Skill>? Skills { get; }

    /// <summary>
    /// Gets the cached <see cref="SkillCategory"/> objects.
    /// </summary>
    IReadOnlyList<SkillCategory>? SkillCategories { get; }

    /// <summary>
    /// Gets the cached <see cref="SkillLine"/> objects.
    /// </summary>
    IReadOnlyList<SkillLine>? SkillLines { get; }

    /// <summary>
    /// Gets the cached <see cref="SkillSet"/> objects.
    /// </summary>
    IReadOnlyList<SkillSet>? SkillSets { get; }

    /// <summary>
    /// Gets the cached <see cref="Vehicle"/> objects.
    /// </summary>
    IReadOnlyList<Vehicle>? Vehicles { get; }

    /// <summary>
    /// Gets the cached <see cref="VehicleLoadout"/> objects.
    /// </summary>
    IReadOnlyList<VehicleLoadout>? VehicleLoadouts { get; }

    /// <summary>
    /// Gets the cached <see cref="VehicleLoadoutSlot"/> objects.
    /// </summary>
    IReadOnlyList<VehicleLoadoutSlot>? VehicleLoadoutSlots { get; }

    /// <summary>
    /// Gets the cached <see cref="VehicleLoadoutSlotItemClass"/> objects.
    /// </summary>
    IReadOnlyList<VehicleLoadoutSlotItemClass>? VehicleLoadoutSlotItemClasses { get; }

    /// <summary>
    /// Gets the cached <see cref="VehicleSkillSet"/> objects.
    /// </summary>
    IReadOnlyList<VehicleSkillSet>? VehicleSkillSets { get; }
}
