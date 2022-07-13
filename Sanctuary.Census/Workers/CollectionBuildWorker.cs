﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.CollectionBuilders;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Controllers;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Workers;

/// <summary>
/// A service worker responsible for building the data collections.
/// </summary>
public class CollectionBuildWorker : BackgroundService
{
    private readonly ILogger<CollectionBuildWorker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly CollectionsContext _collectionsContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContributionController"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="serviceScopeFactory">The service provider.</param>
    /// <param name="collectionsContext">The collections context.</param>
    public CollectionBuildWorker
    (
        ILogger<CollectionBuildWorker> logger,
        IServiceScopeFactory serviceScopeFactory,
        CollectionsContext collectionsContext
    )
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _collectionsContext = collectionsContext;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await using AsyncServiceScope serviceScope = _serviceScopeFactory.CreateAsyncScope();
            IServiceProvider services = serviceScope.ServiceProvider;

            IClientDataCacheService _clientDataCache = services.GetRequiredService<IClientDataCacheService>();
            IServerDataCacheService _serverDataCache = services.GetRequiredService<IServerDataCacheService>();
            ILocaleService _localeService = services.GetRequiredService<ILocaleService>();

            try
            {
                await _clientDataCache.RepopulateAsync(ct).ConfigureAwait(false);
                await _serverDataCache.RepopulateAsync(ct).ConfigureAwait(false);
                await _localeService.RepopulateAsync(ct).ConfigureAwait(false);
                _logger.LogInformation("Caches updated successfully!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cache data");
            }

            ICollectionBuilder[] collectionBuilders =
            {
                new ItemCollectionBuilder(),
                new WeaponCollectionBuilder()
            };

            foreach (ICollectionBuilder collectionBuilder in collectionBuilders)
            {
                try
                {
                    collectionBuilder.Build(_clientDataCache, _serverDataCache, _localeService, _collectionsContext);
                    _logger.LogInformation("Successfully ran the {CollectionBuilder}", collectionBuilder);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to run the {CollectionBuilder}", collectionBuilder);
                }
            }

            await Task.Delay(TimeSpan.FromHours(1), ct).ConfigureAwait(false);
        }
    }
}
