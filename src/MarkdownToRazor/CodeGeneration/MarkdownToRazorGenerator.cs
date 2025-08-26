using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MarkdownToRazor.CodeGeneration;

/// <summary>
/// Represents frontmatter configuration for a markdown file
/// </summary>
public class MarkdownFrontmatter
{
    public string? Route { get; set; }
    public string? Title { get; set; }
    public string? Layout { get; set; }
    public bool? ShowTitle { get; set; } = true;
    public string? Description { get; set; }
    public List<string>? Tags { get; set; }
}

/// <summary>
/// Represents configuration data extracted from HTML comments
/// </summary>
public class HtmlCommentConfiguration
{
    public string? PageDirective { get; set; }
    public string? Title { get; set; }
    public string? Layout { get; set; }
    public bool? ShowTitle { get; set; }
    public string? Description { get; set; }
    public List<string>? Tags { get; set; }
}

/// <summary>
/// Code generator that converts markdown files to Razor pages
/// </summary>
public class MarkdownToRazorGenerator
{
    private readonly IDeserializer _yamlDeserializer;

    public MarkdownToRazorGenerator()
    {
        _yamlDeserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
    }

    /// <summary>
    /// Generates Razor pages from all markdown files in the specified source directory
    /// </summary>
    public async Task GenerateRazorPagesAsync(string sourceDirectory, string outputDirectory)
    {
        if (!Directory.Exists(sourceDirectory))
        {
            Console.WriteLine($"Source directory does not exist: {sourceDirectory}");
            return;
        }

        // Ensure output directory exists
        Directory.CreateDirectory(outputDirectory);

        var markdownFiles = Directory.GetFiles(sourceDirectory, "*.md", SearchOption.AllDirectories);

        Console.WriteLine($"Found {markdownFiles.Length} markdown files to process...");

        foreach (var markdownFile in markdownFiles)
        {
            try
            {
                await GenerateRazorPageAsync(markdownFile, sourceDirectory, outputDirectory);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing {markdownFile}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Generates a single Razor page from a markdown file
    /// </summary>
    private async Task GenerateRazorPageAsync(string markdownFilePath, string sourceDirectory, string outputDirectory)
    {
        var markdownContent = await File.ReadAllTextAsync(markdownFilePath);
        var relativePath = Path.GetRelativePath(sourceDirectory, markdownFilePath);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(markdownFilePath);

        // Check for HTML comment configuration first
        var (htmlConfig, contentAfterHtmlComment) = ParseHtmlCommentConfiguration(markdownContent);

        // Parse frontmatter if exists (from the content after HTML comment processing)
        var (frontmatter, contentWithoutFrontmatter) = ParseFrontmatter(contentAfterHtmlComment);

        // Merge configurations - HTML comment takes precedence over frontmatter
        var mergedConfig = MergeConfigurations(htmlConfig, frontmatter);

        // Generate route from HTML comment @page directive, frontmatter route, or filename
        string route;
        if (!string.IsNullOrEmpty(htmlConfig?.PageDirective))
        {
            // Use the @page directive from HTML comment
            route = htmlConfig.PageDirective;
        }
        else
        {
            // Fallback to frontmatter route or filename-based route
            route = ProcessRoute(frontmatter?.Route ?? GenerateRouteFromFilename(fileNameWithoutExtension));
        }

        // Generate page title from merged configuration or filename
        var title = mergedConfig.Title ?? GenerateTitleFromFilename(fileNameWithoutExtension);

        // Generate the Razor page content
        var razorContent = GenerateRazorPageContent(route, title, relativePath, mergedConfig);

        // Generate output filename
        var outputFileName = GenerateRazorFileName(fileNameWithoutExtension);
        var outputPath = Path.Combine(outputDirectory, outputFileName);

        // Write the generated Razor page
        await File.WriteAllTextAsync(outputPath, razorContent);

        Console.WriteLine($"Generated: {outputFileName} -> {route}");
    }

    /// <summary>
    /// Parses YAML frontmatter from markdown content
    /// </summary>
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
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing frontmatter: {ex.Message}");
            return (null, markdownContent);
        }
    }

    /// <summary>
    /// Parses HTML comment configuration from the first line of markdown content
    /// </summary>
    private (HtmlCommentConfiguration?, string) ParseHtmlCommentConfiguration(string markdownContent)
    {
        // Check if the first line is an HTML comment with configuration data
        var lines = markdownContent.Split('\n');
        if (lines.Length == 0)
        {
            return (null, markdownContent);
        }

        var firstLine = lines[0].Trim();

        // Pattern to match: <!-- This is configuration data -->
        var configCommentPattern = @"^<!--\s*This is configuration data\s*-->$";
        if (!Regex.IsMatch(firstLine, configCommentPattern, RegexOptions.IgnoreCase))
        {
            return (null, markdownContent);
        }

        Console.WriteLine("Found HTML comment configuration marker, parsing configuration...");

        var config = new HtmlCommentConfiguration();
        var contentStartIndex = 1; // Start after the configuration marker line

        // Parse subsequent lines for configuration directives until we hit a non-comment line
        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            // Stop parsing if we hit a non-comment line or empty line
            if (string.IsNullOrEmpty(line) || !line.StartsWith("<!--") || !line.EndsWith("-->"))
            {
                contentStartIndex = i;
                break;
            }

            // Extract content between <!-- and -->
            var commentContent = line.Substring(4, line.Length - 7).Trim();

            // Parse @page directive
            if (commentContent.StartsWith("@page "))
            {
                var pageMatch = Regex.Match(commentContent, @"@page\s+""([^""]+)""");
                if (pageMatch.Success)
                {
                    config.PageDirective = pageMatch.Groups[1].Value;
                    Console.WriteLine($"Found @page directive: {config.PageDirective}");
                }
            }
            // Parse other configuration properties
            else if (commentContent.StartsWith("title:"))
            {
                config.Title = commentContent.Substring(6).Trim().Trim('"');
            }
            else if (commentContent.StartsWith("layout:"))
            {
                config.Layout = commentContent.Substring(7).Trim().Trim('"');
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
                config.Description = commentContent.Substring(12).Trim().Trim('"');
            }
            else if (commentContent.StartsWith("tags:"))
            {
                var tagsContent = commentContent.Substring(5).Trim();
                // Simple parsing for comma-separated tags
                config.Tags = tagsContent.Split(',').Select(t => t.Trim().Trim('"')).ToList();
            }
        }

        // Return remaining content after configuration comments
        var remainingContent = string.Join('\n', lines.Skip(contentStartIndex));
        return (config, remainingContent);
    }

