using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Url2Image;

namespace Url2Image.Web.Pages;

public class IndexModel(IUrlScreenshotService screenshotService, IWebHostEnvironment environment) : PageModel
{
    [BindProperty]
    public new string Url { get; set; } = string.Empty;

    public string? ScreenshotPath { get; set; }

    public bool AreBrowsersInstalled { get; set; }

    public void OnGet()
    {
        AreBrowsersInstalled = screenshotService.AreBrowsersInstalled;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(Url))
        {
            ModelState.AddModelError(string.Empty, "Please enter a valid URL.");
            AreBrowsersInstalled = screenshotService.AreBrowsersInstalled;
            return Page();
        }

        try
        {
            // Ensure URL has protocol
            if (!Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                Url = "https://" + Url;
            }

            // Create images directory if it doesn't exist
            var imagesPath = Path.Combine(environment.WebRootPath, "images");
            Directory.CreateDirectory(imagesPath);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}.png";
            var filePath = Path.Combine(imagesPath, fileName);

            // Capture screenshot
            await screenshotService.SaveToFileAsync(Url, filePath, new ScreenshotOptions(Format: ImageFormat.Png));

            // Set the relative path for display
            ScreenshotPath = $"/images/{fileName}";
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Failed to capture screenshot: {ex.Message}");
        }

        AreBrowsersInstalled = screenshotService.AreBrowsersInstalled;
        return Page();
    }
}
