using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Meeemories.Functions
{
    public class GenerateSkyWayToken
    {
        private readonly Settings _settings;
        public GenerateSkyWayToken(IOptions<Settings> options)
        {
            _settings = options.Value;
        }

        [FunctionName("GenerateSkyWayToken")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/skyway")] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string token = data?.token;

            if (string.IsNullOrEmpty(token))
                return new BadRequestResult();

            var uri = new Uri(token.Replace("?", "/check.txt?"));
            var blob = new BlobClient(uri);

            try
            {
                await blob.DownloadAsync();
            }
            catch
            {
                return new UnauthorizedResult();
            }

            var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            var ttl = 3600 * 3;
            var peerId = Guid.NewGuid().ToString("n");
            using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(_settings.SkyWaySecretKey));
            var authToken = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{timestamp}:{ttl}:{peerId}"));

            return new OkObjectResult(new { peerId, timestamp, ttl, authToken });
        }
    }
}

