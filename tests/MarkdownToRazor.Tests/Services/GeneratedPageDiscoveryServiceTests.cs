using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;
using Moq;
using MarkdownToRazor.Configuration;
using MarkdownToRazor.Services;

namespace MarkdownToRazor.Tests.Services;

public class GeneratedPageDiscoveryServiceTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _tempSourceDirectory;
    private readonly string _tempOutputDirectory;
    private readonly Mock<IHostEnvironment> _mockHostEnvironment;
    private readonly Mock<IMdFileDiscoveryService> _mockMdFileDiscoveryService;
    private readonly IOptions<MarkdownToRazorOptions> _options;

    public GeneratedPageDiscoveryServiceTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _tempSourceDirectory = Path.Combine(_tempDirectory, "source");
        _tempOutputDirectory = Path.Combine(_tempDirectory, "output");

        Directory.CreateDirectory(_tempSourceDirectory);
        Directory.CreateDirectory(_tempOutputDirectory);

        _mockHostEnvironment = new Mock<IHostEnvironment>();
        _mockHostEnvironment.Setup(x => x.ContentRootPath).Returns(_tempDirectory);

        _mockMdFileDiscoveryService = new Mock<IMdFileDiscoveryService>();

        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = _tempSourceDirectory,
            OutputDirectory = _tempOutputDirectory
        };
        _options = Options.Create(options);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Fact]
    public async Task DiscoverGeneratedPagesAsync_WithValidMarkdownFiles_ReturnsCorrectPageInfo()
    {
        // Arrange
        await CreateTestMarkdownFile("test1.md", """
            ---
            title: Test Page 1
            route: /custom/test1
            description: A test page
            tags: ["test", "page"]
            showTitle: true
            ---
            # Test Content 1
            This is test content.
            """);

        await CreateTestMarkdownFile("test2.md", """
            ---
            title: Test Page 2
            description: Another test page
            ---
            # Test Content 2
            More test content.
            """);

        await CreateTestMarkdownFile("no-frontmatter.md", "# Simple Page\nNo frontmatter here.");

        // Setup the mock to return our test files
        var markdownFiles = new[]
        {
            Path.Combine(_tempSourceDirectory, "test1.md"),
            Path.Combine(_tempSourceDirectory, "test2.md"),
            Path.Combine(_tempSourceDirectory, "no-frontmatter.md")
        };
        _mockMdFileDiscoveryService.Setup(x => x.DiscoverMarkdownFilesAsync()).ReturnsAsync(markdownFiles);

        var service = new GeneratedPageDiscoveryService(_options, _mockHostEnvironment.Object, _mockMdFileDiscoveryService.Object);

        // Act
        var pages = await service.DiscoverGeneratedPagesAsync();
        var pagesList = pages.ToList();

        // Assert
        Assert.Equal(3, pagesList.Count);

        var customRoutePage = pagesList.FirstOrDefault(p => p.Route == "/custom/test1");
        Assert.NotNull(customRoutePage);
        Assert.Equal("Test Page 1", customRoutePage.Title);
        Assert.Equal("A test page", customRoutePage.Description);
        Assert.Equal(new[] { "test", "page" }, customRoutePage.Tags);
        Assert.True(customRoutePage.ShowTitle);

        var defaultRoutePage = pagesList.FirstOrDefault(p => p.Route == "/docs/test2");
        Assert.NotNull(defaultRoutePage);
        Assert.Equal("Test Page 2", defaultRoutePage.Title);
        Assert.Equal("Another test page", defaultRoutePage.Description);
        Assert.Empty(defaultRoutePage.Tags);
        Assert.True(defaultRoutePage.ShowTitle); // Default value

        var noFrontmatterPage = pagesList.FirstOrDefault(p => p.Route == "/docs/no-frontmatter");
        Assert.NotNull(noFrontmatterPage);
        Assert.Equal("No Frontmatter", noFrontmatterPage.Title); // Generated from filename
        Assert.Null(noFrontmatterPage.Description);
        Assert.Empty(noFrontmatterPage.Tags);
        Assert.True(noFrontmatterPage.ShowTitle);
    }

    [Fact]
    public async Task DiscoverGeneratedPagesAsync_WithNestedDirectories_ReturnsCorrectRoutes()
    {
        // Arrange
        var nestedDir = Path.Combine(_tempSourceDirectory, "subfolder");
        Directory.CreateDirectory(nestedDir);

        await CreateTestMarkdownFile("root.md", "# Root Page");
        await CreateTestMarkdownFile(Path.Combine("subfolder", "nested.md"), """
            ---
            title: Nested Page
            ---
            # Nested Content
            """);

        var markdownFiles = new[]
        {
            Path.Combine(_tempSourceDirectory, "root.md"),
            Path.Combine(_tempSourceDirectory, "subfolder", "nested.md")
        };
        _mockMdFileDiscoveryService.Setup(x => x.DiscoverMarkdownFilesAsync()).ReturnsAsync(markdownFiles);

        var service = new GeneratedPageDiscoveryService(_options, _mockHostEnvironment.Object, _mockMdFileDiscoveryService.Object);

        // Act
        var pages = await service.DiscoverGeneratedPagesAsync();
        var pagesList = pages.ToList();

        // Assert
        Assert.Equal(2, pagesList.Count);

        var rootPage = pagesList.FirstOrDefault(p => p.Route == "/docs/root");
        Assert.NotNull(rootPage);

        var nestedPage = pagesList.FirstOrDefault(p => p.Route == "/docs/nested");
        Assert.NotNull(nestedPage);
        Assert.Equal("Nested Page", nestedPage.Title);
    }

    [Fact]
    public async Task DiscoverGeneratedPagesAsync_WithInvalidYaml_HandlesGracefully()
    {
        // Arrange
        await CreateTestMarkdownFile("invalid-yaml.md", """
            ---
            title: Test
            invalid: [unclosed array
            ---
            # Content
            """);

        var markdownFiles = new[] { Path.Combine(_tempSourceDirectory, "invalid-yaml.md") };
        _mockMdFileDiscoveryService.Setup(x => x.DiscoverMarkdownFilesAsync()).ReturnsAsync(markdownFiles);

        var service = new GeneratedPageDiscoveryService(_options, _mockHostEnvironment.Object, _mockMdFileDiscoveryService.Object);

        // Act
        var pages = await service.DiscoverGeneratedPagesAsync();
        var pagesList = pages.ToList();

        // Assert
        Assert.Single(pagesList);
        var page = pagesList[0];
        Assert.Equal("/docs/invalid-yaml", page.Route);
        Assert.Equal("Invalid Yaml", page.Title); // Fallback to filename
    }

    [Fact]
    public async Task DiscoverGeneratedPagesAsync_WithEmptyDirectory_ReturnsEmptyCollection()
    {
        // Arrange
        _mockMdFileDiscoveryService.Setup(x => x.DiscoverMarkdownFilesAsync()).ReturnsAsync(Array.Empty<string>());
        var service = new GeneratedPageDiscoveryService(_options, _mockHostEnvironment.Object, _mockMdFileDiscoveryService.Object);

        // Act
        var pages = await service.DiscoverGeneratedPagesAsync();

        // Assert
        Assert.Empty(pages);
    }

    [Fact]
    public async Task GetPageInfoAsync_WithExistingFile_ReturnsCorrectPageInfo()
    {
        // Arrange
        await CreateTestMarkdownFile("test.md", """
            ---
            title: Test Page
            route: /custom/route
            ---
            # Test
            """);

        var service = new GeneratedPageDiscoveryService(_options, _mockHostEnvironment.Object, _mockMdFileDiscoveryService.Object);

        // Act
        var page = await service.GetPageInfoAsync(Path.Combine(_tempSourceDirectory, "test.md"));

        // Assert
        Assert.NotNull(page);
        Assert.Equal("/custom/route", page.Route);
        Assert.Equal("Test Page", page.Title);
    }

    [Fact]
    public async Task GetPageInfoAsync_WithNonExistentFile_ReturnsNull()
    {
        // Arrange
        var service = new GeneratedPageDiscoveryService(_options, _mockHostEnvironment.Object, _mockMdFileDiscoveryService.Object);

        // Act
        var page = await service.GetPageInfoAsync("non-existent.md");

        // Assert
        Assert.Null(page);
    }

    [Fact]
    public async Task GetPagesByTagsAsync_WithTaggedPages_ReturnsCorrectGrouping()
    {
        // Arrange
        await CreateTestMarkdownFile("page1.md", """
            ---
            title: Page 1
            tags: ["test", "demo"]
            ---
            # Page 1
            """);

        await CreateTestMarkdownFile("page2.md", """
            ---
            title: Page 2
            tags: ["test", "tutorial"]
            ---
            # Page 2
            """);

        await CreateTestMarkdownFile("page3.md", """
            ---
            title: Page 3
            tags: ["demo"]
            ---
            # Page 3
            """);

        var markdownFiles = new[]
        {
            Path.Combine(_tempSourceDirectory, "page1.md"),
            Path.Combine(_tempSourceDirectory, "page2.md"),
            Path.Combine(_tempSourceDirectory, "page3.md")
        };
        _mockMdFileDiscoveryService.Setup(x => x.DiscoverMarkdownFilesAsync()).ReturnsAsync(markdownFiles);

        var service = new GeneratedPageDiscoveryService(_options, _mockHostEnvironment.Object, _mockMdFileDiscoveryService.Object);

        // Act
        var pagesByTags = await service.GetPagesByTagsAsync();

        // Assert
        Assert.Equal(3, pagesByTags.Keys.Count); // test, demo, tutorial
        Assert.Contains("test", pagesByTags.Keys);
        Assert.Contains("demo", pagesByTags.Keys);
        Assert.Contains("tutorial", pagesByTags.Keys);

        Assert.Equal(2, pagesByTags["test"].Count);
        Assert.Equal(2, pagesByTags["demo"].Count);
        Assert.Single(pagesByTags["tutorial"]);
    }

    private async Task CreateTestMarkdownFile(string fileName, string content)
    {
        var filePath = Path.Combine(_tempSourceDirectory, fileName);
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        await File.WriteAllTextAsync(filePath, content);
    }
}
