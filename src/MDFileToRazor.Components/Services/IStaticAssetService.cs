namespace MDFileToRazor.Components.Services;

public interface IStaticAssetService
{
    Task<string?> GetAsync(string path);

    /// <summary>
    /// Gets a markdown file from the configured source directory.
    /// </summary>
    /// <param name="relativePath">The relative path to the markdown file within the source directory</param>
    /// <returns>The file content or null if not found</returns>
    Task<string?> GetMarkdownAsync(string relativePath);
}
