using Sanctuary.Census.Abstractions.CollectionBuilders;
using Sanctuary.Census.Abstractions.Database;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Exceptions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MVehicle = Sanctuary.Census.Models.Collections.Vehicle;

namespace Sanctuary.Census.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MVehicle"/> collection.
/// </summary>
public class VehicleCollectionBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;
    private readonly ILocaleDataCacheService _localeDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    public VehicleCollectionBuilder
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
        if (_clientDataCache.ImageSetMappings is null)
            throw new MissingCacheDataException(typeof(ImageSetMapping));

        if (_clientDataCache.Vehicles is null)
            throw new MissingCacheDataException(typeof(Vehicle));

        Dictionary<uint, uint> imageSetToPrimaryImageMap = new();
        foreach (ImageSetMapping mapping in _clientDataCache.ImageSetMappings)
        {
            if (mapping.ImageType is not ImageSetType.Large)
                continue;

            imageSetToPrimaryImageMap[mapping.ImageSetID] = mapping.ImageID;
        }

        Dictionary<int, MVehicle> builtVehicles = new();
        foreach (Vehicle vehicle in _clientDataCache.Vehicles)
        {
            _localeDataCache.TryGetLocaleString(vehicle.NameId, out LocaleString? name);
            _localeDataCache.TryGetLocaleString(vehicle.DescriptionId, out LocaleString? description);

            bool foundImage = imageSetToPrimaryImageMap.TryGetValue(vehicle.Icon, out uint imageId);

            MVehicle built = new
            (
                vehicle.Id,
                name!,
                description!,
                vehicle.VehicleType,
                vehicle.Decay,
                vehicle.AcquireSec,
                vehicle.Icon == 0 ? null : vehicle.Icon,
                foundImage ? imageId : null,
                foundImage ? $"/files/ps2/images/static/{imageId}.png" : null,
                vehicle.Cost,
                vehicle.CurrencyType,
                vehicle.LandingHeight == 0 ? null : vehicle.LandingHeight,
                vehicle.ImpactDamageBlocked,
                vehicle.ImpactDamageMultiplier == 0 ? null : vehicle.ImpactDamageMultiplier,
                vehicle.ImpactDamageInflictedMult == 0 ? null : vehicle.ImpactDamageInflictedMult,
                vehicle.PropulsionType,
                vehicle.SchematicImageSetId == 0 ? null : vehicle.SchematicImageSetId,
                vehicle.HealthImageSetId == 0 ? null : vehicle.HealthImageSetId,
                vehicle.MinimapRange,
                vehicle.AutoDetectRadius,
                vehicle.LockonTimeAdd == 0 ? null : vehicle.LockonTimeAdd,
                vehicle.LockonTimeMult == 0 ? null : vehicle.LockonTimeMult
            );
            builtVehicles.TryAdd(built.VehicleId, built);
        }

        await dbContext.UpsertVehiclesAsync(builtVehicles.Values, ct);
    }
}
