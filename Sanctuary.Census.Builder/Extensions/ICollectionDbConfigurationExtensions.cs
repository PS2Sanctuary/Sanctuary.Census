using MongoDB.Driver;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Common.Abstractions.Objects.Collections;
using Sanctuary.Census.Common.Abstractions.Services;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Builder.Extensions;

/// <summary>
/// Defines extension methods for the <see cref="ICollectionDbConfiguration{TCollection}"/> interface.
/// </summary>
public static class ICollectionDbConfigurationExtensions
{
    /// <summary>
    /// Scaffolds the collection within the database.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="database">The database to scaffold the collection within.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task ScaffoldAsync<TCollection>
    (
        this ICollectionDbConfiguration<TCollection> configuration,
        IMongoContext database,
        CancellationToken ct = default
    ) where TCollection : ISanctuaryCollection
    {
        // ReSharper disable once ArrangeMethodOrOperatorBody
        await database.GetCollection<TCollection>()
            .Indexes
            .CreateManyAsync
            (
                configuration.IndexPropertySelectors.Select(x => new CreateIndexModel<TCollection>
                (
                    Builders<TCollection>.IndexKeys.Ascending(x.Selector),
                    new CreateIndexOptions { Unique = x.IsUnique }
                )),
                ct
            )
            .ConfigureAwait(false);
    }
}
