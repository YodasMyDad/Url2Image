using Microsoft.Playwright;
using System.Runtime.CompilerServices;

namespace Url2Image;

/// <summary>
/// Service for capturing screenshots of URLs using Playwright.
/// </summary>
public sealed class UrlScreenshotService : IUrlScreenshotService, IAsyncDisposable
{
    private readonly Url2ImageOptions _options;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private bool _initialized;

    /// <summary>
    /// Gets whether the service is initialized and ready to capture screenshots.
    /// </summary>
    public bool IsInitialized => _initialized;

    /// <summary>
    /// Gets whether Playwright browsers are installed on the system.
    /// </summary>
    public bool AreBrowsersInstalled => CheckBrowsersInstalled();

    /// <summary>
    /// Initializes a new instance of the <see cref="UrlScreenshotService"/> class.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    public UrlScreenshotService(Url2ImageOptions options)
        => _options = options;

    /// <summary>
    /// Captures a screenshot of the specified URL and returns it as a Stream.
    /// </summary>
    public async Task<Stream> CaptureAsync(
        string url,
        ScreenshotOptions? options = null,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        EnsureHttpOrHttps(url);
        options ??= new();

        await EnsureInitializedAsync(ct).ConfigureAwait(false);

        // New isolated context per capture
        var context = await _browser!.NewContextAsync(new()
        {
            ViewportSize = new() { Width = options.ViewportWidth, Height = options.ViewportHeight ?? 1080 },
            DeviceScaleFactor = options.DeviceScaleFactor,
            UserAgent = options.UserAgent
        }).ConfigureAwait(false);

        try
        {
            var page = await context.NewPageAsync().ConfigureAwait(false);

            await page.GotoAsync(url, new PageGotoOptions
            {
                WaitUntil = options.WaitUntil switch
                {
                    ScreenshotWaitUntil.Load => WaitUntilState.Load,
                    ScreenshotWaitUntil.NetworkIdle => WaitUntilState.NetworkIdle,
                    _ => WaitUntilState.DOMContentLoaded
                },
                Timeout = (float?)(options.NavigationTimeout ?? TimeSpan.FromSeconds(30)).TotalMilliseconds
            }).ConfigureAwait(false);

            if (options.DisableAnimations)
            {
                await page.AddStyleTagAsync(new() { Content = "*{animation:none!important;transition:none!important}" }).ConfigureAwait(false);
                await page.EvaluateAsync("() => document.fonts ? document.fonts.ready : Promise.resolve()")
                          .ConfigureAwait(false);
            }

            var screenshotBytes = await page.ScreenshotAsync(new PageScreenshotOptions
            {
                FullPage = options.FullPage,
                Type = options.Format == ImageFormat.Jpeg ? ScreenshotType.Jpeg : ScreenshotType.Png,
                Quality = options.Format == ImageFormat.Jpeg ? options.JpegQuality : null
            }).ConfigureAwait(false);

            var memoryStream = new MemoryStream(screenshotBytes.Length);
            await memoryStream.WriteAsync(screenshotBytes, ct).ConfigureAwait(false);
            memoryStream.Position = 0;

            return memoryStream;
        }
        finally
        {
            await context.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Captures a screenshot of the specified URL and returns it as a byte array.
    /// </summary>
    public async Task<byte[]> CaptureBytesAsync(string url, ScreenshotOptions? options = null, CancellationToken ct = default)
    {
        await using var stream = await CaptureAsync(url, options, ct).ConfigureAwait(false);
        return ((MemoryStream)stream).ToArray();
    }

    /// <summary>
    /// Captures a screenshot of the specified URL and saves it to a file.
    /// </summary>
    public async Task SaveToFileAsync(string url, string filePath, ScreenshotOptions? options = null, CancellationToken ct = default)
    {
        await using var stream = await CaptureAsync(url, options, ct).ConfigureAwait(false);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await using var fileStream = File.Create(filePath);
        await stream.CopyToAsync(fileStream, ct).ConfigureAwait(false);
    }

    private async Task EnsureInitializedAsync(CancellationToken ct)
    {
        if (_initialized) return;
        await _initLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            if (_initialized) return;

            if (_options.AutoInstallBrowsersOnStartup)
            {
                try
                {
                    // Best-effort install (ignored if not permitted / already installed)
                    _ = Microsoft.Playwright.Program.Main(new[] { "install", _options.BrowserChannel });
                }
                catch { /* ignore */ }
            }

            _playwright = await Playwright.CreateAsync().ConfigureAwait(false);
            _browser = await (_options.BrowserChannel switch
            {
                "firefox" => _playwright.Firefox.LaunchAsync(new() { Headless = true }),
                "webkit" => _playwright.Webkit.LaunchAsync(new() { Headless = true }),
                _ => _playwright.Chromium.LaunchAsync(new() { Headless = true })
            }).ConfigureAwait(false);

            _initialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    /// <summary>
    /// Checks whether Playwright browsers are installed on the system.
    /// This is a static method that can be called independently.
    /// </summary>
    public static bool AreBrowsersInstalledStatic()
    {
        try
        {
            // Check the standard Playwright installation directory
            var playwrightDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ms-playwright");

            if (Directory.Exists(playwrightDir))
            {
                // Check for any chromium installation
                var chromiumDirs = Directory.GetDirectories(playwrightDir, "chromium-*", SearchOption.TopDirectoryOnly);
                if (chromiumDirs.Any())
                {
                    // Verify the chromium directory actually contains the browser executable
                    foreach (var chromiumDir in chromiumDirs)
                    {
                        var chromeExe = Path.Combine(chromiumDir, "chrome-win", "chrome.exe");
                        if (File.Exists(chromeExe))
                        {
                            return true;
                        }
                    }
                }
            }

            // Also check for system-wide installation
            var systemPlaywrightDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".cache", "ms-playwright");

            if (Directory.Exists(systemPlaywrightDir))
            {
                var chromiumDirs = Directory.GetDirectories(systemPlaywrightDir, "chromium-*", SearchOption.TopDirectoryOnly);
                foreach (var chromiumDir in chromiumDirs)
                {
                    var chromeExe = Path.Combine(chromiumDir, "chrome-win", "chrome.exe");
                    if (File.Exists(chromeExe))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks whether Playwright browsers are installed on the system.
    /// </summary>
    private bool CheckBrowsersInstalled() => AreBrowsersInstalledStatic();

    private static void EnsureHttpOrHttps(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new ArgumentException("Only http/https URLs are supported.", nameof(url));
    }

    /// <summary>
    /// Disposes the service and releases resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_browser is not null) await _browser.DisposeAsync();
        _playwright?.Dispose();
        _initLock.Dispose();
    }
}
