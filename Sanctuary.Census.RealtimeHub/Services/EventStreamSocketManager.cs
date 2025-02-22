using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Sanctuary.Census.Common.Abstractions.Objects.RealtimeEvents;
using Sanctuary.Census.Common.Json;
using Sanctuary.Census.RealtimeHub.Json;
using Sanctuary.Census.RealtimeHub.Objects;
using Sanctuary.Census.RealtimeHub.Objects.Commands;
using Sanctuary.Census.RealtimeHub.Objects.Control;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// <summary>
    /// The maximum length that a single websocket message may be.
    /// </summary>
    public const int MAX_WEBSOCKET_MESSAGE_LENGTH = 4096;

    private static readonly JsonSerializerOptions INBOUND_JSON_OPTIONS = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new SubscribeWorldListJsonConverter() }
    };

    // Used for top-level messages
    private static readonly JsonSerializerOptions OUTBOUND_JSON_OPTIONS = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        NumberHandling = JsonNumberHandling.WriteAsString,
        Converters = { new BooleanJsonConverter(true, true), new SubscribeWorldListJsonConverter() }
    };

    // Used for top-level messages
    private static readonly JsonSerializerOptions NON_CENSUS_OUTBOUND_JSON_OPTIONS = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new SubscribeWorldListJsonConverter() }
    };

    // Used only to serialize events, which are wrapped by an outbound service message
    private static readonly JsonSerializerOptions EVENT_JSON_OPTIONS = new()
    {
        PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy(),
        NumberHandling = JsonNumberHandling.WriteAsString,
        Converters = { new BooleanJsonConverter(true, true) }
    };

    // Used only to serialize events, which are wrapped by an outbound service message
    private static readonly JsonSerializerOptions NON_CENSUS_EVENT_JSON_OPTIONS = new()
    {
        PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy()
    };

    private static readonly RecyclableMemoryStreamManager _rmsManager = new();

    private readonly ILogger<EventStreamSocketManager> _logger;
    private readonly Dictionary<WebSocket, WebSocketBundle> _sockets = [];
    private readonly Channel<IRealtimeEvent> _outboundChannel = Channel.CreateBounded<IRealtimeEvent>(10000);

    /// <summary>
    /// Initializes a new instance of the <see cref="EventStreamSocketManager"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    public EventStreamSocketManager(ILogger<EventStreamSocketManager> logger)
    {
        _logger = logger;
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
            using Task heartbeatTask = HeartbeatTask(ct);
            using Task messageReadTask = MessageReadTask(ct);
            await Task.WhenAll(heartbeatTask, messageReadTask);

            await heartbeatTask;
            await messageReadTask;
        }
        catch (OperationCanceledException)
        {
            // This is fine
            _logger.LogInformation($"{nameof(EventStreamSocketManager)}#Run cancelled");
        }
        catch (AggregateException aex)
        {
            foreach (Exception ex in aex.InnerExceptions)
                _logger.LogError(ex, "Error occured while running the socket manager sub-tasks");
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
    /// <param name="useCensusJson">
    /// Whether fields on the outgoing payloads should be serialized in a Census-compatible manner,
    /// rather than respecting the JSON spec.
    /// </param>
    /// <param name="cancelCts">A <see cref="CancellationTokenSource"/> to set when the socket has been closed.</param>
    public async Task RegisterWebSocket(WebSocket socket, bool useCensusJson, CancellationTokenSource cancelCts)
    {
        WebSocketBundle bundle = new(socket, cancelCts, useCensusJson);
        _sockets.Add(socket, bundle);
        await WriteToSingleSocket(bundle, new ConnectionStateChanged(true), cancelCts.Token);
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

                await WriteToSingleSocket(bundle, bundle.Subscription.ToSubscriptionInformation(), ct);
                break;
            }
            case "clearSubscribe":
            {
                ClearSubscribe? clSub = document.Deserialize<ClearSubscribe>(INBOUND_JSON_OPTIONS);
                if (clSub is not null)
                    bundle.Subscription.SubtractSubscription(clSub);

                await WriteToSingleSocket(bundle, bundle.Subscription.ToSubscriptionInformation(), ct);
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

    private static async Task WriteToSingleSocket<TMessage>
    (
        WebSocketBundle bundle,
        TMessage message,
        CancellationToken ct
    )
    {
        Utf8JsonWriter writer = await bundle.StartWritingAsync(ct);
        try
        {
            JsonSerializerOptions jsonOpts = bundle.UsesCensusJson
                ? OUTBOUND_JSON_OPTIONS
                : NON_CENSUS_OUTBOUND_JSON_OPTIONS;

            JsonSerializer.Serialize(writer, message, jsonOpts);
        }
        finally
        {
            await bundle.EndWriting(ct);
        }
    }

    private async Task DispatchMessage<TMessage>
    (
        TMessage message,
        IEnumerable<WebSocketBundle> receivingBundles,
        JsonSerializerOptions jsonOpts,
        CancellationToken ct
    ) where TMessage : MessageBase
    {
        // ReSharper disable once UseAwaitUsing
        using RecyclableMemoryStream ms = _rmsManager.GetStream();
        // ReSharper disable once MethodHasAsyncOverloadWithCancellation
        JsonSerializer.Serialize(ms, message, jsonOpts);
        ms.Seek(0, SeekOrigin.Begin);

        foreach (WebSocketBundle bundle in receivingBundles)
        {
            if (bundle.Socket.CloseStatus is not null)
                continue;

            await bundle.WriteSemaphore.WaitAsync(ct);
            try
            {
                ReadOnlyMemory<byte> toWrite = ms.GetBuffer().AsMemory(0, (int)ms.Position);
                while (toWrite.Length > 0)
                {
                    int take = Math.Min(toWrite.Length, MAX_WEBSOCKET_MESSAGE_LENGTH);

                    await bundle.Socket.SendAsync
                    (
                        toWrite[..take],
                        WebSocketMessageType.Text,
                        toWrite.Length <= MAX_WEBSOCKET_MESSAGE_LENGTH,
                        ct
                    );

                    toWrite = toWrite[take..];
                }
            }
            finally
            {
                bundle.WriteSemaphore.Release();
            }
        }
    }

    private async Task DispatchMessageToAll<TMessage>(TMessage message, CancellationToken ct)
        where TMessage : MessageBase
    {
        await DispatchMessage
        (
            message,
            _sockets.Values.Where(x => x.UsesCensusJson),
            OUTBOUND_JSON_OPTIONS,
            ct
        );
        await DispatchMessage
        (
            message,
            _sockets.Values.Where(x => !x.UsesCensusJson),
            NON_CENSUS_OUTBOUND_JSON_OPTIONS,
            ct
        );
    }

    private async Task MessageReadTask(CancellationToken ct)
    {
        await Task.Yield();

        await foreach (IRealtimeEvent evt in _outboundChannel.Reader.ReadAllAsync(ct))
        {
            JsonNode? censusJsonNode = JsonSerializer.SerializeToNode(evt, evt.GetType(), EVENT_JSON_OPTIONS);
            if (censusJsonNode is null)
                continue;
            // At this point this should not fail
            JsonNode nonCensusJsonNode = JsonSerializer.SerializeToNode(evt, evt.GetType(), NON_CENSUS_EVENT_JSON_OPTIONS)!;

            censusJsonNode["event_name"] = evt.EventName;
            nonCensusJsonNode["event_name"] = evt.EventName;
            ServiceMessage<object> censusMessage = new(censusJsonNode);
            ServiceMessage<object> nonCensusMessage = new(nonCensusJsonNode);

            WebSocketBundle[] validBundles = _sockets.Values.Where
                (
                    x => x.Subscription.IsSubscribedToEvent(evt.EventName) &&
                        x.Subscription.IsSubscribedToWorld(evt.WorldId)
                )
                .ToArray();

            await DispatchMessage
            (
                censusMessage,
                validBundles.Where(x => x.UsesCensusJson),
                OUTBOUND_JSON_OPTIONS,
                ct
            );
            await DispatchMessage
            (
                nonCensusMessage,
                validBundles.Where(x => !x.UsesCensusJson),
                NON_CENSUS_OUTBOUND_JSON_OPTIONS,
                ct
            );
        }
    }

    private async Task HeartbeatTask(CancellationToken ct)
    {
        await Task.Yield();
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(30));

        try
        {
            while (!ct.IsCancellationRequested)
            {
                await timer.WaitForNextTickAsync(ct);
                Heartbeat heartbeat = new(new Dictionary<string, bool>(), DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                try
                {
                    await DispatchMessageToAll(heartbeat, ct);
                }
                catch (Exception ex)
                {
                    // This is kinda fine. We'll just try again in the next loop
                    _logger.LogError(ex, "Failed to dispatch a heartbeat");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // This is fine
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The websocket heartbeat task has failed");
        }
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
        _outboundChannel.Writer.TryComplete();

        foreach (WebSocketBundle bundle in _sockets.Values)
            bundle.Dispose();
    }
}
