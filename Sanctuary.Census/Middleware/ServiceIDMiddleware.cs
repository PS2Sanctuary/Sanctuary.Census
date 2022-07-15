using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Sanctuary.Census.Middleware;

/// <summary>
/// Middleware for handling service ID route parameters.
/// </summary>
public class ServiceIDMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Creates a new instance of <see cref="ServiceIDMiddleware"/>.
    /// </summary>
    /// <param name="next">The delegate representing the next middleware in the request pipeline.</param>
    public ServiceIDMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    /// <summary>
    /// Request handling method.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A <see cref="Task"/> that represents the execution of this middleware.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (!context.Request.Path.HasValue)
        {
            await _next(context);
            return;
        }

        string pathValue = context.Request.Path.Value;
        if (pathValue.StartsWith("/s:"))
        {
            int serviceIdEndIndex = pathValue.IndexOf('/', 1);
            if (serviceIdEndIndex < 0)
            {
                context.Request.PathBase.Add(context.Request.Path);
                context.Request.Path = string.Empty;
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            context.Request.PathBase.Add(pathValue[..serviceIdEndIndex]);
            context.Request.Path = new PathString(pathValue[serviceIdEndIndex..]);
        }

        await _next(context);
    }
}
