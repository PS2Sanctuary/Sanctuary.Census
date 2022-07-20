using System;
using System.IO;

namespace Sanctuary.Census.Common.Objects;

/// <summary>
/// Represents options that are applicable to the entire Sanctuary.Census project.
/// </summary>
public class CommonOptions
{
    /// <summary>
    /// Gets or sets the app data directory to use.
    /// </summary>
    public string AppDataDirectory { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommonOptions"/> class.
    /// </summary>
    public CommonOptions()
    {
        AppDataDirectory = Path.Combine
        (
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Sanctuary.Census"
        );
    }
}
