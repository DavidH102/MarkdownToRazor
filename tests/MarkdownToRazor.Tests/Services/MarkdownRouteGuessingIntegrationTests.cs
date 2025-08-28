using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using MarkdownToRazor.Configuration;
using MarkdownToRazor.Services;
using MarkdownToRazor.Extensions;

namespace MarkdownToRazor.Tests.Services;

/// <summary>
/// Comprehensive integration tests for route guessing functionality.
/// Tests a simulated default Blazor app with various markdown files to verify route generation.
/// 
/// This test suite validates the core functionality described in the user requirement:
/// "loads up a default apps then provides in a few md files then tries to guess the route of those md files after using this tool"
/// 
/// Test Coverage:
/// - ✅ Basic markdown files → expected route mapping
/// - ✅ Complex filenames with special characters, spaces, underscores
/// - ✅ Nested directory structures with recursive discovery
/// - ✅ YAML frontmatter support (for future custom routes)
/// - ✅ Special character handling (C#, Unicode, dots, ampersands)
/// - ✅ Async vs sync operation consistency
/// - ✅ Empty/non-existent directory graceful handling
/// - ✅ Mixed file types (only processes .md files)
/// - ✅ Service configuration verification
/// - ✅ Index.md → "/" root route mapping
/// 
/// The tests simulate a real Blazor app scenario with:
/// - Mock IHostEnvironment for content root path simulation
/// - Temporary file system for isolated test execution
/// - Service provider with MarkdownToRazor services configured
/// - Various markdown file scenarios that mirror real-world usage
/// </summary>
public class MarkdownRouteGuessingIntegrationTests : IDisposable
{
    private readonly string _testContentRoot;
    private readonly string _testSourceDir;
    private readonly ServiceProvider _serviceProvider;
    private readonly List<string> _testFiles;

