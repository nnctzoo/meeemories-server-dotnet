using FFmpeg.NET;
using FFmpeg.NET.Engine;
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
    public static class ConvertMovie
    {
        [FunctionName("ConvertMovie")]
        public static async Task Run([QueueTrigger("movies")]Request req, [Inject]IMediaRepository repo, [Inject]IStorageAccessor storage, ILogger log)
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

            var rawPath = Path.GetTempFileName();
            var jpgPah = $"{rawPath}.jpg";
            var thumbPah = $"{rawPath}_thumb.jpg";
            var resizedPath = $"{rawPath}_resized.mp4";

            try
            {
                var raw = await storage.BlobContainer.ServiceClient.GetBlobReferenceFromServerAsync(new Uri(media.Url));
                await raw.DownloadToFileAsync(rawPath, FileMode.OpenOrCreate);

                var ffmpeg = new FFmpeg.NET.Engine.FFmpeg();
                var inputFile = new MediaFile(rawPath);
                var jpgFile = new MediaFile(jpgPah);
                var thumbFile = new MediaFile(thumbPah);
                var resizedFile = new MediaFile(resizedPath);

                ffmpeg.GetThumbnail(inputFile, jpgFile, new ConversionOptions
                {
                    Seek = TimeSpan.FromMilliseconds(10)
                });

                double aspect;
                using (var image = new MagickImage(jpgPah))
                {
                    aspect = (double)image.Height / (double)image.Width;
                }

                int width = 400, height = (int)(aspect * width);

                await ffmpeg.ConvertAsync(inputFile, resizedFile, new ConversionOptions
                {
                    CustomWidth = width,
                    CustomHeight = height,
                    VideoSize = FFmpeg.NET.Enums.VideoSize.Custom
                });
                
                ffmpeg.GetThumbnail(inputFile, thumbFile, new ConversionOptions
                {
                    Seek = TimeSpan.FromSeconds(3)
                });

                var sources = new List<Source>();

                var thumbBlob = storage.BlobContainer.GetBlockBlobReference($"{raw.Name}_thumb.jpg");
                var resizedBlob = storage.BlobContainer.GetBlockBlobReference($"{raw.Name}_resized.mp4");

                await thumbBlob.UploadFromFileAsync(thumbPah);
                thumbBlob.Properties.ContentType = "image/jpeg";
                await thumbBlob.SetPropertiesAsync();

                sources.Add(new Source
                {
                    Url = thumbBlob.Uri.ToString(),
                    Width = width,
                    Height = height,
                    MimeType = "image/jpeg"
                });

                await resizedBlob.UploadFromFileAsync(resizedPath);
                resizedBlob.Properties.ContentType = "video/mp4";
                await resizedBlob.SetPropertiesAsync();

                sources.Add(new Source
                {
                    Url = resizedBlob.Uri.ToString(),
                    Width = width,
                    Height = height,
                    MimeType = "video/mp4"
                });

                media.Sources = sources;
                media.Status = MediaStatus.Complete;
                await repo.UpdateAsync(media);
            }
            catch (Exception e)
            {
                log.LogError(e.Message);
                media.Status = MediaStatus.Fail;
                await repo.UpdateAsync(media);
            }
            finally
            {
                if (File.Exists(rawPath))
                    File.Delete(rawPath);

                if (File.Exists(jpgPah))
                    File.Delete(jpgPah);

                if (File.Exists(thumbPah))
                    File.Delete(thumbPah);

                if (File.Exists(resizedPath))
                    File.Delete(resizedPath);
            }
        }
    }
}
