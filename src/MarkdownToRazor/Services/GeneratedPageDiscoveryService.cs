using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MarkdownToRazor.Configuration;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MarkdownToRazor.Services;

/// <summary>
/// Represents information about a generated Razor page.
/// </summary>
public class GeneratedPageInfo
{
    /// <summary>
    /// The route/URL for the page (e.g., "/docs/getting-started").
    /// </summary>
    public string Route { get; set; } = string.Empty;

    /// <summary>
    /// The name of the generated Razor file (e.g., "GettingStarted.razor").
    /// </summary>
    public string RazorFileName { get; set; } = string.Empty;

    /// <summary>
    /// The original markdown file path (relative to source directory).
    /// </summary>
    public string MarkdownFilePath { get; set; } = string.Empty;

    /// <summary>
    /// The title of the page.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// The description of the page (if specified in frontmatter).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The layout component to use (if specified in frontmatter).
    /// </summary>
    public string? Layout { get; set; }

    /// <summary>
    /// Tags associated with the page.
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Whether the title should be shown on the page.
    /// </summary>
    public bool ShowTitle { get; set; } = true;
}

/// <summary>
/// Interface for discovering generated Razor pages and their metadata.
/// </summary>
public interface IGeneratedPageDiscoveryService
{
    /// <summary>
    /// Discovers all generated Razor pages with their routes and metadata.
    /// </summary>
    /// <returns>An enumerable of page information</returns>
    IEnumerable<GeneratedPageInfo> DiscoverGeneratedPages();

    /// <summary>
    /// Discovers all generated Razor pages with their routes and metadata asynchronously.
    /// </summary>
    /// <returns>An enumerable of page information</returns>
    Task<IEnumerable<GeneratedPageInfo>> DiscoverGeneratedPagesAsync();

    /// <summary>
    /// Gets page information for a specific markdown file.
    /// </summary>
    /// <param name="markdownFilePath">The path to the markdown file</param>
    /// <returns>Page information or null if not found</returns>
    Task<GeneratedPageInfo?> GetPageInfoAsync(string markdownFilePath);

    /// <summary>
    /// Gets all pages grouped by their tags.
    /// </summary>
    /// <returns>A dictionary where keys are tag names and values are lists of pages</returns>
    Task<Dictionary<string, List<GeneratedPageInfo>>> GetPagesByTagsAsync();
}

/// <summary>
/// Service for discovering generated Razor pages and their metadata.
/// </summary>
public class GeneratedPageDiscoveryService : IGeneratedPageDiscoveryService
{
    private readonly MarkdownToRazorOptions _options;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly IMdFileDiscoveryService _mdFileDiscoveryService;
    private readonly IDeserializer _yamlDeserializer;

