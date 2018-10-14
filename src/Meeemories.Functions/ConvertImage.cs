using ImageMagick;
using Meeemories.Core.Models;
using Meeemories.Core.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace Meeemories.Functions
{
    public static class ConvertImage
    {
        [FunctionName("ConvertImage")]
        public static async Task Run([QueueTrigger("images")]Request req, [Inject]IMediaRepository repo, [Inject]IStorageAccessor storage, ILogger log)
        {
            log.LogInformation($"{req.RoomId}, {req.Id}");
            var media = await repo.FindAsync(req.RoomId, req.Id);
            if (media == null)
            {
                log.LogInformation($"NotFound ({req.RoomId}, {req.Id})");
                return;
            }

            media.Status = MediaStatus.Converting;
            await repo.UpdateAsync(media);

            async Task<Source> Convert(string url, int width)
            {
                var raw = await storage.BlobContainer.ServiceClient.GetBlobReferenceFromServerAsync(new Uri(url));
                var blob = storage.BlobContainer.GetBlockBlobReference($"{raw.Name}_{width:000}w.jpg");
                using (var stream = new MemoryStream())
                {
                    await raw.DownloadToStreamAsync(stream);
                    stream.Position = 0;
                    using (var image = new MagickImage(stream))
                    {
                        var aspect = (double)image.Height / (double)image.Width;
                        var height = (int)(aspect * width);
                        image.Resize(width, height);
                        image.Format = MagickFormat.Jpeg;
                        image.Quality = 85;

                        var binary = image.ToByteArray();
                        await blob.UploadFromByteArrayAsync(binary, 0, binary.Length);
                        blob.Properties.ContentType = "image/jpeg";
                        await blob.SetPropertiesAsync();

                        return new Source
                        {
                            Url = blob.Uri.ToString(),
                            Width = width,
                            Height = height,
                            MimeType = "image/jpeg"
                        };
                    }
                }
            }

            try
            {
                var sources = new List<Source>();
                sources.Add(await Convert(media.Url, 20));
                sources.Add(await Convert(media.Url, 200));
                sources.Add(await Convert(media.Url, 400));
                sources.Add(await Convert(media.Url, 800));

                media.Sources = sources;
                media.Status = MediaStatus.Complete;
                await repo.UpdateAsync(media);
            }
            catch(Exception e)
            {
                log.LogError(e.Message);
                media.Status = MediaStatus.Fail;
                await repo.UpdateAsync(media);
            }
        }
    }
}
