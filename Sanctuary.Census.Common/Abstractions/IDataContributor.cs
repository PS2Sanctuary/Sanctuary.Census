using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Common.Abstractions;

/// <summary>
/// Represents an object that can contribute data of a particular type.
/// </summary>
/// <typeparam name="TContributeFrom">The type of object that this contributor provides data from.</typeparam>
public interface IDataContributor<TContributeFrom>
{
    /// <summary>
    /// Gets a value indicating whether this contributor can contribute
    /// to the given type.
    /// </summary>
    /// <typeparam name="TContributeTo">The type of object to contribute to.</typeparam>
    /// <returns><c>True</c> if this contributor supports the given type, else <c>False</c></returns>
    bool CanContributeTo<TContributeTo>();

    /// <summary>
    /// Request the list of IDs that this contributor can source data for, for
    /// the given <typeparamref name="TContributeTo"/>.
    /// </summary>
    /// <typeparam name="TContributeTo">The type to contribute to.</typeparam>
    /// <param name="ct"></param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the potentially asynchronous operation.</returns>
    ValueTask<IReadOnlyList<uint>> GetContributableIDsAsync<TContributeTo>(CancellationToken ct = default);

    /// <summary>
    /// Request a contribution to the given item.
    /// </summary>
    /// <typeparam name="TContributeTo">The type of the <paramref name="item"/>.</typeparam>
    /// <param name="item">The item to contribute to.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> representing the potentially asynchronous operation.</returns>
    ValueTask ContributeAsync<TContributeTo>(TContributeTo item, CancellationToken ct = default);

    /// <summary>
    /// Gets a source data item.
    /// </summary>
    /// <param name="id">The ID of the item.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A source data item.</returns>
    ValueTask<TContributeFrom> GetDataAsync(uint id, CancellationToken ct = default);
}
