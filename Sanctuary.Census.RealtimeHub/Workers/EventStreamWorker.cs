using Microsoft.Extensions.Hosting;
using Sanctuary.Census.RealtimeHub.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.RealtimeHub.Workers;

/// <summary>
/// Runs the <see cref="EventStreamSocketManager"/> for the lifetime of the application.
/// </summary>
public class EventStreamWorker : BackgroundService
{
    private readonly EventStreamSocketManager _eventStreamSocketManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventStreamWorker"/>.
    /// </summary>
    /// <param name="eventStreamSocketManager"></param>
    public EventStreamWorker(EventStreamSocketManager eventStreamSocketManager)
    {
        _eventStreamSocketManager = eventStreamSocketManager;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken ct)
        => await _eventStreamSocketManager.RunAsync(ct);
}
