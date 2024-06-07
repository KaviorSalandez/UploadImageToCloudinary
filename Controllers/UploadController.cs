using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UploadImageToCloudinary.Service;

namespace UploadImageToCloudinary.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly CloudinaryService _cloudinaryService;

        public UploadController(CloudinaryService cloudinaryService)
        {
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage( IFormFile file, [FromForm] string folderName)
        {
            var uploadResult = await _cloudinaryService.UploadImageAsync(file, folderName);

            if (uploadResult.Error != null)
            {
                return BadRequest(new { error = uploadResult.Error.Message });
            }

            return Ok(new
            {
                url = uploadResult.SecureUrl.ToString(),
                publicId = uploadResult.PublicId
            });
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllImages()
        {
            var images = await _cloudinaryService.GetAllImagesAsync();
            return Ok(images);
        }

        [HttpGet("{publicId}")]
        public async Task<IActionResult> GetImageByKey(string publicId)
        {
            // Combine folder and publicId to get the full publicId with folder path
            var imageUrl = await _cloudinaryService.GetImageByKeyAsync(publicId);

            if (imageUrl == null)
            {
                return NotFound(new { message = "Image not found" });
            }

            return Ok(new { url = imageUrl });
        }
        [HttpDelete("{publicId}")]
        public async Task<IActionResult> DeleteImage(string publicId)
        {
            var deletionResult = await _cloudinaryService.DeleteImageAsync(publicId);

            if (deletionResult.Result == "ok")
            {
                return Ok(new { message = "Image deleted successfully" });
            }
            else
            {
                return BadRequest(new { error = "Failed to delete image" });
            }
        }
    }
}
