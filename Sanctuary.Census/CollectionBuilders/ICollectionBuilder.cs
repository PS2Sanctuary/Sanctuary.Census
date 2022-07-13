using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Represents a collection builder.
/// </summary>
public interface ICollectionBuilder
{
    /// <summary>
    /// Adds data to the given <paramref name="context"/> using the provided caches.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    /// <param name="localeService">The locale service.</param>
    /// <param name="context">The collections context.</param>
    void Build
    (
        IClientDataCacheService clientDataCache,
        IServerDataCacheService serverDataCache,
        ILocaleService localeService,
        CollectionsContext context
    );
}
