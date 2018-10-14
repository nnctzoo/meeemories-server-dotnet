using System;
using Meeemories.Core.Models;

namespace Meeemories.Core.Extensions
{
    public static class MediaExtensions
    {
        private readonly static Random _random = new Random();
        public static Media ToEntity(this IMedia media)
            => new Media
            {
                ETag = "*",
                PartitionKey = media.RoomId,
                RowKey = media.Id ?? $"{(long.MaxValue - DateTimeOffset.Now.Ticks)}{_random.Next(100):000}",
                DeleteToken = media.DeleteToken ?? Guid.NewGuid().ToString(),
                
                Status = media.Status,
                PostedAt = media.PostedAt,
                MediaType = media.MediaType,
                Url = media.Url,
                Sources = media.Sources,
            };
    }
}