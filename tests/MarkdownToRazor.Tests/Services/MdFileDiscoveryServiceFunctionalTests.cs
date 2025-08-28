using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using MarkdownToRazor.Configuration;
using MarkdownToRazor.Services;

namespace MarkdownToRazor.Tests.Services;

/// <summary>
/// Functional integration tests for MdFileDiscoveryService to verify routing and content access functionality.
/// These tests validate the complete workflow from service discovery to content accessibility.
/// </summary>
public class MdFileDiscoveryServiceFunctionalTests : IDisposable
{
    private readonly Mock<IHostEnvironment> _mockHostEnvironment;
    private readonly MarkdownToRazorOptions _options;
    private readonly string _testContentRoot;
    private readonly string _testSourceDir;
    private readonly Dictionary<string, string> _testFiles;

    public MdFileDiscoveryServiceFunctionalTests()
    {
        _mockHostEnvironment = new Mock<IHostEnvironment>();
        _testContentRoot = Path.GetTempPath();
        _testSourceDir = Path.Combine(_testContentRoot, "FunctionalTestMarkdownFiles");

        _mockHostEnvironment.Setup(x => x.ContentRootPath).Returns(_testContentRoot);

        _options = new MarkdownToRazorOptions
        {
            SourceDirectory = "FunctionalTestMarkdownFiles",
            OutputDirectory = "Pages/Generated",
            FilePattern = "*.md",
            SearchRecursively = true
        };

        // Test files with varying content complexity
        _testFiles = new Dictionary<string, string>
        {
            ["index.md"] = @"# Welcome to Our Documentation

This is the main landing page for our documentation system.

## Features
- Dynamic routing
- Service-based navigation
- WASM compatibility

## Getting Started
Click on any of the navigation links to explore our documentation.",

            ["getting-started.md"] = @"---
title: Getting Started Guide
description: Learn how to get started with our system
tags: [tutorial, beginner]
---

# Getting Started

This guide will help you get started with our system.

## Prerequisites
- .NET 8 SDK
- Basic understanding of Blazor

## Installation Steps
1. Clone the repository
2. Run `dotnet restore`
3. Run `dotnet build`

```csharp
// Example code
var service = serviceProvider.GetService<IMdFileDiscoveryService>();
var routes = await service.DiscoverMarkdownFilesWithRoutesAsync();
```",

            ["advanced-features.md"] = @"# Advanced Features

## Performance Optimization
- Caching mechanisms
- Lazy loading
- Efficient file discovery

## Integration Patterns
- Service injection
- Route generation
- Content delivery",

            ["api/endpoints.md"] = @"# API Endpoints

## Discovery Service
The `IMdFileDiscoveryService` provides the following methods:

### DiscoverMarkdownFilesWithRoutesAsync()
Returns a dictionary mapping file names to their generated routes.

### GetSourceDirectory()
Returns the configured source directory path.",

            ["troubleshooting.md"] = @"# Troubleshooting

## Common Issues

### Service Not Found
Ensure the service is properly registered in DI container.

### Files Not Discovered
Check the source directory configuration."
        };

        // Create test directory and files
        SetupTestEnvironment();
    }

    private void SetupTestEnvironment()
    {
        // Clean up any existing files
        if (Directory.Exists(_testSourceDir))
        {
            Directory.Delete(_testSourceDir, true);
        }

        // Create directory structure
        Directory.CreateDirectory(_testSourceDir);
        Directory.CreateDirectory(Path.Combine(_testSourceDir, "api"));

        // Create test markdown files
        foreach (var testFile in _testFiles)
        {
            var filePath = Path.Combine(_testSourceDir, testFile.Key);
            var directory = Path.GetDirectoryName(filePath)!;
            Directory.CreateDirectory(directory);
            File.WriteAllText(filePath, testFile.Value);
        }
    }

