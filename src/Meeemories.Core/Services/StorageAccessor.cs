using System;
using Meeemories.Core.Models;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace Meeemories.Core.Services
{
    public class StorageAccessor : IStorageAccessor
    {
        public StorageAccessor(IOptions<Settings> option)
        {
            var settings = option.Value;
            var account = CloudStorageAccount.Parse(settings.StorageConnectionString);
            Table = account.CreateCloudTableClient().GetTableReference(settings.TableName);
            ImageQueue = account.CreateCloudQueueClient().GetQueueReference(settings.ImageQueneName);
            MovieQueue = account.CreateCloudQueueClient().GetQueueReference(settings.MovieQueneName);
            BlobContainer = account.CreateCloudBlobClient().GetContainerReference(settings.BlobContainerName);
            Table.CreateIfNotExistsAsync().Wait();
            ImageQueue.CreateIfNotExistsAsync().Wait();
            MovieQueue.CreateIfNotExistsAsync().Wait();
            BlobContainer.CreateIfNotExistsAsync().Wait();
        }
        public CloudTable Table { get; set; }
        public CloudBlobContainer BlobContainer { get; set; }

        public CloudQueue ImageQueue { get; set; }

        public CloudQueue MovieQueue { get; set; }

        public CloudBlockBlob CreateNewBlobReference()
            => BlobContainer.GetBlockBlobReference($"{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}");
    }
}