using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sanctuary.Census.Abstractions.Services;
using Sanctuary.Census.Objects;

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

    [HttpGet("data/{fileName}")]
    public async Task<ActionResult> GetManifestFileDataAsync(string fileName, CancellationToken ct)
    {
        try
        {
            ManifestFile file = await _manifestService.GetFileAsync(fileName, PS2Environment.Live, ct);
            await using Stream data = await _manifestService.GetFileDataAsync(file, ct);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve manifest file information");
            return NotFound();
        }
    }
}
