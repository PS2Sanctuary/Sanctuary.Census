using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects.Collections;
using System.Collections.Generic;

namespace Sanctuary.Census.PatchData.Abstractions.Services;

/// <summary>
/// Represents a cache of patch data.
/// </summary>
public interface IPatchDataCacheService : IDataCacheService
{
    /// <summary>
    /// Gets the cached <see cref="FacilityType"/> objects.
    /// </summary>
    IReadOnlyList<FacilityType>? FacilityTypes { get; }
}
