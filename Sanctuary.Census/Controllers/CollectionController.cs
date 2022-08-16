using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using MongoDB.Bson;
using MongoDB.Driver;
using Sanctuary.Census.Common;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Database;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.Json;
using Sanctuary.Census.Models;
using Sanctuary.Census.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    private static readonly JsonSerializerOptions _noIncludeNullOptions;
    private static readonly JsonSerializerOptions _includeNullOptions;

    private readonly IMongoContext _mongoContext;
    private readonly IMemoryCache _memoryCache;

    static CollectionController()
    {
        _noIncludeNullOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy()
        };
        _noIncludeNullOptions.Converters.Add(new DataResponseJsonConverter());
        _noIncludeNullOptions.Converters.Add(new BsonDocumentJsonConverter());

        _includeNullOptions = new JsonSerializerOptions(_noIncludeNullOptions)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionController"/> class.
    /// </summary>
    /// <param name="mongoContext">The Mongo DB collections context.</param>
    /// <param name="memoryCache">The memory cache.</param>
    public CollectionController
    (
        IMongoContext mongoContext,
        IMemoryCache memoryCache
    )
    {
        _mongoContext = mongoContext;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// Gets the available collections.
    /// </summary>
    /// <param name="environment">The environment to retrieve the collections from.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The available collections.</returns>
    /// <response code="200">Returns the list of collections.</response>
    [HttpGet("get/{environment}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<DataResponse<Datatype>> GetDatatypesAsync(string environment, CancellationToken ct = default)
    {
        PS2Environment env = ParseEnvironment(environment);
        IReadOnlyList<Datatype> dataTypes = await GetAndCacheDatatypeListAsync(env, ct);
        return new DataResponse<Datatype>(dataTypes, "datatype", null);
    }

    /// <summary>
    /// Counts the number of available collections.
    /// </summary>
    /// <param name="environment">The environment to retrieve the collections from.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The collection count.</returns>
    /// <response code="200">Returns the number of collections.</response>
    [HttpGet("count/{environment}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<CollectionCount> CountDatatypesAsync(string environment, CancellationToken ct = default)
    {
        PS2Environment env = ParseEnvironment(environment);
        return (await GetAndCacheDatatypeListAsync(env, ct)).Count;
    }

    /// <summary>
    /// Queries a collection.
    /// </summary>
    /// <param name="environment">The environment to retrieve the collection from.</param>
    /// <param name="collectionName">The name of the collection to query.</param>
    /// <param name="queryParams">The query parameters.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The results of the query.</returns>
    /// <response code="200">Returns the result of the query.</response>
    [HttpGet("/get/{environment}/{collectionName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<JsonResult> QueryCollectionAsync
    (
        string environment,
        string collectionName,
        [FromQuery] CollectionQueryParameters queryParams,
        CancellationToken ct = default
    )
    {
        try
        {
            LangProjectionBuilder? langProjections = queryParams.Lang is null
                ? null
                : new LangProjectionBuilder(queryParams.Lang.Split(','));

            IAggregateFluent<BsonDocument> query = ConstructBasicQuery
                (
                    environment,
                    collectionName,
                    queryParams,
                    langProjections,
                    out IMongoCollection<BsonDocument> mongoCollection
                )
                .Skip(queryParams.Start)
                .Limit(queryParams.LimitPerDb ?? queryParams.Limit);

            if (queryParams.Join is not null)
            {
                int totalLookups = 0;
                foreach (string value in queryParams.Join)
                {
                    List<JoinBuilder> builders = JoinBuilder.Parse(value);
                    foreach (JoinBuilder b in builders)
                    {
                        b.Build
                        (
                            ref query,
                            collectionName,
                            mongoCollection.DocumentSerializer,
                            mongoCollection.Settings.SerializerRegistry,
                            !queryParams.IsCaseSensitive,
                            langProjections,
                            out int builtLookups
                        );
                        totalLookups += builtLookups;
                    }

                    if (totalLookups > JoinBuilder.MAX_JOINS)
                    {
                        throw new QueryException
                        (
                            QueryErrorCode.JoinLimitExceeded,
                            $"Up to {JoinBuilder.MAX_JOINS} may be performed in a single query"
                        );
                    }
                }
            }

            if (queryParams.Tree is not null)
            {
                GroupBuilder groupBuilder = GroupBuilder.ParseFromTreeCommand(queryParams.Tree);
                groupBuilder.BuildAndAppendTo(ref query);
            }

            Stopwatch st = new();
            st.Start();
            List<BsonDocument> records = await query.ToListAsync(ct);
            st.Stop();

            return new JsonResult
            (
                new DataResponse<BsonDocument>
                (
                    records,
                    collectionName,
                    queryParams.ShowTimings
                        ? new QueryTimes
                        (
                            st.Elapsed.Milliseconds,
                            _memoryCache.TryGetValue((typeof(Datatype), environment), out IReadOnlyList<Datatype> types)
                                ? types.FirstOrDefault(d => d.Name == collectionName)?.LastUpdated
                                : null
                        )
                        : null
                ),
                queryParams.IncludeNullFields ? _includeNullOptions : _noIncludeNullOptions
            );
        }
        catch (QueryException quex)
        {
            return new JsonResult
            (
                new ErrorResponse(quex.ErrorCode, quex.Message)
            ) { StatusCode = StatusCodes.Status400BadRequest };
        }
        catch (MongoCommandException mcex) when (mcex.Message.StartsWith("Command aggregate failed: Invalid $project"))
        {
            return new JsonResult
            (
                new ErrorResponse(QueryErrorCode.InvalidProjection, "Ensure you don't have conflicting c:show and c:hide commands")
            ) { StatusCode = StatusCodes.Status400BadRequest };
        }
    }

    /// <summary>
    /// Gets the number of documents that match a particular query on a collection.
    /// </summary>
    /// <param name="environment">The environment to retrieve the collection from.</param>
    /// <param name="collectionName">The name of the collection to query.</param>
    /// <param name="queryParams">The query parameters.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The number of documents that match the query.</returns>
    /// <response code="200">Returns the result of the query.</response>
    [HttpGet("/count/{environment}/{collectionName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<object> CountCollectionAsync
    (
        string environment,
        string collectionName,
        [FromQuery] CollectionQueryParameters queryParams,
        CancellationToken ct = default
    )
    {
        try
        {
            IAggregateFluent<AggregateCountResult> count = ConstructBasicQuery
            (
                environment,
                collectionName,
                queryParams,
                null,
                out _
            ).Count();

            AggregateCountResult res = await count.FirstAsync(ct);
            return new CollectionCount((ulong)res.Count);
        }
        catch (QueryException quex)
        {
            return new ErrorResponse(quex.ErrorCode, quex.Message);
        }
        catch (MongoCommandException mcex) when (mcex.Message.StartsWith("Command aggregate failed: Invalid $project"))
        {
            return new JsonResult
            (
                new ErrorResponse(QueryErrorCode.InvalidProjection, "Ensure you don't have conflicting c:show and c:hide commands")
            ) { StatusCode = StatusCodes.Status400BadRequest };
        }
    }

    private IAggregateFluent<BsonDocument> ConstructBasicQuery
    (
        string environment,
        string collectionName,
        CollectionQueryParameters queryParams,
        LangProjectionBuilder? langProjections,
        out IMongoCollection<BsonDocument> mongoCollection
    )
    {
        ValidateQueryParams(queryParams);
        if (!CollectionUtils.CheckCollectionExists(collectionName))
            throw new QueryException(QueryErrorCode.UnknownCollection, $"The {collectionName} collection does not exist");

        PS2Environment env = ParseEnvironment(environment);
        IMongoDatabase db = _mongoContext.GetDatabase(env);
        mongoCollection = db.GetCollection<BsonDocument>(collectionName);

        FilterDefinitionBuilder<BsonDocument> filterBuilder = Builders<BsonDocument>.Filter;
        FilterDefinition<BsonDocument> filter = filterBuilder.Empty;
        if (queryParams.HasFields is not null)
        {
            foreach (string value in queryParams.HasFields.SelectMany(s => s.Split(',')))
                filter &= filterBuilder.Ne(value, BsonNull.Value);
        }

        foreach ((string paramName, StringValues paramValues) in HttpContext.Request.Query)
        {
            if (paramName.AsSpan().StartsWith(QueryCommandIdentifier))
                continue;

            foreach (string value in paramValues)
            {
                FilterBuilder fb = FilterBuilder.Parse(collectionName, paramName, value);
                fb.Build(ref filter, !queryParams.IsCaseSensitive);
            }
        }

        ProjectionBuilder projection;
        if (queryParams.Show is not null)
            projection = new ProjectionBuilder(false, queryParams.Show.SelectMany(s => s.Split(',')));
        else if (queryParams.Hide is not null)
            projection = new ProjectionBuilder(true, queryParams.Hide.SelectMany(s => s.Split(',')));
        else
            projection = new ProjectionBuilder(true);

        langProjections?.AppendToProjection(projection, collectionName);

        IAggregateFluent<BsonDocument> aggregate = mongoCollection.Aggregate()
            .Match(filter)
            .Project(projection.Build());

        if (queryParams.Sorts is not null)
        {
            foreach (string value in queryParams.Sorts.SelectMany(s => s.Split(',')))
            {
                string[] components = value.Split(':');
                int sortDirection = 1;

                if (components.Length == 2 && components[1] == "-1")
                    sortDirection = -1;

                aggregate = aggregate.Sort(new BsonDocument(components[0], sortDirection));
            }
        }

        return aggregate;
    }

    private static void ValidateQueryParams(CollectionQueryParameters queryParams)
    {
        if (queryParams.Limit > MAX_LIMIT || queryParams.LimitPerDb > MAX_LIMIT)
        {
            throw new QueryException
            (
                QueryErrorCode.InvalidCommandValue,
                "The maximum value of the c:limit command is " + MAX_LIMIT
            );
        }

        if (queryParams.Hide is not null && queryParams.Show is not null)
        {
            throw new QueryException
            (
                QueryErrorCode.Malformed,
                "The c:show and c:hide commands are not compatible with each other"
            );
        }
    }

    private static PS2Environment ParseEnvironment(string environment)
        => environment.ToLower() switch {
            "ps2" or "ps2:v2" => PS2Environment.PS2,
            "pts" => PS2Environment.PTS,
            _ => throw new QueryException
            (
                QueryErrorCode.InvalidNamespace,
                "Valid namespaces are: " + string.Join(", ", Enum.GetNames<PS2Environment>())
            )
        };

    private async ValueTask<IReadOnlyList<Datatype>> GetAndCacheDatatypeListAsync(PS2Environment environment, CancellationToken ct)
    {
        if (_memoryCache.TryGetValue((typeof(Datatype), environment), out List<Datatype>? dataTypes) && dataTypes is not null)
            return dataTypes;

        IMongoCollection<Datatype> db = _mongoContext.GetCollection<Datatype>(environment);
        dataTypes = await db.Find(new BsonDocument())
            .Sort(Builders<Datatype>.Sort.Ascending(x => x.Name))
            .ToListAsync(ct);

        _memoryCache.Set((typeof(Datatype), environment), dataTypes, TimeSpan.FromMinutes(15));
        return dataTypes;
    }

    // private RedirectResult GetRedirectToCensusResult()
    // {
    //     string url = "http://census.daybreakgames.com";
    //     if (HttpContext.Items.TryGetValue("ServiceId", out object? serviceId))
    //         url += (string)serviceId!;
    //
    //     return Redirect(url + HttpContext.Request.Path + HttpContext.Request.QueryString);
    // }
}
