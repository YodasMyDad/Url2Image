namespace Url2Image;

/// <summary>
/// Defines when to take the screenshot after navigation.
/// </summary>
public enum ScreenshotWaitUntil
{
    /// <summary>
    /// Wait until the page has loaded (load event fired).
    /// </summary>
    Load,

    /// <summary>
    /// Wait until DOM content is loaded (DOMContentLoaded event fired). Default.
    /// </summary>
    DomContentLoaded,

    /// <summary>
    /// Wait until network is idle (no network activity for at least 500ms).
    /// </summary>
    NetworkIdle
}
