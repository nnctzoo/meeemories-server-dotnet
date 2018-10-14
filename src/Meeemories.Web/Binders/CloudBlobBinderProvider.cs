using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Meeemories.Web.Binders
{
    public class CloudBlobBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(CloudBlob))
            {
                return new BinderTypeModelBinder(typeof(CloudBlobBinder));
            }

            return null;
        }
    }
}