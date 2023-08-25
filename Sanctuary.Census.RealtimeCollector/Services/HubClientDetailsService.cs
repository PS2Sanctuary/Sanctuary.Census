using Sanctuary.Census.RealtimeHub;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.RealtimeCollector.Services;

/// <summary>
/// Represents a <see cref="IClientDetailsService"/> that retrieves data from the realtime hub.
/// </summary>
public class HubClientDetailsService : IClientDetailsService
{
    private readonly RealtimeIngress.RealtimeIngressClient _hubClient;

    // Yay for concurrency issues! (in seriousness this should be fine)
    private static ZoneConnectionResponse? _zoneConnectionDetails;
    private static long _lastRequestTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="HubClientDetailsService"/> class.
    /// </summary>
    /// <param name="hubClient">The hub client to use.</param>
    public HubClientDetailsService(RealtimeIngress.RealtimeIngressClient hubClient)
    {
        _hubClient = hubClient;
    }

    /// <inheritdoc />
    public async ValueTask<string> GetVersionAsync(CancellationToken ct = default)
    {
        ZoneConnectionResponse resp = await GetZoneConnectionDetails(ct);
        return resp.ClientVersion;
    }

    /// <inheritdoc />
    public async ValueTask<string> GetClientProtocolVersionAsync(CancellationToken ct = default)
    {
        ZoneConnectionResponse resp = await GetZoneConnectionDetails(ct);
        return resp.ClientProtocolString;
    }

    [MemberNotNull(nameof(_zoneConnectionDetails))]
    private async ValueTask<ZoneConnectionResponse> GetZoneConnectionDetails(CancellationToken ct)
    {
        if (Stopwatch.GetElapsedTime(_lastRequestTime) > TimeSpan.FromMinutes(1) || _zoneConnectionDetails is null)
        {
            _zoneConnectionDetails = await _hubClient.RequestZoneConnectionAsync
            (
                new ZoneConnectionRequest(),
                cancellationToken: ct
            );
            _lastRequestTime = Stopwatch.GetTimestamp();
        }

        return _zoneConnectionDetails;
    }
}
