using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Meeemories.Controllers
{
    public static class Fonts
    {
        [FunctionName("Fonts")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "fonts/{file}")] HttpRequest req,
            string file,
            ILogger log)
        {
            var stream = File.OpenRead($"wwwroot/fonts/{file}");

            return new FileStreamResult(stream, "application/octet-stream");
        }
    }
}
