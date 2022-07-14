using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Exceptions;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using System.Collections.Generic;
using CCurrency = Sanctuary.Census.ClientData.ClientDataModels.Currency;
using MCurrency = Sanctuary.Census.Models.Currency;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MCurrency"/> collection.
/// </summary>
public class CurrencyCollectionBuilder : ICollectionBuilder
{
    /// <inheritdoc />
    public void Build
    (
        IClientDataCacheService clientDataCache,
        IServerDataCacheService serverDataCache,
        ILocaleService localeService,
        CollectionsContext context
    )
    {
        if (clientDataCache.Currencies.Count == 0)
            throw new MissingCacheDataException(typeof(CCurrency));

        Dictionary<uint, MCurrency> builtCurrencies = new();
        foreach (CCurrency currency in clientDataCache.Currencies)
        {
            localeService.TryGetLocaleString(currency.NameID, out LocaleString? name);
            localeService.TryGetLocaleString(currency.DescriptionID, out LocaleString? description);

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

        context.Currencies = builtCurrencies;
    }
}
