using Microsoft.AspNetCore.Mvc;
using Url2Image;

namespace Url2Image.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScreenshotController(IUrlScreenshotService screenshotService, IWebHostEnvironment environment) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CaptureScreenshot([FromForm] string url)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return BadRequest(new { success = false, error = "Please enter a valid URL." });
            }

            // Ensure URL has protocol
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }

            // Create images directory if it doesn't exist
            var webRootPath = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
            var imagesPath = Path.Combine(webRootPath, "images");
            Directory.CreateDirectory(imagesPath);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}.png";
            var filePath = Path.Combine(imagesPath, fileName);

            // Capture screenshot
            await screenshotService.SaveToFileAsync(url, filePath, new ScreenshotOptions(Format: ImageFormat.Png));

            // Return success with image path
            return Ok(new 
            { 
                success = true, 
                imagePath = $"/images/{fileName}",
                viewUrl = $"/images/{fileName}",
                downloadUrl = $"/images/{fileName}"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = $"Failed to capture screenshot: {ex.Message}" });
        }
    }
}
