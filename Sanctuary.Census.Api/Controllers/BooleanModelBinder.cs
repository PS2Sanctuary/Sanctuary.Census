using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;

namespace Sanctuary.Census.Api.Controllers;

/// <inheritdoc />
public class BooleanModelBinder : IModelBinder
{
    /// <inheritdoc />
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ValueProviderResult result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (result.Length != 1)
        {
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        string? value = result.FirstValue;

        if (value == "0")
            bindingContext.Result = ModelBindingResult.Success(false);
        else if (value == "1")
            bindingContext.Result = ModelBindingResult.Success(true);
        else if (bool.TryParse(value, out bool parsed))
            bindingContext.Result = ModelBindingResult.Success(parsed);
        else
            bindingContext.Result = ModelBindingResult.Failed();

        return Task.CompletedTask;
    }
}