    /// <summary>
    /// Merges HTML comment configuration with YAML frontmatter configuration
    /// HTML comment configuration takes precedence
    /// </summary>
    private MarkdownFrontmatter MergeConfigurations(HtmlCommentConfiguration? htmlConfig, MarkdownFrontmatter? frontmatter)
    {
        var merged = new MarkdownFrontmatter();

        // Start with frontmatter values
        if (frontmatter != null)
        {
            merged.Route = frontmatter.Route;
            merged.Title = frontmatter.Title;
            merged.Layout = frontmatter.Layout;
            merged.ShowTitle = frontmatter.ShowTitle;
            merged.Description = frontmatter.Description;
            merged.Tags = frontmatter.Tags;
        }

        // Override with HTML comment configuration (takes precedence)
        if (htmlConfig != null)
        {
            if (!string.IsNullOrEmpty(htmlConfig.PageDirective))
            {
                merged.Route = htmlConfig.PageDirective;
            }
            if (!string.IsNullOrEmpty(htmlConfig.Title))
            {
                merged.Title = htmlConfig.Title;
            }
            if (!string.IsNullOrEmpty(htmlConfig.Layout))
            {
                merged.Layout = htmlConfig.Layout;
            }
            if (htmlConfig.ShowTitle.HasValue)
            {
                merged.ShowTitle = htmlConfig.ShowTitle;
            }
            if (!string.IsNullOrEmpty(htmlConfig.Description))
            {
                merged.Description = htmlConfig.Description;
            }
            if (htmlConfig.Tags?.Any() == true)
            {
                merged.Tags = htmlConfig.Tags;
            }
        }

        return merged;
    }

