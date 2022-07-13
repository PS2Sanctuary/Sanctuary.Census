using CommunityToolkit.HighPerformance.Buffers;
using Mandible.Pack2;
using Mandible.Services;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.Objects;
using Sanctuary.Census.ClientData.Util;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.ClientData.Services;

/// <inheritdoc />
public class DatasheetLoaderService : IDatasheetLoaderService
{
    private readonly IManifestService _manifestService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatasheetLoaderService"/> class.
    /// </summary>
    /// <param name="manifestService">The manifest service.</param>
    public DatasheetLoaderService(IManifestService manifestService)
    {
        _manifestService = manifestService;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<TDataType>> LoadDatasheetDataAsync<TDataType>
    (
        PackedFileInfo datasheet,
        PS2Environment environment,
        CancellationToken ct = default
    )
    {
        using MemoryOwner<byte> data = await GetRawDatasheetAsync(datasheet, environment, ct)
            .ConfigureAwait(false);

        return DatasheetSerializer.Deserialize<TDataType>(data.Memory);
    }

    /// <inheritdoc />
    public async Task<MemoryOwner<byte>> GetRawDatasheetAsync
    (
        PackedFileInfo datasheet,
        PS2Environment environment,
        CancellationToken ct = default
    )
    {
        ManifestFile packFile = await _manifestService.GetFileAsync
        (
            datasheet.PackName,
            environment,
            ct
        ).ConfigureAwait(false);

        await using Stream packFileStream = await _manifestService.GetFileDataAsync(packFile, ct)
            .ConfigureAwait(false);

        await using StreamDataReaderService sdrs = new(packFileStream, false);
        using Pack2Reader reader = new(sdrs);

        IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(ct)
            .ConfigureAwait(false);

        Asset2Header? header = assetHeaders.FirstOrDefault(h => h.NameHash == datasheet.NameHash);
        if (header is null)
            throw new ArgumentException("The given datasheet could not be found");

        return await reader.ReadAssetDataAsync(header, ct).ConfigureAwait(false);
    }
}
