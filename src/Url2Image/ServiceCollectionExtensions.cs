using Microsoft.Extensions.DependencyInjection;

namespace Url2Image;

/// <summary>
/// Extension methods for registering Url2Image services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Url2Image services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddUrl2Image(
        this IServiceCollection services,
        Action<Url2ImageOptions>? configure = null)
    {
        var options = new Url2ImageOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        services.AddSingleton<IUrlScreenshotService, UrlScreenshotService>();
        return services;
    }
}
