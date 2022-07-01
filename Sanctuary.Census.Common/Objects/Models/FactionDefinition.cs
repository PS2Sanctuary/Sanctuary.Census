namespace Sanctuary.Census.Common.Objects.Models;

/// <summary>
/// Enumerates the various faction IDs.
/// </summary>
public enum FactionDefinition : byte
{
    /// <summary>
    /// The given entity is compatible with all factions.
    /// </summary>
    All = 0,

    /// <summary>
    /// The given entity is compatible with the Vanu Sovereignty.
    /// </summary>
    VS = 1,

    /// <summary>
    /// The given entity is compatible with the New Conglomerate.
    /// </summary>
    NC = 2,

    /// <summary>
    /// The given entity is compatible with the Terran Republic.
    /// </summary>
    TR = 3,

    /// <summary>
    /// The given entity is compatible with the Nanite Systems Operatives.
    /// </summary>
    NSO = 4
}
