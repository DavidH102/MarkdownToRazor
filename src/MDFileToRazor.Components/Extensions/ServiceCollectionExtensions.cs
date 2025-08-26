using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.FluentUI.AspNetCore.Components;
using MDFileToRazor.Components.Configuration;
using MDFileToRazor.Components.Services;

namespace MDFileToRazor.Components.Extensions;

/// <summary>
/// Extension methods for configuring MDFileToRazor services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MDFileToRazor services to the dependency injection container with default configuration.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMdFileToRazorServices(this IServiceCollection services)
    {
        return services.AddMdFileToRazorServices(_ => { });
    }

    /// <summary>
    /// Adds MDFileToRazor services to the dependency injection container with custom configuration.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Configuration action for MDFileToRazor options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMdFileToRazorServices(
        this IServiceCollection services,
        Action<MdFileToRazorOptions> configureOptions)
    {
        // Configure options
        services.Configure(configureOptions);

        // Validate options on startup
        services.AddOptions<MdFileToRazorOptions>()
            .PostConfigure(options => options.Validate());

        // Add required dependencies
        services.AddHttpClient();

        // Add FluentUI components (required for styling)
        services.AddFluentUIComponents();

        // Register MDFileToRazor services
        services.TryAddScoped<IStaticAssetService, StaticAssetService>();
        services.TryAddScoped<IMdFileDiscoveryService, MdFileDiscoveryService>();
        services.TryAddScoped<IGeneratedPageDiscoveryService, GeneratedPageDiscoveryService>();

        return services;
    }

    /// <summary>
    /// Adds MDFileToRazor services with source directory configuration.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="sourceDirectory">The source directory for markdown files</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMdFileToRazorServices(
        this IServiceCollection services,
        string sourceDirectory)
    {
        return services.AddMdFileToRazorServices(options =>
        {
            options.SourceDirectory = sourceDirectory;
        });
    }

    /// <summary>
    /// Adds MDFileToRazor services with source and output directory configuration.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="sourceDirectory">The source directory for markdown files</param>
    /// <param name="outputDirectory">The output directory for generated pages</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMdFileToRazorServices(
        this IServiceCollection services,
        string sourceDirectory,
        string outputDirectory)
    {
        return services.AddMdFileToRazorServices(options =>
        {
            options.SourceDirectory = sourceDirectory;
            options.OutputDirectory = outputDirectory;
        });
    }
}
