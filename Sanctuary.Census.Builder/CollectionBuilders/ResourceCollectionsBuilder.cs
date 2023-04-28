using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.CommonModels;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using MResource = Sanctuary.Census.Common.Objects.Collections.Resource;
using MResourceType = Sanctuary.Census.Common.Objects.Collections.ResourceType;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MResource"/> and <see cref="MResourceType"/> collection.
/// </summary>
public class ResourceCollectionsBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;
    private readonly IImageSetHelperService _imageSetHelper;
    private readonly ILocaleDataCacheService _localeDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceCollectionsBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="imageSetHelper">The image set helper service.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    public ResourceCollectionsBuilder
    (
        IClientDataCacheService clientDataCache,
        IImageSetHelperService imageSetHelper,
        ILocaleDataCacheService localeDataCache
    )
    {
        _clientDataCache = clientDataCache;
        _imageSetHelper = imageSetHelper;
        _localeDataCache = localeDataCache;
    }

    /// <inheritdoc />
    public async Task BuildAsync(ICollectionsContext dbContext, CancellationToken ct = default)
    {
        await BuildResourceCollectionAsync(dbContext, ct).ConfigureAwait(false);
        await BuildResourceTypeCollectionAsync(dbContext, ct).ConfigureAwait(false);
    }

    private async Task BuildResourceCollectionAsync(ICollectionsContext dbContext, CancellationToken ct)
    {
        if (_clientDataCache.Resources is null)
            throw new MissingCacheDataException(typeof(Resource));

        Dictionary<uint, MResource> builtResources = new();
        foreach (Resource resource in _clientDataCache.Resources)
        {
            _localeDataCache.TryGetLocaleString(resource.NameId, out LocaleString? name);
            _localeDataCache.TryGetLocaleString(resource.DescriptionId, out LocaleString? description);
            bool hasDefaultImage = _imageSetHelper.TryGetDefaultImage(resource.IconId, out uint defaultImage);

            builtResources.Add(resource.Id, new MResource
            (
                resource.Id,
                resource.ResourceType,
                name,
                description,
                resource.ActivatedAbilityId.ToNullableUInt(),
                resource.DepletedAbilityId.ToNullableUInt(),
                resource.RemovedAbilityId.ToNullableUInt(),
                resource.InitialValue,
                resource.InitialValueMax.ToNullableDecimal(),
                resource.UseInitialValueAsMax,
                resource.MaxValue.ToNullableDecimal(),
                resource.ValueMarkerLo.ToNullableDecimal(),
                resource.ValueMarkerMed.ToNullableDecimal(),
                resource.ValueMarkerHi.ToNullableDecimal(),
                resource.RegenPerMs.ToNullableDecimal(),
                resource.RegenDelayMs.ToNullableUInt(),
                resource.RegenDamageInterruptMs.ToNullableUInt(),
                resource.RegenTickMsec.ToNullableUInt(),
                resource.BurnPerMs.ToNullableDecimal(),
                resource.BurnTickMsec.ToNullableUInt(),
                resource.BurnDelayMs.ToNullableUInt(),
                resource.FlagProfileScope,
                resource.FlagNotVehMountScope,
                resource.AllowBurnHealXp,
                resource.FullAbilityId.ToNullableUInt(),
                resource.IconId.ToNullableUInt(),
                defaultImage.ToNullableUInt(),
                hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null
            ));
        }

        await dbContext.UpsertCollectionAsync(builtResources.Values, ct).ConfigureAwait(false);
    }

    private async Task BuildResourceTypeCollectionAsync(ICollectionsContext dbContext, CancellationToken ct)
    {
        if (_clientDataCache.ResourceTypes is null)
            throw new MissingCacheDataException(typeof(ResourceType));

        Dictionary<nuint, MResourceType> builtTypes = new();
        foreach (ResourceType type in _clientDataCache.ResourceTypes)
        {
            // A bit useless having a type without a name
            if (string.IsNullOrEmpty(type.TypeName) && type.NameID == 0)
                continue;

            _localeDataCache.TryGetLocaleString(type.NameID, out LocaleString? name);
            bool hasDefaultImage = _imageSetHelper.TryGetDefaultImage(type.ImageSet, out uint defaultImage);

            builtTypes.Add(type.TypeID, new MResourceType
            (
                type.TypeID,
                type.TypeName?.Replace("ResourceType", string.Empty),
                name,
                type.ImageSet.ToNullableUInt(),
                defaultImage.ToNullableUInt(),
                hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null
            ));
        }

        await dbContext.UpsertCollectionAsync(builtTypes.Values, ct).ConfigureAwait(false);
    }
}
