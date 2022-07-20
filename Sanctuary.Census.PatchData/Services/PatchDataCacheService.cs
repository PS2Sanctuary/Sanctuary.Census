﻿using Sanctuary.Census.PatchData.Abstractions.Services;
using Sanctuary.Census.PatchData.PatchDataModels;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;

namespace Sanctuary.Census.PatchData.Services;

/// <inheritdoc />
public class PatchDataCacheService : IPatchDataCacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public DateTimeOffset LastPopulated { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<FacilityLinkPatch> FacilityLinks { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<MapRegionPatch> MapRegions { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PatchDataCacheService"/> class.
    /// </summary>
    public PatchDataCacheService()
    {
        Clear();
    }

    /// <inheritdoc />
    public async Task RepopulateAsync(CancellationToken ct = default)
    {
        string basePath = Path.Combine
        (
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "Data"
        );

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
        MapRegions = regions ?? throw new Exception("Failed to deserialize map regions");
    }

    /// <inheritdoc />
    [MemberNotNull(nameof(FacilityLinks))]
    [MemberNotNull(nameof(MapRegions))]
    public void Clear()
    {
        LastPopulated = DateTimeOffset.MinValue;
        FacilityLinks = new List<FacilityLinkPatch>();
        MapRegions = new List<MapRegionPatch>();
    }
}
