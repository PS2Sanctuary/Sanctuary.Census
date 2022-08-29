using Sanctuary.Census.PatchData.Abstractions.Services;
using Sanctuary.Census.PatchData.PatchDataModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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
    }

    /// <inheritdoc />
    public void Clear()
    {
        LastPopulated = DateTimeOffset.MinValue;
        FacilityLinks = null;
    }
}
