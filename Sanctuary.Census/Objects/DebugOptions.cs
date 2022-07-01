namespace Sanctuary.Census.Objects;

/// <summary>
/// Represents options that can control debug mode operations.
/// </summary>
public class DebugOptions
{
    /// <summary>
    /// The path to the directory containing manifest files.
    /// </summary>
    public string LocalManifestFilePath { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugOptions"/> class.
    /// </summary>
    public DebugOptions()
    {
        LocalManifestFilePath = string.Empty;
    }
}
