using CommunityToolkit.HighPerformance.Buffers;
using Mandible.Abstractions.Pack2;
using Mandible.Pack2;
using Mandible.Services;
using Mandible.Util;
using Sanctuary.Census.ClientData.Abstractions;
using Sanctuary.Census.ClientData.Abstractions.ClientDataModels;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Sanctuary.Census.ClientData.Services;

/// <inheritdoc />
public class ClientDataCacheService : IClientDataCacheService
{
    private readonly IManifestService _manifestService;
    private readonly EnvironmentContextProvider _environmentContextProvider;

    /// <inheritdoc />
    public DateTimeOffset LastPopulated { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<AbilityEx>? AbilityExs { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<AbilitySet>? AbilitySets { get; private set; }

    /// <inheritdoc />
    public IReadOnlyDictionary<AssetZone, IReadOnlyList<AreaDefinition>>? AreaDefinitions { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<ClientItemDatasheetData>? ClientItemDatasheetDatas { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<ClientItemDefinition>? ClientItemDefinitions { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<ClientPrestigeLevel>? ClientPrestigeLevels { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<ClientRequirementExpression>? ClientRequirementExpressions { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<Currency>? Currencies { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<Experience>? Experiences { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<ExperienceRank>? ExperienceRanks { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<Faction>? Factions { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<FireModeDisplayStat>? FireModeDisplayStats { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<Image>? Images { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<ImageSet>? ImageSets { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<ImageSetMapping>? ImageSetMappings { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<ImageSetType>? ImageSetTypes { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<ItemLineMember>? ItemLineMembers { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<ItemProfile>? ItemProfiles { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<ItemVehicle>? ItemVehicles { get; set; }

    /// <inheritdoc />
    public IReadOnlyList<Loadout>? Loadouts { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<LoadoutAttachment>? LoadoutAttachments { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<LoadoutAttachmentGroupMap>? LoadoutAttachmentGroupMaps { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<LoadoutItemAttachment>? LoadoutItemAttachments { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<LoadoutSlot>? LoadoutSlots { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<LoadoutSlotItemClass>? LoadoutSlotItemClasses { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<LoadoutSlotTintItemClass>? LoadoutSlotTintItemClasses { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<PrestigeLevelRankSet>? PrestigeLevelRankSets { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<PrestigeRankSetMapping>? PrestigeRankSetMappings { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<ResistInfo>? ResistInfos { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<Resource>? Resources { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<ResourceType>? ResourceTypes { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<Skill>? Skills { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<SkillCategory>? SkillCategories { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<SkillLine>? SkillLines { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<SkillSet>? SkillSets { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<Vehicle>? Vehicles { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<VehicleLoadout>? VehicleLoadouts { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<VehicleLoadoutSlot>? VehicleLoadoutSlots { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<VehicleLoadoutSlotItemClass>? VehicleLoadoutSlotItemClasses { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<VehicleLoadoutSlotTintItemClass>? VehicleLoadoutSlotTintItemClasses { get; private set; }

    /// <inheritdoc />
    public IReadOnlyList<VehicleSkillSet>? VehicleSkillSets { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientDataCacheService"/> class.
    /// </summary>
    /// <param name="manifestService">The manifest service.</param>
    /// <param name="environmentContextProvider">The environment context provider.</param>
    public ClientDataCacheService
    (
        IManifestService manifestService,
        EnvironmentContextProvider environmentContextProvider
    )
    {
        _manifestService = manifestService;
        _environmentContextProvider = environmentContextProvider;

        Clear();
    }

    /// <inheritdoc />
    public async Task RepopulateAsync(CancellationToken ct = default)
    {
        ManifestFileWrapper dataPackManifest = await _manifestService.GetFileAsync
        (
            "data_x64_0.pack2",
            _environmentContextProvider.Environment,
            ct
        ).ConfigureAwait(false);

        await using Stream dataPackStream = await _manifestService.GetFileDataAsync
        (
            dataPackManifest,
            _environmentContextProvider.Environment,
            ct
        ).ConfigureAwait(false);
        await using StreamDataReaderService sdrs = new(dataPackStream, false);

        using Pack2Reader reader = new(sdrs);
        IReadOnlyList<Asset2Header> assetHeaders = await reader.ReadAssetHeadersAsync(ct).ConfigureAwait(false);

        await ExtractAreasAsync(assetHeaders, reader, ct).ConfigureAwait(false);

        AbilityExs = await ExtractDatasheet<AbilityEx>
        (
            "AbilityEx.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        AbilitySets = await ExtractDatasheet<AbilitySet>
        (
            "AbilitySets.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        ClientItemDatasheetDatas = await ExtractDatasheet<ClientItemDatasheetData>
        (
            "ClientItemDatasheetData.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        ClientItemDefinitions = await ExtractDatasheet<ClientItemDefinition>
        (
            "ClientItemDefinitions.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        ClientPrestigeLevels = await ExtractDatasheet<ClientPrestigeLevel>
        (
            "PrestigeLevel.txt",
            assetHeaders,
            reader,
            ct
        );

        ClientRequirementExpressions = await ExtractDatasheet<ClientRequirementExpression>
        (
            "ClientRequirementExpressions.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        Currencies = await ExtractDatasheet<Currency>
        (
            "Currency.txt",
            assetHeaders,
            reader,
            ct
        );

        Experiences = await ExtractDatasheet<Experience>
        (
            "Experience.txt",
            assetHeaders,
            reader,
            ct
        );

        ExperienceRanks = await ExtractDatasheet<ExperienceRank>
        (
            "ExperienceRanks.txt",
            assetHeaders,
            reader,
            ct
        );

        Factions = await ExtractDatasheet<Faction>
        (
            "Factions.txt",
            assetHeaders,
            reader,
            ct
        );

        FireModeDisplayStats = await ExtractDatasheet<FireModeDisplayStat>
        (
            "FireModeDisplayStats.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        Images = await ExtractDatasheet<Image>
        (
            "Images.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        ImageSets = await ExtractDatasheet<ImageSet>
        (
            "ImageSets.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        ImageSetMappings = await ExtractDatasheet<ImageSetMapping>
        (
            "ImageSetMappings.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        ImageSetTypes = await ExtractDatasheet<ImageSetType>
        (
            "ImageSetTypes.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        ItemLineMembers = await ExtractDatasheet<ItemLineMember>
        (
            "ItemLineMembers.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        ItemProfiles = await ExtractDatasheet<ItemProfile>
        (
            "ItemProfiles.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        ItemVehicles = await ExtractDatasheet<ItemVehicle>
        (
            "ItemVehicles.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        Loadouts = await ExtractDatasheet<Loadout>
        (
            "Loadouts.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        LoadoutAttachments = await ExtractDatasheet<LoadoutAttachment>
        (
            "LoadoutAttachments.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        LoadoutAttachmentGroupMaps = await ExtractDatasheet<LoadoutAttachmentGroupMap>
        (
            "LoadoutAttachmentGroupMap.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        LoadoutItemAttachments = await ExtractDatasheet<LoadoutItemAttachment>
        (
            "LoadoutItemAttachments.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        LoadoutSlots = await ExtractDatasheet<LoadoutSlot>
        (
            "LoadoutSlots.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        LoadoutSlotItemClasses = await ExtractDatasheet<LoadoutSlotItemClass>
        (
            "LoadoutSlotItemClasses.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        LoadoutSlotTintItemClasses = await ExtractDatasheet<LoadoutSlotTintItemClass>
        (
            "LoadoutSlotTintItemClasses.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        PrestigeLevelRankSets = await ExtractDatasheet<PrestigeLevelRankSet>
        (
            "PrestigeLevelRankSet.txt",
            assetHeaders,
            reader,
            ct
        );

        PrestigeRankSetMappings = await ExtractDatasheet<PrestigeRankSetMapping>
        (
            "PrestigeRankSetMapping.txt",
            assetHeaders,
            reader,
            ct
        );

        ResistInfos = await ExtractDatasheet<ResistInfo>
        (
            "ResistInfo.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        Resources = await ExtractDatasheet<Resource>
        (
            "Resources.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        ResourceTypes = await ExtractDatasheet<ResourceType>
        (
            "ResourceTypes.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        Skills = await ExtractDatasheet<Skill>
        (
            "Skills.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        SkillCategories = await ExtractDatasheet<SkillCategory>
        (
            "SkillCategories.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        SkillLines = await ExtractDatasheet<SkillLine>
        (
            "SkillLines.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        SkillSets = await ExtractDatasheet<SkillSet>
        (
            "SkillSets.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        Vehicles = await ExtractDatasheet<Vehicle>
        (
            "Vehicles.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        VehicleLoadouts = await ExtractDatasheet<VehicleLoadout>
        (
            "VehicleLoadouts.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        VehicleLoadoutSlots = await ExtractDatasheet<VehicleLoadoutSlot>
        (
            "VehicleLoadoutSlots.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        VehicleLoadoutSlotItemClasses = await ExtractDatasheet<VehicleLoadoutSlotItemClass>
        (
            "VehicleLoadoutSlotItemClasses.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        VehicleLoadoutSlotTintItemClasses = await ExtractDatasheet<VehicleLoadoutSlotTintItemClass>
        (
            "VehicleLoadoutSlotTintItemClasses.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        VehicleSkillSets = await ExtractDatasheet<VehicleSkillSet>
        (
            "VehicleSkillSets.txt",
            assetHeaders,
            reader,
            ct
        ).ConfigureAwait(false);

        LastPopulated = DateTimeOffset.UtcNow;
    }

    /// <inheritdoc />

    public void Clear()
    {
        LastPopulated = DateTimeOffset.MinValue;
        AbilityExs = null;
        AbilitySets = null;
        AreaDefinitions = null;
        ClientItemDatasheetDatas = null;
        ClientItemDefinitions = null;
        ClientPrestigeLevels = null;
        ClientRequirementExpressions = null;
        Currencies = null;
        Experiences = null;
        ExperienceRanks = null;
        Factions = null;
        FireModeDisplayStats = null;
        Images = null;
        ImageSets = null;
        ImageSetMappings = null;
        ImageSetTypes = null;
        ItemProfiles = null;
        ItemVehicles = null;
        Loadouts = null;
        LoadoutAttachmentGroupMaps = null;
        LoadoutSlots = null;
        LoadoutSlotItemClasses = null;
        LoadoutSlotTintItemClasses = null;
        PrestigeLevelRankSets = null;
        PrestigeRankSetMappings = null;
        ResistInfos = null;
        Resources = null;
        ResourceTypes = null;
        Skills = null;
        SkillCategories = null;
        SkillLines = null;
        SkillSets = null;
        Vehicles = null;
        VehicleLoadouts = null;
        VehicleLoadoutSlots = null;
        VehicleLoadoutSlotItemClasses = null;
        VehicleLoadoutSlotTintItemClasses = null;
        VehicleSkillSets = null;
    }

    private static async Task<List<TDataType>> ExtractDatasheet<TDataType>
    (
        string datasheetFileName,
        IEnumerable<Asset2Header> assetHeaders,
        IPack2Reader packReader,
        CancellationToken ct
    ) where TDataType : class, IDatasheet<TDataType>
    {
        ulong nameHash = PackCrc64.Calculate(datasheetFileName);
        Asset2Header header = assetHeaders.First(ah => ah.NameHash == nameHash);

        using MemoryOwner<byte> data = await packReader.ReadAssetDataAsync(header, ct).ConfigureAwait(false);
        return TDataType.Deserialize(data.Span);
    }

    private async Task ExtractAreasAsync
    (
        IReadOnlyList<Asset2Header> assetHeaders,
        IPack2Reader packReader,
        CancellationToken ct
    )
    {
        Dictionary<AssetZone, IReadOnlyList<AreaDefinition>> areas = new();

        foreach (AssetZone zone in Enum.GetValues<AssetZone>())
        {
            ulong nameHash = PackCrc64.Calculate($"{zone}Areas.xml");
            Asset2Header? header = assetHeaders.FirstOrDefault(ah => ah.NameHash == nameHash);
            if (header is null)
                continue;

            List<AreaDefinition> builtDefinitions = new();
            using MemoryOwner<byte> data = await packReader.ReadAssetDataAsync(header, ct).ConfigureAwait(false);
            await using MemoryStream ms = new(data.Memory.ToArray());

            XmlReaderSettings xmlSettings = new()
            {
                Async = true,
                CloseInput = false,
                ConformanceLevel = ConformanceLevel.Fragment // This is required as the area definitions are stored in one file as multiple root-level objects.
            };
            using XmlReader reader = XmlReader.Create(ms, xmlSettings);

            AreaDefinition? lastArea = null;

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                ct.ThrowIfCancellationRequested();

                if (reader.NodeType != XmlNodeType.Element)
                    continue;

                if (reader.Name == "AreaDefinition")
                {
                    _ = TryParseAreaDefinition(reader, out lastArea);
                }
                else if (reader.Name == "Property")
                {
                    if (lastArea is null)
                        continue;

                    if (reader.GetAttribute("type") != "SundererNoDeploy")
                        continue;

                    if (!TryParseAreaProperty(reader, out uint requirementId, out uint facilityId))
                        continue;

                    builtDefinitions.Add(lastArea with
                    {
                        RequirementID = requirementId,
                        FacilityID = facilityId
                    });
                }
            }

            if (builtDefinitions.Count > 0)
                areas[zone] = builtDefinitions;
        }

        AreaDefinitions = areas;
    }

    private static bool TryParseAreaDefinition(XmlReader reader, [NotNullWhen(true)] out AreaDefinition? area)
    {
        area = null;
        string? idVal = reader.GetAttribute("id");
        string? nameVal = reader.GetAttribute("name");
        string? shapeVal = reader.GetAttribute("shape");
        string? x1Val = reader.GetAttribute("x1");
        string? y1Val = reader.GetAttribute("y1");
        string? z1Val = reader.GetAttribute("z1");
        string? x2Val = reader.GetAttribute("x2");
        string? y2Val = reader.GetAttribute("y2");
        string? z2Val = reader.GetAttribute("z2");
        string? rotXVal = reader.GetAttribute("rotX");
        string? rotYVal = reader.GetAttribute("rotY");
        string? rotZVal = reader.GetAttribute("rotZ");
        string? radVal = reader.GetAttribute("radius");

        if (!uint.TryParse(idVal, out uint id))
            return false;

        if (shapeVal is null)
            return false;

        if (!decimal.TryParse(x1Val, out decimal x1))
            return false;

        if (!decimal.TryParse(z1Val, out decimal z1))
            return false;

        if (!decimal.TryParse(y1Val, out decimal y1))
            return false;

        decimal? x2 = null;
        if (x2Val is not null)
        {
            if (!decimal.TryParse(x2Val, out decimal x2Temp))
                return false;

            x2 = x2Temp;
        }

        decimal? z2 = null;
        if (z2Val is not null)
        {
            if (!decimal.TryParse(z2Val, out decimal z2Temp))
                return false;

            z2 = z2Temp;
        }

        decimal? y2 = null;
        if (y2Val is not null)
        {
            if (!decimal.TryParse(y2Val, out decimal y2Temp))
                return false;

            y2 = y2Temp;
        }

        decimal? rotX = null;
        if (rotXVal is not null)
        {
            if (!decimal.TryParse(rotXVal, out decimal rotXTemp))
                return false;

            rotX = rotXTemp;
        }

        decimal? rotY = null;
        if (rotYVal is not null)
        {
            if (!decimal.TryParse(rotYVal, out decimal rotYTemp))
                return false;

            rotY = rotYTemp;
        }

        decimal? rotZ = null;
        if (rotZVal is not null)
        {
            if (!decimal.TryParse(rotZVal, out decimal rotZTemp))
                return false;

            rotZ = rotZTemp;
        }

        decimal? radius = null;
        if (radVal is not null)
        {
            if (!decimal.TryParse(radVal, out decimal radiusTemp))
                return false;

            radius = radiusTemp;
        }

        area = new AreaDefinition
        (
            id,
            nameVal,
            shapeVal,
            x1,
            y1,
            z1,
            x2,
            y2,
            z2,
            rotX,
            rotY,
            rotZ,
            radius,
            0,
            0
        );

        return true;
    }

    private static bool TryParseAreaProperty(XmlReader reader, out uint requirementID, out uint facilityID)
    {
        requirementID = 0;
        facilityID = 0;

        string? reqVal = reader.GetAttribute("Requirement");
        string? facVal = reader.GetAttribute("FacilityId");

        if (!uint.TryParse(reqVal, out requirementID))
            return false;

        if (!uint.TryParse(facVal, out facilityID))
            return false;

        return true;
    }
}
