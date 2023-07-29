using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Attributes;
using Sanctuary.Census.Common.Objects.CommonModels;

namespace Sanctuary.Census.Common.Objects.Collections;

/// <summary>
/// Represents a PlanetSide 2 server.
/// </summary>
/// <param name="WorldID">The ID of the server.</param>
/// <param name="Name">The name of the server.</param>
/// <param name="IsLocked">Indicates whether the server is locked.</param>
/// <param name="LockState">The lock state of the server.</param>
/// <param name="IsUnprivilegedAccessAllowed">Indicates whether standard accounts are allowed access to the server.</param>
[Collection]
public record World
(
    [property: JoinKey] uint WorldID,
    LocaleString Name,
    bool IsLocked,
    string LockState,
    bool IsUnprivilegedAccessAllowed
) : ISanctuaryCollection;
