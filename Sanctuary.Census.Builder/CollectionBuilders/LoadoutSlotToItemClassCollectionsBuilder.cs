using Microsoft.Extensions.Logging;
using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="LoadoutSlotToItemClass"/> and <see cref="VehicleLoadoutSlotToItemClass"/> collections.
/// </summary>
public class LoadoutSlotToItemClassCollectionsBuilder : ICollectionBuilder
{
    private readonly ILogger<LoadoutSlotToItemClassCollectionsBuilder> _logger;
    private readonly IClientDataCacheService _clientDataCache;
    private readonly IRequirementsHelperService _requirementsHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadoutSlotToItemClassCollectionsBuilder"/> class.
    /// </summary>
    /// <param name="logger">The logging interface to use.</param>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="requirementsHelper">The requirements helper service.</param>
    public LoadoutSlotToItemClassCollectionsBuilder
    (
        ILogger<LoadoutSlotToItemClassCollectionsBuilder> logger,
        IClientDataCacheService clientDataCache,
        IRequirementsHelperService requirementsHelper
    )
    {
        _logger = logger;
        _clientDataCache = clientDataCache;
        _requirementsHelper = requirementsHelper;
    }

    /// <inheritdoc />
    public async Task BuildAsync
    (
        ICollectionsContext dbContext,
        CancellationToken ct = default
    )
    {
        await BuildForNormalItems(dbContext, ct);
        await BuildForVehicleItems(dbContext, ct);
    }

    private async Task BuildForNormalItems(ICollectionsContext dbContext, CancellationToken ct)
    {
        try
        {
            if (_clientDataCache.LoadoutSlotItemClasses is null)
                throw new MissingCacheDataException(typeof(LoadoutSlotItemClass));

            List<LoadoutSlotToItemClass> builtMappings = new();

            foreach (LoadoutSlotItemClass map in _clientDataCache.LoadoutSlotItemClasses)
            {
                _requirementsHelper.TryGetClientExpression(map.ClientRequirementId, out string? requirementExpression);

                builtMappings.Add(new LoadoutSlotToItemClass
                (
                    map.LoadoutId,
                    map.Slot,
                    map.ItemClass,
                    requirementExpression
                ));
            }

            await dbContext.UpsertCollectionAsync(builtMappings, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to build the {nameof(LoadoutSlotToItemClass)} collection");
        }
    }

    private async Task BuildForVehicleItems(ICollectionsContext dbContext, CancellationToken ct)
    {
        try
        {
            if (_clientDataCache.VehicleLoadoutSlotItemClasses is null)
                throw new MissingCacheDataException(typeof(VehicleLoadoutSlotItemClass));

            List<VehicleLoadoutSlotToItemClass> builtMappings = _clientDataCache.VehicleLoadoutSlotItemClasses
                .Select(map => new VehicleLoadoutSlotToItemClass(map.LoadoutID, map.SlotID, map.ItemClass))
                .ToList();

            await dbContext.UpsertCollectionAsync(builtMappings, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to build the {nameof(VehicleLoadoutSlotToItemClass)} collection");
        }
    }
}
