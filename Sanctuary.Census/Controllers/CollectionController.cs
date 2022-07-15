using Microsoft.AspNetCore.Mvc;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Models;
using System.Collections.Generic;

namespace Sanctuary.Census.Controllers;

/// <summary>
/// Returns collection data.
/// </summary>
[ApiController]
[Produces("application/json")]
public class CollectionController : ControllerBase
{
    private readonly CollectionsContext _collectionsContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionController"/> class.
    /// </summary>
    /// <param name="collectionsContext">The collections context.</param>
    public CollectionController(CollectionsContext collectionsContext)
    {
        _collectionsContext = collectionsContext;
    }

    /// <summary>
    /// Gets the available collections.
    /// </summary>
    /// <param name="environment">The environment to retrieve the collections of.</param>
    /// <returns>The available collections.</returns>
    [HttpGet("get/{environment}")]
    public ActionResult<IReadOnlyList<CollectionInfo>> GetCollectionInfos(PS2Environment environment)
        => environment is not PS2Environment.PS2
            ? GetInvalidEnvironmentResult()
            : new ActionResult<IReadOnlyList<CollectionInfo>>(_collectionsContext.CollectionInfos);

    /// <summary>
    /// Counts the number of a available collections.
    /// </summary>
    /// <param name="environment">The environment to retrieve the collections of.</param>
    /// <returns>The collection count.</returns>
    [HttpGet("count/{environment}")]
    public ActionResult<CollectionCount> CountCollectionInfos(PS2Environment environment) =>
        environment is not PS2Environment.PS2
            ? GetInvalidEnvironmentResult()
            : new ActionResult<CollectionCount>(_collectionsContext.CollectionInfos.Count);

    private NotFoundObjectResult GetInvalidEnvironmentResult()
        => NotFound("Valid environments are " + PS2Environment.PS2);
}
