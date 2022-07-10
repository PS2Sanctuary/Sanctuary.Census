namespace Sanctuary.Census.Common.Objects;

/// <summary>
/// Represents the result of a contribution to an item.
/// </summary>
/// <typeparam name="TContributedTo">The type that was contributed to.</typeparam>
public readonly record struct ContributionResult<TContributedTo>
    where TContributedTo : class
{
    /// <summary>
    /// Indicates whether the <see cref="Item"/> was contributed to.
    /// </summary>
    public bool WasContributedTo { get; }

    /// <summary>
    /// The item.
    /// </summary>
    public TContributedTo Item { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContributionResult{TContributedTo}"/> struct.
    /// </summary>
    /// <param name="wasContributedTo">Indicates whether the item was contributed to.</param>
    /// <param name="item">The item.</param>
    public ContributionResult(bool wasContributedTo, TContributedTo item)
    {
        WasContributedTo = wasContributedTo;
        Item = item;
    }
}
