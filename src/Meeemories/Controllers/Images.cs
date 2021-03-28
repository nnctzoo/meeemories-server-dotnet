using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Meeemories.Controllers
{
    public static class Images
    {
        [FunctionName("Images")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "img/{file}")] HttpRequest req,
            string file,
            ILogger log)
        {
            var stream = File.OpenRead(StaticFiles.Path($"wwwroot/img/{file}"));

            return new FileStreamResult(stream, "application/octet-stream");
        }
    }
}

