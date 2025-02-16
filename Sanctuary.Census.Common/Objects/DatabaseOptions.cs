namespace Sanctuary.Census.Common.Objects;

/// <summary>
/// Represents options that are applicable to the entire Sanctuary.Census project.
/// </summary>
public sealed class DatabaseOptions
{
    /// <summary>
    /// The standard config key string to retrieve these options using.
    /// </summary>
    public const string CONFIG_KEY = "DatabaseOptions";

    /// <summary>
    /// The connection string with which to connect to the database.
    /// </summary>
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
}
