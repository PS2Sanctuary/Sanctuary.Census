using Microsoft.Extensions.Hosting;
using Sanctuary.Census.RealtimeHub.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.RealtimeHub.Workers;

/// <summary>
/// A hosted service for managing long-running websocket operations.
/// </summary>
public class WebSocketWorker : BackgroundService
{
    private readonly WebSocketManager _webSocketManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebSocketWorker"/>.
    /// </summary>
    /// <param name="webSocketManager"></param>
    public WebSocketWorker(WebSocketManager webSocketManager)
    {
        _webSocketManager = webSocketManager;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken ct)
        => await _webSocketManager.RunAsync(ct);
}
