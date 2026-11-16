using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Services;

namespace RaceDay.Web.Controllers
{
    [AllowAnonymous]
    public class EventImagesController : Controller
    {
        private readonly AzureBlobStorageService _blobStorage;

        public EventImagesController(AzureBlobStorageService blobStorage)
        {
            _blobStorage = blobStorage;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName) ||
                Path.GetFileName(blobName) != blobName)
            {
                return BadRequest();
            }

            var image = await _blobStorage.DownloadAsync(blobName);

            if (image == null)
                return NotFound();

            return File(image.Value.Content, image.Value.ContentType);
        }
    }
}
