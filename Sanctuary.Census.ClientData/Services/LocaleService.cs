using Mandible.Util;
using Sanctuary.Census.ClientData.Abstractions.Services;
using Sanctuary.Census.ClientData.Objects;
using Sanctuary.Census.Common.Objects;
using Sanctuary.Census.Common.Objects.CommonModels;
using Sanctuary.Census.Common.Services;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sanctuary.Census.ClientData.Services;

/// <inheritdoc />
public class LocaleService : ILocaleService
{
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

    private readonly IManifestService _manifestService;
    private readonly PS2Environment _environment;

    private bool _isCached;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocaleService"/>
    /// </summary>
    /// <param name="manifestService">The manifest service.</param>
    /// <param name="environment">The environment to source data from.</param>
    public LocaleService
    (
        IManifestService manifestService,
        EnvironmentContextProvider environment
    )
    {
        _manifestService = manifestService;
        _environment = environment.Environment;
    }

    /// <inheritdoc />
    public async ValueTask<LocaleString?> GetLocaleStringAsync(uint stringID, CancellationToken ct = default)
    {
        await CacheLocaleAsync(ct).ConfigureAwait(false);
        stringID = Jenkins.GetItemLocaleID(stringID);

        _chinese.TryGetValue(stringID, out string? chinese);
        _english.TryGetValue(stringID, out string? english);
        _french.TryGetValue(stringID, out string? french);
        _german.TryGetValue(stringID, out string? german);
        _italian.TryGetValue(stringID, out string? italian);
        _korean.TryGetValue(stringID, out string? korean);
        _portuguese.TryGetValue(stringID, out string? portuguese);
        _russian.TryGetValue(stringID, out string? russian);
        _spanish.TryGetValue(stringID, out string? spanish);
        _turkish.TryGetValue(stringID, out string? turkish);

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
            return null;

        return new LocaleString
        (
            stringID,
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
    }

    private async ValueTask CacheLocaleAsync(CancellationToken ct)
    {
        if (_isCached)
            return;

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
                _environment,
                ct
            ).ConfigureAwait(false);

            await using Stream fileData = await _manifestService.GetFileDataAsync(manifestFile, ct)
                .ConfigureAwait(false);

            await StoreLocaleDataAsync(fileData, store, ct).ConfigureAwait(false);
        }

        _isCached = true;
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

            string[] components = line.Split('\t');
            if (components.Length < 3)
                continue;

            uint id = uint.Parse(components[0]);
            store.TryAdd(id, components[2]);
        }
    }
}
