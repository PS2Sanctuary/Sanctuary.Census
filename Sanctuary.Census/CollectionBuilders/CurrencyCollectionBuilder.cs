using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CCurrency = Sanctuary.Census.ClientData.ClientDataModels.Currency;
using MCurrency = Sanctuary.Census.Models.Collections.Currency;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MCurrency"/> collection.
/// </summary>
public class CurrencyCollectionBuilder : ICollectionBuilder
{
    /// <inheritdoc />
    public async Task BuildAsync
    (
        IClientDataCacheService clientDataCache,
        IServerDataCacheService serverDataCache,
        ILocaleDataCacheService localeDataCache,
        IMongoContext dbContext,
        CancellationToken ct
    )
    {
        if (clientDataCache.Currencies.Count == 0)
            throw new MissingCacheDataException(typeof(CCurrency));

        Dictionary<uint, MCurrency> builtCurrencies = new();
        foreach (CCurrency currency in clientDataCache.Currencies)
        {
            localeDataCache.TryGetLocaleString(currency.NameID, out LocaleString? name);
            localeDataCache.TryGetLocaleString(currency.DescriptionID, out LocaleString? description);

            MCurrency built = new
            (
                currency.ID,
                name!,
                description,
                currency.IconID,
                currency.MapIconID,
                currency.ValueMax > 0 ? currency.ValueMax : null
            );
            builtCurrencies.TryAdd(built.CurrencyID, built);
        }

        await dbContext.UpsertCurrenciesAsync(builtCurrencies.Values, ct);
    }
}
