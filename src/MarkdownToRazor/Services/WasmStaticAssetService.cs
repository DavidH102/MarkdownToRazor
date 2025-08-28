using Microsoft.Extensions.Options;
using MarkdownToRazor.Configuration;

namespace MarkdownToRazor.Services;

/// <summary>
/// WASM-compatible static asset service that uses HTTP client to fetch content
/// </summary>
public class WasmStaticAssetService : IStaticAssetService
{
    private readonly HttpClient _httpClient;
    private readonly MarkdownToRazorOptions? _options;
    private readonly Dictionary<string, string> _cache = new();

    public WasmStaticAssetService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _options = null; // Backward compatibility - options not required
    }

    public WasmStaticAssetService(HttpClient httpClient, IOptions<MarkdownToRazorOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string?> GetAsync(string path)
    {
        // Check cache first for better WASM performance
        if (_cache.TryGetValue(path, out var cachedContent))
        {
            Console.WriteLine($"WasmStaticAssetService: Cache hit for {path}");
            return cachedContent;
        }

        try
        {
            // For WASM, we need to construct the correct URL to the content
            string requestUrl;

            if (path.StartsWith("http"))
            {
                // Absolute URL - use as-is
                requestUrl = path;
            }
            else if (path.StartsWith("./") || path.StartsWith("/"))
            {
                // Relative or absolute path - use as-is
                requestUrl = path;
            }
            else if (path.StartsWith("content/"))
            {
                // Already has content prefix, just add leading slash
                requestUrl = $"/{path}";
            }
            else
            {
                // Convert file path to web-accessible URL
                // Assume content is in wwwroot/content/ directory
                requestUrl = $"/content/{path.TrimStart('/', '\\')}";
            }

            Console.WriteLine($"WasmStaticAssetService: Requesting URL: {requestUrl}");

            var response = await _httpClient.GetAsync(requestUrl);

            Console.WriteLine($"WasmStaticAssetService: Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"WasmStaticAssetService: Content length: {content?.Length ?? 0}");

                // Cache the content for future requests
                if (!string.IsNullOrEmpty(content))
                {
                    _cache[path] = content;
                }

                return content;
            }
            else
            {
                Console.WriteLine($"WasmStaticAssetService: Failed to load {requestUrl} - Status: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WasmStaticAssetService: Exception loading {path}: {ex.Message}");
            // Return null if file not found or error occurred
        }

        return null;
    }

    /// <summary>
    /// Gets a markdown file from the content directory.
    /// </summary>
    /// <param name="relativePath">The relative path to the markdown file</param>
    /// <returns>The file content or null if not found</returns>
    public async Task<string?> GetMarkdownAsync(string relativePath)
    {
        try
        {
            // Ensure the path has .md extension
            var path = relativePath;
            if (!path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                path += ".md";
            }

            return await GetAsync(path);
        }
        catch
        {
            // Return null if file not found or error occurred
        }

        return null;
    }
}
