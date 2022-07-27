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
    /// Gets the cached <see cref="ClientItemDatasheetData"/> objects.
    /// </summary>
    IReadOnlyList<ClientItemDatasheetData> ClientItemDatasheetDatas { get; }

    /// <summary>
    /// Gets the cached <see cref="ClientItemDefinition"/> objects.
    /// </summary>
    IReadOnlyList<ClientItemDefinition> ClientItemDefinitions { get; }

    /// <summary>
    /// Gets the cached <see cref="Currency"/> objects.
    /// </summary>
    IReadOnlyList<Currency> Currencies { get; }

    /// <summary>
    /// Gets the cached <see cref="Experience"/> objects.
    /// </summary>
    IReadOnlyList<Experience> Experiences { get; }

    /// <summary>
    /// Gets the cached <see cref="Faction"/> objects.
    /// </summary>
    IReadOnlyList<Faction> Factions { get; }

    /// <summary>
    /// Gets the cached <see cref="FireModeDisplayStats"/> objects.
    /// </summary>
    IReadOnlyList<FireModeDisplayStat> FireModeDisplayStats { get; }

    /// <summary>
    /// Gets the cached <see cref="ImageSetMapping"/> objects.
    /// </summary>
    IReadOnlyList<ImageSetMapping> ImageSetMappings { get; }

    /// <summary>
    /// Gets the cached <see cref="ItemProfile"/> objects.
    /// </summary>
    IReadOnlyList<ItemProfile> ItemProfiles { get; }

    /// <summary>
    /// Gets the cached <see cref="Loadout"/> objects.
    /// </summary>
    IReadOnlyList<Loadout> Loadouts { get; }

    /// <summary>
    /// Gets the cached <see cref="LoadoutSlot"/> objects.
    /// </summary>
    IReadOnlyList<LoadoutSlot> LoadoutSlots { get; }

    /// <summary>
    /// Gets the cached <see cref="ResourceType"/> objects.
    /// </summary>
    IReadOnlyList<ResourceType> ResourceTypes { get; }

    /// <summary>
    /// Gets the cached <see cref="Vehicle"/> objects.
    /// </summary>
    IReadOnlyList<Vehicle> Vehicles { get; }

    /// <summary>
    /// Gets the cached <see cref="VehicleLoadout"/> objects.
    /// </summary>
    IReadOnlyList<VehicleLoadout> VehicleLoadouts { get; }

    /// <summary>
    /// Gets the cached <see cref="VehicleLoadoutSlot"/> objects.
    /// </summary>
    IReadOnlyList<VehicleLoadoutSlot> VehicleLoadoutSlots { get; }
}
