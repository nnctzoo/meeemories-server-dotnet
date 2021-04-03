using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace Meeemories.Functions
{
    public class GenerateSAS
    {
        private readonly Settings _settings;
        public GenerateSAS(IOptions<Settings> options)
        {
            _settings = options.Value;
        }

        [FunctionName("GenerateSAS")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/token")] HttpRequest req,
            [Blob("%Meeemories:ContainerName%/check.txt")]CloudBlockBlob check,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string password = data?.password;

            if (password != _settings.Password)
                return new UnauthorizedResult();

            await check.UploadTextAsync(string.Empty);

            var token = await GetServiceSasUriForBlob();

            return new OkObjectResult(token);
        }

        private async Task<Uri> GetServiceSasUriForBlob()
        {
            var containerClient = new BlobContainerClient(_settings.ConnectionString, _settings.ContainerName);

            try
            {
                await containerClient.CreateIfNotExistsAsync();
            }
            catch (Azure.RequestFailedException ex)
            {
                if (ex.Status != 409)
                    throw;
            }

            if (!containerClient.CanGenerateSasUri)
                throw new NotSupportedException("cannot generate sas token");

            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = containerClient.Name,
                Resource = "c"
            };

            sasBuilder.ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(_settings.ExpiredMunites);
            sasBuilder.SetPermissions(BlobSasPermissions.Create | BlobSasPermissions.Write | BlobSasPermissions.Read);

            return containerClient.GenerateSasUri(sasBuilder);
        }
    }
}

