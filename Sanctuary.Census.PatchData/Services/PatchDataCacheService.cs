using Sanctuary.Census.Common;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.PatchData.Abstractions.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.PatchData.Services;

/// <inheritdoc />
public class PatchDataCacheService : IPatchDataCacheService
{
    private static readonly JsonSerializerOptions JsonOptions;

    /// <inheritdoc />
    public DateTimeOffset LastPopulated { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<FacilityType>? FacilityTypes { get; private set; }

    static PatchDataCacheService()
    {
        JsonOptions = new JsonSerializerOptions
        {
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy()
        };
    }

    /// <inheritdoc />
    public async Task RepopulateAsync(CancellationToken ct = default)
    {
        string basePath = Path.Combine(AppContext.BaseDirectory, "Data");

        await using FileStream facTypesStream = File.OpenRead(Path.Combine(basePath, "facility_type.json"));
        FacilityTypes = await JsonSerializer.DeserializeAsync<IReadOnlyList<FacilityType>>
        (
            facTypesStream,
            JsonOptions,
            ct
        ).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Clear()
    {
        FacilityTypes = null;
        LastPopulated = DateTimeOffset.MinValue;
    }
}
