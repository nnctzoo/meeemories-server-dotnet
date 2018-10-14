using System;
using System.IO;
using System.Threading.Tasks;
using Meeemories.Core.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Meeemories.Web.Binders
{
    public class CloudBlobBinder : IModelBinder
    {
        public readonly IStorageAccessor _storage;

        public CloudBlobBinder(IStorageAccessor storage)
        {
            _storage = storage;
        }

        private static readonly FormOptions _defaultFormOptions = new FormOptions();
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));
            
            return IsMultipartContentType(bindingContext.HttpContext.Request.ContentType)
                    ? MultipartBindModelAsync(bindingContext)
                    : SimpleBindModelAsync(bindingContext);
        }
        public async Task SimpleBindModelAsync(ModelBindingContext bindingContext)
        {
            var request = bindingContext.HttpContext.Request;

            var blob = _storage.CreateNewBlobReference();
            await blob.UploadFromStreamAsync(request.Body);
            blob.Properties.ContentType = request.ContentType;
            await blob.SetPropertiesAsync();

            bindingContext.Result = ModelBindingResult.Success(blob);
        }

        public async Task MultipartBindModelAsync(ModelBindingContext bindingContext)
        {
            var request = bindingContext.HttpContext.Request;

            var blob = _storage.CreateNewBlobReference();

            var boundary = GetBoundary(MediaTypeHeaderValue.Parse(request.ContentType), _defaultFormOptions.MultipartBoundaryLengthLimit);
            var reader = new MultipartReader(boundary, request.Body);
            var section = await reader.ReadNextSectionAsync();
            while (section != null)
            {
                if (ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition))
                {
                    if (contentDisposition.IsFileDisposition())
                    {
                        await blob.UploadFromStreamAsync(section.Body);
                        blob.Properties.ContentType = section.ContentType;
                        await blob.SetPropertiesAsync();
                    }
                }

                section = await reader.ReadNextSectionAsync();
            }

            bindingContext.Result = ModelBindingResult.Success(blob);
        }

        public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary);
            if (!boundary.HasValue)
            {
                throw new InvalidDataException("Missing content-type boundary.");
            }

            if (boundary.Length > lengthLimit)
            {
                throw new InvalidDataException(
                    $"Multipart boundary length limit {lengthLimit} exceeded.");
            }

            return boundary.ToString();
        }

        public static bool IsMultipartContentType(string contentType)
        {
            return !string.IsNullOrEmpty(contentType)
                   && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}