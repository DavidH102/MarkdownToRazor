using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.FluentUI.AspNetCore.Components;
using MarkdownToRazor.Configuration;
using MarkdownToRazor.Services;

namespace MarkdownToRazor.Extensions;

/// <summary>
/// Extension methods for configuring MarkdownToRazor services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds MarkdownToRazor services to the dependency injection container with default configuration.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMarkdownToRazorServices(this IServiceCollection services)
    {
        return services.AddMarkdownToRazorServices(_ => { });
    }

    /// <summary>
    /// Adds MarkdownToRazor services to the dependency injection container with custom configuration.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Configuration action for MarkdownToRazor options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMarkdownToRazorServices(
        this IServiceCollection services,
        Action<MarkdownToRazorOptions> configureOptions)
    {
        // Configure options
        services.Configure(configureOptions);

        // Validate options on startup
        services.AddOptions<MarkdownToRazorOptions>()
            .PostConfigure(options => options.Validate());

        // Add required dependencies
        services.AddHttpClient();

        // Add FluentUI components (required for styling)
        services.AddFluentUIComponents();

        // Register MarkdownToRazor services
        services.TryAddScoped<IStaticAssetService, StaticAssetService>();
        services.TryAddScoped<IMdFileDiscoveryService, MdFileDiscoveryService>();
        services.TryAddScoped<IGeneratedPageDiscoveryService, GeneratedPageDiscoveryService>();

        return services;
    }

    /// <summary>
    /// Adds MarkdownToRazor services with source directory configuration.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="sourceDirectory">The source directory for markdown files</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMarkdownToRazorServices(
        this IServiceCollection services,
        string sourceDirectory)
    {
        return services.AddMarkdownToRazorServices(options =>
        {
            options.SourceDirectory = sourceDirectory;
        });
    }

    /// <summary>
    /// Adds MarkdownToRazor services with source and output directory configuration.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="sourceDirectory">The source directory for markdown files</param>
    /// <param name="outputDirectory">The output directory for generated pages</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMarkdownToRazorServices(
        this IServiceCollection services,
        string sourceDirectory,
        string outputDirectory)
    {
        return services.AddMarkdownToRazorServices(options =>
        {
            options.SourceDirectory = sourceDirectory;
            options.OutputDirectory = outputDirectory;
        });
    }
}
