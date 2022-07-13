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
    List<ClientItemDatasheetData> ClientItemDatasheetDatas { get; }

    /// <summary>
    /// Gets the cached <see cref="ClientItemDefinition"/> objects.
    /// </summary>
    List<ClientItemDefinition> ClientItemDefinitions { get; }

    /// <summary>
    /// Gets the cached <see cref="FireModeDisplayStats"/> objects.
    /// </summary>
    List<FireModeDisplayStat> FireModeDisplayStats { get; }

    /// <summary>
    /// Gets the cached <see cref="ImageSetMapping"/> objects.
    /// </summary>
    List<ImageSetMapping> ImageSetMappings { get; }

    /// <summary>
    /// Gets the cached <see cref="ItemProfile"/> objects.
    /// </summary>
    List<ItemProfile> ItemProfiles { get; }

    /// <summary>
    /// Gets the cached <see cref="ResourceType"/> objects.
    /// </summary>
    List<ResourceType> ResourceTypes { get; }
}
