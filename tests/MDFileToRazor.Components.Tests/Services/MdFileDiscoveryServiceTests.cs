using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using MDFileToRazor.Components.Configuration;
using MDFileToRazor.Components.Services;

namespace MDFileToRazor.Components.Tests.Services;

public class MdFileDiscoveryServiceTests : IDisposable
{
    private readonly Mock<IHostEnvironment> _mockHostEnvironment;
    private readonly MdFileToRazorOptions _options;
    private readonly string _testContentRoot;
    private readonly string _testSourceDir;

    public MdFileDiscoveryServiceTests()
    {
        _mockHostEnvironment = new Mock<IHostEnvironment>();
        _testContentRoot = Path.GetTempPath();
        _testSourceDir = Path.Combine(_testContentRoot, "TestMarkdownFiles");
        
        _mockHostEnvironment.Setup(x => x.ContentRootPath).Returns(_testContentRoot);
        
        _options = new MdFileToRazorOptions
        {
            SourceDirectory = "TestMarkdownFiles",
            OutputDirectory = "Pages/Generated",
            FilePattern = "*.md",
            SearchRecursively = false
        };

        // Create test directory and files
        Directory.CreateDirectory(_testSourceDir);
        CreateTestMarkdownFiles();
    }

    private void CreateTestMarkdownFiles()
    {
        // Clean up any existing files
        if (Directory.Exists(_testSourceDir))
        {
            Directory.Delete(_testSourceDir, true);
        }
        Directory.CreateDirectory(_testSourceDir);

        // Create test markdown files
        File.WriteAllText(Path.Combine(_testSourceDir, "index.md"), "# Home Page");
        File.WriteAllText(Path.Combine(_testSourceDir, "about.md"), "# About Page");
        File.WriteAllText(Path.Combine(_testSourceDir, "getting-started.md"), "# Getting Started");
        File.WriteAllText(Path.Combine(_testSourceDir, "user_guide.md"), "# User Guide");
        File.WriteAllText(Path.Combine(_testSourceDir, "API Reference.md"), "# API Reference");
    }

    private MdFileDiscoveryService CreateService()
    {
        var optionsWrapper = new OptionsWrapper<MdFileToRazorOptions>(_options);
        return new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);
    }

    [Fact]
    public void DiscoverMarkdownFilesWithRoutes_ReturnsCorrectFileToRouteMapping()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = service.DiscoverMarkdownFilesWithRoutes();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(5, result.Count);

        // Check specific mappings
        Assert.Contains("index.md", result.Keys);
        Assert.Equal("/", result["index.md"]);

        Assert.Contains("about.md", result.Keys);
        Assert.Equal("/about", result["about.md"]);

        Assert.Contains("getting-started.md", result.Keys);
        Assert.Equal("/getting-started", result["getting-started.md"]);

        Assert.Contains("user_guide.md", result.Keys);
        Assert.Equal("/user-guide", result["user_guide.md"]);

        Assert.Contains("API Reference.md", result.Keys);
        Assert.Equal("/api-reference", result["API Reference.md"]);
    }

    [Fact]
    public async Task DiscoverMarkdownFilesWithRoutesAsync_ReturnsCorrectFileToRouteMapping()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.DiscoverMarkdownFilesWithRoutesAsync();

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(5, result.Count);

        // Check that async and sync methods return the same results
        var syncResult = service.DiscoverMarkdownFilesWithRoutes();
        Assert.Equal(syncResult.Count, result.Count);
        
        foreach (var kvp in syncResult)
        {
            Assert.Contains(kvp.Key, result.Keys);
            Assert.Equal(kvp.Value, result[kvp.Key]);
        }
    }

    [Fact]
    public void DiscoverMarkdownFilesWithRoutes_EmptyDirectory_ReturnsEmptyDictionary()
    {
        // Arrange
        var emptyOptions = new MdFileToRazorOptions
        {
            SourceDirectory = "NonExistentDirectory",
            OutputDirectory = "Pages/Generated",
            FilePattern = "*.md"
        };
        var optionsWrapper = new OptionsWrapper<MdFileToRazorOptions>(emptyOptions);
        var service = new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);

        // Act
        var result = service.DiscoverMarkdownFilesWithRoutes();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void DiscoverMarkdownFilesWithRoutes_SpecialCharacters_GeneratesValidRoutes()
    {
        // Arrange
        var specialFile = Path.Combine(_testSourceDir, "Special File & More.md");
        File.WriteAllText(specialFile, "# Special Content");
        
        var service = CreateService();

        // Act
        var result = service.DiscoverMarkdownFilesWithRoutes();

        // Assert
        Assert.Contains("Special File & More.md", result.Keys);
        Assert.Equal("/special-file-&-more", result["Special File & More.md"]);
    }

    [Fact]
    public void DiscoverMarkdownFilesWithRoutes_ComplexFilenames_GeneratesValidRoutes()
    {
        // Arrange - Create additional test files with complex names
        File.WriteAllText(Path.Combine(_testSourceDir, "Multiple   Spaces.md"), "# Multiple Spaces");
        File.WriteAllText(Path.Combine(_testSourceDir, "under_score_heavy.md"), "# Underscore Heavy");
        File.WriteAllText(Path.Combine(_testSourceDir, "Mixed-Case_And   Spaces.md"), "# Mixed Case");
        File.WriteAllText(Path.Combine(_testSourceDir, "HOME.md"), "# Home Uppercase");
        
        var service = CreateService();

        // Act
        var result = service.DiscoverMarkdownFilesWithRoutes();

        // Assert
        Assert.Contains("Multiple   Spaces.md", result.Keys);
        Assert.Equal("/multiple-spaces", result["Multiple   Spaces.md"]);
        
        Assert.Contains("under_score_heavy.md", result.Keys);
        Assert.Equal("/under-score-heavy", result["under_score_heavy.md"]);
        
        Assert.Contains("Mixed-Case_And   Spaces.md", result.Keys);
        Assert.Equal("/mixed-case-and-spaces", result["Mixed-Case_And   Spaces.md"]);
        
        // Test with different uppercase filename
        Assert.Contains("HOME.md", result.Keys);
        Assert.Equal("/home", result["HOME.md"]);
    }

    public void Dispose()
    {
        // Clean up test files
        if (Directory.Exists(_testSourceDir))
        {
            try
            {
                Directory.Delete(_testSourceDir, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
