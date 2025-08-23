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
