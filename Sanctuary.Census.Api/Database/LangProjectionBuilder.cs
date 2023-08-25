using Sanctuary.Census.Api.Util;
using Sanctuary.Census.Common.Json;
using Sanctuary.Census.Common.Objects.CommonModels;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sanctuary.Census.Api.Database;

/// <summary>
/// Stores data related to the query lang command, and appends
/// relevant exclude projections to a <see cref="ProjectionBuilder"/>.
/// </summary>
public class LangProjectionBuilder
{
    private static readonly List<string> AllLangCodes;

    private readonly IReadOnlyList<string> _includeLangCodes;

    static LangProjectionBuilder()
    {
        AllLangCodes = new List<string>();
        foreach (PropertyInfo prop in typeof(LocaleString).GetProperties())
            AllLangCodes.Add(SnakeCaseJsonNamingPolicy.Default.ConvertName(prop.Name));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LangProjectionBuilder"/> class.
    /// </summary>
    /// <param name="includeLangCodes">The language codes to include in any projections.</param>
    public LangProjectionBuilder(IReadOnlyList<string> includeLangCodes)
    {
        _includeLangCodes = includeLangCodes;
    }

    /// <summary>
    /// Excludes unwanted language codes from the given projection builder.
    /// </summary>
    /// <param name="builder">The projection builder.</param>
    /// <param name="collectionName">The collection to exclude relevant locale languages from.</param>
    public void AppendToProjection(ProjectionBuilder builder, string collectionName)
    {
        IReadOnlyList<string> localeFields = CollectionUtils.GetLocaleFields(collectionName);
        foreach (string localeField in localeFields)
        {
            if (builder.IsExclusionProjection)
            {
                if (builder.ContainsProjection(localeField))
                    continue;

                foreach (string code in AllLangCodes)
                {
                    if (!_includeLangCodes.Contains(code))
                        builder.Project($"{localeField}.{code}");
                }
            }
            else
            {
                if (!builder.ContainsProjection(localeField))
                    continue;
                builder.RemoveProjection(localeField);

                foreach (string code in _includeLangCodes)
                    builder.Project($"{localeField}.{code}");
            }
        }
    }
}
