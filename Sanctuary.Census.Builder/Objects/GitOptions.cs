namespace Sanctuary.Census.Builder.Objects;

/// <summary>
/// Contains options for the git service.
/// </summary>
public sealed class GitOptions
{
    /// <summary>
    /// The key under which these options are stored in configuration data.
    /// </summary>
    public const string CONFIG_KEY = "GitOptions";

    /// <summary>
    /// Whether Git diffing of the collections is enabled.
    /// </summary>
    public bool DoGitDiffing { get; set; }

    /// <summary>
    /// The local path at which to store the cloned repository. If left null it will be cloned into AppData.
    /// </summary>
    public string? LocalRepositoryPath { get; set; }

    /// <summary>
    /// The name of the branch to clone.
    /// </summary>
    public string BranchName { get; set; } = "main";

    /// <summary>
    /// The URL of the remote. May be left null to disable syncing with a remote.
    /// </summary>
    public string? RemoteHttpUrl { get; set; }

    /// <summary>
    /// The username to connect to the remote using. If null, credentials will not be used.
    /// </summary>
    public string? RemoteUsername { get; set; }

    /// <summary>
    /// The password of the remote user. May be left null if the remote does not require a password.
    /// </summary>
    public string? RemotePassword { get; set; }

    /// <summary>
    /// The username to associate with commits.
    /// </summary>
    public string CommitUserName { get; set; } = "Sanctuary Census Differ";

    /// <summary>
    /// The email to associate with commits.
    /// </summary>
    public required string CommitEmail { get; set; }
}
