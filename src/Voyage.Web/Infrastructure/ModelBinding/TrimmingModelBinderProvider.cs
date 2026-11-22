using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Voyage.Web.Infrastructure.ModelBinding
{
    public class TrimmingModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            bool isPassword = context.Metadata.DataTypeName == nameof(DataType.Password);

            return context.Metadata.ModelType == typeof(string) && !isPassword
                ? new TrimmingModelBinder()
                : null;
        }
    }
}
