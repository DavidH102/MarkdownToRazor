namespace MarkdownToRazor.Configuration;

/// <summary>
/// Configuration options for MarkdownToRazor services.
/// </summary>
public class MarkdownToRazorOptions
{
    /// <summary>
    /// The source directory where markdown files are located (relative to project root).
    /// Default: "MDFilesToConvert"
    /// </summary>
    public string SourceDirectory { get; set; } = "MDFilesToConvert";

    /// <summary>
    /// The output directory where generated Razor pages will be created (relative to project root).
    /// Only required for build-time code generation scenarios.
    /// Default: null (not needed for runtime-only scenarios)
    /// </summary>
    public string? OutputDirectory { get; set; }

    /// <summary>
    /// The file pattern to search for markdown files.
    /// Default: "*.md"
    /// </summary>
    public string FilePattern { get; set; } = "*.md";

    /// <summary>
    /// Whether to search for markdown files recursively in subdirectories.
    /// Default: true
    /// </summary>
    public bool SearchRecursively { get; set; } = true;

    /// <summary>
    /// Whether to enable HTML comment configuration parsing.
    /// Default: true
    /// </summary>
    public bool EnableHtmlCommentConfiguration { get; set; } = true;

    /// <summary>
    /// Whether to enable YAML frontmatter configuration parsing.
    /// Default: true
    /// </summary>
    public bool EnableYamlFrontmatter { get; set; } = true;

    /// <summary>
    /// The base URL path to prefix all generated routes (e.g., "/docs").
    /// Default: null (no prefix)
    /// </summary>
    public string? BaseRoutePath { get; set; }

    /// <summary>
    /// Gets the absolute source directory path relative to the content root.
    /// </summary>
    /// <param name="contentRootPath">The application's content root path</param>
    /// <returns>The absolute path to the source directory</returns>
    public string GetAbsoluteSourcePath(string contentRootPath)
    {
        // Handle absolute paths - if SourceDirectory is already absolute, use it as-is
        if (Path.IsPathRooted(SourceDirectory))
        {
            return Path.GetFullPath(SourceDirectory);
        }

        // Handle relative paths - combine with content root
        return Path.GetFullPath(Path.Combine(contentRootPath, SourceDirectory));
    }

    /// <summary>
    /// Gets the absolute output directory path relative to the content root.
    /// </summary>
    /// <param name="contentRootPath">The application's content root path</param>
    /// <returns>The absolute path to the output directory, or null if OutputDirectory is not set</returns>
    public string? GetAbsoluteOutputPath(string contentRootPath)
    {
        // Return null if OutputDirectory is not set (runtime-only scenarios)
        if (string.IsNullOrWhiteSpace(OutputDirectory))
        {
            return null;
        }

        // Handle absolute paths - if OutputDirectory is already absolute, use it as-is
        if (Path.IsPathRooted(OutputDirectory))
        {
            return Path.GetFullPath(OutputDirectory);
        }

        // Handle relative paths - combine with content root
        return Path.GetFullPath(Path.Combine(contentRootPath, OutputDirectory));
    }

    /// <summary>
    /// Validates the configuration options and throws exceptions for invalid settings.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SourceDirectory))
            throw new ArgumentException("SourceDirectory cannot be null or empty.", nameof(SourceDirectory));

        // OutputDirectory is optional for runtime-only scenarios
        // No validation needed since it can be null

        if (string.IsNullOrWhiteSpace(FilePattern))
            throw new ArgumentException("FilePattern cannot be null or empty.", nameof(FilePattern));

        if (!EnableHtmlCommentConfiguration && !EnableYamlFrontmatter)
            throw new ArgumentException("At least one configuration method (HTML comments or YAML frontmatter) must be enabled.");
    }
}
