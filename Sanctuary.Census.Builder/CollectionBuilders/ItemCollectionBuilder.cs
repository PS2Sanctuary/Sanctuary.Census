using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Abstractions.Services;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.ServerData.Internal.Abstractions.Services;
using Sanctuary.Zone.Packets.ReferenceData;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="Item"/> collection.
/// </summary>
public class ItemCollectionBuilder : ICollectionBuilder
{
    private const int VEHICLE_WEAPONS_CATEGORY_ID = 104;

    private readonly IClientDataCacheService _clientDataCache;
    private readonly ILocaleDataCacheService _localeDataCache;
    private readonly IServerDataCacheService _serverDataCache;
    private readonly IImageSetHelperService _imageSetHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    /// <param name="imageSetHelper">The image set helper service.</param>
    public ItemCollectionBuilder
    (
        IClientDataCacheService clientDataCache,
        ILocaleDataCacheService localeDataCache,
        IServerDataCacheService serverDataCache,
        IImageSetHelperService imageSetHelper
    )
    {
        _clientDataCache = clientDataCache;
        _localeDataCache = localeDataCache;
        _serverDataCache = serverDataCache;
        _imageSetHelper = imageSetHelper;
    }

    /// <inheritdoc />
    public async Task BuildAsync
    (
        ICollectionsContext dbContext,
        CancellationToken ct
    )
    {
        if (_clientDataCache.ClientItemDefinitions is null)
            throw new MissingCacheDataException(typeof(ClientItemDefinition));

        if (_clientDataCache.ItemProfiles is null)
            throw new MissingCacheDataException(typeof(ItemProfile));

        if (_clientDataCache.ItemVehicles is null)
            throw new MissingCacheDataException(typeof(ItemVehicle));

        if (_serverDataCache.ItemCategories is null)
            throw new MissingCacheDataException(typeof(ItemCategories));

        HashSet<uint> vehicleWeaponCategories = new();
        foreach (CategoryHierarchy heirarchy in _serverDataCache.ItemCategories.Hierarchies)
        {
            if (heirarchy.ParentCategoryID == VEHICLE_WEAPONS_CATEGORY_ID)
                vehicleWeaponCategories.Add(heirarchy.ChildCategoryID);
        }

        // Add some categories manually because they don't have inheritance setup
        vehicleWeaponCategories.Add(209); // Bastion Bombard
        vehicleWeaponCategories.Add(210); // Bastion Weapon System
        vehicleWeaponCategories.Add(211); // Colossus Primary Weapon
        vehicleWeaponCategories.Add(212); // Colossus Front Right Weapon
        vehicleWeaponCategories.Add(213); // Colossus Front Left Weapon
        vehicleWeaponCategories.Add(214); // Colossus Rear Right Weapon
        vehicleWeaponCategories.Add(215); // Colossus Rear Left Weapon
        vehicleWeaponCategories.Add(216); // Javelin Primary Weapon
        vehicleWeaponCategories.Add(217); // Chimera Primary Weapons
        vehicleWeaponCategories.Add(218); // Chimera Secondary Weapons
        vehicleWeaponCategories.Add(221); // Corsair Front Turret
        vehicleWeaponCategories.Add(222); // Corsair Rear Turret

        Dictionary<uint, FactionDefinition> itemFactionMap = new();
        foreach (ItemProfile profile in _clientDataCache.ItemProfiles)
        {
            itemFactionMap.TryAdd(profile.ItemID, profile.FactionID);
            if (itemFactionMap[profile.ItemID] != profile.FactionID)
                itemFactionMap[profile.ItemID] = FactionDefinition.All;
        }

        foreach (ItemVehicle vItem in _clientDataCache.ItemVehicles)
        {
            itemFactionMap.TryAdd(vItem.ItemID, (FactionDefinition)vItem.FactionID);
            if (itemFactionMap[vItem.ItemID] != (FactionDefinition)vItem.FactionID)
                itemFactionMap[vItem.ItemID] = FactionDefinition.All;
        }

        Dictionary<uint, Item> builtItems = new();
        foreach (ClientItemDefinition definition in _clientDataCache.ClientItemDefinitions)
        {
            _localeDataCache.TryGetLocaleString(definition.NameID, out LocaleString? name);
            _localeDataCache.TryGetLocaleString(definition.DescriptionID, out LocaleString? description);

            bool hasDefaultImage = false;
            uint defaultImage = 0;
            if (definition.ImageSetID > 0)
                hasDefaultImage = _imageSetHelper.TryGetDefaultImage((uint)definition.ImageSetID, out defaultImage);

            if (!itemFactionMap.TryGetValue(definition.ID, out FactionDefinition faction))
                faction = FactionDefinition.All;

            Item built = new
            (
                definition.ID,
                definition.ItemType,
                definition.CategoryID == 0 ? null : definition.CategoryID,
                (uint)faction,
                name,
                description,
                definition.ActivatableAbilityID == 0 ? null : definition.ActivatableAbilityID,
                definition.PassiveAbilityID == 0 ? null : definition.PassiveAbilityID,
                definition.PassiveAbilitySetID == 0 ? null: definition.PassiveAbilitySetID,
                definition.SkillSetID == 0 ? null : definition.SkillSetID,
                definition.ImageSetID <= 0 ? null : (uint)definition.ImageSetID,
                hasDefaultImage ? defaultImage : null,
                hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null,
                definition.HudImageSetID == 0 ? null : definition.HudImageSetID,
                definition.MaxStackSize,
                vehicleWeaponCategories.Contains(definition.CategoryID)
            );
            builtItems.TryAdd(built.ItemID, built);
        }

        await dbContext.UpsertCollectionAsync(builtItems.Values, ct).ConfigureAwait(false);
    }
}
