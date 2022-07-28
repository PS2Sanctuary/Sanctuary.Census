using Sanctuary.Census.Common.Objects;

namespace Sanctuary.Census.Common.Services;

/// <summary>
/// Provides the current environment for a DI scope.
/// </summary>
public class EnvironmentContextProvider
{
    /// <summary>
    /// The current environment.
    /// </summary>
    public PS2Environment Environment { get; set; } = PS2Environment.PS2;
}
