using System;
using System.IO;
using System.Threading.Tasks;
using Meeemories.Functions.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Meeemories.Functions
{
    public class Upload
    {
        private readonly MediaService _service;
        public Upload(MediaService service)
        {
            _service = service;
        }

        [FunctionName("Upload")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "api/upload")] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string id = data?.id;

            await _service.AddAsync(id);

            return new CreatedResult($"/api/medias/{Uri.EscapeUriString(id)}", null);
        }
    }
}

