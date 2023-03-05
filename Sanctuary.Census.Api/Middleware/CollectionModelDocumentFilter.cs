using Microsoft.OpenApi.Models;
using Sanctuary.Census.Common.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Sanctuary.Census.Api.Middleware;

/// <summary>
/// Registers collection models as swagger documents.
/// </summary>
public class CollectionModelDocumentFilter : IDocumentFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        IEnumerable<Type> collTypes = typeof(CollectionAttribute).Assembly
            .GetTypes()
            .Where
            (
                t =>
                {
                    CollectionAttribute? collAttr = t.GetCustomAttribute<CollectionAttribute>();
                    return collAttr is { IsHidden: false, IsNestedType: false };
                }
            );

        foreach (Type collType in collTypes)
            context.SchemaGenerator.GenerateSchema(collType, context.SchemaRepository);
    }
}
