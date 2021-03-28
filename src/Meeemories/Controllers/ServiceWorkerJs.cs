using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Meeemories.Controllers
{
    public static class ServiceWorkerJs
    {
        [FunctionName("ServiceWorker")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "/service-worker.js")] HttpRequest req,
            ILogger log)
        {
            var stream = File.OpenRead($"wwwroot/service-worker.js");

            return new FileStreamResult(stream, "text/javascript");
        }
    }
}
