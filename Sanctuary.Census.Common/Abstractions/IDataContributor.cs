using Sanctuary.Census.Common.Objects;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Common.Abstractions;

/// <summary>
/// Represents an object that can contribute data of a particular type.
/// </summary>
public interface IDataContributor
{
    /// <summary>
    /// Gets a value indicating whether this contributor can contribute
    /// to the given type.
    /// </summary>
    /// <typeparam name="TContributeTo">The type of object to contribute to.</typeparam>
    /// <returns><c>True</c> if this contributor supports the given type, else <c>False</c></returns>
    bool CanContributeTo<TContributeTo>();

    /// <summary>
    /// Request the list of IDs that this contributor KNOWS that it can source data for, for
    /// the given <typeparamref name="TContributeTo"/>. The contributor may also be able to
    /// contribute data to the type, based on other properties of the type.
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
    /// <returns>A <see cref="ContributionResult{TContributedTo}"/>.</returns>
    ValueTask<ContributionResult<TContributeTo>> ContributeAsync<TContributeTo>(TContributeTo item, CancellationToken ct = default)
        where TContributeTo : class;
}
