using System.Threading.Tasks;
using Meeemories.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Meeemories.Functions
{
    public class Delete
    {
        private readonly MediaService _service;
        public Delete(MediaService service)
        {
            _service = service;
        }

        [FunctionName("Delete")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "api/delete/{id}")] HttpRequest req,
            string id,
            ILogger log)
        {
            var token = req.Query["token"];

            var media = await _service.FindAsync(id);

            if (media == null)
                return new NotFoundResult();

            if (media.DeleteToken != token)
                return new ForbidResult();

            await _service.RemoveAsync(media);

            return new NoContentResult();
        }
    }
}

