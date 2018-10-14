using Meeemories.Core.Models;
using Meeemories.Web.Models;

namespace Meeemories.Web.Extensions
{
    public static class MediaExtensions
    {
        public static MediaViewModel ToViewModel(this IMedia media)
            => new MediaViewModel
            {
                Id = media.Id,
                Status = media.Status,
                PostedAt = media.PostedAt,
                MediaType = media.MediaType,
                Url = media.Url,
                Sources  = media.Sources,
            };
    }
}