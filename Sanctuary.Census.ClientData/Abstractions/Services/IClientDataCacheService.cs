﻿using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Abstractions.Services;
using System.Collections.Generic;

namespace Sanctuary.Census.ClientData.Abstractions.Services;

/// <summary>
/// Represents a cache of client data.
/// </summary>
public interface IClientDataCacheService : IDataCacheService
{
    /// <summary>
    /// Gets the cached <see cref="AbilityEx"/> objects.
    /// </summary>
    IReadOnlyList<AbilityEx>? AbilityExs { get; }

    /// <summary>
    /// Gets the cached <see cref="AbilitySet"/> objects.
    /// </summary>
    IReadOnlyList<AbilitySet>? AbilitySets { get; }

    /// <summary>
    /// Gets the cached admin command aliases.
    /// </summary>
    IReadOnlyList<Alias>? AdminCommandAliases { get; }

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
    /// Gets the cached <see cref="ClientPrestigeLevel"/> objects.
    /// </summary>
    IReadOnlyList<ClientPrestigeLevel>? ClientPrestigeLevels { get; }

    /// <summary>
    /// Gets the cached <see cref="ClientRequirementExpression"/> objects.
    /// </summary>
    IReadOnlyList<ClientRequirementExpression>? ClientRequirementExpressions { get; }

    /// <summary>
    /// Gets the cached <see cref="Currency"/> objects.
    /// </summary>
    IReadOnlyList<Currency>? Currencies { get; }

    /// <summary>
    /// Gets the cached <see cref="Experience"/> objects.
    /// </summary>
    IReadOnlyList<Experience>? Experiences { get; }

    /// <summary>
    /// Gets the cached <see cref="ExperienceRank"/> objects.
    /// </summary>
    IReadOnlyList<ExperienceRank>? ExperienceRanks { get; }

    /// <summary>
    /// Gets the cached <see cref="Faction"/> objects.
    /// </summary>
    IReadOnlyList<Faction>? Factions { get; }

    /// <summary>
    /// Gets the cached <see cref="FireModeDisplayStats"/> objects.
    /// </summary>
    IReadOnlyList<FireModeDisplayStat>? FireModeDisplayStats { get; }

    /// <summary>
    /// Gets the cached <see cref="Fish"/> objects.
    /// </summary>
    IReadOnlyList<Fish>? Fishes { get; }

    /// <summary>
    /// Gets the cached <see cref="FishRarity"/> objects.
    /// </summary>
    IReadOnlyList<FishRarity>? FishRarities { get; }

    /// <summary>
    /// Gets the cached <see cref="FishSizeType"/> objects.
    /// </summary>
    IReadOnlyList<FishSizeType>? FishSizeTypes { get; }

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
    /// Gets the cached <see cref="ItemLineMember"/> objects.
    /// </summary>
    IReadOnlyList<ItemLineMember>? ItemLineMembers { get; }

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
    /// Gets the cached <see cref="LoadoutAttachment"/> objects.
    /// </summary>
    IReadOnlyList<LoadoutAttachment>? LoadoutAttachments { get; }

    /// <summary>
    /// Gets the cached <see cref="LoadoutAttachmentGroupMap"/> objects.
    /// </summary>
    IReadOnlyList<LoadoutAttachmentGroupMap>? LoadoutAttachmentGroupMaps { get; }

    /// <summary>
    /// Gets the cached <see cref="LoadoutItemAttachment"/> objects.
    /// </summary>
    IReadOnlyList<LoadoutItemAttachment>? LoadoutItemAttachments { get; }

    /// <summary>
    /// Gets the cached <see cref="LoadoutSlot"/> objects.
    /// </summary>
    IReadOnlyList<LoadoutSlot>? LoadoutSlots { get; }

    /// <summary>
    /// Gets the cached <see cref="LoadoutSlotItemClass"/> objects.
    /// </summary>
    IReadOnlyList<LoadoutSlotItemClass>? LoadoutSlotItemClasses { get; }

    /// <summary>
    /// Gets the cached <see cref="LoadoutSlotTintItemClass"/> objects.
    /// </summary>
    IReadOnlyList<LoadoutSlotTintItemClass>? LoadoutSlotTintItemClasses { get; }

    /// <summary>
    /// Gets the cached <see cref="PrestigeLevelRankSet"/> objects.
    /// </summary>
    IReadOnlyList<PrestigeLevelRankSet>? PrestigeLevelRankSets { get; }

    /// <summary>
    /// Gets the cached <see cref="PrestigeRankSetMapping"/> objects.
    /// </summary>
    IReadOnlyList<PrestigeRankSetMapping>? PrestigeRankSetMappings { get; }

    /// <summary>
    /// Gets the cached <see cref="ResistInfo"/> objects.
    /// </summary>
    IReadOnlyList<ResistInfo>? ResistInfos { get; }

    /// <summary>
    /// Gets the cached <see cref="Resource"/> objects.
    /// </summary>
    IReadOnlyList<Resource>? Resources { get; }

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
    /// Gets the cached <see cref="LoadoutSlotTintItemClass"/> objects.
    /// </summary>
    IReadOnlyList<VehicleLoadoutSlotTintItemClass>? VehicleLoadoutSlotTintItemClasses { get; }

    /// <summary>
    /// Gets the cached <see cref="VehicleSkillSet"/> objects.
    /// </summary>
    IReadOnlyList<VehicleSkillSet>? VehicleSkillSets { get; }

    /// <summary>
    /// Gets the cached <see cref="ZoneSetMapping"/> objects.
    /// </summary>
    IReadOnlyList<ZoneSetMapping>? ZoneSetMappings { get; }
}
