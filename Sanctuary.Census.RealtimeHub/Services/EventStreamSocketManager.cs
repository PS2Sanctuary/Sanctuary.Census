using Microsoft.Extensions.Logging;
using Sanctuary.Census.Common.Abstractions.Objects.RealtimeEvents;
using Sanctuary.Census.Common.Json;
using Sanctuary.Census.RealtimeHub.Json;
using Sanctuary.Census.RealtimeHub.Objects;
using Sanctuary.Census.RealtimeHub.Objects.Commands;
using Sanctuary.Census.RealtimeHub.Objects.Control;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Sanctuary.Census.RealtimeHub.Services;

/// <summary>
/// Manages the WebSocket connections required to serve the event stream.
/// </summary>
public sealed class EventStreamSocketManager : IDisposable
{
    private static readonly JsonSerializerOptions INBOUND_JSON_OPTIONS = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new SubscribeWorldListJsonConverter() }
    };

    private static readonly JsonSerializerOptions OUTBOUND_JSON_OPTIONS = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new SubscribeWorldListJsonConverter() }
    };

    private static readonly JsonSerializerOptions EVENT_JSON_OPTIONS = new()
    {
        PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy(),
        NumberHandling = JsonNumberHandling.WriteAsString,
        Converters = { new BooleanJsonConverter(true) }
    };

    /// <summary>
    /// The maximum length that a single websocket message may be.
    /// </summary>
    public const int MAX_WEBSOCKET_MESSAGE_LENGTH = 4096;

    private readonly ILogger<EventStreamSocketManager> _logger;
    private readonly Dictionary<WebSocket, WebSocketBundle> _sockets;
    private readonly Channel<IRealtimeEvent> _outboundChannel;
    private readonly ArrayBufferWriter<byte> _bufferWriter;
    private readonly Utf8JsonWriter _jsonWriter;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventStreamSocketManager"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    public EventStreamSocketManager(ILogger<EventStreamSocketManager> logger)
    {
        _logger = logger;
        _sockets = new Dictionary<WebSocket, WebSocketBundle>();
        _outboundChannel = Channel.CreateBounded<IRealtimeEvent>(10000);
        _bufferWriter = new ArrayBufferWriter<byte>(MAX_WEBSOCKET_MESSAGE_LENGTH);
        _jsonWriter = new Utf8JsonWriter(_bufferWriter);
    }

    /// <summary>
    /// Runs the manager. This will cause <see cref="SubmitEvent{T}"/> to become
    /// effective, and will mark all sockets as cancelled upon exit.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel this operation.</param>
    public async Task RunAsync(CancellationToken ct = default)
    {
        try
        {
            await foreach (IRealtimeEvent evt in _outboundChannel.Reader.ReadAllAsync(ct))
            {
                JsonNode? node = JsonSerializer.SerializeToNode(evt, evt.GetType(), EVENT_JSON_OPTIONS);
                if (node is null)
                    continue;

                node["event_name"] = evt.EventName;
                ServiceMessage<object> message = new(node);

                _jsonWriter.Reset();
                _bufferWriter.Clear();
                JsonSerializer.Serialize(_jsonWriter, message, OUTBOUND_JSON_OPTIONS);

                foreach ((WebSocket socket, WebSocketBundle bundle) in _sockets)
                {
                    if (socket.CloseStatus is not null)
                        continue;

                    if (!bundle.Subscription.IsSubscribedToEvent(evt.EventName))
                        continue;

                    if (!bundle.Subscription.IsSubscribedToWorld(evt.WorldId))
                        continue;

                    await bundle.WriteSemaphore.WaitAsync(ct);
                    ReadOnlyMemory<byte> toWrite = _bufferWriter.WrittenMemory;

                    while (toWrite.Length > 0)
                    {
                        int take = Math.Min(toWrite.Length, MAX_WEBSOCKET_MESSAGE_LENGTH);

                        await socket.SendAsync
                        (
                            toWrite[..take],
                            WebSocketMessageType.Text,
                            toWrite.Length <= MAX_WEBSOCKET_MESSAGE_LENGTH,
                            ct
                        );

                        toWrite = toWrite[take..];
                    }

                    bundle.WriteSemaphore.Release();
                }
            }
        }
        catch (OperationCanceledException)
        {
            // This is fine
            _logger.LogInformation($"{nameof(EventStreamSocketManager)}#Run cancelled");
        }
        finally
        {
            _outboundChannel.Writer.Complete();
            ReleaseSocketsAsync();
        }
    }

    /// <summary>
    /// Registers a websocket with the manager.
    /// </summary>
    /// <param name="socket">The socket.</param>
    /// <param name="cancelCts">A <see cref="CancellationTokenSource"/> to set when the socket has been closed.</param>
    public async Task RegisterWebSocket(WebSocket socket, CancellationTokenSource cancelCts)
    {
        WebSocketBundle bundle = new(cancelCts);
        _sockets.Add(socket, bundle);

        Utf8JsonWriter writer = await bundle.StartWritingAsync(cancelCts.Token);
        JsonSerializer.Serialize(writer, new ConnectionStateChanged(true), OUTBOUND_JSON_OPTIONS);
        await bundle.EndWriting(socket, cancelCts.Token);
    }

    /// <summary>
    /// De-registers a websocket from the manager.
    /// </summary>
    /// <param name="socket">The socket.</param>
    public void DeregisterWebSocket(WebSocket socket)
    {
        if (_sockets.Remove(socket, out WebSocketBundle? bundle))
            bundle.Dispose();
    }

    /// <summary>
    /// Processes a WebSocket message.
    /// </summary>
    /// <param name="socket">The socket that the message was received from.</param>
    /// <param name="messageData">The message data.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to cancel this operation.</param>
    public async ValueTask ProcessMessage
    (
        WebSocket socket,
        ReadOnlyMemory<byte> messageData,
        CancellationToken ct = default
    )
    {
        if (!_sockets.TryGetValue(socket, out WebSocketBundle? bundle))
            throw new InvalidOperationException("Unknown socket");

        JsonDocument? document = JsonSerializer.Deserialize<JsonDocument>(messageData.Span, INBOUND_JSON_OPTIONS);
        if (document is null)
        {
            await socket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Failed to parse message as JSON", ct);
            return;
        }

        bool hasActionField = document.RootElement.TryGetProperty("action"u8, out JsonElement actionElement);
        if (!hasActionField)
        {
            await socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Unrecognised action", ct);
            return;
        }

        string? action = actionElement.GetString();
        switch (action)
        {
            case "subscribe":
            {
                Subscribe? sub = document.Deserialize<Subscribe>(INBOUND_JSON_OPTIONS);
                if (sub is not null)
                    bundle.Subscription.AddSubscription(sub);

                Utf8JsonWriter writer = await bundle.StartWritingAsync(ct);
                JsonSerializer.Serialize(writer, bundle.Subscription.ToSubscriptionInformation(), OUTBOUND_JSON_OPTIONS);
                await bundle.EndWriting(socket, ct);

                break;
            }
            case "clearSubscribe":
            {
                ClearSubscribe? clSub = document.Deserialize<ClearSubscribe>(INBOUND_JSON_OPTIONS);
                if (clSub is not null)
                    bundle.Subscription.SubtractSubscription(clSub);

                Utf8JsonWriter writer = await bundle.StartWritingAsync(ct);
                JsonSerializer.Serialize(writer, bundle.Subscription.ToSubscriptionInformation(), OUTBOUND_JSON_OPTIONS);
                await bundle.EndWriting(socket, ct);

                break;
            }
            default: // Echo
            {
                await bundle.WriteSemaphore.WaitAsync(ct);
                await socket.SendAsync(messageData, WebSocketMessageType.Text, true, ct);
                bundle.WriteSemaphore.Release();
                break;
            }
        }
    }

    /// <summary>
    /// Submits an event to be sent over the event stream.
    /// </summary>
    /// <typeparam name="T">The type of the event.</typeparam>
    /// <param name="evt">The event.</param>
    public void SubmitEvent<T>(T evt)
        where T : IRealtimeEvent
    {
        bool written = _outboundChannel.Writer.TryWrite(evt);
        if (!written)
            _logger.LogWarning("An outbound ESS object was dropped! Running behind?");
    }

    /// <summary>
    /// Cancels and releases all sockets held by the manager.
    /// </summary>
    private void ReleaseSocketsAsync()
    {
        foreach (WebSocketBundle bundle in _sockets.Values)
            bundle.Dispose();

        _sockets.Clear();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _jsonWriter.Dispose();
        _bufferWriter.Clear();
        _outboundChannel.Writer.TryComplete();

        foreach (WebSocketBundle bundle in _sockets.Values)
            bundle.Dispose();
    }
}
