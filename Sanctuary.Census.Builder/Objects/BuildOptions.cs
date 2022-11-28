using System;

namespace Sanctuary.Census.Builder.Objects;

/// <summary>
/// Represents options relevant to the collection build process.
/// </summary>
public class BuildOptions
{
    /// <summary>
    /// Gets or sets the interval in seconds between each collection build.
    /// </summary>
    public int BuildIntervalSeconds { get; set; }

    /// <summary>
    /// Gets the path to the git repo used to store collection diffs.
    /// </summary>
    public string? GitDiffRepoPath { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BuildOptions"/> class.
    /// </summary>
    public BuildOptions()
    {
        BuildIntervalSeconds = TimeSpan.FromHours(3).Seconds;
    }
}
