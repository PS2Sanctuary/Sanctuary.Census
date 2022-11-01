using Sanctuary.Census.ClientData.Attributes;

namespace Sanctuary.Census.ClientData.ClientDataModels;

/// <summary>
/// Represents client ability data.
/// </summary>
/// <param name="TypeName"></param>
/// <param name="Id"></param>
/// <param name="NameId"></param>
/// <param name="DescriptionId"></param>
/// <param name="IconId"></param>
/// <param name="AbilityClassId"></param>
/// <param name="ExpireMsec"></param>
/// <param name="FirstUseDelayMsec"></param>
/// <param name="NextUseDelayMsec"></param>
/// <param name="ReuseDelayMsec"></param>
/// <param name="ResourceType"></param>
/// <param name="ResourceThreshold"></param>
/// <param name="ResourceFirstCost"></param>
/// <param name="ResourceCostPerMsec"></param>
/// <param name="ReqSetId"></param>
/// <param name="TargetReqSetId"></param>
/// <param name="ClientReqSetId"></param>
/// <param name="ClientTargetReqSetId"></param>
/// <param name="FlagRunOnClient"></param>
/// <param name="FlagRunOnServer"></param>
/// <param name="FlagUseTargetResource"></param>
/// <param name="FlagToggle"></param>
/// <param name="FlagGroupScope"></param>
/// <param name="FlagVehicleSpawnScope"></param>
/// <param name="FlagVehicleMountScope"></param>
/// <param name="FlagNotVehMountScope"></param>
/// <param name="FlagDeathScope"></param>
/// <param name="FlagNotDeathScope"></param>
/// <param name="FlagEquipScope"></param>
/// <param name="FlagNotUnequipScope"></param>
/// <param name="FlagSpawnScope"></param>
/// <param name="FlagReviveScope"></param>
/// <param name="FlagRumbleSeatScope"></param>
/// <param name="CannotStartInFireBlock"></param>
/// <param name="DistanceMax"></param>
/// <param name="RadiusMax"></param>
/// <param name="Param1"></param>
/// <param name="Param2"></param>
/// <param name="Param3"></param>
/// <param name="Param4"></param>
/// <param name="Param5"></param>
/// <param name="Param6"></param>
/// <param name="Param7"></param>
/// <param name="Param8"></param>
/// <param name="Param9"></param>
/// <param name="Param10"></param>
/// <param name="Param11"></param>
/// <param name="Param12"></param>
/// <param name="Param13"></param>
/// <param name="Param14"></param>
/// <param name="String1"></param>
/// <param name="String2"></param>
/// <param name="String3"></param>
/// <param name="String4"></param>
/// <param name="FlagSurvivesProfileSwap"></param>
/// <param name="ReapplyAbility"></param>
/// <param name="IgnoreHeight"></param>
/// <param name="ResourceBurnInVr"></param>
/// <param name="UnderwaterScope"></param>
/// <param name="NotUnderwaterScope"></param>
/// <param name="UseWeaponCharge"></param>
[Datasheet]
public partial record AbilityEx
(
    string TypeName,
    uint Id,
    uint NameId,
    uint DescriptionId,
    uint IconId,
    uint AbilityClassId,
    uint ExpireMsec,
    uint FirstUseDelayMsec,
    uint NextUseDelayMsec,
    uint ReuseDelayMsec,
    uint ResourceType,
    uint ResourceThreshold,
    uint ResourceFirstCost,
    uint ResourceCostPerMsec,
    uint ReqSetId,
    uint TargetReqSetId,
    uint ClientReqSetId,
    uint ClientTargetReqSetId,
    bool FlagRunOnClient,
    bool FlagRunOnServer,
    bool FlagUseTargetResource,
    bool FlagToggle,
    bool FlagGroupScope,
    bool FlagVehicleSpawnScope,
    bool FlagVehicleMountScope,
    bool FlagNotVehMountScope,
    bool FlagDeathScope,
    bool FlagNotDeathScope,
    bool FlagEquipScope,
    bool FlagNotUnequipScope,
    bool FlagSpawnScope,
    bool FlagReviveScope,
    bool FlagRumbleSeatScope,
    bool CannotStartInFireBlock,
    uint DistanceMax,
    uint RadiusMax,
    uint Param1,
    uint Param2,
    uint Param3,
    uint Param4,
    uint Param5,
    uint Param6,
    uint Param7,
    uint Param8,
    uint Param9,
    uint Param10,
    uint Param11,
    uint Param12,
    uint Param13,
    uint Param14,
    string String1,
    string String2,
    string String3,
    string String4,
    bool FlagSurvivesProfileSwap,
    bool ReapplyAbility,
    bool IgnoreHeight,
    bool ResourceBurnInVr,
    bool UnderwaterScope,
    bool NotUnderwaterScope,
    bool UseWeaponCharge
);
