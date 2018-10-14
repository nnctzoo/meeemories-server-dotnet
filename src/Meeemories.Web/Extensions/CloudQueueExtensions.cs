using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Meeemories.Web.Extensions
{
    public static class CloudQueueExtensions
    {
        public static async Task AddJsonAsync(this CloudQueue queue, object payload)
        {
            var json = JsonConvert.SerializeObject(payload);
            var message = new CloudQueueMessage(json);
            await queue.AddMessageAsync(message);
        }
    }
}
