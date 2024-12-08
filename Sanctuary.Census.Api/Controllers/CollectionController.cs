using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using MongoDB.Bson;
using MongoDB.Driver;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Api.Database;
using Sanctuary.Census.Api.Exceptions;
using Sanctuary.Census.Api.Json;
using Sanctuary.Census.Api.Models;
using Sanctuary.Census.Api.Services;
using Sanctuary.Census.Api.Util;
using Sanctuary.Census.Common.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Api.Controllers;

/// <summary>
/// Returns collection data.
/// </summary>
/// <response code="400">Returns an error indicating how the request was malformed.</response>
/// <response code="500">Returns an error indicating that the application has malfunctioned.</response>
[ApiController]
[Produces("application/json")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class CollectionController : ControllerBase
{
    /// <summary>
    /// The maximum number of elements that may be returned from a query.
    /// </summary>
    public const int MAX_LIMIT = 10000;

    private static readonly char[] QueryCommandIdentifier = { 'c', ':' };
    private static readonly JsonSerializerOptions _defaultOptions;
    private static readonly JsonSerializerOptions _incNullOptions;
    private static readonly JsonSerializerOptions _allStringOptions;
    private static readonly JsonSerializerOptions _incNullAndAllStringOptions;

    private readonly IMongoContext _mongoContext;
    private readonly IMemoryCache _memoryCache;
    private readonly CollectionDescriptionService _descriptionService;

    static CollectionController()
    {
        _defaultOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy()
        };
        _defaultOptions.Converters.Add(new BsonDocumentJsonConverter(false));
        _defaultOptions.Converters.Add(new DataResponseJsonConverter());
        _defaultOptions.Converters.Add(new BsonDecimal128JsonConverter());

        _incNullOptions = new JsonSerializerOptions(_defaultOptions)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        _allStringOptions = new JsonSerializerOptions(_defaultOptions);
        _allStringOptions.Converters.Remove(_allStringOptions.Converters.First(x => x is BsonDocumentJsonConverter));
        _allStringOptions.Converters.Add(new BsonDocumentJsonConverter(true));
        _allStringOptions.NumberHandling = JsonNumberHandling.WriteAsString;
        _allStringOptions.Converters.Insert(0, new BooleanJsonConverter(true));

        _incNullAndAllStringOptions = new JsonSerializerOptions(_allStringOptions)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionController"/> class.
    /// </summary>
    /// <param name="mongoContext">The Mongo DB collections context.</param>
    /// <param name="memoryCache">The memory cache.</param>
    /// <param name="descriptionService">The description service.</param>
    public CollectionController
    (
        IMongoContext mongoContext,
        IMemoryCache memoryCache,
        CollectionDescriptionService descriptionService
    )
    {
        _mongoContext = mongoContext;
        _memoryCache = memoryCache;
        _descriptionService = descriptionService;
    }

    /// <summary>
    /// Gets the available collections.
    /// </summary>
    /// <param name="environment">The environment to retrieve the collections from.</param>
    /// <param name="censusJSON">Whether Census JSON mode is enabled.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The available collections.</returns>
    /// <response code="200">Returns a list of the collections in the given environment.</response>
    [HttpGet("get/{environment}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<JsonResult> GetDatatypesAsync
    (
        string environment,
        bool censusJSON = true,
        CancellationToken ct = default
    )
    {
        PS2Environment env = ParseEnvironment(environment);
        IReadOnlyList<Datatype> dataTypes = await GetAndCacheDatatypeListAsync(env, ct);

        return new JsonResult
        (
            new DataResponse<Datatype>(dataTypes, "datatype", null),
            GetJsonOptions(censusJSON, true)
        );
    }

    /// <summary>
    /// Counts the number of available collections.
    /// </summary>
    /// <param name="environment">The environment to retrieve the collections from.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>The collection count.</returns>
    /// <response code="200">Returns the number of collections in the given environment.</response>
    [HttpGet("count/{environment}")]
    [Produces(typeof(CollectionCount))]
    public async Task<JsonResult> CountDatatypesAsync
    (
        string environment,
        CancellationToken ct = default
    )
    {
        PS2Environment env = ParseEnvironment(environment);
        int count = (await GetAndCacheDatatypeListAsync(env, ct).ConfigureAwait(false)).Count;

        return new JsonResult
        (
            new CollectionCount((ulong)count),
            _incNullOptions
        );
    }

    /// <summary>
    /// Describes the fields of a collection.
    /// </summary>
    /// <param name="environment">The environment that the collection is from.</param>
    /// <param name="collectionName">The name of the collection.</param>
    /// <param name="queryParams">The query parameters.</param>
    /// <returns>Descriptions of the collection's fields.</returns>
    /// <response code="200">Returns a description of the collection's structure.</response>
    [HttpGet("/describe/{environment}/{collectionName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public JsonResult DescribeCollection
    (
        string environment,
        string collectionName,
        [FromQuery] CollectionQueryParameters queryParams
    )
    {
        JsonSerializerOptions jsonOptions = GetJsonOptions(queryParams.CensusJsonMode, queryParams.IncludeNullFields);
        IReadOnlyList<CollectionFieldInformation> fieldInfos = _descriptionService.GetFieldInformation(collectionName);

        return new JsonResult
        (
            new DataResponse<CollectionFieldInformation>(fieldInfos, fieldInfos.Count, collectionName, null),
            jsonOptions
        );
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
        JsonSerializerOptions jsonOptions = GetJsonOptions(queryParams.CensusJsonMode, queryParams.IncludeNullFields);

        try
        {
            IMongoCollection<BsonDocument> collection = ValidateAndGetCollection
            (
                collectionName,
                environment,
                queryParams,
                out PS2Environment parsedEnvironment
            );

            ApiTelemetry.QueryCounter.Add
            (
                1,
                new KeyValuePair<string, object?>("environment", parsedEnvironment.ToString().ToLower()),
                new KeyValuePair<string, object?>("collection", collectionName)
            );

            List<object> results;
            Stopwatch st = new();

            if (queryParams.Distinct is not null)
            {
                st.Start();
                results = await collection.Distinct<object>
                (
                    queryParams.Distinct,
                    BuildFilter(collectionName, queryParams),
                    new DistinctOptions { MaxTime = TimeSpan.FromSeconds(1) },
                    ct
                ).ToListAsync(ct).ConfigureAwait(false);
                st.Stop();
            }
            else
            {
                LangProjectionBuilder? langProjections = queryParams.Lang is null
                    ? null
                    : new LangProjectionBuilder(queryParams.Lang.Split(','));

                IAggregateFluent<BsonDocument> query = ConstructBasicQuery
                    (
                        collection,
                        collectionName,
                        queryParams,
                        langProjections
                    );

                RenderArgs<BsonDocument> renderArgs = new
                (
                    collection.DocumentSerializer,
                    collection.Settings.SerializerRegistry
                );

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
                                renderArgs,
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

                query = query.Skip(queryParams.Start)
                    .Limit(queryParams.LimitPerDb ?? queryParams.Limit);

                if (queryParams.Tree is not null)
                {
                    GroupBuilder groupBuilder = GroupBuilder.ParseFromTreeCommand(queryParams.Tree);
                    groupBuilder.BuildAndAppendTo(ref query);
                }

                st.Start();
                results = (await query.ToListAsync(ct).ConfigureAwait(false)).Cast<object>().ToList();
                st.Stop();
            }

            Datatype? datatypeInfo = queryParams.ShowTimings
                ? (await GetAndCacheDatatypeListAsync(parsedEnvironment, ct).ConfigureAwait(false))
                    .FirstOrDefault(d => d.Name == collectionName)
                : null;

            return new JsonResult
            (
                new DataResponse<object>
                (
                    results,
                    collectionName,
                    queryParams.ShowTimings
                        ? new QueryTimes
                        (
                            st.Elapsed.Milliseconds,
                            datatypeInfo?.LastUpdated,
                            datatypeInfo?.UpdateIntervalSec ?? -1
                        )
                        : null
                ),
                jsonOptions
            );
        }
        catch (QueryException quex)
        {
            return new JsonResult
            (
                new ErrorResponse(quex.ErrorCode, quex.Message),
                jsonOptions
            ) { StatusCode = StatusCodes.Status400BadRequest };
        }
        catch (MongoCommandException mcex) when (mcex.Message.StartsWith("Command aggregate failed: Invalid $project"))
        {
            return new JsonResult
            (
                new ErrorResponse(QueryErrorCode.InvalidProjection, "Ensure you don't have conflicting c:show and c:hide commands"),
                jsonOptions
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
    /// <response code="200">Returns the number of elements in the query result.</response>
    [HttpGet("/count/{environment}/{collectionName}")]
    [Produces(typeof(CollectionCount))]
    public async Task<JsonResult> CountCollectionAsync
    (
        string environment,
        string collectionName,
        [FromQuery] CollectionQueryParameters queryParams,
        CancellationToken ct = default
    )
    {
        JsonSerializerOptions jsonOptions = _incNullOptions;

        try
        {
            IMongoCollection<BsonDocument> collection = ValidateAndGetCollection
            (
                collectionName,
                environment,
                queryParams,
                out _
            );

            IAggregateFluent<AggregateCountResult> count = ConstructBasicQuery
            (
                collection,
                collectionName,
                queryParams,
                null
            ).Count();

            AggregateCountResult res = await count.FirstAsync(ct);

            return new JsonResult
            (
                new CollectionCount((ulong)res.Count),
                jsonOptions
            );
        }
        catch (QueryException quex)
        {
            return new JsonResult
            (
                new ErrorResponse(quex.ErrorCode, quex.Message),
                jsonOptions
            );
        }
        catch (MongoCommandException mcex) when (mcex.Message.StartsWith("Command aggregate failed: Invalid $project"))
        {
            return new JsonResult
            (
                new ErrorResponse(QueryErrorCode.InvalidProjection, "Ensure you don't have conflicting c:show and c:hide commands"),
                jsonOptions
            ) { StatusCode = StatusCodes.Status400BadRequest };
        }
    }

    private static JsonSerializerOptions GetJsonOptions(bool censusJSON, bool includeNull)
    {
        JsonSerializerOptions options = _defaultOptions;
        if (includeNull && !censusJSON)
            options = _incNullOptions;
        else if (includeNull && censusJSON)
            options = _incNullAndAllStringOptions;
        else if (censusJSON)
            options = _allStringOptions;

        return options;
    }

    private IMongoCollection<BsonDocument> ValidateAndGetCollection
    (
        string collectionName,
        string environment,
        CollectionQueryParameters queryParams,
        out PS2Environment parsedEnvironment
    )
    {
        if (!CollectionUtils.CheckCollectionExists(collectionName))
            throw new QueryException(QueryErrorCode.UnknownCollection, $"The {collectionName} collection does not exist");

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

        parsedEnvironment = ParseEnvironment(environment);
        IMongoDatabase db = _mongoContext.GetDatabase(parsedEnvironment);
        return db.GetCollection<BsonDocument>(collectionName);
    }

    private IAggregateFluent<BsonDocument> ConstructBasicQuery
    (
        IMongoCollection<BsonDocument> mongoCollection,
        string collectionName,
        CollectionQueryParameters queryParams,
        LangProjectionBuilder? langProjections
    )
    {
        FilterDefinition<BsonDocument> filter = BuildFilter(collectionName, queryParams);

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

                if (components is [_, "-1"])
                    sortDirection = -1;

                aggregate = aggregate.Sort(new BsonDocument(components[0], sortDirection));
            }
        }

        return aggregate;
    }

    private FilterDefinition<BsonDocument> BuildFilter(string collectionName, CollectionQueryParameters queryParams)
    {
        FilterDefinitionBuilder<BsonDocument> filterBuilder = Builders<BsonDocument>.Filter;
        FilterDefinition<BsonDocument> filter = filterBuilder.Empty;

        if (queryParams.HasFields is not null)
        {
            foreach (string value in queryParams.HasFields.SelectMany(s => s.Split(',')))
            {
                if (value.StartsWith('!'))
                    filter &= filterBuilder.Eq(value[1..], BsonNull.Value);
                else
                    filter &= filterBuilder.Ne(value, BsonNull.Value);
            }
        }

        foreach ((string paramName, StringValues paramValues) in HttpContext.Request.Query)
        {
            if (paramName.AsSpan().StartsWith(QueryCommandIdentifier))
                continue;

            foreach (string? value in paramValues)
            {
                if (value is null)
                    continue;

                FilterBuilder fb = FilterBuilder.Parse(collectionName, paramName, value);
                fb.Build(ref filter, !queryParams.IsCaseSensitive);
            }
        }

        return filter;
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

        dataTypes = dataTypes.Where(x => !CollectionUtils.IsCollectionHidden(x.Name))
            .ToList();

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
