# Url2Image = URL → Full‑page PNG/JPEG for .NET

Tiny service that turns any **URL** into a **full‑page screenshot** using [Microsoft.Playwright]. Works on Windows, Linux, macOS. Default **PNG**, optional **JPEG** with quality. Returns a **Stream** with helpers to save to disk.

[![NuGet](https://img.shields.io/nuget/v/Url2Image.svg)](https://www.nuget.org/packages/Url2Image/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)

## Install

```bash
dotnet add package Url2Image
```

Or install with automatic Playwright browser setup:

```bash
dotnet add package Url2Image
# Add to your project file: <InstallPlaywright>true</InstallPlaywright>
```

> **Heads‑up about Playwright browsers**
>
> Playwright requires browser binaries. This package can **auto‑install** them at app startup (recommended in containers) *or* you can install them once for your app using the Playwright script. **First run will hang for 1-5 minutes** while downloading ~200MB of browser binaries. See **Setup** below.

## Quick start (ASP.NET Core minimal API)

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddUrl2Image(o => o.AutoInstallBrowsersOnStartup = true); // optional

var app = builder.Build();

app.MapGet("/shot", async (string url, IUrlScreenshotService svc) =>
{
    await using var png = await svc.CaptureAsync(url);
    return Results.File(png, "image/png");
});

await app.RunAsync();
```

## API

```csharp
Task<Stream> CaptureAsync(string url, ScreenshotOptions? options = null, CancellationToken ct = default);
Task SaveToFileAsync(string url, string filePath, ScreenshotOptions? options = null, CancellationToken ct = default);
Task<byte[]> CaptureBytesAsync(string url, ScreenshotOptions? options = null, CancellationToken ct = default);
```

### ScreenshotOptions

* `Format`: `Png` (default) or `Jpeg`
* `JpegQuality`: 1–100 (only for JPEG; default 80)
* `ViewportWidth`: default 1400
* `ViewportHeight`: default 1080 (full‑page capture ignores height for final image)
* `DeviceScaleFactor`: default 1.0 (use >1 for sharper text)
* `NavigationTimeout`: default 30s
* `WaitUntil`: `DomContentLoaded` (default) | `Load` | `NetworkIdle`
* `FullPage`: default true
* `DisableAnimations`: default true
* `UserAgent`: optional string

## Setup

You have two options. **Pick one**:

**A) Auto‑install on app startup (recommended in containers/CI)**

```csharp
builder.Services.AddUrl2Image(o => o.AutoInstallBrowsersOnStartup = true);
```

This triggers a best‑effort Playwright install for Chromium. **First run will hang for 1-5 minutes** while downloading ~200MB of browser binaries. Requires network/file system access.

**B) Install browsers once per project (no code)**

Run after adding the NuGet (replace `net9.0` with your TFM if different):

```powershell
pwsh bin/Debug/net9.0/playwright.ps1 install
```

### Linux notes

On Debian/Ubuntu‑based hosts you may also need OS packages (fonts, NSS, etc.). You can install both **browsers and OS deps** with:

```bash
pwsh bin/Debug/net9.0/playwright.ps1 install --with-deps
```

In Docker, base from `mcr.microsoft.com/dotnet/aspnet:9.0-jammy` and consider:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0-jammy
WORKDIR /app
COPY ./publish .
# Install Playwright browsers + deps into the container layer
RUN pwsh -NoProfile ./playwright.ps1 install --with-deps chromium || true
ENTRYPOINT ["dotnet","YourApp.dll"]
```

## Example endpoint with JPEG

```csharp
app.MapGet("/shot.jpg", async (string url, IUrlScreenshotService svc) =>
{
    var opts = new ScreenshotOptions(Format: ImageFormat.Jpeg, JpegQuality: 85);
    var jpg = await svc.CaptureBytesAsync(url, opts);
    return Results.File(jpg, "image/jpeg");
});
```

## Gotchas / Tips

* **First run delay** → Auto-installing browsers takes 1-5 minutes and ~200MB. Use manual install for faster startup.
* **Huge pages** → Full‑page PNGs can be large. Prefer JPEG for very long pages or downscale via `DeviceScaleFactor`.
* **Flaky pages** → Try `WaitUntil = NetworkIdle` or increase `NavigationTimeout`.
* **Security** → Validate/whitelist input URLs if exposing this as a public API.
* **Headful vs headless** → This library uses headless by default.

## License

* Library: **MIT**
* Dependency: `Microsoft.Playwright` — **Apache‑2.0**
