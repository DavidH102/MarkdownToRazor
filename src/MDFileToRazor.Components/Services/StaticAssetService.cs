using Microsoft.Extensions.Hosting;

namespace MDFileToRazor.Components.Services;

public class StaticAssetService : IStaticAssetService
{
    private readonly HttpClient _httpClient;
    private readonly IHostEnvironment _hostEnvironment;

    public StaticAssetService(HttpClient httpClient, IHostEnvironment hostEnvironment)
    {
        _httpClient = httpClient;
        _hostEnvironment = hostEnvironment;
    }

    public async Task<string?> GetAsync(string path)
    {
        try
        {
            // First try to read from wwwroot
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
}
