using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Meeemories.Controllers
{
    public static class Index
    {
        [FunctionName("Index")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "/")] HttpRequest req,
            ILogger log)
        {
            var stream = File.OpenRead($"wwwroot/index.html");

            return new FileStreamResult(stream, "text/html");
        }
    }
}
