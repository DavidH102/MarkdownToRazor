using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MarkdownToRazor.Configuration;

namespace MarkdownToRazor.Services;

public class StaticAssetService : IStaticAssetService
{
    private readonly HttpClient _httpClient;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly MarkdownToRazorOptions? _options;

    public StaticAssetService(HttpClient httpClient, IHostEnvironment hostEnvironment)
    {
        _httpClient = httpClient;
        _hostEnvironment = hostEnvironment;
        _options = null; // Backward compatibility - options not required
    }

    public StaticAssetService(HttpClient httpClient, IHostEnvironment hostEnvironment, IOptions<MarkdownToRazorOptions> options)
    {
        _httpClient = httpClient;
        _hostEnvironment = hostEnvironment;
        _options = options.Value;
    }

    public async Task<string?> GetAsync(string path)
    {
        try
        {
            // First try to read from configured source directory (if available)
            if (_options != null && !path.StartsWith("http") && !path.StartsWith("./"))
            {
                var sourceDirectory = _options.GetAbsoluteSourcePath(_hostEnvironment.ContentRootPath);
                var filePath = Path.Combine(sourceDirectory, path);
                if (File.Exists(filePath))
                {
                    return await File.ReadAllTextAsync(filePath);
                }
            }

            // Try to read from wwwroot
            if (path.StartsWith("./"))
            {
                var filePath = Path.Combine(_hostEnvironment.ContentRootPath, "wwwroot", path[2..]);
                if (File.Exists(filePath))
                {
                    return await File.ReadAllTextAsync(filePath);
                }
            }

            // Fallback to HTTP client for remote resources
            var response = await _httpClient.GetAsync(path);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
        }
        catch
        {
            // Return null if file not found or error occurred
        }

        return null;
    }

    /// <summary>
    /// Gets a markdown file from the configured source directory.
    /// </summary>
    /// <param name="relativePath">The relative path to the markdown file within the source directory</param>
    /// <returns>The file content or null if not found</returns>
    public async Task<string?> GetMarkdownAsync(string relativePath)
    {
        if (_options == null)
        {
            return await GetAsync(relativePath);
        }

        try
        {
            var sourceDirectory = _options.GetAbsoluteSourcePath(_hostEnvironment.ContentRootPath);
            var filePath = Path.Combine(sourceDirectory, relativePath);

            if (File.Exists(filePath) && Path.GetExtension(filePath).Equals(".md", StringComparison.OrdinalIgnoreCase))
            {
                return await File.ReadAllTextAsync(filePath);
            }
        }
        catch
        {
            // Return null if file not found or error occurred
        }

        return null;
    }
}
