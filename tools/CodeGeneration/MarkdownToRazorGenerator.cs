using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MDFIleTORazor.CodeGeneration;

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

        // Parse frontmatter if exists
        var (frontmatter, contentWithoutFrontmatter) = ParseFrontmatter(markdownContent);

        // Generate route from filename or use frontmatter route
        var route = frontmatter?.Route ?? GenerateRouteFromFilename(fileNameWithoutExtension);

        // Generate page title
        var title = frontmatter?.Title ?? GenerateTitleFromFilename(fileNameWithoutExtension);

        // Generate the Razor page content
        var razorContent = GenerateRazorPageContent(route, title, relativePath, frontmatter);

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
    /// Generates a route from filename (e.g., "getting-started" -> "/getting-started")
    /// </summary>
    private string GenerateRouteFromFilename(string filename)
    {
        return $"/{filename.ToLowerInvariant()}";
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
        sb.AppendLine("@using MDFIleTORazor.Components");
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
