using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sanctuary.Census.RealtimeHub.Services;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.RealtimeHub.Controllers;

/// <summary>
/// Establishes and maintains WebSocket connections for the event stream.
/// </summary>
public class EventStreamController : ControllerBase
{
    private readonly ILogger<EventStreamController> _logger;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly EventStreamSocketManager _manager;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventStreamController"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="lifetime">The host lifetime.</param>
    /// <param name="manager">The event stream manager.</param>
    public EventStreamController
    (
        ILogger<EventStreamController> logger,
        IHostApplicationLifetime lifetime,
        EventStreamSocketManager manager
    )
    {
        _logger = logger;
        _lifetime = lifetime;
        _manager = manager;
    }

    /// <summary>
    /// Opens an event stream WebSocket connection.
    /// </summary>
    [Route("/streaming")]
    [AllowAnonymous]
    public async Task OpenEventStreamConnection()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        using CancellationTokenSource cancelCts
            = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.ApplicationStopping);

        await _manager.RegisterWebSocket(webSocket, cancelCts);

        try
        {
            while (!cancelCts.IsCancellationRequested && webSocket.State is WebSocketState.Open)
            {
                byte[] buffer = new byte[EventStreamSocketManager.MAX_WEBSOCKET_MESSAGE_LENGTH];
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffer, cancelCts.Token);

                if (result.MessageType is WebSocketMessageType.Close)
                {
                    _logger.LogDebug("Websocket closed with type {Type}", result.CloseStatus);
                    break;
                }

                if (result.EndOfMessage)
                {
                    await _manager.ProcessMessage(webSocket, buffer.AsMemory(0, result.Count), cancelCts.Token);
                    continue;
                }

                await webSocket.CloseAsync
                (
                    WebSocketCloseStatus.MessageTooBig,
                    $"Maximum message length is {EventStreamSocketManager.MAX_WEBSOCKET_MESSAGE_LENGTH} bytes",
                    cancelCts.Token
                );

                break;
            }

            if (webSocket.CloseStatus is not null)
            {
                await webSocket.CloseAsync
                (
                    WebSocketCloseStatus.NormalClosure,
                    "Server stopping",
                    CancellationToken.None
                );
            }
        }
        catch (OperationCanceledException)
        {
            // This is fine
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to maintain a WebSocket connection");
        }
        finally
        {
            _manager.DeregisterWebSocket(webSocket);
            cancelCts.Cancel();
        }
    }
}
