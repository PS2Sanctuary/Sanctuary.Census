using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance.Buffers;
using Mandible.Pack2;
using Mandible.Services;
using Mandible.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.Objects;
using Sanctuary.Census.ClientData.Objects.ClientDataModels;
using Sanctuary.Census.ClientData.Util;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects;

namespace Sanctuary.Census.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class ManifestController : ControllerBase
{
    private readonly ILogger<ManifestController> _logger;
    private readonly IManifestService _manifestService;

    public ManifestController
    (
        ILogger<ManifestController> logger,
        IManifestService manifestService
    )
    {
        _logger = logger;
        _manifestService = manifestService;
    }

    [HttpGet("{fileName}")]
    public async Task<ActionResult<ManifestFile>> GetManifestFileAsync(string fileName, CancellationToken ct)
    {
        try
        {
            return await _manifestService.GetFileAsync(fileName, PS2Environment.Live, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve manifest file information");
            return NotFound();
        }
    }

    [HttpGet("data")]
    public async Task<ActionResult<IEnumerable<ClientItemDatasheetData>>> ParseFireModeDataAsync(CancellationToken ct)
    {
        try
        {
            ManifestFile file = await _manifestService.GetFileAsync("data_x64_0.pack2", PS2Environment.Live, ct);
            await using Stream fileData = await _manifestService.GetFileDataAsync(file, ct);

            await using StreamDataReaderService sdrs = new(fileData, false);
            using Pack2Reader reader = new(sdrs);

            ulong nameHash = PackCrc64.Calculate("ClientItemDatasheetData.txt");
            IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(ct).ConfigureAwait(false);

            Asset2Header? header = assetHeaders.FirstOrDefault(h => h.NameHash == nameHash);
            if (header is null)
                return NotFound();

            using MemoryOwner<byte> data = await reader.ReadAssetDataAsync(header, ct).ConfigureAwait(false);
            IEnumerable<ClientItemDatasheetData> records = DatasheetSerializer.Deserialize<ClientItemDatasheetData>(data.Memory);
            return new ActionResult<IEnumerable<ClientItemDatasheetData>>(records);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve manifest file information");
            return StatusCode(500);
        }
    }
}
