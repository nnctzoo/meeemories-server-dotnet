using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Meeemories.Controllers
{
    public static class Favicon
    {
        [FunctionName("Favicon")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "/favicon.ico")] HttpRequest req,
            ILogger log)
        {
            var stream = File.OpenRead($"wwwroot/favicon.ico");

            return new FileStreamResult(stream, "image/vnd.microsoft.icon");
        }
    }
}

