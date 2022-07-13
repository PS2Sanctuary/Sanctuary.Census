using Sanctuary.Census.ClientData.Objects.ClientDataModels;
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
    /// Gets the cached <see cref="ResourceType"/> objects.
    /// </summary>
    IReadOnlyList<ResourceType> ResourceTypes { get; }
}
