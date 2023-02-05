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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Loadout = Sanctuary.Census.ClientData.ClientDataModels.Loadout;
using VehicleLoadout = Sanctuary.Census.ClientData.ClientDataModels.VehicleLoadout;

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
    private readonly IRequirementsHelperService _requirementsHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemCollectionBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    /// <param name="localeDataCache">The locale data cache.</param>
    /// <param name="serverDataCache">The server data cache.</param>
    /// <param name="imageSetHelper">The image set helper service.</param>
    /// <param name="requirementsHelper">The requirements helper service.</param>
    public ItemCollectionBuilder
    (
        IClientDataCacheService clientDataCache,
        ILocaleDataCacheService localeDataCache,
        IServerDataCacheService serverDataCache,
        IImageSetHelperService imageSetHelper,
        IRequirementsHelperService requirementsHelper
    )
    {
        _clientDataCache = clientDataCache;
        _localeDataCache = localeDataCache;
        _serverDataCache = serverDataCache;
        _imageSetHelper = imageSetHelper;
        _requirementsHelper = requirementsHelper;
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

        if (_serverDataCache.ItemCategories is null)
            throw new MissingCacheDataException(typeof(ItemCategories));

        ItemFactionResolver factionResolver = new(_clientDataCache);

        HashSet<uint> defaultAttachmentItems = GetDefaultAttachmentItems();

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

        Dictionary<uint, Item> builtItems = new();
        foreach (ClientItemDefinition definition in _clientDataCache.ClientItemDefinitions)
        {
            _localeDataCache.TryGetLocaleString(definition.NameID, out LocaleString? name);
            _localeDataCache.TryGetLocaleString(definition.DescriptionID, out LocaleString? description);
            _requirementsHelper.TryGetClientExpression(definition.ClientUseRequirementID, out string? useRequirement);
            _requirementsHelper.TryGetClientExpression(definition.ClientDisplayRequirementID, out string? equipRequirement);

            // Image set IDs can be negative on item definitions,
            // hence the added logic
            bool hasDefaultImage = false;
            uint defaultImage = 0;
            if (definition.ImageSetID > 0)
                hasDefaultImage = _imageSetHelper.TryGetDefaultImage((uint)definition.ImageSetID, out defaultImage);

            FactionDefinition faction = factionResolver.Resolve
            (
                definition.ID,
                definition.ItemClass,
                useRequirement,
                equipRequirement
            );

            Item built = new
            (
                definition.ID,
                definition.ItemType,
                definition.CategoryID.ToNullableUInt(),
                (uint)faction,
                name,
                description,
                definition.ActivatableAbilityID.ToNullableUInt(),
                definition.PassiveAbilityID.ToNullableUInt(),
                definition.PassiveAbilitySetID.ToNullableUInt(),
                definition.SkillSetID.ToNullableUInt(),
                definition.ImageSetID <= 0 ? null : (uint)definition.ImageSetID,
                hasDefaultImage ? defaultImage : null,
                hasDefaultImage ? _imageSetHelper.GetRelativeImagePath(defaultImage) : null,
                definition.HudImageSetID.ToNullableUInt(),
                definition.MaxStackSize,
                vehicleWeaponCategories.Contains(definition.CategoryID),
                defaultAttachmentItems.Contains(definition.ID),
                definition.CodeFactoryName,
                useRequirement,
                equipRequirement
            );
            builtItems.TryAdd(built.ItemID, built);
        }

        await dbContext.UpsertCollectionAsync(builtItems.Values, ct).ConfigureAwait(false);
    }

    private HashSet<uint> GetDefaultAttachmentItems()
    {
        if (_clientDataCache.ItemLineMembers is null)
            throw new MissingCacheDataException(typeof(ItemLineMember));

        if (_clientDataCache.LoadoutAttachments is null)
            throw new MissingCacheDataException(typeof(LoadoutAttachment));

        if (_clientDataCache.LoadoutItemAttachments is null)
            throw new MissingCacheDataException(typeof(LoadoutItemAttachment));

        Dictionary<uint, List<uint>> itemLineToItems = new();
        foreach (ItemLineMember ilm in _clientDataCache.ItemLineMembers)
        {
            if (!itemLineToItems.TryGetValue(ilm.ItemLineId, out List<uint>? items))
            {
                items = new List<uint>();
                itemLineToItems.Add(ilm.ItemLineId, items);
            }
            items.Add(ilm.ItemId);
        }

        Dictionary<uint, uint> attachmentToItemLine = _clientDataCache.LoadoutAttachments
            .ToDictionary(x => x.Id, x => x.ItemLineId);

        HashSet<uint> defaultAttachmentItems = new();
        foreach (LoadoutItemAttachment lia in _clientDataCache.LoadoutItemAttachments)
        {
            // These are all default attachments

            if (!attachmentToItemLine.TryGetValue(lia.AttachmentId, out uint attachmentItemLine))
                continue;

            if (!itemLineToItems.TryGetValue(attachmentItemLine, out List<uint>? items))
                continue;

            foreach (uint element in items)
                defaultAttachmentItems.Add(element);
        }

        return defaultAttachmentItems;
    }

    private class ItemFactionResolver
    {
        private readonly IReadOnlyDictionary<uint, FactionDefinition> _itemClassToFaction;
        private readonly IReadOnlyDictionary<uint, FactionDefinition> _itemToFaction;

        public ItemFactionResolver(IClientDataCacheService clientDataCache)
        {
            if (clientDataCache.ItemProfiles is null)
                throw new MissingCacheDataException(typeof(ItemProfile));

            if (clientDataCache.ItemVehicles is null)
                throw new MissingCacheDataException(typeof(ItemVehicle));

            if (clientDataCache.Loadouts is null)
                throw new MissingCacheDataException(typeof(Loadout));

            if (clientDataCache.VehicleLoadouts is null)
                throw new MissingCacheDataException(typeof(VehicleLoadout));

            if (clientDataCache.LoadoutSlotItemClasses is null)
                throw new MissingCacheDataException(typeof(LoadoutSlotItemClass));

            if (clientDataCache.LoadoutSlotTintItemClasses is null)
                throw new MissingCacheDataException(typeof(LoadoutSlotTintItemClass));

            if (clientDataCache.VehicleLoadoutSlotItemClasses is null)
                throw new MissingCacheDataException(typeof(VehicleLoadoutSlotItemClass));

            if (clientDataCache.VehicleLoadoutSlotTintItemClasses is null)
                throw new MissingCacheDataException(typeof(VehicleLoadoutSlotTintItemClass));

            Dictionary<uint, uint> loadoutToFaction = clientDataCache.Loadouts
                .ToDictionary(x => x.LoadoutID, x => x.FactionID);
            Dictionary<uint, uint> vehicleLoadoutToFaction = clientDataCache.VehicleLoadouts
                .ToDictionary(x => x.ID, x => x.FactionID);

            Dictionary<uint, FactionDefinition> itemClassToFaction = new();

            void AddItemClassToFaction(uint classId, FactionDefinition faction)
            {
                if (itemClassToFaction.TryAdd(classId, faction))
                    return;

                if (itemClassToFaction[classId] != faction)
                    itemClassToFaction[classId] = FactionDefinition.All;
            }

            foreach (LoadoutSlotItemClass lsic in clientDataCache.LoadoutSlotItemClasses)
            {
                FactionDefinition faction = (FactionDefinition)loadoutToFaction[lsic.LoadoutId];
                AddItemClassToFaction(lsic.ItemClass, faction);
            }

            foreach (LoadoutSlotTintItemClass lstic in clientDataCache.LoadoutSlotTintItemClasses)
            {
                FactionDefinition faction = (FactionDefinition)loadoutToFaction[lstic.LoadoutId];
                AddItemClassToFaction(lstic.TintItemClass, faction);
            }

            foreach (VehicleLoadoutSlotItemClass vlsic in clientDataCache.VehicleLoadoutSlotItemClasses)
            {
                FactionDefinition faction = (FactionDefinition)vehicleLoadoutToFaction[vlsic.LoadoutID];
                AddItemClassToFaction(vlsic.ItemClass, faction);
            }

            foreach (VehicleLoadoutSlotTintItemClass vlstic in clientDataCache.VehicleLoadoutSlotTintItemClasses)
            {
                FactionDefinition faction = (FactionDefinition)vehicleLoadoutToFaction[vlstic.LoadoutId];
                AddItemClassToFaction(vlstic.TintItemClass, faction);
            }

            _itemClassToFaction = itemClassToFaction;

            Dictionary<uint, FactionDefinition> itemFactionMap = new();
            foreach (ItemProfile profile in clientDataCache.ItemProfiles)
            {
                itemFactionMap.TryAdd(profile.ItemID, profile.FactionID);
                if (itemFactionMap[profile.ItemID] != profile.FactionID)
                    itemFactionMap[profile.ItemID] = FactionDefinition.All;
            }

            foreach (ItemVehicle vItem in clientDataCache.ItemVehicles)
            {
                itemFactionMap.TryAdd(vItem.ItemID, (FactionDefinition)vItem.FactionID);
                if (itemFactionMap[vItem.ItemID] != (FactionDefinition)vItem.FactionID)
                    itemFactionMap[vItem.ItemID] = FactionDefinition.All;
            }

            _itemToFaction = itemFactionMap;
        }

        public FactionDefinition Resolve(uint itemId, uint itemClassId, string? useRequirement, string? equipRequirement)
        {
            if (useRequirement == "LocalPlayerVS" || equipRequirement == "LocalPlayerVS")
                return FactionDefinition.VS;
            if (useRequirement == "LocalPlayerNC" || equipRequirement == "LocalPlayerNC")
                return FactionDefinition.NC;
            if (useRequirement == "LocalPlayerTR" || equipRequirement == "LocalPlayerTR")
                return FactionDefinition.TR;
            if (useRequirement == "LocalPlayerNSO" || equipRequirement == "LocalPlayerNSO")
                return FactionDefinition.NSO;

            if (_itemClassToFaction.ContainsKey(itemClassId))
                return _itemClassToFaction[itemClassId];

            if (_itemToFaction.ContainsKey(itemId))
                return _itemToFaction[itemId];

            return FactionDefinition.All;
        }
    }
}
