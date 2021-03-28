using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Meeemories.Functions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ImageMagick;

namespace Meeemories.Functions
{
    public class ResizeImage
    {
        private readonly MediaService _service;
        public ResizeImage(MediaService service)
        {
            _service = service;
        }

        [FunctionName("ResizeImage")]
        public async Task Run([QueueTrigger("image-%Meeemories:ContainerName%")]string id, ILogger log)
        {
            var media = await _service.FindAsync(id);

            if (media == null)
            {
                log.LogInformation($"NotFound {id})");
                return;
            }

            media.Status = MediaStatus.Converting;
            await _service.UpdateAsync(media);

            async Task<MediaSource> Convert(Media media, int width)
            {
                var raw = _service.OpenBlob(media);
                var blob = _service.CreateBlob($"{raw.Name}.{width:000}w.jpg");
                using (var stream = new MemoryStream())
                {
                    await raw.DownloadToStreamAsync(stream);
                    stream.Position = 0;
                    using (var image = new MagickImage(stream))
                    {
                        var aspect = (double)image.Height / image.Width;
                        var height = (int)(aspect * width);
                        image.Resize(width, height);
                        image.Format = MagickFormat.Jpeg;
                        image.Quality = 85;

                        var binary = image.ToByteArray();
                        await blob.UploadFromByteArrayAsync(binary, 0, binary.Length);
                        blob.Properties.ContentType = "image/jpeg";
                        await blob.SetPropertiesAsync();

                        return new MediaSource
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
                var sources = new List<MediaSource>();
                sources.Add(await Convert(media, 20));
                sources.Add(await Convert(media, 200));
                sources.Add(await Convert(media, 400));
                sources.Add(await Convert(media, 800));

                media.Sources = sources;
                media.Status = MediaStatus.Complete;
                await _service.UpdateAsync(media);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                media.Status = MediaStatus.Fail;
                await _service.UpdateAsync(media);
            }
        }
    }
}
