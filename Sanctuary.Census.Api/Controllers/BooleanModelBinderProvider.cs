using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Sanctuary.Census.Api.Controllers;

/// <inheritdoc />
public class BooleanModelBinderProvider : IModelBinderProvider
{
    /// <inheritdoc />
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
        => context.Metadata.ModelType == typeof(bool)
            ? new BooleanModelBinder()
            : null;
}
