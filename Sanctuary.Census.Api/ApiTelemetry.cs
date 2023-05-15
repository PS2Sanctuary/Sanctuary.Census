using System.Diagnostics.Metrics;
using System.Reflection;

namespace Sanctuary.Census.Api;

/// <summary>
/// Gets the API telemetry context.
/// </summary>
public sealed class ApiTelemetry
{
    private static readonly AssemblyName AssemblyName = typeof(ApiTelemetry).Assembly.GetName();

    /// <summary>
    /// Gets the telemetry service name.
    /// </summary>
    public static readonly string SERVICE_NAME = AssemblyName.Name ?? AssemblyName.FullName;

    /// <summary>
    /// Gets the telemetry service version.
    /// </summary>
    public static readonly string? SERVICE_VERSION = AssemblyName.Version?.ToString();

    /// <summary>
    /// Gets the main metrics source.
    /// </summary>
    public static Meter Meter { get; } = new(SERVICE_NAME, SERVICE_VERSION);

    /// <summary>
    /// Gets the primary request counter.
    /// </summary>
    public static Counter<long> QueryCounter { get; } = Meter.CreateCounter<long>("api.query_counter");
}
