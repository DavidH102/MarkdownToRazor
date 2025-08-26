using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MDFileToRazor.Components.Configuration;

namespace MDFileToRazor.Components.Services;

/// <summary>
/// Interface for discovering markdown files based on configuration.
/// </summary>
public interface IMdFileDiscoveryService
{
    /// <summary>
    /// Discovers all markdown files in the configured source directory.
    /// </summary>
    /// <returns>An enumerable of file paths</returns>
    IEnumerable<string> DiscoverMarkdownFiles();

    /// <summary>
    /// Discovers all markdown files in the configured source directory asynchronously.
    /// </summary>
    /// <returns>An enumerable of file paths</returns>
    Task<IEnumerable<string>> DiscoverMarkdownFilesAsync();

    /// <summary>
    /// Discovers all markdown files and returns a dictionary mapping filenames to their generated routes.
    /// </summary>
    /// <returns>A dictionary where keys are markdown filenames (with .md extension) and values are the generated routes</returns>
    Dictionary<string, string> DiscoverMarkdownFilesWithRoutes();

    /// <summary>
    /// Discovers all markdown files and returns a dictionary mapping filenames to their generated routes asynchronously.
    /// </summary>
    /// <returns>A dictionary where keys are markdown filenames (with .md extension) and values are the generated routes</returns>
    Task<Dictionary<string, string>> DiscoverMarkdownFilesWithRoutesAsync();

    /// <summary>
    /// Gets the absolute path to the source directory.
    /// </summary>
    /// <returns>The absolute source directory path</returns>
    string GetSourceDirectory();

    /// <summary>
    /// Gets the absolute path to the output directory.
    /// </summary>
    /// <returns>The absolute output directory path</returns>
    string GetOutputDirectory();
}

/// <summary>
/// Service for discovering markdown files based on configuration.
/// </summary>
public class MdFileDiscoveryService : IMdFileDiscoveryService
{
    private readonly MdFileToRazorOptions _options;
    private readonly IHostEnvironment _hostEnvironment;

    public MdFileDiscoveryService(IOptions<MdFileToRazorOptions> options, IHostEnvironment hostEnvironment)
    {
        _options = options.Value;
        _hostEnvironment = hostEnvironment;
    }

    /// <inheritdoc />
    public IEnumerable<string> DiscoverMarkdownFiles()
    {
        var sourceDirectory = GetSourceDirectory();

        if (!Directory.Exists(sourceDirectory))
        {
            return Enumerable.Empty<string>();
        }

        var searchOption = _options.SearchRecursively
            ? SearchOption.AllDirectories
            : SearchOption.TopDirectoryOnly;

        try
        {
            return Directory.GetFiles(sourceDirectory, _options.FilePattern, searchOption)
                .Where(file => Path.GetExtension(file).Equals(".md", StringComparison.OrdinalIgnoreCase));
        }
        catch (DirectoryNotFoundException)
        {
            return Enumerable.Empty<string>();
        }
        catch (UnauthorizedAccessException)
        {
            return Enumerable.Empty<string>();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> DiscoverMarkdownFilesAsync()
    {
        return await Task.Run(() => DiscoverMarkdownFiles());
    }

    /// <inheritdoc />
    public Dictionary<string, string> DiscoverMarkdownFilesWithRoutes()
    {
        var markdownFiles = DiscoverMarkdownFiles();
        var result = new Dictionary<string, string>();

        foreach (var filePath in markdownFiles)
        {
            var fileName = Path.GetFileName(filePath);
            var route = GenerateRouteFromFileName(fileName);
            result[fileName] = route;
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, string>> DiscoverMarkdownFilesWithRoutesAsync()
    {
        return await Task.Run(() => DiscoverMarkdownFilesWithRoutes());
    }

    /// <summary>
    /// Generates a route from a markdown filename following the same logic as the code generation.
    /// </summary>
    /// <param name="fileName">The markdown filename (with .md extension)</param>
    /// <returns>The generated route</returns>
    private string GenerateRouteFromFileName(string fileName)
    {
        // Remove the .md extension
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        // Handle special cases
        if (nameWithoutExtension.Equals("index", StringComparison.OrdinalIgnoreCase))
        {
            return "/";
        }

        // Convert to lowercase and replace spaces/underscores with hyphens for URL-friendly routes
        var route = nameWithoutExtension.ToLowerInvariant()
            .Replace(' ', '-')
            .Replace('_', '-');

        // Clean up multiple consecutive hyphens by replacing them with single hyphens
        while (route.Contains("--"))
        {
            route = route.Replace("--", "-");
        }

        // Remove leading/trailing hyphens
        route = route.Trim('-');

        // Ensure route starts with /
        if (!route.StartsWith('/'))
        {
            route = "/" + route;
        }

        return route;
    }

    /// <inheritdoc />
    public string GetSourceDirectory()
    {
        return _options.GetAbsoluteSourcePath(_hostEnvironment.ContentRootPath);
    }

    /// <inheritdoc />
    public string GetOutputDirectory()
    {
        return _options.GetAbsoluteOutputPath(_hostEnvironment.ContentRootPath);
    }
}
