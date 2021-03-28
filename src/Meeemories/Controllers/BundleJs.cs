using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Meeemories.Controllers
{
    public static class BundleJs
    {
        [FunctionName("BundleJs")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "bundle.js")] HttpRequest req,
            ILogger log)
        {
            var stream = File.OpenRead($"wwwroot/bundle.js");

            return new FileStreamResult(stream, "text/javascript");
        }
    }
}