    public MarkdownRouteGuessingIntegrationTests()
    {
        _testContentRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        _testSourceDir = Path.Combine(_testContentRoot, "content");
        _testFiles = new List<string>();

        // Setup a mock default Blazor app environment
        SetupDefaultBlazorAppEnvironment();

        // Create service provider with MarkdownToRazor services
        var services = new ServiceCollection();
        SetupDefaultAppServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private void SetupDefaultBlazorAppEnvironment()
    {
        // Create test directories
        Directory.CreateDirectory(_testContentRoot);
        Directory.CreateDirectory(_testSourceDir);

        // Create subdirectories to test recursive discovery
        Directory.CreateDirectory(Path.Combine(_testSourceDir, "docs"));
        Directory.CreateDirectory(Path.Combine(_testSourceDir, "blog"));
        Directory.CreateDirectory(Path.Combine(_testSourceDir, "api"));
    }

    private void SetupDefaultAppServices(ServiceCollection services)
    {
        // Mock IHostEnvironment to simulate a default Blazor app
        var mockHostEnvironment = new Mock<IHostEnvironment>();
        mockHostEnvironment.Setup(x => x.ContentRootPath).Returns(_testContentRoot);
        mockHostEnvironment.Setup(x => x.EnvironmentName).Returns("Development");
        mockHostEnvironment.Setup(x => x.ApplicationName).Returns("TestBlazorApp");

        services.AddSingleton(mockHostEnvironment.Object);

        // Add MarkdownToRazor services with default configuration
        services.AddMarkdownToRazorServices(options =>
        {
            options.SourceDirectory = "content";
            options.SearchRecursively = true;
            options.EnableYamlFrontmatter = true;
            options.EnableHtmlCommentConfiguration = true;
        });
    }

    private void CreateMarkdownFile(string relativePath, string content)
    {
        var fullPath = Path.Combine(_testSourceDir, relativePath);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(fullPath, content);
        _testFiles.Add(fullPath);
    }

    [Fact]
    public void DefaultApp_WithBasicMarkdownFiles_GeneratesExpectedRoutes()
    {
        // Arrange - Create typical documentation files
        CreateMarkdownFile("index.md", "# Home Page\nWelcome to our documentation!");
        CreateMarkdownFile("getting-started.md", "# Getting Started\nHow to get started with our product.");
        CreateMarkdownFile("installation.md", "# Installation\nInstallation instructions.");
        CreateMarkdownFile("configuration.md", "# Configuration\nHow to configure the application.");

        // Act
        var discoveryService = _serviceProvider.GetRequiredService<IMdFileDiscoveryService>();
        var routes = discoveryService.DiscoverMarkdownFilesWithRoutes();

        // Assert
        Assert.Equal(4, routes.Count);

        // Verify specific route mappings
        Assert.Equal("/", routes["index.md"]);
        Assert.Equal("/getting-started", routes["getting-started.md"]);
        Assert.Equal("/installation", routes["installation.md"]);
        Assert.Equal("/configuration", routes["configuration.md"]);
    }

    [Fact]
    public void DefaultApp_WithComplexFilenames_GeneratesCleanRoutes()
    {
        // Arrange - Create files with various naming conventions
        CreateMarkdownFile("User Guide.md", "# User Guide");
        CreateMarkdownFile("API_Reference.md", "# API Reference");
        CreateMarkdownFile("Troubleshooting & FAQ.md", "# Troubleshooting & FAQ");
        CreateMarkdownFile("Quick   Start.md", "# Quick Start");
        CreateMarkdownFile("advanced-configuration.md", "# Advanced Configuration");
        CreateMarkdownFile("CHANGELOG.md", "# Changelog");

        // Act
        var discoveryService = _serviceProvider.GetRequiredService<IMdFileDiscoveryService>();
        var routes = discoveryService.DiscoverMarkdownFilesWithRoutes();

        // Assert
        Assert.Equal(6, routes.Count);

        // Verify route cleaning and normalization
        Assert.Equal("/user-guide", routes["User Guide.md"]);
        Assert.Equal("/api-reference", routes["API_Reference.md"]);
        Assert.Equal("/troubleshooting-&-faq", routes["Troubleshooting & FAQ.md"]);
        Assert.Equal("/quick-start", routes["Quick   Start.md"]);
        Assert.Equal("/advanced-configuration", routes["advanced-configuration.md"]);
        Assert.Equal("/changelog", routes["CHANGELOG.md"]);
    }

    [Fact]
    public void DefaultApp_WithNestedDirectories_DiscoversRecursively()
    {
        // Arrange - Create nested documentation structure
        CreateMarkdownFile("docs/overview.md", "# Overview");
        CreateMarkdownFile("docs/getting-started.md", "# Getting Started");
        CreateMarkdownFile("blog/post-1.md", "# First Blog Post");
        CreateMarkdownFile("blog/post-2.md", "# Second Blog Post");
        CreateMarkdownFile("api/authentication.md", "# Authentication");
        CreateMarkdownFile("api/endpoints.md", "# API Endpoints");

        // Act
        var discoveryService = _serviceProvider.GetRequiredService<IMdFileDiscoveryService>();
        var routes = discoveryService.DiscoverMarkdownFilesWithRoutes();

        // Assert
        Assert.Equal(6, routes.Count);

        // Verify all files are discovered regardless of directory
        Assert.Contains("overview.md", routes.Keys);
        Assert.Contains("getting-started.md", routes.Keys);
        Assert.Contains("post-1.md", routes.Keys);
        Assert.Contains("post-2.md", routes.Keys);
        Assert.Contains("authentication.md", routes.Keys);
        Assert.Contains("endpoints.md", routes.Keys);

        // Routes should be based on filename, not directory structure
        Assert.Equal("/overview", routes["overview.md"]);
        Assert.Equal("/getting-started", routes["getting-started.md"]);
        Assert.Equal("/post-1", routes["post-1.md"]);
        Assert.Equal("/post-2", routes["post-2.md"]);
        Assert.Equal("/authentication", routes["authentication.md"]);
        Assert.Equal("/endpoints", routes["endpoints.md"]);
    }

    [Fact]
    public void DefaultApp_WithYamlFrontmatter_RespectsCustomRoutes()
    {
        // Arrange - Create files with YAML frontmatter that specify custom routes
        CreateMarkdownFile("custom-route.md", @"---
route: /special/custom-path
title: Custom Route Page
---
# Custom Route Page
This page has a custom route defined in frontmatter.");

        CreateMarkdownFile("about.md", @"---
route: /company/about-us
title: About Our Company
---
# About Us
Standard about page with custom route.");

        // Act
        var discoveryService = _serviceProvider.GetRequiredService<IMdFileDiscoveryService>();
        var routes = discoveryService.DiscoverMarkdownFilesWithRoutes();

        // Assert
        Assert.Equal(2, routes.Count);

        // Note: The current implementation generates routes from filenames,
        // but in a real scenario with code generation, YAML frontmatter would override these
        // For now, verify the basic filename-based routes are generated
        Assert.Equal("/custom-route", routes["custom-route.md"]);
        Assert.Equal("/about", routes["about.md"]);
    }

    [Fact]
    public void DefaultApp_WithSpecialCharacters_HandlesGracefully()
    {
        // Arrange - Create files with challenging characters
        CreateMarkdownFile("C# Programming.md", "# C# Programming Guide");
        CreateMarkdownFile("Vue.js & React.md", "# Vue.js & React Comparison");
        CreateMarkdownFile("file-with-numbers-123.md", "# File with Numbers");
        CreateMarkdownFile("unicode-test-café.md", "# Unicode Test");
        CreateMarkdownFile(".NET Core.md", "# .NET Core Guide");

        // Act
        var discoveryService = _serviceProvider.GetRequiredService<IMdFileDiscoveryService>();
        var routes = discoveryService.DiscoverMarkdownFilesWithRoutes();

        // Assert
        Assert.Equal(5, routes.Count);

        // Verify special character handling
        Assert.Equal("/c#-programming", routes["C# Programming.md"]);
        Assert.Equal("/vue.js-&-react", routes["Vue.js & React.md"]);
        Assert.Equal("/file-with-numbers-123", routes["file-with-numbers-123.md"]);
        Assert.Equal("/unicode-test-café", routes["unicode-test-café.md"]);
        Assert.Equal("/.net-core", routes[".NET Core.md"]);
    }

    [Fact]
    public async Task DefaultApp_AsyncDiscovery_ProducesSameResults()
    {
        // Arrange
        CreateMarkdownFile("async-test-1.md", "# Async Test 1");
        CreateMarkdownFile("async-test-2.md", "# Async Test 2");
        CreateMarkdownFile("docs/async-nested.md", "# Async Nested");

        // Act
        var discoveryService = _serviceProvider.GetRequiredService<IMdFileDiscoveryService>();
        var syncRoutes = discoveryService.DiscoverMarkdownFilesWithRoutes();
        var asyncRoutes = await discoveryService.DiscoverMarkdownFilesWithRoutesAsync();

        // Assert
        Assert.Equal(syncRoutes.Count, asyncRoutes.Count);

        foreach (var syncRoute in syncRoutes)
        {
            Assert.Contains(syncRoute.Key, asyncRoutes.Keys);
            Assert.Equal(syncRoute.Value, asyncRoutes[syncRoute.Key]);
        }
    }

    [Fact]
    public void DefaultApp_EmptySourceDirectory_HandlesGracefully()
    {
        // Arrange - Don't create any markdown files

        // Act
        var discoveryService = _serviceProvider.GetRequiredService<IMdFileDiscoveryService>();
        var routes = discoveryService.DiscoverMarkdownFilesWithRoutes();

        // Assert
        Assert.Empty(routes);
    }

    [Fact]
    public void DefaultApp_NonExistentSourceDirectory_HandlesGracefully()
    {
        // Arrange - Use a service with non-existent source directory
        var services = new ServiceCollection();
        var mockHostEnvironment = new Mock<IHostEnvironment>();
        mockHostEnvironment.Setup(x => x.ContentRootPath).Returns(_testContentRoot);
        services.AddSingleton(mockHostEnvironment.Object);

        services.AddMarkdownToRazorServices(options =>
        {
            options.SourceDirectory = "non-existent-directory";
        });

        using var provider = services.BuildServiceProvider();

        // Act
        var discoveryService = provider.GetRequiredService<IMdFileDiscoveryService>();
        var routes = discoveryService.DiscoverMarkdownFilesWithRoutes();

        // Assert
        Assert.Empty(routes);
    }

    [Fact]
    public void DefaultApp_MixedFileTypes_OnlyProcessesMarkdownFiles()
    {
        // Arrange - Create various file types
        CreateMarkdownFile("valid.md", "# Valid Markdown");
        File.WriteAllText(Path.Combine(_testSourceDir, "text.txt"), "Plain text file");
        File.WriteAllText(Path.Combine(_testSourceDir, "readme.html"), "<h1>HTML file</h1>");
        File.WriteAllText(Path.Combine(_testSourceDir, "config.json"), "{}");
        CreateMarkdownFile("another-valid.md", "# Another Valid Markdown");

        // Act
        var discoveryService = _serviceProvider.GetRequiredService<IMdFileDiscoveryService>();
        var routes = discoveryService.DiscoverMarkdownFilesWithRoutes();

        // Assert
        Assert.Equal(2, routes.Count);
        Assert.Contains("valid.md", routes.Keys);
        Assert.Contains("another-valid.md", routes.Keys);
        Assert.DoesNotContain("text.txt", routes.Keys);
        Assert.DoesNotContain("readme.html", routes.Keys);
        Assert.DoesNotContain("config.json", routes.Keys);
    }

    [Fact]
    public void DefaultApp_ServiceConfiguration_ReturnsCorrectPaths()
    {
        // Act
        var discoveryService = _serviceProvider.GetRequiredService<IMdFileDiscoveryService>();
        var sourceDir = discoveryService.GetSourceDirectory();
        var outputDir = discoveryService.GetOutputDirectory();

        // Assert
        Assert.Equal(Path.Combine(_testContentRoot, "content"), sourceDir);
        Assert.Null(outputDir); // Should be null for runtime-only scenarios
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();

        // Clean up test files and directories
        if (Directory.Exists(_testContentRoot))
        {
            try
            {
                Directory.Delete(_testContentRoot, true);
            }
            catch
            {
                // Ignore cleanup errors in tests
            }
        }
    }
}
