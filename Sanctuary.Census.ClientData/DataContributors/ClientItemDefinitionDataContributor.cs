using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.Objects;
using Sanctuary.Census.ClientData.Objects.ClientDataModels;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Common.Objects.DtoModels;
using Sanctuary.Census.Common.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.ClientData.DataContributors;

/// <summary>
/// Contributes data from the client <see cref="ClientItemDefinition"/> data source.
/// </summary>
public class ClientItemDefinitionDataContributor : BaseDataContributor<ClientItemDefinition>
{
    private readonly ILocaleService _localeService;
    private readonly Dictionary<uint, ClientItemDefinition> _itemDefs;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientItemDefinitionDataContributor"/> class.
    /// </summary>
    /// <param name="datasheetLoader">The datasheet loader service.</param>
    /// <param name="environment">The environment to source data from.</param>
    /// <param name="localeService">The locale service.</param>
    public ClientItemDefinitionDataContributor
    (
        IDatasheetLoaderService datasheetLoader,
        EnvironmentContextProvider environment,
        ILocaleService localeService
    )
    : base
    (
        datasheetLoader,
        new PackedFileInfo("ClientItemDefinitions.txt", "data_x64_0.pack2"),
        environment
    )
    {
        _localeService = localeService;
        _itemDefs = new Dictionary<uint, ClientItemDefinition>();
    }

    /// <inheritdoc />
    public override bool CanContributeTo<TContributeTo>()
        => typeof(TContributeTo) == typeof(Item);

    /// <inheritdoc />
    public override async ValueTask<IReadOnlyList<uint>> GetContributableIDsAsync<TContributeTo>(CancellationToken ct = default)
    {
        if (typeof(TContributeTo) != typeof(Item))
            throw GetTypeNotSupportedException<TContributeTo>();

        await CacheRecordsAsync(ct).ConfigureAwait(false);
        return _itemDefs.Keys.ToList();
    }

    /// <inheritdoc />
    public override async ValueTask<ContributionResult<TContributeTo>> ContributeAsync<TContributeTo>
    (
        TContributeTo item,
        CancellationToken ct = default
    ) where TContributeTo : class
    {
        if (item is not Item i)
            throw GetTypeNotSupportedException<TContributeTo>();

        await CacheRecordsAsync(ct).ConfigureAwait(false);

        if (!_itemDefs.TryGetValue(i.ItemID, out ClientItemDefinition? definition))
            return new ContributionResult<TContributeTo>(false, item);

        LocaleString? name = definition.NameID == -1
            ? null
            : await _localeService.GetLocaleStringAsync((uint)definition.NameID, ct).ConfigureAwait(false);

        LocaleString? description = definition.DescriptionID == -1
            ? null
            : await _localeService.GetLocaleStringAsync((uint)definition.DescriptionID, ct).ConfigureAwait(false);

        Item contributed = i with
        {
            ItemTypeID = definition.ItemType,
            ItemCategoryID = definition.CategoryID,
            Name = name,
            Description = description,
            ActivatableAbilityID = definition.ActivatableAbilityID == 0 ? null : definition.ActivatableAbilityID,
            PassiveAbilityID = definition.PassiveAbilityID == 0 ? null : definition.PassiveAbilityID,
            PassiveAbilitySetID = definition.PassiveAbilitySetID == 0 ? null : definition.PassiveAbilitySetID,
            SkillSetID = definition.SkillSetID == 0 ? null : definition.SkillSetID,
            ImageSetID = definition.ImageSetID == -1 ? null : (uint)definition.ImageSetID,
            HudImageSetID = definition.HudImageSetID == 0 ? null : definition.HudImageSetID,
            MaxStackSize = definition.MaxStackSize,
            IsAccountScoped = definition.FlagAccountScope
        };

        return new ContributionResult<TContributeTo>
        (
            true,
            (contributed as TContributeTo)!
        );
    }

    /// <inheritdoc />
    protected override void StoreRecords(IEnumerable<ClientItemDefinition> records)
    {
        foreach (ClientItemDefinition itemDef in records)
            _itemDefs[itemDef.ID] = itemDef;
    }
}
