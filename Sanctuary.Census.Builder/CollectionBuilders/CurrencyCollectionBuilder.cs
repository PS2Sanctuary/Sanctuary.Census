using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CCurrency = Sanctuary.Census.ClientData.ClientDataModels.Currency;
using MCurrency = Sanctuary.Census.Common.Objects.Collections.Currency;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MCurrency"/> collection.
/// </summary>
public class CurrencyCollectionBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;
    private readonly ILocaleDataCacheService _localeDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrencyCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    public CurrencyCollectionBuilder
    (
        IClientDataCacheService clientDataCache,
        ILocaleDataCacheService localeDataCache
    )
    {
        _clientDataCache = clientDataCache;
        _localeDataCache = localeDataCache;
    }

    /// <inheritdoc />
    public async Task BuildAsync
    (
        ICollectionsContext dbContext,
        CancellationToken ct
    )
    {
        if (_clientDataCache.Currencies is null)
            throw new MissingCacheDataException(typeof(CCurrency));

        Dictionary<uint, MCurrency> builtCurrencies = new();
        foreach (CCurrency currency in _clientDataCache.Currencies)
        {
            _localeDataCache.TryGetLocaleString(currency.NameID, out LocaleString? name);
            _localeDataCache.TryGetLocaleString(currency.DescriptionID, out LocaleString? description);

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
