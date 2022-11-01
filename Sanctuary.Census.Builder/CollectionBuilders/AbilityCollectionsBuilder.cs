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

using MAbility = Sanctuary.Census.Common.Objects.Collections.Ability;
using MAbilitySet = Sanctuary.Census.Common.Objects.Collections.AbilitySet;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="MAbility"/> and <see cref="MAbilitySet"/> collection.
/// </summary>
public class AbilityCollectionsBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;
    private readonly IImageSetHelperService _imageSetHelper;
    private readonly ILocaleDataCacheService _localeDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="AbilityCollectionsBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="imageSetHelper">The image set helper service.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    public AbilityCollectionsBuilder
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
        await BuildAbilityCollectionAsync(dbContext, ct).ConfigureAwait(false);
        await BuildAbilitySetCollectionAsync(dbContext, ct).ConfigureAwait(false);
    }

    private async Task BuildAbilityCollectionAsync(ICollectionsContext dbContext, CancellationToken ct)
    {
        if (_clientDataCache.AbilityExs is null)
            throw new MissingCacheDataException(typeof(AbilityEx));

        Dictionary<uint, MAbility> builtAbilities = new();
        foreach (AbilityEx ability in _clientDataCache.AbilityExs)
        {
            _localeDataCache.TryGetLocaleString(ability.NameId, out LocaleString? name);
            _localeDataCache.TryGetLocaleString(ability.DescriptionId, out LocaleString? description);
            bool hasDefaultImage = _imageSetHelper.TryGetDefaultImage(ability.IconId, out uint defaultImage);

            builtAbilities.Add(ability.Id, new MAbility
            (
                ability.Id,
                ability.TypeName,
                name,
                description,
                ability.CannotStartInFireBlock,
                ability.ExpireMsec.ToNullableUInt(),
                ability.FlagSurvivesProfileSwap,
                ability.FlagToggle,
                ability.FlagUseTargetResource,
                ability.FirstUseDelayMsec.ToNullableUInt(),
                ability.NextUseDelayMsec.ToNullableUInt(),
                ability.ReuseDelayMsec.ToNullableUInt(),
                ability.ResourceBurnInVr,
                ability.ResourceCostPerMsec.ToNullableDecimal(),
                ability.ResourceFirstCost.ToNullableUInt(),
                ability.ResourceThreshold.ToNullableUInt(),
                ability.ResourceType.ToNullableUInt(),
                ability.DistanceMax.ToNullableUInt(),
                ability.RadiusMax.ToNullableUInt(),
                ability.Param1.ToNullableUInt(),
                ability.Param2.ToNullableUInt(),
                ability.Param3.ToNullableUInt(),
                ability.Param4.ToNullableUInt(),
                ability.Param5.ToNullableUInt(),
                ability.Param6.ToNullableUInt(),
                ability.Param7.ToNullableUInt(),
                ability.Param8.ToNullableUInt(),
                ability.Param9.ToNullableUInt(),
                ability.Param10.ToNullableUInt(),
                ability.Param11.ToNullableUInt(),
                ability.Param12.ToNullableUInt(),
                ability.Param13.ToNullableUInt(),
                ability.Param14.ToNullableUInt(),
                ability.String1.ToNullableString(),
                ability.String2.ToNullableString(),
                ability.String3.ToNullableString(),
                ability.String4.ToNullableString(),
                ability.UseWeaponCharge,
                ability.IconId.ToNullableUInt(),
                defaultImage.ToNullableUInt(),
                hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null
            ));
        }

        await dbContext.UpsertCollectionAsync(builtAbilities.Values, ct).ConfigureAwait(false);
    }

    private async Task BuildAbilitySetCollectionAsync(ICollectionsContext dbContext, CancellationToken ct)
    {
        if (_clientDataCache.AbilitySets is null)
            throw new MissingCacheDataException(typeof(AbilitySet));

        List<MAbilitySet> builtSets = new();
        foreach (AbilitySet set in _clientDataCache.AbilitySets)
        {
            builtSets.Add(new MAbilitySet
            (
                set.AbilitySetId,
                set.AbilityId,
                set.OrderIndex
            ));
        }

        await dbContext.UpsertCollectionAsync(builtSets, ct).ConfigureAwait(false);
    }
}