    private MdFileDiscoveryService CreateService()
    {
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(_options);
        return new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);
    }

    [Fact]
    public async Task ServiceDiscovery_CanFindAllTestFiles_ReturnsExpectedCount()
    {
        // Arrange
        var service = CreateService();

        // Act
        var files = await service.DiscoverMarkdownFilesAsync();
        var routes = await service.DiscoverMarkdownFilesWithRoutesAsync();

        // Assert
        Assert.Equal(_testFiles.Count, files.Count());
        Assert.Equal(_testFiles.Count, routes.Count);
        
        // Verify all test files are discovered
        foreach (var testFileName in _testFiles.Keys)
        {
            var fileName = Path.GetFileName(testFileName);
            Assert.Contains(fileName, routes.Keys);
        }
    }

    [Fact]
    public async Task RouteGeneration_ProducesValidRoutes_ForAllFileTypes()
    {
        // Arrange
        var service = CreateService();

        // Act
        var routes = await service.DiscoverMarkdownFilesWithRoutesAsync();

        // Assert - Check specific route mappings
        Assert.Equal("/", routes["index.md"]);
        Assert.Equal("/getting-started", routes["getting-started.md"]);
        Assert.Equal("/advanced-features", routes["advanced-features.md"]);
        Assert.Equal("/endpoints", routes["endpoints.md"]);
        Assert.Equal("/troubleshooting", routes["troubleshooting.md"]);

        // Verify all routes start with / and are URL-friendly
        foreach (var route in routes.Values)
        {
            Assert.StartsWith("/", route);
            Assert.DoesNotContain(" ", route);
            Assert.DoesNotContain("_", route);
        }
    }

    [Fact]
    public async Task ContentAccess_FilesAreAccessible_ViaGeneratedPaths()
    {
        // Arrange
        var service = CreateService();
        var sourceDir = service.GetSourceDirectory();

        // Act
        var routes = await service.DiscoverMarkdownFilesWithRoutesAsync();

        // Assert - Verify files are accessible via their paths
        foreach (var fileRoute in routes)
        {
            var fileName = fileRoute.Key;
            var route = fileRoute.Value;

            // Find the actual file path
            var possiblePaths = new[]
            {
                Path.Combine(sourceDir, fileName),
                Path.Combine(sourceDir, "api", fileName) // Check subdirectory
            };

            var actualFilePath = possiblePaths.FirstOrDefault(File.Exists);
            Assert.NotNull(actualFilePath);

            // Verify file is readable and contains expected content
            var content = await File.ReadAllTextAsync(actualFilePath);
            Assert.NotEmpty(content);
            
            // Verify content matches what we created
            var expectedContent = _testFiles.First(tf => tf.Key.EndsWith(fileName)).Value;
            Assert.Equal(expectedContent, content);
        }
    }

    [Fact]
    public void ServiceConfiguration_ReturnsCorrectDirectories()
    {
        // Arrange
        var service = CreateService();

        // Act
        var sourceDir = service.GetSourceDirectory();
        var outputDir = service.GetOutputDirectory();

        // Assert
        Assert.NotNull(sourceDir);
        Assert.Contains("FunctionalTestMarkdownFiles", sourceDir);
        
        // Output directory should be an absolute path that ends with our configured relative path
        Assert.NotNull(outputDir);
        Assert.EndsWith("Pages/Generated", outputDir.Replace('\\', '/'));
    }

    [Fact]
    public async Task NavigationMenu_CanUseDiscoveredRoutes_ForDynamicMenuGeneration()
    {
        // Arrange
        var service = CreateService();

        // Act
        var routes = await service.DiscoverMarkdownFilesWithRoutesAsync();

        // Assert - Simulate how NavMenu would use the service
        var menuItems = routes
            .Where(r => r.Key != "index.md") // Exclude home page from menu
            .Select(r => new { 
                FileName = r.Key, 
                Route = r.Value,
                DisplayName = GenerateDisplayName(r.Key)
            })
            .ToList();

        Assert.Equal(4, menuItems.Count); // 5 total files - 1 index = 4 menu items
        
        // Verify menu items have valid properties
        foreach (var menuItem in menuItems)
        {
            Assert.NotEmpty(menuItem.FileName);
            Assert.StartsWith("/", menuItem.Route);
            Assert.NotEmpty(menuItem.DisplayName);
        }

        // Check specific menu items
        var gettingStartedItem = menuItems.FirstOrDefault(m => m.FileName == "getting-started.md");
        Assert.NotNull(gettingStartedItem);
        Assert.Equal("/getting-started", gettingStartedItem.Route);
        Assert.Equal("Getting Started", gettingStartedItem.DisplayName);
    }

    [Fact]
    public void WasmCompatibility_ServiceWorksWithoutHostEnvironment()
    {
        // Arrange - Create service without host environment (simulating WASM)
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(_options);
        var wasmService = new MdFileDiscoveryService(optionsWrapper, null);

        // Act & Assert - Service should not throw when host environment is null
        var sourceDir = wasmService.GetSourceDirectory();
        var outputDir = wasmService.GetOutputDirectory();

        Assert.Equal(_options.SourceDirectory, sourceDir);
        Assert.Equal(_options.OutputDirectory, outputDir);
    }

    [Fact]
    public async Task RecursiveSearch_FindsFilesInSubdirectories()
    {
        // Arrange
        var service = CreateService();

        // Act
        var routes = await service.DiscoverMarkdownFilesWithRoutesAsync();

        // Assert - Should find files in subdirectories
        Assert.Contains("endpoints.md", routes.Keys);
        Assert.Equal("/endpoints", routes["endpoints.md"]);
    }

    [Fact]
    public async Task ServiceIntegration_WorksWithDependencyInjection()
    {
        // Arrange - Setup DI container like a real application would
        var services = new ServiceCollection();
        services.Configure<MarkdownToRazorOptions>(opts =>
        {
            opts.SourceDirectory = _options.SourceDirectory;
            opts.OutputDirectory = _options.OutputDirectory;
            opts.FilePattern = _options.FilePattern;
            opts.SearchRecursively = _options.SearchRecursively;
        });
        services.AddSingleton(_mockHostEnvironment.Object);
        services.AddTransient<IMdFileDiscoveryService, MdFileDiscoveryService>();

        var serviceProvider = services.BuildServiceProvider();

        // Act
        var discoveryService = serviceProvider.GetRequiredService<IMdFileDiscoveryService>();
        var routes = await discoveryService.DiscoverMarkdownFilesWithRoutesAsync();

        // Assert
        Assert.NotNull(discoveryService);
        Assert.Equal(_testFiles.Count, routes.Count);
        Assert.Contains("/", routes.Values); // Should have index route
    }

    /// <summary>
    /// Helper method to generate display names for menu items from file names
    /// </summary>
    private static string GenerateDisplayName(string fileName)
    {
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        return nameWithoutExtension
            .Replace("-", " ")
            .Replace("_", " ")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => char.ToUpper(word[0]) + word[1..].ToLower())
            .Aggregate((current, next) => current + " " + next);
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
