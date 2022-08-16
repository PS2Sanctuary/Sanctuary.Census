using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Sanctuary.Census.Common.Objects;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;

namespace Sanctuary.Census.Api.Middleware;

/// <summary>
/// A Swagger filter to show the possible values of an <c>environment</c> query parameter.
/// </summary>
public class EnvironmentParameterFilter : IParameterFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
    {
        if (parameter.Name != "environment")
            return;

        parameter.Schema.Enum = Enum.GetNames<PS2Environment>()
            .Select(e => new OpenApiString(e))
            .ToList<IOpenApiAny>();
    }
}
