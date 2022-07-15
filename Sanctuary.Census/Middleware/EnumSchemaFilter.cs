using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace Sanctuary.Census.Middleware;

/// <summary>
/// An Swagger filter to show enums by their name.
/// </summary>
public class EnumSchemaFilter : ISchemaFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiSchema model, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum)
            return;

        model.Enum.Clear();

        foreach (object? value in Enum.GetValues(context.Type))
        {
            model.Enum.Add
            (
                new OpenApiString(Enum.GetName(context.Type, value) + $" ({value:d})")
            );
        }
    }
}
