using Sanctuary.Census.ClientData.Objects.ClientDataModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.ClientData.Abstractions.Services;

/// <summary>
/// Represents a cache of client data.
/// </summary>
public interface IClientDataCacheService
{
    /// <summary>
    /// Gets the time in UTC at which the cache was last populated.
    /// </summary>
    DateTimeOffset LastPopulated { get; }

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

    /// <summary>
    /// Repopulates the cache.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task Repopulate(CancellationToken ct = default);
}
