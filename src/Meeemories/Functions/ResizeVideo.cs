using System.IO;
using System.Threading.Tasks;
using Meeemories.Functions.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using FFmpeg.NET;
using System;
using ImageMagick;
using System.Collections.Generic;

namespace Meeemories.Functions
{
    public class ResizeVideo
    {
        private readonly MediaService _service;
        public ResizeVideo(MediaService service)
        {
            _service = service;
        }

        [FunctionName("ResizeVideo")]
        [Singleton]
        public async Task Run([QueueTrigger("video-%Meeemories:ContainerName%")]string id, ILogger log)
        {
            var media = await _service.FindAsync(id);
            if (media == null)
            {
                log.LogInformation($"NotFound {id}");
                return;
            }

            media.Status = MediaStatus.Converting;
            await _service.UpdateAsync(media);

            var rawPath = Path.GetTempFileName();
            var jpgPath = $"{rawPath}.jpg";
            var thumbPath = $"{rawPath}_thumb.jpg";
            var resizedPath = $"{rawPath}_resized.mp4";

            try
            {
                var raw = _service.OpenBlob(media);
                await raw.DownloadToFileAsync(rawPath, FileMode.OpenOrCreate);

                var ffmpeg = new Engine(StaticFiles.Path("ffmpeg.exe"));
                var inputFile = new MediaFile(rawPath);
                var jpgFile = new MediaFile(jpgPath);
                var thumbFile = new MediaFile(thumbPath);
                var resizedFile = new MediaFile(resizedPath);

                await ffmpeg.GetThumbnailAsync(inputFile, jpgFile, new ConversionOptions
                {
                    Seek = TimeSpan.FromMilliseconds(10)
                });

                double aspect;
                using (var image = new MagickImage(jpgPath))
                {
                    aspect = (double)image.Height / image.Width;
                }

                int width = 400, height = (int)(aspect * width);

                if (height % 2 == 1) height += 1;

                await ffmpeg.ConvertAsync(inputFile, resizedFile, new ConversionOptions
                {
                    CustomWidth = width,
                    CustomHeight = height,
                    VideoSize = FFmpeg.NET.Enums.VideoSize.Custom,
                });

                await ffmpeg.GetThumbnailAsync(inputFile, thumbFile, new ConversionOptions
                {
                    Seek = TimeSpan.FromSeconds(3)
                });

                var sources = new List<MediaSource>();

                var thumbBlob = _service.CreateBlob($"{width}w/{raw.Name}.jpg");
                var resizedBlob = _service.CreateBlob($"{width}w/{raw.Name}.mp4");

                await thumbBlob.UploadFromFileAsync(thumbPath);
                thumbBlob.Properties.ContentType = "image/jpeg";
                await thumbBlob.SetPropertiesAsync();

                sources.Add(new MediaSource
                {
                    Url = thumbBlob.Uri.ToString(),
                    Width = width,
                    Height = height,
                    MimeType = "image/jpeg"
                });

                await resizedBlob.UploadFromFileAsync(resizedPath);
                resizedBlob.Properties.ContentType = "video/mp4";
                await resizedBlob.SetPropertiesAsync();

                sources.Add(new MediaSource
                {
                    Url = resizedBlob.Uri.ToString(),
                    Width = width,
                    Height = height,
                    MimeType = "video/mp4"
                });

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
            finally
            {
                if (File.Exists(rawPath))
                    File.Delete(rawPath);

                if (File.Exists(jpgPath))
                    File.Delete(jpgPath);

                if (File.Exists(thumbPath))
                    File.Delete(thumbPath);

                if (File.Exists(resizedPath))
                    File.Delete(resizedPath);
            }
        }
    }
}
