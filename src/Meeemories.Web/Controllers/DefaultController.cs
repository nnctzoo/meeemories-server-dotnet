using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Meeemories.Web.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using Meeemories.Core.Services;
using MediaStatus = Meeemories.Core.Models.MediaStatus;
using MediaType = Meeemories.Core.Models.MediaType;
using Meeemories.Web.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace Meeemories.Web.Controllers
{
    [Authorize]
    public class DefaultController : Controller
    {
        private readonly IMediaRepository _repository;
        private readonly IStorageAccessor _storage;
        public DefaultController(IMediaRepository repository, IStorageAccessor storage)
        {
            _repository = repository;
            _storage = storage;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("~/page")]
        public async Task<IActionResult> Page([FromQuery]string skipToken)
        {
            var medias = await _repository.ListAsync("Default", skipToken);
            var model = medias.Select(media => media.ToViewModel());
            return View(model);
        }

        [HttpGet("~/medias")]
        public async Task<IActionResult> List([FromQuery]string skipToken = null)
        {
            var medias = await _repository.ListAsync("Default", skipToken);
            return Json(medias.Select(media => media.ToViewModel()));
        }

        [HttpGet("~/medias/{id}")]
        public async Task<IActionResult> Lookup(string id)
        {
            var media = await _repository.FindAsync("Default", id);
            
            if (media == null)
                return NotFound();

            return Json(media.ToViewModel());
        }

        [HttpPost("~/medias")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Upload(CloudBlob blob)
        {
            if (blob == null)
                return BadRequest();

            var isImage = blob.Properties.ContentType.StartsWith("image");
            var isMovie = blob.Properties.ContentType.StartsWith("video");
            if (!isImage && !isMovie)
            {
                await blob.DeleteAsync();
                return BadRequest();
            }

            var media = await _repository.AddAsync(new Media
            {
                Status = MediaStatus.Ready,
                MediaType = isMovie ? MediaType.Video : MediaType.Image,
                PostedAt = DateTimeOffset.Now,
                DeleteToken = Guid.NewGuid().ToString(),
                Url = blob.Uri.ToString()
            });

            var convertRequest = new { media.RoomId, media.Id };

            if (isImage)
                await _storage.ImageQueue.AddJsonAsync(convertRequest);
            if (isMovie)
                await _storage.MovieQueue.AddJsonAsync(convertRequest);

            return CreatedAtAction(nameof(Lookup), new { id = media.Id }, new 
            { 
                Url = Url.Action(nameof(Lookup), new { id = media.Id }),
                DeleteToken = media.DeleteToken
            });
        }

        [HttpDelete("~/medias/{id}")]
        public async Task<IActionResult> Delete(string id, [FromQuery]string token)
        {
            var media = await _repository.FindAsync("Default", id);

            if (media == null)
                return NotFound();

            if (media.DeleteToken != token)
                return BadRequest();

            await _repository.RemoveAsync(media);
            
            return Ok();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
