using Mandible.Util;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.Common.Abstractions.Services;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Common.Services;
using Sanctuary.Census.Common.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.ClientData.Services;

/// <inheritdoc />
public class LocaleDataCacheService : ILocaleDataCacheService
{
    private readonly IManifestService _manifestService;
    private readonly EnvironmentContextProvider _environmentContextProvider;

    private readonly Dictionary<uint, string> _chinese = new();
    private readonly Dictionary<uint, string> _english = new();
    private readonly Dictionary<uint, string> _french = new();
    private readonly Dictionary<uint, string> _german = new();
    private readonly Dictionary<uint, string> _italian = new();
    private readonly Dictionary<uint, string> _korean = new();
    private readonly Dictionary<uint, string> _portuguese = new();
    private readonly Dictionary<uint, string> _russian = new();
    private readonly Dictionary<uint, string> _spanish = new();
    private readonly Dictionary<uint, string> _turkish = new();

    /// <inheritdoc />
    public DateTimeOffset LastPopulated { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LocaleDataCacheService"/>
    /// </summary>
    /// <param name="manifestService">The manifest service.</param>
    /// <param name="environmentContextProvider">The environment to source data from.</param>
    public LocaleDataCacheService
    (
        IManifestService manifestService,
        EnvironmentContextProvider environmentContextProvider
    )
    {
        _manifestService = manifestService;
        _environmentContextProvider = environmentContextProvider;
    }

    /// <inheritdoc />
    public bool TryGetLocaleString(long stringID, [NotNullWhen(true)] out LocaleString? localeString)
    {
        localeString = null;
        if (stringID is < 1 or > uint.MaxValue)
            return false;

        uint actualID = Jenkins.GetItemLocaleID((uint)stringID);
        _chinese.TryGetValue(actualID, out string? chinese);
        _english.TryGetValue(actualID, out string? english);
        _french.TryGetValue(actualID, out string? french);
        _german.TryGetValue(actualID, out string? german);
        _italian.TryGetValue(actualID, out string? italian);
        _korean.TryGetValue(actualID, out string? korean);
        _portuguese.TryGetValue(actualID, out string? portuguese);
        _russian.TryGetValue(actualID, out string? russian);
        _spanish.TryGetValue(actualID, out string? spanish);
        _turkish.TryGetValue(actualID, out string? turkish);

        bool isNotValid = chinese is null
                          && english is null
                          && french is null
                          && german is null
                          && italian is null
                          && korean is null
                          && portuguese is null
                          && russian is null
                          && spanish is null
                          && turkish is null;

        if (isNotValid)
            return false;

        localeString = new LocaleString
        (
            german,
            english,
            spanish,
            french,
            italian,
            korean,
            portuguese,
            russian,
            turkish,
            chinese
        );
        return true;
    }

    /// <inheritdoc />
    public async Task RepopulateAsync(CancellationToken ct)
    {
        (string, Dictionary<uint, string>)[] localePacks = {
            ("de_de_data.dat", _german),
            ("en_us_data.dat", _english),
            ("es_es_data.dat", _spanish),
            ("fr_fr_data.dat", _french),
            ("it_it_data.dat", _italian),
            ("ko_kr_data.dat", _korean),
            ("pt_br_data.dat", _portuguese),
            ("ru_ru_data.dat", _russian),
            ("tr_tr_data.dat", _turkish),
            ("zh_cn_data.dat", _chinese)
        };

        foreach ((string packName, Dictionary<uint, string> store) in localePacks)
        {
            ManifestFile manifestFile = await _manifestService.GetFileAsync
            (
                packName,
                _environmentContextProvider.Environment,
                ct
            ).ConfigureAwait(false);

            await using Stream fileData = await _manifestService.GetFileDataAsync(manifestFile, ct)
                .ConfigureAwait(false);

            await StoreLocaleDataAsync(fileData, store, ct).ConfigureAwait(false);
        }

        LastPopulated = DateTimeOffset.UtcNow;
    }

    /// <inheritdoc />
    public void Clear()
    {
        LastPopulated = DateTimeOffset.MinValue;
        _german.Clear();
        _english.Clear();
        _spanish.Clear();
        _french.Clear();
        _italian.Clear();
        _korean.Clear();
        _portuguese.Clear();
        _russian.Clear();
        _turkish.Clear();
        _chinese.Clear();
    }

    private static async Task StoreLocaleDataAsync(Stream fileData, Dictionary<uint, string> store, CancellationToken ct)
    {
        using StreamReader sr = new(fileData);
        while (!sr.EndOfStream)
        {
            ct.ThrowIfCancellationRequested();

            string? line = await sr.ReadLineAsync().ConfigureAwait(false);
            if (string.IsNullOrEmpty(line))
                continue;

            ProcessLine(line, store);
        }
    }

    private static void ProcessLine(string line, Dictionary<uint, string> store)
    {
        SpanReader<char> reader = new(line);

        if (!reader.TryReadTo(out ReadOnlySpan<char> idSpan, '\t'))
            return;

        if (!uint.TryParse(idSpan, out uint id))
            return;

        if (!reader.TryAdvanceTo('\t'))
            return;

        if (!reader.TryReadExact(out ReadOnlySpan<char> value, reader.Remaining))
            return;

        store.TryAdd(id, value.ToString());
    }
}
