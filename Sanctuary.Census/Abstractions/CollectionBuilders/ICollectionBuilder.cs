using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
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
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    /// <param name="localeDataCache">The locale service.</param>
    /// <param name="dbContext">The collections context.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> that can be used to stop the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task BuildAsync
    (
        IClientDataCacheService clientDataCache,
        IServerDataCacheService serverDataCache,
        ILocaleDataCacheService localeDataCache,
        IMongoContext dbContext,
        CancellationToken ct
    );
}
