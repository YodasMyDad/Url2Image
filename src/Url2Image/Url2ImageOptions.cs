namespace Url2Image;

/// <summary>
/// Configuration options for the Url2Image service.
/// </summary>
public sealed class Url2ImageOptions
{
    /// <summary>
    /// Install Playwright browsers at application startup (recommended in CI/containers).
    /// Default is false.
    /// </summary>
    public bool AutoInstallBrowsersOnStartup { get; set; } = false;

    /// <summary>
    /// Which browser to use. Options: "chromium" (default), "firefox", "webkit".
    /// </summary>
    public string BrowserChannel { get; set; } = "chromium";

    /// <summary>
    /// Maximum parallel contexts. Null means unlimited (Playwright will queue internally).
    /// </summary>
    public int? MaxConcurrency { get; set; } = null;
}
