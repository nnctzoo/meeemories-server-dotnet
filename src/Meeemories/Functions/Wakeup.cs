using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Meeemories.Functions
{
    public class Wakeup
    {
        private readonly HttpClient _client;
        private readonly string _endpoint;
        public Wakeup(IHttpClientFactory factory, IOptions<Settings> settings)
        {
            _client = factory.CreateClient();
            _endpoint = settings.Value.WakeupUrl;
        }

        [FunctionName("Wakeup")]
        public async Task Run([TimerTrigger("0 */3 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"wakeup: {DateTime.Now}");

            if (!string.IsNullOrEmpty(_endpoint))
            {
                var response = await _client.GetAsync(_endpoint);
                log.LogInformation(response.StatusCode.ToString());
            }
        }
    }
}
