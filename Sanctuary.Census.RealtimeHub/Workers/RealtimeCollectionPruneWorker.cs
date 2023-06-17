using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Abstractions.Services;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.RealtimeHub.Workers;

/// <summary>
/// A background worker that prunes stale entries from realtime collections.
/// </summary>
public class RealtimeCollectionPruneWorker : BackgroundService
{
    /// <summary>
    /// Gets the interval after which a realtime collection entry is considered to be stale.
    /// </summary>
    public static readonly TimeSpan REALTIME_STALE_AFTER = TimeSpan.FromMinutes(10);

    private readonly ILogger<RealtimeCollectionPruneWorker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="RealtimeCollectionPruneWorker"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="serviceScopeFactory">The service scope factory.</param>
    public RealtimeCollectionPruneWorker
    (
        ILogger<RealtimeCollectionPruneWorker> logger,
        IServiceScopeFactory serviceScopeFactory
    )
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        PeriodicTimer pruneTimer = new(TimeSpan.FromMinutes(2));

        Type[] realtimeCollections = typeof(IRealtimeCollection).Assembly
            .GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IRealtimeCollection)))
            .ToArray();

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
                IMongoContext dbContext = scope.ServiceProvider.GetRequiredService<IMongoContext>();

                foreach (Type collType in realtimeCollections)
                {
                    MethodInfo? addRemovalTestMethodInfo = typeof(Program).GetMethod
                    (
                        nameof(DeleteStaleEntriesAsync),
                        BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic
                    );

                    if (addRemovalTestMethodInfo is null)
                        continue;

                    addRemovalTestMethodInfo = addRemovalTestMethodInfo.MakeGenericMethod(collType);
                    Task deleteTask = (Task)addRemovalTestMethodInfo.Invoke(null, new object[] { dbContext, ct })!;
                    await deleteTask;
                }
            }
            catch (OperationCanceledException)
            {
                // This is fine
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run an iteration of the realtime prune worker");
            }

            await pruneTimer.WaitForNextTickAsync(ct);
        }
    }

    private static async Task DeleteStaleEntriesAsync<TCollection>(IMongoContext context, CancellationToken ct)
        where TCollection : IRealtimeCollection
    {
        IMongoCollection<TCollection> collection = context.GetCollection<TCollection>();

        await collection.DeleteManyAsync
        (
            c => DateTimeOffset.FromUnixTimeSeconds(c.Timestamp).Add(REALTIME_STALE_AFTER) < DateTimeOffset.UtcNow,
            cancellationToken: ct
        );
    }
}
