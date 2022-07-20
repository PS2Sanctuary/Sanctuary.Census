using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using MongoDB.Bson;
using MongoDB.Driver;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

    private static readonly char[] QueryCommandIdentifier = { 'c', ':' };

    private readonly IMongoContext _mongoContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionController"/> class.
    /// </summary>
    /// <param name="mongoContext">The Mongo DB collections context.</param>
    public CollectionController(IMongoContext mongoContext)
    {
        _mongoContext = mongoContext;
    }

    /// <summary>
    /// Gets the available collections.
    /// </summary>
    /// <param name="environment">The environment to retrieve the collections of.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The available collections.</returns>
    /// <response code="200">Returns the list of collections.</response>
    [HttpGet("get/{environment}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<DataResponse<Datatype>> GetDatatypesAsync(PS2Environment environment, CancellationToken ct = default)
    {
        IMongoDatabase db = _mongoContext.GetDatabase(environment);

        List<Datatype> dataTypes = new();
        IAsyncCursor<string> collNames = await db.ListCollectionNamesAsync(cancellationToken: ct);
        while (await collNames.MoveNextAsync(ct))
        {
            foreach (string collName in collNames.Current)
            {
                IMongoCollection<BsonDocument> coll = db.GetCollection<BsonDocument>(collName);
                long count = await coll.CountDocumentsAsync(new BsonDocument(), null, ct);
                dataTypes.Add(new Datatype(collName, (int)count));
            }
        }

        return new DataResponse<Datatype>(dataTypes, "datatype", null);
    }

    /// <summary>
    /// Counts the number of available collections.
    /// </summary>
    /// <param name="environment">The environment to retrieve the collections of.</param>
    /// <returns>The collection count.</returns>
    /// <response code="200">Returns the number of collections.</response>
    [HttpGet("count/{environment}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public CollectionCount CountDatatypes(PS2Environment environment)
    {
        IMongoDatabase db = _mongoContext.GetDatabase(environment);
        return db.ListCollections().ToList().Count;
    }

    [HttpGet("/get/{environment}/{collectionName}")]
    public async Task<DataResponse<BsonDocument>> Test
    (
        PS2Environment environment,
        string collectionName,
        [FromQuery(Name = "c:start")] int start = 0,
        [FromQuery(Name = "c:limit")] int limit = 100,
        [FromQuery(Name = "c:show")] IEnumerable<string>? show = null,
        [FromQuery(Name = "c:hide")] IEnumerable<string>? hide = null,
        [FromQuery(Name = "c:sort")] IEnumerable<string>? sortList = null,
        [FromQuery(Name = "c:has")] IEnumerable<string>? has = null,
        [FromQuery(Name = "c:timing")] bool timing = false
    )
    {
        IMongoDatabase db = _mongoContext.GetDatabase(environment);
        IMongoCollection<BsonDocument> coll = db.GetCollection<BsonDocument>(collectionName);

        ProjectionDefinition<BsonDocument>? projection = Builders<BsonDocument>.Projection
            .Exclude("_id");

        if (show is not null)
        {
            foreach (string value in show.SelectMany(s => s.Split(',')))
                projection = projection.Include(value);
        }

        if (hide is not null)
        {
            foreach (string value in hide.SelectMany(h => h.Split(',')))
                projection = projection.Exclude(value);
        }

        FilterDefinitionBuilder<BsonDocument> filterBuilder = Builders<BsonDocument>.Filter;
        FilterDefinition<BsonDocument> filter = filterBuilder.Empty;
        if (has is not null)
        {
            foreach (string value in has.SelectMany(s => s.Split(',')))
                filter &= filterBuilder.Ne(value, BsonNull.Value);
        }

        foreach ((string paramName, StringValues paramValues) in HttpContext.Request.Query)
        {
            if (paramName.AsSpan().StartsWith(QueryCommandIdentifier))
                continue;

            foreach (string value in paramValues.SelectMany(s => s.Split(',')))
            {
                if (value.Length == 0)
                    continue;

                if (value[0] is '<' or '[' or '>' or ']' or '^' or '*' or '!' && value.Length < 2)
                    continue;

                // TODO: Need to convert value to correct type
                filter &= value[0] switch {
                    '<' => filterBuilder.Lt(paramName, value[1..]),
                    '[' => filterBuilder.Lte(paramName, value[1..]),
                    '>' => filterBuilder.Gt(paramName, value[1..]),
                    ']' => filterBuilder.Gte(paramName, value[1..]),
                    '^' => filterBuilder.Regex(paramName, value),
                    '*' => filterBuilder.Regex(paramName, value[1..]),
                    '!' => filterBuilder.Ne(paramName, value[1..]),
                    _ => filterBuilder.Eq(paramName, value)
                };
            }
        }

        IAggregateFluent<BsonDocument> aggregate = coll.Aggregate()
            .Match(filter)
            .Project(projection);

        if (sortList is not null)
        {
            foreach (string value in sortList.SelectMany(s => s.Split(',')))
            {
                string[] components = value.Split(':');
                int sortDirection = 1;

                if (components.Length == 2 && components[1] == "-1")
                    sortDirection = -1;

                aggregate = aggregate.Sort(new BsonDocument(components[0], sortDirection));
            }
        }

        BsonArray subPipeline = new()
        {
            new BsonDocument
            (
                "$match",
                new BsonDocument
                (
                    "$expr",
                    new BsonDocument
                    (
                        "$eq",
                        new BsonArray { 0, "$fire_mode_index" }
                    )
                )
            ),
            new BsonDocument
            (
                "$project",
                new BsonDocument("_id", 0)
            ),
            new BsonDocument
            (
                "$lookup",
                new BsonDocument("from", "fire_group")
                    .Add("localField", "fire_group_id")
                    .Add("foreignField", "fire_group_id")
                    .Add("as", "fire_mode_to_fire_group")
            )
        };

        BsonDocument lookup = new
        (
            "$lookup",
            new BsonDocument("from", "fire_group_to_fire_mode")
                .Add("localField", "fire_group_id")
                .Add("foreignField", "fire_group_id")
                .Add("pipeline", subPipeline)
                .Add("as", "fire_group_to_fire_mode")
        );

        aggregate = aggregate.Skip(start)
            .Limit(limit)
            .AppendStage<BsonDocument>(lookup);

        Stopwatch st = new();
        st.Start();
        List<BsonDocument> records = await aggregate.ToListAsync();
        st.Stop();
        return new DataResponse<BsonDocument>
        (
            records,
            collectionName,
            timing ? st.Elapsed : null
        );
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
        IReadOnlyList<object> filtered = FilterCollection(collection, id, start, limit);
        return new DataResponse<object>(filtered, dataTypeName, null);
    }

    private static IReadOnlyList<object> FilterCollection<TValue>
    (
        IReadOnlyDictionary<uint, TValue> collection,
        uint? id,
        int start,
        int limit
    ) where TValue : notnull
    {
        if (typeof(TValue).IsGenericType && typeof(TValue).GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
        {
            if (id is not null)
            {
                return !collection.ContainsKey(id.Value)
                    ? Array.Empty<object>()
                    : (IReadOnlyList<object>)collection[id.Value];
            }

            return collection.Values
                .Cast<IReadOnlyList<object>>()
                .SelectMany(o => o)
                .Skip(start)
                .Take(limit)
                .ToList();
        }

        if (id is not null)
        {
            return !collection.ContainsKey(id.Value)
                ? Array.Empty<object>()
                : new object[] { collection[id.Value] };
        }

        List<object> elements2 = collection.Values
            .Skip(start)
            .Take(limit)
            .Cast<object>()
            .ToList();

        return elements2;
    }

    private RedirectResult GetRedirectToCensusResult()
    {
        string url = "http://census.daybreakgames.com";
        if (HttpContext.Items.TryGetValue("ServiceId", out object? serviceId))
            url += (string)serviceId!;

        return Redirect(url + HttpContext.Request.Path + HttpContext.Request.QueryString);
    }

    private BadRequestObjectResult GetInvalidEnvironmentResult()
        => BadRequest("Valid environments are " + PS2Environment.PS2);
}