    public GeneratedPageDiscoveryService(
        IOptions<MarkdownToRazorOptions> options,
        IHostEnvironment hostEnvironment,
        IMdFileDiscoveryService mdFileDiscoveryService)
    {
        _options = options.Value;
        _hostEnvironment = hostEnvironment;
        _mdFileDiscoveryService = mdFileDiscoveryService;
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    /// <inheritdoc />
    public IEnumerable<GeneratedPageInfo> DiscoverGeneratedPages()
    {
        var markdownFiles = _mdFileDiscoveryService.DiscoverMarkdownFiles();
        var sourceDirectory = _mdFileDiscoveryService.GetSourceDirectory();

        foreach (var markdownFile in markdownFiles)
        {
            var pageInfo = ProcessMarkdownFile(markdownFile, sourceDirectory);
            if (pageInfo != null)
            {
                yield return pageInfo;
            }
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<GeneratedPageInfo>> DiscoverGeneratedPagesAsync()
    {
        var markdownFiles = await _mdFileDiscoveryService.DiscoverMarkdownFilesAsync();
        var sourceDirectory = _mdFileDiscoveryService.GetSourceDirectory();
        var results = new List<GeneratedPageInfo>();

        foreach (var markdownFile in markdownFiles)
        {
            var pageInfo = await ProcessMarkdownFileAsync(markdownFile, sourceDirectory);
            if (pageInfo != null)
            {
                results.Add(pageInfo);
            }
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<GeneratedPageInfo?> GetPageInfoAsync(string markdownFilePath)
    {
        if (string.IsNullOrEmpty(markdownFilePath))
        {
            return null;
        }

        try
        {
            var sourceDirectory = _mdFileDiscoveryService.GetSourceDirectory();
            var fullPath = Path.IsPathFullyQualified(markdownFilePath)
                ? markdownFilePath
                : !string.IsNullOrEmpty(sourceDirectory)
                    ? Path.Combine(sourceDirectory, markdownFilePath)
                    : markdownFilePath;

            if (!File.Exists(fullPath))
            {
                return null;
            }

            return await ProcessMarkdownFileAsync(fullPath, sourceDirectory);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<Dictionary<string, List<GeneratedPageInfo>>> GetPagesByTagsAsync()
    {
        var pages = await DiscoverGeneratedPagesAsync();
        var tagGroups = new Dictionary<string, List<GeneratedPageInfo>>();

        foreach (var page in pages)
        {
            foreach (var tag in page.Tags)
            {
                if (!tagGroups.ContainsKey(tag))
                {
                    tagGroups[tag] = new List<GeneratedPageInfo>();
                }
                tagGroups[tag].Add(page);
            }
        }

        return tagGroups;
    }

    private GeneratedPageInfo? ProcessMarkdownFile(string markdownFilePath, string sourceDirectory)
    {
        try
        {
            var markdownContent = File.ReadAllText(markdownFilePath);
            return ProcessMarkdownContent(markdownFilePath, markdownContent, sourceDirectory);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task<GeneratedPageInfo?> ProcessMarkdownFileAsync(string markdownFilePath, string sourceDirectory)
    {
        try
        {
            var markdownContent = await File.ReadAllTextAsync(markdownFilePath);
            return ProcessMarkdownContent(markdownFilePath, markdownContent, sourceDirectory);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private GeneratedPageInfo? ProcessMarkdownContent(string markdownFilePath, string markdownContent, string sourceDirectory)
    {
        if (string.IsNullOrEmpty(markdownFilePath))
        {
            return null;
        }

        try
        {
            var relativePath = !string.IsNullOrEmpty(sourceDirectory) && Path.IsPathFullyQualified(sourceDirectory)
                ? Path.GetRelativePath(sourceDirectory, markdownFilePath)
                : Path.GetFileName(markdownFilePath);

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(markdownFilePath);
            if (string.IsNullOrEmpty(fileNameWithoutExtension))
            {
                return null;
            }

            // Parse HTML comment configuration and frontmatter (same logic as code generation)
            var (htmlConfig, contentAfterHtmlComment) = ParseHtmlCommentConfiguration(markdownContent);
            var (frontmatter, _) = ParseFrontmatter(contentAfterHtmlComment);
            var mergedConfig = MergeConfigurations(htmlConfig, frontmatter);

            // Generate route (same logic as code generation)
            string route;
            if (!string.IsNullOrEmpty(htmlConfig?.PageDirective))
            {
                route = htmlConfig.PageDirective;
            }
            else
            {
                route = ProcessRoute(frontmatter?.Route ?? GenerateRouteFromFilename(fileNameWithoutExtension));
            }

            var title = mergedConfig.Title ?? GenerateTitleFromFilename(fileNameWithoutExtension);
            var razorFileName = GenerateRazorFileName(fileNameWithoutExtension);

            return new GeneratedPageInfo
            {
                Route = route,
                RazorFileName = razorFileName,
                MarkdownFilePath = relativePath,
                Title = title,
                Description = mergedConfig.Description,
                Layout = mergedConfig.Layout,
                Tags = mergedConfig.Tags ?? new List<string>(),
                ShowTitle = mergedConfig.ShowTitle ?? true
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    // The following methods are copied from MarkdownToRazorGenerator to ensure consistent logic
    private (HtmlCommentConfiguration?, string) ParseHtmlCommentConfiguration(string markdownContent)
    {
        var lines = markdownContent.Split('\n');
        if (lines.Length == 0)
        {
            return (null, markdownContent);
        }

        var firstLine = lines[0].Trim();
        var configCommentPattern = @"^<!--\s*This is configuration data\s*-->$";
        if (!Regex.IsMatch(firstLine, configCommentPattern, RegexOptions.IgnoreCase))
        {
            return (null, markdownContent);
        }

        var config = new HtmlCommentConfiguration();
        var contentStartIndex = 1;

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrEmpty(line) || !line.StartsWith("<!--") || !line.EndsWith("-->"))
            {
                contentStartIndex = i;
                break;
            }

            var commentContent = line.Substring(4, line.Length - 7).Trim();

            if (commentContent.StartsWith("@page "))
            {
                var pagePattern = @"@page\s+""([^""]+)""";
                var pageMatch = Regex.Match(commentContent, pagePattern);
                if (pageMatch.Success)
                {
                    config.PageDirective = pageMatch.Groups[1].Value;
                }
            }
            else if (commentContent.StartsWith("title:"))
            {
                config.Title = commentContent.Substring(6).Trim();
            }
            else if (commentContent.StartsWith("layout:"))
            {
                config.Layout = commentContent.Substring(7).Trim();
            }
            else if (commentContent.StartsWith("showTitle:"))
            {
                if (bool.TryParse(commentContent.Substring(10).Trim(), out bool showTitle))
                {
                    config.ShowTitle = showTitle;
                }
            }
            else if (commentContent.StartsWith("description:"))
            {
                config.Description = commentContent.Substring(12).Trim();
            }
            else if (commentContent.StartsWith("tags:"))
            {
                var tagsValue = commentContent.Substring(5).Trim();
                config.Tags = tagsValue.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList();
            }
        }

        var remainingContent = string.Join('\n', lines.Skip(contentStartIndex));
        return (config, remainingContent);
    }

    private (MarkdownFrontmatter?, string) ParseFrontmatter(string markdownContent)
    {
        var frontmatterPattern = @"^---\s*\n(.*?)\n---\s*\n(.*)$";
        var match = Regex.Match(markdownContent, frontmatterPattern, RegexOptions.Singleline);

        if (!match.Success)
        {
            return (null, markdownContent);
        }

        try
        {
            var yamlContent = match.Groups[1].Value;
            var contentWithoutFrontmatter = match.Groups[2].Value;
            var frontmatter = _yamlDeserializer.Deserialize<MarkdownFrontmatter>(yamlContent);
            return (frontmatter, contentWithoutFrontmatter);
        }
        catch (Exception)
        {
            return (null, markdownContent);
        }
    }

    private MarkdownFrontmatter MergeConfigurations(HtmlCommentConfiguration? htmlConfig, MarkdownFrontmatter? frontmatter)
    {
        var merged = new MarkdownFrontmatter();

        if (frontmatter != null)
        {
            merged.Route = frontmatter.Route;
            merged.Title = frontmatter.Title;
            merged.Layout = frontmatter.Layout;
            merged.ShowTitle = frontmatter.ShowTitle;
            merged.Description = frontmatter.Description;
            merged.Tags = frontmatter.Tags;
        }

        if (htmlConfig != null)
        {
            if (!string.IsNullOrEmpty(htmlConfig.PageDirective))
                merged.Route = htmlConfig.PageDirective;
            if (!string.IsNullOrEmpty(htmlConfig.Title))
                merged.Title = htmlConfig.Title;
            if (!string.IsNullOrEmpty(htmlConfig.Layout))
                merged.Layout = htmlConfig.Layout;
            if (htmlConfig.ShowTitle.HasValue)
                merged.ShowTitle = htmlConfig.ShowTitle;
            if (!string.IsNullOrEmpty(htmlConfig.Description))
                merged.Description = htmlConfig.Description;
            if (htmlConfig.Tags?.Any() == true)
                merged.Tags = htmlConfig.Tags;
        }

        return merged;
    }

    private string GenerateRouteFromFilename(string filename)
    {
        return $"/docs/{filename.ToLowerInvariant()}";
    }

    private string ProcessRoute(string route)
    {
        if (!route.StartsWith("/"))
            route = "/" + route;

        // If the route is explicitly provided (not generated from filename), respect it as-is
        // Only auto-prefix with /docs for routes generated from filenames
        return route;
    }

    private string GenerateTitleFromFilename(string filename)
    {
        return string.Join(" ", filename.Split('-', '_'))
            .Replace(" ", " ")
            .Trim()
            .Split(' ')
            .Select(word => char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant())
            .Aggregate((a, b) => $"{a} {b}");
    }

    private string GenerateRazorFileName(string markdownFileName)
    {
        var pascalCase = string.Join("", markdownFileName.Split('-', '_')
            .Select(word => char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant()));
        return $"{pascalCase}.razor";
    }
}

// Helper classes (should match those in MarkdownToRazorGenerator)
public class MarkdownFrontmatter
{
    public string? Route { get; set; }
    public string? Title { get; set; }
    public string? Layout { get; set; }
    public bool? ShowTitle { get; set; } = true;
    public string? Description { get; set; }
    public List<string>? Tags { get; set; }
}

public class HtmlCommentConfiguration
{
    public string? PageDirective { get; set; }
    public string? Title { get; set; }
    public string? Layout { get; set; }
    public bool? ShowTitle { get; set; }
    public string? Description { get; set; }
    public List<string>? Tags { get; set; }
}
