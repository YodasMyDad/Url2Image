namespace Url2Image;

/// <summary>
/// Options for configuring screenshot capture.
/// </summary>
/// <param name="Format">Image format. Default is PNG.</param>
/// <param name="JpegQuality">JPEG quality (1-100). Only used when Format is Jpeg. Default is 80.</param>
/// <param name="ViewportWidth">Viewport width in pixels. Default is 1400.</param>
/// <param name="ViewportHeight">Viewport height in pixels. Default is null (unlimited for full-page).</param>
/// <param name="DeviceScaleFactor">Device scale factor. Default is 1.0.</param>
/// <param name="NavigationTimeout">Navigation timeout. Default is 30 seconds.</param>
/// <param name="WaitUntil">When to take the screenshot after navigation. Default is DomContentLoaded.</param>
/// <param name="FullPage">Whether to capture the full page. Default is true.</param>
/// <param name="DisableAnimations">Whether to disable animations and transitions. Default is true.</param>
/// <param name="UserAgent">Custom user agent string. Default is null (uses browser default).</param>
public sealed record ScreenshotOptions(
    ImageFormat Format = ImageFormat.Png,
    int? JpegQuality = 80,
    int ViewportWidth = 1400,
    int? ViewportHeight = null,
    float DeviceScaleFactor = 1.0f,
    TimeSpan? NavigationTimeout = null,
    ScreenshotWaitUntil WaitUntil = ScreenshotWaitUntil.DomContentLoaded,
    bool FullPage = true,
    bool DisableAnimations = true,
    string? UserAgent = null
);
