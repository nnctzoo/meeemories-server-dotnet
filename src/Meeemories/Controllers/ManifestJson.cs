using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Meeemories.Controllers
{
    public static class ManifestJson
    {
        [FunctionName("ManifestJson")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "/manifest.json")] HttpRequest req,
            ILogger log)
        {
            var stream = File.OpenRead($"wwwroot/manifest.json");

            return new FileStreamResult(stream, "application/json");
        }
    }
}
