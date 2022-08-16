using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Builder.Abstractions.Services;

/// <summary>
/// Represents an interface for generating collection differentials.
/// </summary>
public interface ICollectionDiffService
{
    /// <summary>
    /// Sets a document as having been added to the collection.
    /// </summary>
    /// <typeparam name="T">The type of the document.</typeparam>
    /// <param name="document">The added document.</param>
    void SetAdded<T>(T document);

    /// <summary>
    /// Sets a document as having been deleted.
    /// </summary>
    /// <typeparam name="T">The type of the document.</typeparam>
    /// <param name="document">The deleted document.</param>
    void SetDeleted<T>(T document);

    /// <summary>
    /// Sets a document as having been updated.
    /// </summary>
    /// <typeparam name="T">The type of the document.</typeparam>
    /// <param name="oldDocument">The old document.</param>
    /// <param name="newDocument">The new document.</param>
    void SetUpdated<T>(T oldDocument, T newDocument);

    /// <summary>
    /// Applies any changes made since the last commit.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task CommitAsync(CancellationToken ct = default);
}
