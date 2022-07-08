using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.Objects;
using Sanctuary.Census.Common.Abstractions;
using Sanctuary.Census.Common.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.ClientData.DataContributors;

/// <summary>
/// <inheritdoc />
/// This abstract contributor is designed to source data from a packed datasheet.
/// </summary>
public abstract class BaseDataContributor<TContributeFrom> : IDataContributor<TContributeFrom>
{
    private readonly IDatasheetLoaderService _datasheetLoader;
    private readonly Dictionary<Type, Func<TContributeFrom, uint>> _typeIDBindings;
    private readonly Dictionary<Type, Dictionary<uint, TContributeFrom>> _indexedStore;

    private bool _isCached;

    /// <summary>
    /// The datasheet file from which this contributor should source data.
    /// </summary>
    protected PackedFileInfo SourceDatasheet { get; }

    /// <summary>
    /// The environment that this contributor is sourcing data from.
    /// </summary>
    protected PS2Environment Environment { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseDataContributor{TContributeFrom}"/>
    /// </summary>
    /// <param name="datasheetLoader">The datasheet loader service.</param>
    /// <param name="sourceDatasheet">The datasheet file from which this contributor should source data.</param>
    /// <param name="environment">The environment to source data from.</param>
    /// <param name="typeIDBindings">
    /// A dictionary of methods that will retrieve the ID value for which the given
    /// key type expects data to be contributed.
    /// </param>
    protected BaseDataContributor
    (
        IDatasheetLoaderService datasheetLoader,
        PackedFileInfo sourceDatasheet,
        PS2Environment environment,
        Dictionary<Type, Func<TContributeFrom, uint>> typeIDBindings
    )
    {
        _datasheetLoader = datasheetLoader;
        SourceDatasheet = sourceDatasheet;
        Environment = environment;
        _typeIDBindings = typeIDBindings;

        _indexedStore = new Dictionary<Type, Dictionary<uint, TContributeFrom>>();
        foreach (Type element in _typeIDBindings.Keys)
            _indexedStore.Add(element, new Dictionary<uint, TContributeFrom>());
    }

    /// <inheritdoc />
    public abstract ValueTask ContributeAsync<TContributeTo>(TContributeTo item, CancellationToken ct = default);

    /// <inheritdoc />
    public bool CanContributeTo<TContributeTo>()
        => _typeIDBindings.ContainsKey(typeof(TContributeTo));

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<uint>> GetContributableIDsAsync<TContributeTo>(CancellationToken ct = default)
    {
        await CacheRecordsAsync(ct).ConfigureAwait(false);
        return _indexedStore[typeof(TContributeTo)].Keys.ToList();
    }

    /// <inheritdoc />
    public async ValueTask<TContributeFrom> GetDataAsync(uint id, CancellationToken ct = default)
    {
        await CacheRecordsAsync(ct).ConfigureAwait(false);
        return _indexedStore[typeof(TContributeFrom)][id];
    }

    private async ValueTask CacheRecordsAsync(CancellationToken ct)
    {
        if (_isCached)
            return;

        IEnumerable<TContributeFrom> records = await _datasheetLoader.LoadDatasheetDataAsync<TContributeFrom>
        (
            SourceDatasheet,
            Environment,
            ct
        ).ConfigureAwait(false);

        foreach (TContributeFrom record in records)
        {
            foreach (KeyValuePair<Type, Func<TContributeFrom, uint>> typeIDBinding in _typeIDBindings)
            {
                uint id = typeIDBinding.Value(record);
                _indexedStore[typeIDBinding.Key][id] = record;
            }
        }

        _isCached = true;
    }
}
