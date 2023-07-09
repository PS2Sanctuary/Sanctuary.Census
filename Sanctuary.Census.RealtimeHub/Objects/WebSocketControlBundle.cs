using System;
using System.Buffers;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.RealtimeHub.Objects;

internal sealed class WebSocketBundle : IDisposable
{
    private readonly CancellationTokenSource _socketConnectionCancelCts;
    private readonly ArrayBufferWriter<byte> _bufferWriter;
    private readonly Utf8JsonWriter _jsonWriter;

    public SemaphoreSlim WriteSemaphore { get; }
    public EventSubscription Subscription { get; set; }

    public WebSocketBundle(CancellationTokenSource socketConnectionCancelCts)
    {
        _socketConnectionCancelCts = socketConnectionCancelCts;
        _bufferWriter = new ArrayBufferWriter<byte>(4096);
        _jsonWriter = new Utf8JsonWriter(_bufferWriter);
        WriteSemaphore = new SemaphoreSlim(1);
        Subscription = new EventSubscription();
    }

    public async Task<Utf8JsonWriter> StartWritingAsync(CancellationToken ct)
    {
        await WriteSemaphore.WaitAsync(ct);

        _jsonWriter.Reset();
        _bufferWriter.Clear();

        return _jsonWriter;
    }

    public async Task EndWriting(WebSocket socket, CancellationToken ct)
    {
        await socket.SendAsync(_bufferWriter.WrittenMemory, WebSocketMessageType.Text, true, ct);
        WriteSemaphore.Release();
    }

    public void Dispose()
    {
        if (!_socketConnectionCancelCts.IsCancellationRequested)
            _socketConnectionCancelCts.Cancel();

        _jsonWriter.Dispose();
        _bufferWriter.Clear();
        WriteSemaphore.Dispose();
    }
}
