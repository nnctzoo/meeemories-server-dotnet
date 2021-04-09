using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.Options;

namespace Meeemories.Controllers
{
    public class Index
    {
        private readonly Settings _settings;
        private static string _html;
        private static object _lock = new object();
        public Index(IOptions<Settings> options)
        {
            _settings = options.Value;
        }

        [FunctionName("Index")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "/")] HttpRequest req,
            ILogger log)
        {
            req.HttpContext.Response.Headers["Cache-Control"] = "no-cache";
            req.HttpContext.Response.Headers["Expires"] = DateTime.UtcNow.AddMinutes(1).ToString("R");

            var html = GetHtml(StaticFiles.Path($"wwwroot/index.html"));

            return new ContentResult
            {
                StatusCode = 200,
                Content = html,
                ContentType = "text/html"
            };
        }

        public string GetHtml(string path)
        {
            if (!string.IsNullOrEmpty(_html))
                return _html;

            lock (_lock)
            {
                if (!string.IsNullOrEmpty(_html))
                    return _html;

                var html = File.ReadAllText(path);

                _html = html.Replace("$version$", _settings.ContainerName)
                            .Replace("<!--extension-->", _settings.ExtensionHtml??string.Empty);
                
                return _html;
            }
        }
    }
}
