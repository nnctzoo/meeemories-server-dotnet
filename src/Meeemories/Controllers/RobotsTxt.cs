using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Meeemories.Controllers
{
    public static class RobotsTxt
    {
        [FunctionName("Robots")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "/robots.txt")] HttpRequest req,
            ILogger log)
        {
            var stream = File.OpenRead(StaticFiles.Path($"wwwroot/robots.txt"));

            return new FileStreamResult(stream, "text/plain");
        }
    }
}
