using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Sanctuary.Census.RealtimeHub.Services;

public class WebSocketManager
{
    private readonly ILogger<WebSocketManager> _logger;
    private readonly List<(WebSocket Socket, TaskCompletionSource<object> Tcs)> _sockets;
    private readonly Channel<object> _outboundChannel;

    public WebSocketManager(ILogger<WebSocketManager> logger)
    {
        _logger = logger;
        _sockets = new List<(WebSocket Socket, TaskCompletionSource<object> Tcs)>();
        _outboundChannel = Channel.CreateBounded<object>(10000);
    }

    public async Task RunAsync(CancellationToken ct = default)
    {
        try
        {
            await foreach (object evt in _outboundChannel.Reader.ReadAllAsync(ct))
            {
                // TODO: Send
            }
        }
        catch (OperationCanceledException)
        {
            // This is fine
        }

        await CloseSocketsAsync(ct);
    }

    /// <summary>
    /// Registers a websocket with the manager.
    /// </summary>
    /// <param name="socket">The socket.</param>
    /// <param name="tcs">A task completion source that will be set when the manager is closing.</param>
    public void AddWebSocket(WebSocket socket, TaskCompletionSource<object> tcs)
        => _sockets.Add((socket, tcs));

    /// <summary>
    /// Submits an event to be sent over the event stream.
    /// </summary>
    /// <typeparam name="T">The type of the event.</typeparam>
    /// <param name="evt">The event.</param>
    public void SubmitEvent<T>(T evt)
        where T : notnull
    {
        bool written = _outboundChannel.Writer.TryWrite(evt);
        if (!written)
            _logger.LogWarning("An outbound ESS object was dropped! Running behind?");
    }

    /// <summary>
    /// Closes and forgets all sockets held by the manager.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel this operation.</param>
    private async Task CloseSocketsAsync(CancellationToken ct = default)
    {
        foreach ((WebSocket socket, TaskCompletionSource<object> tcs) in _sockets)
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server stopping", ct);
            tcs.TrySetResult(new object());
        }

        _sockets.Clear();
    }
}
