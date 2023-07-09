namespace Sanctuary.Census.RealtimeHub.Objects;

/// <summary>
/// Represents an 'all' value that can be used in a Census Event Stream subscription.
/// </summary>
public readonly struct All
{
    /// <inheritdoc />
    public override string ToString()
        => "all";
}
