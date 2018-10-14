using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace Meeemories.Core.Services
{
    public interface IStorageAccessor
    {
        CloudTable Table { get; }
        CloudQueue ImageQueue { get; }
        CloudQueue MovieQueue { get; }
        CloudBlobContainer BlobContainer { get; }

        CloudBlockBlob CreateNewBlobReference();
    }
}