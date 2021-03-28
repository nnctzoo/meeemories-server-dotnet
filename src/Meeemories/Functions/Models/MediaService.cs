using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Meeemories.Functions.Models
{
    public class MediaService
    {
        const string ROW_KEY = "MEDIA";
        private readonly CloudBlobContainer _container;
        private readonly CloudTable _table;
        private readonly CloudQueue _imageQueue;
        private readonly CloudQueue _videoQueue;
        public MediaService(IOptions<Settings> options)
        {
            var settings = options.Value;
            var account = CloudStorageAccount.Parse(settings.ConnectionString);
            _container = account.CreateCloudBlobClient().GetContainerReference(settings.ContainerName);
            _table = account.CreateCloudTableClient().GetTableReference(settings.ContainerName);
            _imageQueue = account.CreateCloudQueueClient().GetQueueReference("image-" + settings.ContainerName);
            _videoQueue = account.CreateCloudQueueClient().GetQueueReference("video-" + settings.ContainerName);
            _container.CreateIfNotExistsAsync().Wait();
            _table.CreateIfNotExistsAsync().Wait();
            _imageQueue.CreateIfNotExistsAsync().Wait();
            _videoQueue.CreateIfNotExistsAsync().Wait();
        }

        public async Task<Media> AddAsync(string id)
        {
            var blob = _container.GetBlockBlobReference(id);

            await blob.FetchAttributesAsync();

            var isImage = blob.Properties.ContentType?.StartsWith("image") ?? throw new ArgumentException("no content type");
            var isVideo = blob.Properties.ContentType?.StartsWith("video") ?? throw new ArgumentException("no content type");

            var media = new Media
            {
                PartitionKey = id,
                RowKey = ROW_KEY,
                MediaType = isImage ? MediaType.Image : isVideo ? MediaType.Video : throw new ArgumentException("invalid media type"),
                Status = MediaStatus.Ready,
                Url = _container.GetBlockBlobReference(id).Uri.ToString(),
                PostedAt = DateTimeOffset.Now,
                DeleteToken = Guid.NewGuid().ToString(),
            };

            var op = TableOperation.Insert(media);
            await _table.ExecuteAsync(op);

            var next = new CloudQueueMessage(media.PartitionKey);

            if (media.MediaType == MediaType.Image)
                await _imageQueue.AddMessageAsync(next);

            if (media.MediaType == MediaType.Video)
                await _videoQueue.AddMessageAsync(next);

            return media;
        }

        public async Task<Media> FindAsync(string id)
        {
            var op = TableOperation.Retrieve<Media>(id, ROW_KEY);
            var response = await _table.ExecuteAsync(op);
            return response.Result as Media;
        }

        public async Task<IEnumerable<Media>> ListAsync(string skipToken)
        {
            var result = new List<Media>();
            var query = new TableQuery<Media>().Where(TableQuery.CombineFilters(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan, skipToken),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, ROW_KEY)
                    ),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("StatusStr", QueryComparisons.Equal, MediaStatus.Complete.ToString())
                )).Take(100);
            TableContinuationToken ct = null;
            do
            {
                var response = await _table.ExecuteQuerySegmentedAsync(query, ct);
                result.AddRange(response.Results);
                ct = response.ContinuationToken;
            } while (ct != null);
            return result;
        }

        public async Task RemoveAsync(Media media)
        {
            var op = TableOperation.Delete(media);
            await _table.ExecuteAsync(op);
        }

        public async Task UpdateAsync(Media media)
        {
            var op = TableOperation.Replace(media);
            await _table.ExecuteAsync(op);
        }

        public CloudBlockBlob OpenBlob(Media media)
        {
            return _container.GetBlockBlobReference(media.Id);
        }
        public CloudBlockBlob CreateBlob(string name)
        {
            return _container.GetBlockBlobReference(name);
        }
    }
}
