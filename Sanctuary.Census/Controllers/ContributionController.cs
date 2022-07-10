using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects.DtoModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Controllers;

/// <summary>
/// Returns data built through the contribution factory.
/// </summary>
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class ContributionController
{
    private readonly ILogger<ContributionController> _logger;
    private readonly IContributionService _contributionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContributionController"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="contributionService">The contribution service.</param>
    public ContributionController
    (
        ILogger<ContributionController> logger,
        IContributionService contributionService
    )
    {
        _logger = logger;
        _contributionService = contributionService;
    }

    /// <summary>
    /// Builds the Item collection.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The built collection.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Item>>> GetAsync(CancellationToken ct)
    {
        try
        {
            IReadOnlyList<Item> builtItems = await _contributionService.BuildThroughContributions(id => Item.Default with { ItemID = id }, ct)
                .ConfigureAwait(false);

            return new ActionResult<IEnumerable<Item>>(builtItems.Take(10));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build type");
            return new StatusCodeResult(500);
        }
    }
}
