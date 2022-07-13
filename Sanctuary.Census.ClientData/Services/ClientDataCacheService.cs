using CommunityToolkit.HighPerformance.Buffers;
using Mandible.Abstractions.Pack2;
using Mandible.Pack2;
using Mandible.Services;
using Mandible.Util;
using Sanctuary.Census.ClientData.Abstractions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.Objects.ClientDataModels;
using Sanctuary.Census.ClientData.Util;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.ClientData.Services;

/// <inheritdoc />
public class ClientDataCacheService : IClientDataCacheService
{
    private readonly IManifestService _manifestService;
    private readonly EnvironmentContextProvider _environmentContextProvider;

    /// <inheritdoc />
    public DateTimeOffset LastPopulated { get; private set; }

    /// <inheritdoc />
    public List<ClientItemDatasheetData> ClientItemDatasheetDatas { get; private set; }

    /// <inheritdoc />
    public List<ClientItemDefinition> ClientItemDefinitions { get; private set; }

    /// <inheritdoc />
    public List<FireModeDisplayStat> FireModeDisplayStats { get; private set; }

    /// <inheritdoc />
    public List<ImageSetMapping> ImageSetMappings { get; private set; }

    /// <inheritdoc />
    public List<ItemProfile> ItemProfiles { get; private set; }

    /// <inheritdoc />
    public List<ResourceType> ResourceTypes { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientDataCacheService"/> class.
    /// </summary>
    /// <param name="manifestService">The manifest service.</param>
    /// <param name="environmentContextProvider">The environment context provider.</param>
    public ClientDataCacheService
    (
        IManifestService manifestService,
        EnvironmentContextProvider environmentContextProvider
    )
    {
        _manifestService = manifestService;
        _environmentContextProvider = environmentContextProvider;

        ClientItemDatasheetDatas = new List<ClientItemDatasheetData>();
        ClientItemDefinitions = new List<ClientItemDefinition>();
        FireModeDisplayStats = new List<FireModeDisplayStat>();
        ImageSetMappings = new List<ImageSetMapping>();
        ItemProfiles = new List<ItemProfile>();
        ResourceTypes = new List<ResourceType>();
    }

    /// <inheritdoc />
    public async Task Repopulate(CancellationToken ct = default)
    {
        ManifestFile dataPackManifest = await _manifestService.GetFileAsync
        (
            "data_x64_0.pack2",
            _environmentContextProvider.Environment,
            ct
        ).ConfigureAwait(false);

        await using Stream dataPackStrema = await _manifestService.GetFileDataAsync(dataPackManifest, ct)
            .ConfigureAwait(false);
        await using StreamDataReaderService sdrs = new(dataPackStrema, false);

        using Pack2Reader reader = new(sdrs);
        IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(ct).ConfigureAwait(false);

        ClientItemDatasheetDatas = await ExtractDatasheet<ClientItemDatasheetData>
        (
            "ClientItemDatasheetData.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        ClientItemDefinitions = await ExtractDatasheet<ClientItemDefinition>
        (
            "ClientItemDefinitions.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        FireModeDisplayStats = await ExtractDatasheet<FireModeDisplayStat>
        (
            "FireModeDisplayStats.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        ImageSetMappings = await ExtractDatasheet<ImageSetMapping>
        (
            "ImageSetMappings.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        ItemProfiles = await ExtractDatasheet<ItemProfile>
        (
            "ItemProfiles.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        ResourceTypes = await ExtractDatasheet<ResourceType>
        (
            "ResourceTypes.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        LastPopulated = DateTimeOffset.UtcNow;
    }

    private static async Task<List<TDataType>> ExtractDatasheet<TDataType>
    (
        string datasheetFileName,
        IEnumerable<Asset2Header> assetHeaders,
        IPack2Reader packReader,
        CancellationToken ct
    )
    {
        ulong nameHash = PackCrc64.Calculate(datasheetFileName);
        Asset2Header header = assetHeaders.First(ah => ah.NameHash == nameHash);

        using MemoryOwner<byte> data = await packReader.ReadAssetDataAsync(header, ct).ConfigureAwait(false);
        return DatasheetSerializer.Deserialize<TDataType>(data.Memory).ToList();
    }
}
