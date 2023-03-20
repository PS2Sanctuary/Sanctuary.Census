namespace Sanctuary.Census.RealtimeCollector;

/// <summary>
/// Contains configuration options for the realtime collection.
/// </summary>
public class CollectorConfig
{
    /// <summary>
    /// The name of the config section that this data should be stored under.
    /// </summary>
    public const string ConfigName = "CollectorConfig";

    /// <summary>
    /// Gets or sets the address of the realtime hub service.
    /// </summary>
    public string? HubEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the bearer token used to authenticate with the realtime hub service.
    /// </summary>
    public string? HubToken { get; set; }
}
