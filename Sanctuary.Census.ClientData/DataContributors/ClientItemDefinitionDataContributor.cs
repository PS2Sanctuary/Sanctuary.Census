using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.Objects;
using Sanctuary.Census.ClientData.Objects.ClientDataModels;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Common.Objects.DtoModels;
using Sanctuary.Census.Common.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.ClientData.DataContributors;

/// <summary>
/// Contributes data from the client <see cref="ClientItemDefinition"/> data source.
/// </summary>
public class ClientItemDefinitionDataContributor : BaseDataContributor<ClientItemDefinition>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClientItemDefinitionDataContributor"/> class.
    /// </summary>
    /// <param name="datasheetLoader">The datasheet loader service.</param>
    /// <param name="environment">The environment to source data from.</param>
    public ClientItemDefinitionDataContributor
    (
        IDatasheetLoaderService datasheetLoader,
        EnvironmentContextProvider environment
    )
    : base
    (
        datasheetLoader,
        new PackedFileInfo("ClientItemDefinitions.txt", "data_x64_0.pack2"),
        environment.Environment,
        new Dictionary<Type, Func<ClientItemDefinition, uint>> {
            { typeof(Item), cid => cid.ID }
        }
    )
    {
    }

    /// <inheritdoc />
    public override async ValueTask<ContributionResult<TContributeTo>> ContributeAsync<TContributeTo>
    (
        TContributeTo item,
        CancellationToken ct = default
    ) where TContributeTo : class
    {
        await CacheRecordsAsync(ct).ConfigureAwait(false);

        if (item is not Item i)
            throw GetTypeNotSupportedException<TContributeTo>();

        if (!IndexedStore[typeof(TContributeTo)].TryGetValue(i.ItemID, out ClientItemDefinition? definition))
            return new ContributionResult<TContributeTo>(false, item);

        Item contributed = i with
        {
            ItemTypeID = definition.ItemType,
            ItemCategoryID = definition.CategoryID,
            Name = definition.NameID == -1 ? null : LocaleString.Default, // TODO
            Description = definition.DescriptionID == -1 ? null : LocaleString.Default,
            ActivatableAbilityID = definition.ActivatableAbilityID == 0 ? null : definition.ActivatableAbilityID,
            PassiveAbilityID = definition.PassiveAbilityID == 0 ? null : definition.PassiveAbilityID,
            PassiveAbilitySetID = definition.PassiveAbilitySetID == 0 ? null : definition.PassiveAbilitySetID,
            SkillSetID = definition.SkillSetID == 0 ? null : definition.SkillSetID,
            ImageSetID = definition.ImageSetID == -1 ? null : (uint)definition.ImageSetID,
            ImageID = definition.ImageSetID == -1 ? null : 0, // TODO
            ImagePath = definition.ImageSetID == -1 ? null : string.Empty, // TODO,
            HudImageSetID = definition.HudImageSetID == 0 ? null : definition.HudImageSetID,
            MaxStackSize = definition.MaxStackSize,
            IsAccountScoped = definition.FlagAccountScope,
            IsVehicleWeapon = false // TODO
        };

        return new ContributionResult<TContributeTo>
        (
            true,
            (contributed as TContributeTo)!
        );
    }
}
