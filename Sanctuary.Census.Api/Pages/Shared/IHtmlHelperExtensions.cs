using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace Sanctuary.Census.Api.Pages.Shared;

/// <summary>
/// Contains extension methods for the <see cref="IHtmlHelper{TModel}"/> class.
/// </summary>
public static class IHtmlHelperExtensions
{
    /// <summary>
    /// Initializes the local date display feature.
    /// </summary>
    /// <param name="htmlHelper">The HTML helper.</param>
    /// <returns>A <see cref="HtmlString"/> containing the initialization script.</returns>
    public static HtmlString InitializeLocalDateDisplay(this IHtmlHelper htmlHelper)
        => new
        (
            """
            <script type="text/javascript">
                (function() {
                    'use strict';
                    const elements = document.getElementsByTagName('time');
                    for (const element of elements) {
                        const timestamp = element.innerHTML;
                        element.innerHTML = new Date(parseInt(timestamp)).toLocaleString();
                    }
                })();
            </script>
            """
        );

    /// <summary>
    /// Generates an HTML tag that will be replaced with a datetime
    /// represented in the user's local timezone.
    /// </summary>
    /// <param name="htmlHelper">The HTML helper.</param>
    /// <param name="dateTime">The datetime to display.</param>
    /// <returns>The generated HTML string.</returns>
    public static HtmlString LocalDateDisplay(this IHtmlHelper htmlHelper, DateTime? dateTime)
        => dateTime is null
            ? HtmlString.Empty
            : new HtmlString($"<time>{new DateTimeOffset(dateTime.Value).ToUnixTimeMilliseconds()}</time>");

    /// <summary>
    /// Generates an HTML tag that will be replaced with a datetime
    /// represented in the user's local timezone.
    /// </summary>
    /// <param name="htmlHelper">The HTML helper.</param>
    /// <param name="dateTime">The datetime to display.</param>
    /// <returns>The generated HTML string.</returns>
    public static HtmlString LocalDateDisplay(this IHtmlHelper htmlHelper, DateTimeOffset? dateTime)
        => dateTime is null
            ? HtmlString.Empty
            : new HtmlString($"<time>{dateTime.Value.ToUnixTimeMilliseconds()}</time>");
}
