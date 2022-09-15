using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
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
    private readonly IImageSetHelperService _imageSetHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrencyCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="imageSetHelper">The image set helper service.</param>
    public CurrencyCollectionBuilder
    (
        IClientDataCacheService clientDataCache,
        ILocaleDataCacheService localeDataCache,
        IImageSetHelperService imageSetHelper
    )
    {
        _clientDataCache = clientDataCache;
        _localeDataCache = localeDataCache;
        _imageSetHelper = imageSetHelper;
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
            bool hasDefaultImage = _imageSetHelper.TryGetDefaultImage(currency.IconID, out uint defaultImage);

            MCurrency built = new
            (
                currency.ID,
                name!,
                description,
                currency.ValueMax > 0 ? currency.ValueMax : null,
                currency.IconID,
                currency.IconID,
                hasDefaultImage ? defaultImage : null,
                hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null,
                currency.MapIconID
            );
            builtCurrencies.TryAdd(built.CurrencyID, built);
        }

        await dbContext.UpsertCollectionAsync(builtCurrencies.Values, ct).ConfigureAwait(false);
    }
}
