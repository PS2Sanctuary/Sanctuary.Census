using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanctuary.Census.Controllers;

/// <summary>
/// Returns collection data.
/// </summary>
[ApiController]
[Produces("application/json")]
public class CollectionController : ControllerBase
{
    /// <summary>
    /// The maximum number of elements that may be returned from a query.
    /// </summary>
    public const int MAX_LIMIT = 10000;

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
    public ActionResult<DataResponse<Datatype>> GetCollectionInfos(PS2Environment environment)
        => environment is not PS2Environment.PS2
            ? GetInvalidEnvironmentResult()
            : new DataResponse<Datatype>(_collectionsContext.Datatypes, "datatype");

    /// <summary>
    /// Counts the number of a available collections.
    /// </summary>
    /// <param name="environment">The environment to retrieve the collections of.</param>
    /// <returns>The collection count.</returns>
    [HttpGet("count/{environment}")]
    public ActionResult<CollectionCount> CountCollectionInfos(PS2Environment environment) =>
        environment is not PS2Environment.PS2
            ? GetInvalidEnvironmentResult()
            : new ActionResult<CollectionCount>(_collectionsContext.Datatypes.Count);

    /// <summary>
    /// Retrieves data from a collection.
    /// </summary>
    /// <param name="environment">The environment to retrieve the collection from.</param>
    /// <param name="collectionName">The name of the collection to retrieve data from.</param>
    /// <param name="start">The index into the collection at which to start returning elements.</param>
    /// <param name="limit">The maximum number of elements to return from the collection.</param>
    /// <returns>Elements of the collection.</returns>
    [HttpGet("get/{environment}/{collectionName}")]
    public ActionResult<DataResponse<object>> GetCollection
    (
        PS2Environment environment,
        string collectionName,
        [FromQuery(Name = "c:start")] int start,
        [FromQuery(Name = "c:limit")] int limit = 100
    )
    {
        if (start < 0)
            return BadRequest("c:start may not be less than zero");

        if (limit is < 1 or > MAX_LIMIT)
            return BadRequest($"c:limit must be between 1 and {MAX_LIMIT}, inclusive");

        uint? id = null;
        foreach ((string paramName, StringValues paramValues) in HttpContext.Request.Query)
        {
            if (paramName.StartsWith("c:"))
                continue;

            if (!paramName.Contains("_id"))
                return BadRequest("Filtering on fields other than the collection's ID field is not currently support");

            if (paramValues.Count != 1)
                return BadRequest("Searching for more than a single ID is not currently supported");

            if (!int.TryParse(paramValues[0], out int maybeId))
                return BadRequest("Failed to parse the ID value");

            id = (uint?)maybeId;
        }

        return collectionName switch {
            "currency" => ConvertCollection(_collectionsContext.Currencies, id, start, limit, "currency"),
            "experience" => ConvertCollection(_collectionsContext.Experiences, id, start, limit, "experience"),
            "faction" => ConvertCollection(_collectionsContext.Factions, id, start, limit, "faction"),
            "fire_group" => ConvertCollection(_collectionsContext.FireGroups, id, start, limit, "fire_group"),
            "fire_group_to_fire_mode" => ConvertCollection(_collectionsContext.FireGroupsToFireModes, id, start, limit, "fire_group_to_fire_mode"),
            "fire_mode_2" => ConvertCollection(_collectionsContext.FireModes, id, start, limit, "fire_mode_2"),
            "fire_mode_to_projectile" => ConvertCollection(_collectionsContext.FireModeToProjectileMap, id, start, limit, "fire_mode_to_projectile"),
            "item" => ConvertCollection(_collectionsContext.Items, id, start, limit, "item"),
            "item_category" => ConvertCollection(_collectionsContext.ItemCategories, id, start, limit, "item_category"),
            "item_to_weapon" => ConvertCollection(_collectionsContext.ItemsToWeapon, id, start, limit, "item_to_weapon"),
            "weapon" => ConvertCollection(_collectionsContext.Weapons, id, start, limit, "weapon"),
            "weapon_ammo_slot" => ConvertCollection(_collectionsContext.WeaponAmmoSlots, id, start, limit, "weapon_ammo_slot"),
            "world" => ConvertCollection(_collectionsContext.Worlds, id, start, limit, "world"),
            _ => NotFound()
        };
    }

    private static DataResponse<object> ConvertCollection<TValue>
    (
        IReadOnlyDictionary<uint, TValue> collection,
        uint? id,
        int start,
        int limit,
        string dataTypeName
    ) where TValue : notnull
    {
        if (typeof(TValue).IsGenericType && typeof(TValue).GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
        {
            if (id is not null)
            {
                return !collection.ContainsKey(id.Value)
                    ? new DataResponse<object>(Array.Empty<TValue>(), dataTypeName)
                    : new DataResponse<object>((IReadOnlyList<object>)collection[id.Value], dataTypeName);
            }

            List<object> elements = collection.Values
                .Cast<IReadOnlyList<object>>()
                .SelectMany(o => o)
                .Skip(start)
                .Take(limit)
                .ToList();

            return new DataResponse<object>(elements, dataTypeName);
        }

        if (id is not null)
        {
            return !collection.ContainsKey(id.Value)
                ? new DataResponse<object>(Array.Empty<TValue>(), dataTypeName)
                : new DataResponse<object>(collection[id.Value], dataTypeName);
        }

        List<object> elements2 = collection.Values
            .Skip(start)
            .Take(limit)
            .Cast<object>()
            .ToList();

        return new DataResponse<object>(elements2, dataTypeName);
    }

    private NotFoundObjectResult GetInvalidEnvironmentResult()
        => NotFound("Valid environments are " + PS2Environment.PS2);
}
