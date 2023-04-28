using Sanctuary.Census.Builder.Abstractions.CollectionBuilders;
using Sanctuary.Census.Builder.Abstractions.Database;
using Sanctuary.Census.Builder.Exceptions;
using Sanctuary.Census.ClientData.Abstractions;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.ClientDataModels;
using Sanctuary.Census.Common.Objects.Collections;
using Sanctuary.Common.Objects;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.Builder.CollectionBuilders;

/// <summary>
/// Builds the <see cref="NoDeployArea"/> collection.
/// </summary>
public class AreaCollectionsBuilder : ICollectionBuilder
{
    private readonly IClientDataCacheService _clientDataCache;

    /// <summary>
    /// Initializes a new instance of the <see cref="AreaCollectionsBuilder"/> class.
    /// </summary>
    /// <param name="clientDataCache">The client data cache.</param>
    public AreaCollectionsBuilder(IClientDataCacheService clientDataCache)
    {
        _clientDataCache = clientDataCache;
    }

    /// <inheritdoc />
    public async Task BuildAsync(ICollectionsContext dbContext, CancellationToken ct = default)
    {
        if (_clientDataCache.AreaDefinitions is null)
            throw new MissingCacheDataException(typeof(AreaDefinition));

        Dictionary<uint, NoDeployArea> builtNDZAreas = new();
        foreach ((AssetZone zone, IReadOnlyList<AreaDefinition> areas) in _clientDataCache.AreaDefinitions)
        {
            ZoneDefinition actualZone = AssetZoneToZone(zone);

            foreach (AreaDefinition area in areas)
            {
                builtNDZAreas.TryAdd(area.AreaID, new NoDeployArea
                (
                    area.AreaID,
                    (uint)actualZone,
                    area.Name,
                    AreaRequirementToNDZType(area.RequirementID),
                    area.Shape,
                    area.LocationX1,
                    area.LocationY1,
                    area.LocationZ1,
                    area.Radius,
                    area.LocationX2,
                    area.LocationY2,
                    area.LocationZ2,
                    area.RotationX,
                    area.RotationY,
                    area.RotationZ,
                    area.FacilityID == 0 ? null : area.FacilityID
                ));
            }
        }

        await dbContext.UpsertCollectionAsync(builtNDZAreas.Values, ct).ConfigureAwait(false);
    }

    private static ZoneDefinition AssetZoneToZone(AssetZone assetZone)
        => assetZone switch
        {
            AssetZone.Amerish => ZoneDefinition.Amerish,
            AssetZone.Esamir => ZoneDefinition.Esamir,
            AssetZone.Hossin => ZoneDefinition.Hossin,
            AssetZone.Indar => ZoneDefinition.Indar,
            AssetZone.Nexus => ZoneDefinition.Nexus,
            AssetZone.Oshur => ZoneDefinition.Oshur,
            AssetZone.Quickload => ZoneDefinition.Nexus,
            AssetZone.Sanctuary => ZoneDefinition.Sanctuary,
            AssetZone.Tutorial => ZoneDefinition.Tutorial,
            AssetZone.OutfitWars => ZoneDefinition.Desolation,
            AssetZone.VR => ZoneDefinition.VRTrainingNC,
            AssetZone.VRTutorial2 => ZoneDefinition.Tutorial2,
            _ => throw new Exception("Unsupported asset zone")
        };

    private static string AreaRequirementToNDZType(uint requirementID)
        => requirementID switch
        {
            200 => "Sunderer",
            2336 => "ANT",
            _ => throw new Exception("Unknown deployment requirement ID")
        };
}
