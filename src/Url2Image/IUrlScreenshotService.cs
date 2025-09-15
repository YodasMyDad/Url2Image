namespace Url2Image;

/// <summary>
/// Service for capturing screenshots of URLs.
/// </summary>
public interface IUrlScreenshotService
{
    /// <summary>
    /// Gets whether the service is initialized and ready to capture screenshots.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Gets whether Playwright browsers are installed on the system.
    /// </summary>
    bool AreBrowsersInstalled { get; }
    /// <summary>
    /// Captures a screenshot of the specified URL and returns it as a Stream.
    /// </summary>
    /// <param name="url">The URL to capture. Must be http or https.</param>
    /// <param name="options">Screenshot options. If null, default options are used.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A Stream containing the screenshot image data.</returns>
    Task<Stream> CaptureAsync(
        string url,
        ScreenshotOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Captures a screenshot of the specified URL and saves it to a file.
    /// </summary>
    /// <param name="url">The URL to capture. Must be http or https.</param>
    /// <param name="filePath">The file path where the screenshot will be saved.</param>
    /// <param name="options">Screenshot options. If null, default options are used.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SaveToFileAsync(
        string url,
        string filePath,
        ScreenshotOptions? options = null,
        CancellationToken ct = default);

    /// <summary>
    /// Captures a screenshot of the specified URL and returns it as a byte array.
    /// </summary>
    /// <param name="url">The URL to capture. Must be http or https.</param>
    /// <param name="options">Screenshot options. If null, default options are used.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A byte array containing the screenshot image data.</returns>
    Task<byte[]> CaptureBytesAsync(
        string url,
        ScreenshotOptions? options = null,
        CancellationToken ct = default);
}
