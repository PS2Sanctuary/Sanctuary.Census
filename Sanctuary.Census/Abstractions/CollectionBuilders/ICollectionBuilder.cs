using Sanctuary.Census.Abstractions.Database;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Abstractions.CollectionBuilders;

/// <summary>
/// Represents a collection builder.
/// </summary>
public interface ICollectionBuilder
{
    /// <summary>
    /// Adds data to the given <paramref name="dbContext"/> using the provided caches.
    /// </summary>
    /// <param name="dbContext">The collections context.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task BuildAsync(ICollectionsContext dbContext, CancellationToken ct = default);
}