    /// <summary>
    /// Generates a route from filename (e.g., "getting-started" -> "/docs/getting-started")
    /// </summary>
    private string GenerateRouteFromFilename(string filename)
    {
        return $"/docs/{filename.ToLowerInvariant()}";
    }

    /// <summary>
    /// Processes a route to ensure it has the docs/ prefix
    /// </summary>
    private string ProcessRoute(string route)
    {
        // Ensure route starts with /
        if (!route.StartsWith("/"))
        {
            route = "/" + route;
        }

        // Add docs/ prefix if not already present and not root route
        if (route != "/" && !route.StartsWith("/docs/"))
        {
            route = "/docs" + route;
        }

        return route;
    }

    /// <summary>
    /// Generates a page title from filename (e.g., "getting-started" -> "Getting Started")
    /// </summary>
    private string GenerateTitleFromFilename(string filename)
    {
        return string.Join(" ", filename.Split('-', '_'))
            .Replace(" ", " ")
            .Trim()
            .Split(' ')
            .Select(word => char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant())
            .Aggregate((a, b) => $"{a} {b}");
    }

    /// <summary>
    /// Generates the Razor filename (e.g., "about" -> "About.razor")
    /// </summary>
    private string GenerateRazorFileName(string markdownFileName)
    {
        var pascalCase = string.Join("", markdownFileName.Split('-', '_')
            .Select(word => char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant()));

        return $"{pascalCase}.razor";
    }

    /// <summary>
    /// Generates the complete Razor page content
    /// </summary>
    private string GenerateRazorPageContent(string route, string title, string markdownFile, MarkdownFrontmatter? frontmatter)
    {
        var sb = new StringBuilder();

        // Page directive
        sb.AppendLine($"@page \"{route}\"");

        // Using statements
        sb.AppendLine("@using MarkdownToRazor.Components");
        sb.AppendLine();

        // Generated file comment
        sb.AppendLine("@* This file was auto-generated from markdown. Do not edit directly. *@");
        sb.AppendLine();

        // Page title
        sb.AppendLine($"<PageTitle>{title}</PageTitle>");
        sb.AppendLine();

        // Optional page description meta tag
        if (!string.IsNullOrEmpty(frontmatter?.Description))
        {
            sb.AppendLine($"<HeadContent>");
            sb.AppendLine($"    <meta name=\"description\" content=\"{frontmatter.Description}\" />");
            sb.AppendLine($"</HeadContent>");
            sb.AppendLine();
        }

        // Optional page title display
        if (frontmatter?.ShowTitle != false)
        {
            sb.AppendLine($"<h1>{title}</h1>");
            sb.AppendLine();
        }

        // Markdown section component
        sb.AppendLine($"<MarkdownSection FromAsset=\"{markdownFile.Replace('\\', '/')}\" />");

        // Optional tags section
        if (frontmatter?.Tags?.Any() == true)
        {
            sb.AppendLine();
            sb.AppendLine("<div class=\"page-tags\" style=\"margin-top: 2rem; padding-top: 1rem; border-top: 1px solid var(--neutral-stroke-divider-rest);\">");
            sb.AppendLine("    <strong>Tags:</strong>");
            foreach (var tag in frontmatter.Tags)
            {
                sb.AppendLine($"    <span class=\"tag\" style=\"margin-left: 0.5rem; padding: 0.25rem 0.5rem; background: var(--accent-fill-rest); color: var(--neutral-foreground-on-accent); border-radius: 0.25rem; font-size: 0.875rem;\">{tag}</span>");
            }
            sb.AppendLine("</div>");
        }

        return sb.ToString();
    }
}
