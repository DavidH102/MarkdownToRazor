using Microsoft.Extensions.Options;
using MarkdownToRazor.Configuration;
using System.Net.Http.Json;

namespace MarkdownToRazor.Services;

/// <summary>
/// WASM-specific implementation of markdown file discovery service.
/// Uses HTTP requests to discover files since file system access is not available in WASM.
/// </summary>
public class WasmFileDiscoveryService : IMdFileDiscoveryService
{
    private readonly MarkdownToRazorOptions _options;
    private readonly HttpClient _httpClient;
    private readonly HashSet<string> _verifiedFiles = new();
    private bool _filesVerified = false;

    // Pre-configured list of markdown files for WASM (since we can't scan directories)
    // Updated to match actual files in the WASM demo content directory
    private readonly string[] _knownMarkdownFiles =
    {
        "documentation.md",
        "features.md", 
        "getting-started.md",
        "wasm-performance.md"
    };

    public WasmFileDiscoveryService(IOptions<MarkdownToRazorOptions> options, HttpClient httpClient)
    {
        _options = options.Value;
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public IEnumerable<string> DiscoverMarkdownFiles()
    {
        // Return cached results if available for better WASM performance
        if (_filesVerified)
        {
            return _verifiedFiles.Where(file => file.EndsWith(".md", StringComparison.OrdinalIgnoreCase));
        }

        // Fallback to known files
        return _knownMarkdownFiles.Where(file => file.EndsWith(".md", StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> DiscoverMarkdownFilesAsync()
    {
        // Return cached results if already verified
        if (_filesVerified)
        {
            return _verifiedFiles;
        }

        _verifiedFiles.Clear();

        // For each known file, check if it exists by making a HEAD request
        var tasks = _knownMarkdownFiles.Select(async fileName =>
        {
            try
            {
                var url = $"{_options.SourceDirectory.TrimEnd('/')}/{fileName}";
                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                if (response.IsSuccessStatusCode)
                {
                    lock (_verifiedFiles)
                    {
                        _verifiedFiles.Add(fileName);
                    }
                }
            }
            catch
            {
                // File doesn't exist or can't be accessed, skip it
            }
        });

        await Task.WhenAll(tasks);
        _filesVerified = true;

        return _verifiedFiles;
    }

    /// <inheritdoc />
    public Dictionary<string, string> DiscoverMarkdownFilesWithRoutes()
    {
        var files = DiscoverMarkdownFiles();
        var routes = new Dictionary<string, string>();

        foreach (var file in files)
        {
            var route = GenerateRouteFromFileName(file);
            routes[file] = route;
        }

        return routes;
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, string>> DiscoverMarkdownFilesWithRoutesAsync()
    {
        var files = await DiscoverMarkdownFilesAsync();
        var routes = new Dictionary<string, string>();

        foreach (var file in files)
        {
            var route = GenerateRouteFromFileName(file);
            routes[file] = route;
        }

        return routes;
    }

    /// <inheritdoc />
    public string GetSourceDirectory()
    {
        return _options.SourceDirectory;
    }

    /// <inheritdoc />
    public string? GetOutputDirectory()
    {
        return _options.OutputDirectory;
    }

    /// <summary>
    /// Generates a route from a markdown filename.
    /// </summary>
    /// <param name="fileName">The markdown filename</param>
    /// <returns>The generated route</returns>
    private string GenerateRouteFromFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "/";

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

        // Add base route path if configured
        if (!string.IsNullOrWhiteSpace(_options.BaseRoutePath))
        {
            var basePath = _options.BaseRoutePath.Trim('/');
            route = $"/{basePath}/{route}";
        }
        else
        {
            // Ensure route starts with /
            if (!route.StartsWith('/'))
            {
                route = "/" + route;
            }
        }

        return route;
    }
}
