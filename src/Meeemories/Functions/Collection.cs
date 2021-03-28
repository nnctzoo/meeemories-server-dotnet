using System.Threading.Tasks;
using Meeemories.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Meeemories.Functions
{
    public class Collection
    {
        private readonly MediaService _service;
        public Collection(MediaService service)
        {
            _service = service;
        }

        [FunctionName("Collection")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "api/medias/{id?}")] HttpRequest req,
            string id,
            ILogger log)
        {
            if (string.IsNullOrEmpty(id))
            {
                var skipToken = req.Query["skipToken"];
                var medias = await _service.ListAsync(skipToken);
                return new OkObjectResult(medias);
            }
            else
            {
                var media = await _service.FindAsync(id);
                return new OkObjectResult(media);
            }
        }
    }
}

