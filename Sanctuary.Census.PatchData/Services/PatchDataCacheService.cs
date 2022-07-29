using Sanctuary.Census.PatchData.Abstractions.Services;
using Sanctuary.Census.PatchData.PatchDataModels;
using System.Text.Json;

namespace Sanctuary.Census.PatchData.Services;

/// <inheritdoc />
public class PatchDataCacheService : IPatchDataCacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public DateTimeOffset LastPopulated { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<FacilityLinkPatch>? FacilityLinks { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<MapRegionPatch>? MapRegions { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<StaticFacilityInfo>? StaticFacilityInfos { get; private set; }

    /// <inheritdoc />
    public async Task RepopulateAsync(CancellationToken ct = default)
    {
        string basePath = Path.Combine(AppContext.BaseDirectory, "Data");

        await using FileStream linkStream = new(Path.Combine(basePath, "facility_link.json"), FileMode.Open);
        List<FacilityLinkPatch>? links = await JsonSerializer.DeserializeAsync<List<FacilityLinkPatch>>
        (
            linkStream,
            JsonOptions,
            ct
        );
        FacilityLinks = links ?? throw new Exception("Failed to deserialize facility links");

        await using FileStream regionsStream = new(Path.Combine(basePath, "map_region.json"), FileMode.Open);
        List<MapRegionPatch>? regions = await JsonSerializer.DeserializeAsync<List<MapRegionPatch>>
        (
            regionsStream,
            JsonOptions,
            ct
        );

        if (regions is null)
            throw new Exception("Failed to deserialize map regions");

        regions.Sort((x1, x2) => x1.RegionID.CompareTo(x2.RegionID));
        MapRegions = regions;

        await using FileStream staticFacilitiesStream = new(Path.Combine(basePath, "static_facility_info.json"), FileMode.Open);
        List<StaticFacilityInfo>? staticFacilities = await JsonSerializer.DeserializeAsync<List<StaticFacilityInfo>>
        (
            staticFacilitiesStream,
            JsonOptions,
            ct
        );

        if (staticFacilities is null)
            throw new Exception("Failed to deserialize map regions");

        StaticFacilityInfos = staticFacilities.OrderBy(x => x.ZoneDefinition)
            .ThenBy(x => x.FacilityID)
            .ToList();
    }

    /// <inheritdoc />
    public void Clear()
    {
        LastPopulated = DateTimeOffset.MinValue;
        FacilityLinks = null;
        MapRegions = null;
        StaticFacilityInfos = null;
    }
}
