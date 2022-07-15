using Microsoft.AspNetCore.Mvc;
using Sanctuary.Census.CollectionModels;
using System.Collections.Generic;
using System.Linq;

namespace Sanctuary.Census.Controllers;

/// <summary>
/// Returns data built through the contribution factory.
/// </summary>
[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class ContributionController : ControllerBase
{
    private readonly CollectionsContext _collectionsContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContributionController"/> class.
    /// </summary>
    /// <param name="collectionsContext">The collections context.</param>
    public ContributionController(CollectionsContext collectionsContext)
    {
        _collectionsContext = collectionsContext;
    }

    /// <summary>
    /// Retrieves data from the Item collection.
    /// </summary>
    /// <param name="id">The ID of the item to retrieve.</param>
    /// <param name="start">The position in the collection at which to start listing items from.</param>
    /// <returns>The selected items.</returns>
    [HttpGet("item")]
    public ActionResult<IEnumerable<Item>> GetItems
    (
        [FromQuery(Name = "item_id")] uint? id = null,
        [FromQuery(Name = "c:start")] int start = 0
    )
    {
        if (start < 0)
            return BadRequest("c:start must be non-negative");

        if (id is null)
            return new ActionResult<IEnumerable<Item>>(_collectionsContext.Items.Values.Skip(start).Take(100));

        if (_collectionsContext.Items.ContainsKey(id.Value))
            return new[] { _collectionsContext.Items[id.Value] };

        return NotFound();
    }

    /// <summary>
    /// Retrieves data from the Weapon collection.
    /// </summary>
    /// <param name="id">The ID of the weapon to retrieve.</param>
    /// <param name="start">The position in the collection at which to start listing weapons from.</param>
    /// <returns>The selected weapons.</returns>
    [HttpGet("weapon")]
    public ActionResult<IEnumerable<Weapon>> GetWeapons
    (
        [FromQuery(Name = "weapon_id")] uint? id = null,
        [FromQuery(Name = "c:start")] int start = 0
    )
    {
        if (start < 0)
            return BadRequest("c:start must be non-negative");

        if (id is null)
            return new ActionResult<IEnumerable<Weapon>>(_collectionsContext.Weapons.Values.Skip(start).Take(100));

        if (_collectionsContext.Weapons.ContainsKey(id.Value))
            return new[] { _collectionsContext.Weapons[id.Value] };

        return NotFound();
    }

    /// <summary>
    /// Retrieves data from the Weapon collection.
    /// </summary>
    /// <param name="id">The ID of the weapon to retrieve.</param>
    /// <param name="start">The position in the collection at which to start listing weapons from.</param>
    /// <returns>The selected weapons.</returns>
    [HttpGet("test")]
    public ActionResult<IEnumerable<FireMode>> TestGet
    (
        [FromQuery(Name = "world_id")] uint? id = null,
        [FromQuery(Name = "c:start")] int start = 0
    )
    {
        if (start < 0)
            return BadRequest("c:start must be non-negative");

        if (id is null)
            return new ActionResult<IEnumerable<FireMode>>(_collectionsContext.FireModes.Values.Skip(start).Take(100));

        if (_collectionsContext.FireModes.ContainsKey(id.Value))
            return new[] { _collectionsContext.FireModes[id.Value] };

        return NotFound();
    }
}
