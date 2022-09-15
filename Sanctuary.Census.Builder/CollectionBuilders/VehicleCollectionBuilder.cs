using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MVehicle = Sanctuary.Census.Common.Objects.Collections.Vehicle;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MVehicle"/> collection.
/// </summary>
public class VehicleCollectionBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IImageSetHelperService _imageSetHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="VehicleCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="imageSetHelper">The image set helper service.</param>
    public VehicleCollectionBuilder
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
        if (_clientDataCache.Vehicles is null)
            throw new MissingCacheDataException(typeof(Vehicle));

        Dictionary<int, MVehicle> builtVehicles = new();
        foreach (Vehicle vehicle in _clientDataCache.Vehicles)
        {
            _localeDataCache.TryGetLocaleString(vehicle.NameId, out LocaleString? name);
            _localeDataCache.TryGetLocaleString(vehicle.DescriptionId, out LocaleString? description);

            bool hasDefaultImage = _imageSetHelper.TryGetDefaultImage(vehicle.Icon, out uint defaultImage);

            MVehicle built = new
            (
                vehicle.Id,
                name!,
                description!,
                vehicle.VehicleType,
                vehicle.Decay,
                vehicle.AcquireSec,
                vehicle.Icon == 0 ? null : vehicle.Icon,
                hasDefaultImage ? defaultImage : null,
                hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null,
                vehicle.Cost,
                vehicle.CurrencyType,
                vehicle.LandingHeight == 0 ? null : vehicle.LandingHeight,
                vehicle.ImpactDamageBlocked,
                vehicle.ImpactDamageMultiplier == 0 ? null : new decimal(vehicle.ImpactDamageMultiplier),
                vehicle.ImpactDamageInflictedMult == 0 ? null : new decimal(vehicle.ImpactDamageInflictedMult),
                vehicle.PropulsionType,
                vehicle.SchematicImageSetId == 0 ? null : vehicle.SchematicImageSetId,
                vehicle.HealthImageSetId == 0 ? null : vehicle.HealthImageSetId,
                vehicle.MinimapRange,
                vehicle.AutoDetectRadius,
                vehicle.LockonTimeAdd == 0 ? null : vehicle.LockonTimeAdd,
                vehicle.LockonTimeMult == 0 ? null : new decimal(vehicle.LockonTimeMult)
            );
            builtVehicles.TryAdd(built.VehicleId, built);
        }

        await dbContext.UpsertCollectionAsync(builtVehicles.Values, ct).ConfigureAwait(false);
    }
}
