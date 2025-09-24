namespace Sanctuary.Census.RealtimeHub.Objects;

/// <summary>
/// Contains zone connection options.
/// </summary>
public class ZoneConnectionOptions
{
    /// <summary>
    /// Gets the name of the configuration section this class should be defined under.
    /// </summary>
    public const string OPTIONS_NAME = "ZoneConnectionOptions";

    /// <summary>
    /// Gets the version of the PS2 client to connect with.
    /// </summary>
    public string ClientVersion { get; set; }

    /// <summary>
    /// Gets the client protocol string to connect with.
    /// </summary>
    public string ClientProtocolString { get; set; }

    /// <summary>
    /// Indicates whether BattlEye warnings should be ignored - there has been periods in the game's lifespan where
    /// BattlEye sends heartbeats from the server, but doesn't actually kick invalid clients.
    /// </summary>
    public bool ShouldIgnoreBattlEye { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ZoneConnectionOptions"/>.
    /// </summary>
    public ZoneConnectionOptions()
    {
        ClientVersion = string.Empty;
        ClientProtocolString = string.Empty;
    }
}
