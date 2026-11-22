using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Voyage.Web.Infrastructure.ModelBinding
{
    public class TrimmingModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext);

            var valueResult = bindingContext.ValueProvider
                .GetValue(bindingContext.ModelName);

            if (valueResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(
                bindingContext.ModelName,
                valueResult);

            var value = valueResult.FirstValue?.Trim();

            bindingContext.Result = ModelBindingResult.Success(
                string.IsNullOrEmpty(value) ? null : value);

            return Task.CompletedTask;
        }
    }
}
