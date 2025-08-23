namespace MDFileToRazor.Components.Configuration;

/// <summary>
/// Configuration options for MDFileToRazor services.
/// </summary>
public class MdFileToRazorOptions
{
    /// <summary>
    /// The source directory where markdown files are located (relative to project root).
    /// Default: "MDFilesToConvert"
    /// </summary>
    public string SourceDirectory { get; set; } = "MDFilesToConvert";

    /// <summary>
    /// The output directory where generated Razor pages will be created (relative to project root).
    /// Default: "Pages/Generated"
    /// </summary>
    public string OutputDirectory { get; set; } = "Pages/Generated";

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
    /// The default layout to use for generated pages when not specified in frontmatter.
    /// Default: null (uses Blazor default)
    /// </summary>
    public string? DefaultLayout { get; set; }

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
        return Path.Combine(contentRootPath, SourceDirectory);
    }

    /// <summary>
    /// Gets the absolute output directory path relative to the content root.
    /// </summary>
    /// <param name="contentRootPath">The application's content root path</param>
    /// <returns>The absolute path to the output directory</returns>
    public string GetAbsoluteOutputPath(string contentRootPath)
    {
        return Path.Combine(contentRootPath, OutputDirectory);
    }

    /// <summary>
    /// Validates the configuration options and throws exceptions for invalid settings.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(SourceDirectory))
            throw new ArgumentException("SourceDirectory cannot be null or empty.", nameof(SourceDirectory));

        if (string.IsNullOrWhiteSpace(OutputDirectory))
            throw new ArgumentException("OutputDirectory cannot be null or empty.", nameof(OutputDirectory));

        if (string.IsNullOrWhiteSpace(FilePattern))
            throw new ArgumentException("FilePattern cannot be null or empty.", nameof(FilePattern));

        if (!EnableHtmlCommentConfiguration && !EnableYamlFrontmatter)
            throw new ArgumentException("At least one configuration method (HTML comments or YAML frontmatter) must be enabled.");
    }
}
