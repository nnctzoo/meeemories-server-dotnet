using System.Collections.Generic;
using System.Threading.Tasks;
using Meeemories.Core.Models;

namespace Meeemories.Core.Services
{
    public interface IMediaRepository
    {
        Task<IMedia> FindAsync(string roomId, string id);
        Task<IEnumerable<IMedia>> ListAsync(string roomId, string skipToken);
        Task<IMedia> AddAsync(IMedia media);
        Task RemoveAsync(IMedia media);
        Task UpdateAsync(IMedia media);
    }
}