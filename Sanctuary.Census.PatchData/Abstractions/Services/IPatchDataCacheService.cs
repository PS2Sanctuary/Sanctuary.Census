using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.PatchData.PatchDataModels;

namespace Sanctuary.Census.PatchData.Abstractions.Services;

/// <summary>
/// Represents a cache of patch data.
/// </summary>
public interface IPatchDataCacheService : IDataCacheService
{
    /// <summary>
    /// Gets the cached <see cref="FacilityLinkPatch"/> objects.
    /// </summary>
    IReadOnlyList<FacilityLinkPatch> FacilityLinks { get; }

    /// <summary>
    /// Gets the cached <see cref="MapRegionPatch"/> objects.
    /// </summary>
    IReadOnlyList<MapRegionPatch> MapRegions { get; }
}
