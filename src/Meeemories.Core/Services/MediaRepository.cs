using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage;
using Meeemories.Core.Extensions;
using Meeemories.Core.Models;

namespace Meeemories.Core.Services
{
    public class MediaRepository : IMediaRepository
    {
        private readonly IStorageAccessor _storage;
        public MediaRepository(IStorageAccessor storage)
        {
            _storage = storage;
        }

        public async Task<IMedia> AddAsync(IMedia media)
        {
            var entity = media.ToEntity();
            var op = TableOperation.Insert(entity);
            await _storage.Table.ExecuteAsync(op);
            return entity;
        }

        public async Task<IMedia> FindAsync(string roomId, string id)
        {
            var op = TableOperation.Retrieve<Media>(roomId, id);
            var response = await _storage.Table.ExecuteAsync(op);
            return response.Result as IMedia;
        }

        public async Task<IEnumerable<IMedia>> ListAsync(string roomId, string skipToken)
        {
            var result = new List<IMedia>();
            var query = new TableQuery<Media>().Where(TableQuery.CombineFilters(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, roomId),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, skipToken)
                    ),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition("StatusStr", QueryComparisons.Equal, MediaStatus.Complete.ToString())
                )).Take(100);
            TableContinuationToken ct = null;
            do
            {
                var response = await _storage.Table.ExecuteQuerySegmentedAsync(query, ct);
                result.AddRange(response.Results);
                ct = response.ContinuationToken;
            } while (ct != null);
            return result;
        }

        public async Task RemoveAsync(IMedia media)
        {
            var entity = media.ToEntity();
            var op = TableOperation.Delete(entity);
            await _storage.Table.ExecuteAsync(op);
        }

        public async Task UpdateAsync(IMedia media)
        {
            var entity = media.ToEntity();
            var op = TableOperation.Replace(entity);
            await _storage.Table.ExecuteAsync(op);
        }
    }
}