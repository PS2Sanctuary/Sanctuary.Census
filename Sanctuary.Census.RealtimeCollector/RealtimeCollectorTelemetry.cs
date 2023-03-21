using System.Diagnostics;
using System.Reflection;

namespace Sanctuary.Census.RealtimeCollector;

/// <summary>
/// Gets the server data telemetry context.
/// </summary>
public sealed class RealtimeCollectorTelemetry
{
    private static readonly AssemblyName AssemblyName = typeof(RealtimeCollectorTelemetry).Assembly.GetName();

    /// <summary>
    /// Gets the telemetry service name.
    /// </summary>
    public static readonly string SERVICE_NAME = AssemblyName.Name ?? AssemblyName.FullName;

    /// <summary>
    /// Gets the telemetry service version.
    /// </summary>
    public static readonly string? SERVICE_VERSION = AssemblyName.Version?.ToString();

    /// <summary>
    /// Gets the main telemetry source.
    /// </summary>
    public static ActivitySource Source { get; } = new(SERVICE_NAME, SERVICE_VERSION);
}
