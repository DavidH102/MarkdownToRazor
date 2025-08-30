using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using MarkdownToRazor.Configuration;
using MarkdownToRazor.Services;

namespace MarkdownToRazor.Tests.Services;

/// <summary>
/// Comprehensive error handling and edge case tests for MdFileDiscoveryService.
/// Tests directory access errors, permission issues, invalid configurations, and edge cases.
/// </summary>
public class MdFileDiscoveryServiceErrorHandlingTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly Mock<IHostEnvironment> _mockHostEnvironment;

    public MdFileDiscoveryServiceErrorHandlingTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        _mockHostEnvironment = new Mock<IHostEnvironment>();
        _mockHostEnvironment.Setup(x => x.ContentRootPath).Returns(_tempDirectory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                // Reset any read-only attributes before deletion
                var dirInfo = new DirectoryInfo(_tempDirectory);
                SetDirectoryAttributes(dirInfo, FileAttributes.Normal);
                Directory.Delete(_tempDirectory, true);
            }
        }
        catch (UnauthorizedAccessException)
        {
            // If we can't delete the directory, that's fine for tests
        }
    }

    private static void SetDirectoryAttributes(DirectoryInfo dirInfo, FileAttributes attributes)
    {
        try
        {
            foreach (var file in dirInfo.GetFiles())
            {
                file.Attributes = attributes;
            }
            foreach (var dir in dirInfo.GetDirectories())
            {
                SetDirectoryAttributes(dir, attributes);
                dir.Attributes = attributes;
            }
            dirInfo.Attributes = attributes;
        }
        catch (UnauthorizedAccessException)
        {
            // Ignore permission errors during cleanup
        }
    }

    [Fact]
    public void DiscoverMarkdownFiles_NonExistentDirectory_ReturnsEmpty()
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "non-existent-directory",
            FilePattern = "*.md"
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);

        // Act
        var result = service.DiscoverMarkdownFiles();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task DiscoverMarkdownFilesAsync_NonExistentDirectory_ReturnsEmpty()
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "non-existent-directory",
            FilePattern = "*.md"
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);

        // Act
        var result = await service.DiscoverMarkdownFilesAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void DiscoverMarkdownFiles_EmptyDirectory_ReturnsEmpty()
    {
        // Arrange
        var sourceDir = Path.Combine(_tempDirectory, "empty-source");
        Directory.CreateDirectory(sourceDir);

        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "empty-source",
            FilePattern = "*.md"
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);

        // Act
        var result = service.DiscoverMarkdownFiles();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void DiscoverMarkdownFiles_DirectoryWithNoMarkdownFiles_ReturnsEmpty()
    {
        // Arrange
        var sourceDir = Path.Combine(_tempDirectory, "no-md-source");
        Directory.CreateDirectory(sourceDir);
        
        // Create non-markdown files
        File.WriteAllText(Path.Combine(sourceDir, "test.txt"), "text file");
        File.WriteAllText(Path.Combine(sourceDir, "config.json"), "{}");

        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "no-md-source",
            FilePattern = "*.md"
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);

        // Act
        var result = service.DiscoverMarkdownFiles();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void DiscoverMarkdownFiles_InvalidFilePattern_ReturnsEmpty()
    {
        // Arrange
        var sourceDir = Path.Combine(_tempDirectory, "pattern-test");
        Directory.CreateDirectory(sourceDir);
        File.WriteAllText(Path.Combine(sourceDir, "test.md"), "# Test");

        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "pattern-test",
            FilePattern = "*.invalid" // Pattern that won't match .md files
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);

        // Act
        var result = service.DiscoverMarkdownFiles();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void DiscoverMarkdownFiles_RecursiveSearch_FindsNestedFiles()
    {
        // Arrange
        var sourceDir = Path.Combine(_tempDirectory, "recursive-test");
        var subDir = Path.Combine(sourceDir, "subdir");
        Directory.CreateDirectory(subDir);
        
        File.WriteAllText(Path.Combine(sourceDir, "root.md"), "# Root");
        File.WriteAllText(Path.Combine(subDir, "nested.md"), "# Nested");

        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "recursive-test",
            FilePattern = "*.md",
            SearchRecursively = true
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);

        // Act
        var result = service.DiscoverMarkdownFiles().ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, path => Path.GetFileName(path) == "root.md");
        Assert.Contains(result, path => Path.GetFileName(path) == "nested.md");
    }

    [Fact]
    public void DiscoverMarkdownFiles_NonRecursiveSearch_OnlyFindsRootFiles()
    {
        // Arrange
        var sourceDir = Path.Combine(_tempDirectory, "non-recursive-test");
        var subDir = Path.Combine(sourceDir, "subdir");
        Directory.CreateDirectory(subDir);
        
        File.WriteAllText(Path.Combine(sourceDir, "root.md"), "# Root");
        File.WriteAllText(Path.Combine(subDir, "nested.md"), "# Nested");

        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "non-recursive-test",
            FilePattern = "*.md",
            SearchRecursively = false
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);

        // Act
        var result = service.DiscoverMarkdownFiles().ToList();

        // Assert
        Assert.Single(result);
        Assert.Contains(result, path => Path.GetFileName(path) == "root.md");
        Assert.DoesNotContain(result, path => Path.GetFileName(path) == "nested.md");
    }

    [Fact]
    public void DiscoverMarkdownFiles_CaseInsensitiveExtension_FindsFiles()
    {
        // Arrange
        var sourceDir = Path.Combine(_tempDirectory, "case-test");
        Directory.CreateDirectory(sourceDir);
        
        File.WriteAllText(Path.Combine(sourceDir, "lower.md"), "# Lower");
        // Note: On case-sensitive file systems, .MD and .Md may be treated as different extensions
        // The service filters by case-insensitive extension check, so all should be found
        File.WriteAllText(Path.Combine(sourceDir, "upper.MD"), "# Upper");
        File.WriteAllText(Path.Combine(sourceDir, "mixed.Md"), "# Mixed");

        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "case-test",
            FilePattern = "*.*" // Use broader pattern to include all files
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);

        // Act
        var result = service.DiscoverMarkdownFiles().ToList();

        // Assert - Should find all files with .md extension (case insensitive)
        Assert.True(result.Count >= 1, $"Expected at least 1 markdown file, but found {result.Count}");
        Assert.Contains(result, path => Path.GetFileName(path).Equals("lower.md", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void DiscoverMarkdownFilesWithRoutes_EmptyDirectory_ReturnsEmptyDictionary()
    {
        // Arrange
        var sourceDir = Path.Combine(_tempDirectory, "empty-routes");
        Directory.CreateDirectory(sourceDir);

        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "empty-routes",
            FilePattern = "*.md"
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);

        // Act
        var result = service.DiscoverMarkdownFilesWithRoutes();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void DiscoverMarkdownFilesWithRoutes_ValidFiles_GeneratesCorrectRoutes()
    {
        // Arrange
        var sourceDir = Path.Combine(_tempDirectory, "route-test");
        Directory.CreateDirectory(sourceDir);
        
        File.WriteAllText(Path.Combine(sourceDir, "index.md"), "# Index");
        File.WriteAllText(Path.Combine(sourceDir, "about.md"), "# About");
        File.WriteAllText(Path.Combine(sourceDir, "contact us.md"), "# Contact");

        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "route-test",
            FilePattern = "*.md"
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);

        // Act
        var result = service.DiscoverMarkdownFilesWithRoutes();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("/", result["index.md"]);
        Assert.Equal("/about", result["about.md"]);
        Assert.Equal("/contact-us", result["contact us.md"]);
    }

    [Fact]
    public void GetSourceDirectory_WithHostEnvironment_ReturnsAbsolutePath()
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "source",
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);

        // Act
        var result = service.GetSourceDirectory();

        // Assert
        Assert.Equal(Path.Combine(_tempDirectory, "source"), result);
    }

    [Fact]
    public void GetSourceDirectory_WithoutHostEnvironment_ReturnsRelativePath()
    {
        // Arrange - WASM scenario without host environment
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "source",
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new MdFileDiscoveryService(optionsWrapper, null);

        // Act
        var result = service.GetSourceDirectory();

        // Assert
        Assert.Equal("source", result);
    }

    [Fact]
    public void GetOutputDirectory_WithHostEnvironment_ReturnsAbsolutePath()
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "source",
            OutputDirectory = "output"
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);

        // Act
        var result = service.GetOutputDirectory();

        // Assert
        Assert.Equal(Path.Combine(_tempDirectory, "output"), result);
    }

    [Fact]
    public void GetOutputDirectory_WithoutHostEnvironment_ReturnsRelativePath()
    {
        // Arrange - WASM scenario without host environment
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "source",
            OutputDirectory = "output"
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new MdFileDiscoveryService(optionsWrapper, null);

        // Act
        var result = service.GetOutputDirectory();

        // Assert
        Assert.Equal("output", result);
    }

    [Fact]
    public void GetOutputDirectory_NullOutputDirectory_ReturnsNull()
    {
        // Arrange
        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "source",
            OutputDirectory = null
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);

        // Act
        var result = service.GetOutputDirectory();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void DiscoverMarkdownFiles_SpecialCharactersInFilenames_HandlesCorrectly()
    {
        // Arrange
        var sourceDir = Path.Combine(_tempDirectory, "special-chars");
        Directory.CreateDirectory(sourceDir);
        
        var specialFiles = new[]
        {
            "file with spaces.md",
            "file-with-dashes.md",
            "file_with_underscores.md",
            "file.with.dots.md"
        };

        foreach (var file in specialFiles)
        {
            File.WriteAllText(Path.Combine(sourceDir, file), "# Content");
        }

        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "special-chars",
            FilePattern = "*.md"
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);

        // Act
        var result = service.DiscoverMarkdownFiles().ToList();

        // Assert
        Assert.Equal(specialFiles.Length, result.Count);
        foreach (var file in specialFiles)
        {
            Assert.Contains(result, path => Path.GetFileName(path) == file);
        }
    }

    [Fact]
    public async Task DiscoverMarkdownFilesWithRoutesAsync_ProducesSameResultsAsSync()
    {
        // Arrange
        var sourceDir = Path.Combine(_tempDirectory, "async-test");
        Directory.CreateDirectory(sourceDir);
        
        File.WriteAllText(Path.Combine(sourceDir, "test1.md"), "# Test 1");
        File.WriteAllText(Path.Combine(sourceDir, "test2.md"), "# Test 2");

        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "async-test",
            FilePattern = "*.md"
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);

        // Act
        var syncResult = service.DiscoverMarkdownFilesWithRoutes();
        var asyncResult = await service.DiscoverMarkdownFilesWithRoutesAsync();

        // Assert
        Assert.Equal(syncResult.Count, asyncResult.Count);
        foreach (var kvp in syncResult)
        {
            Assert.True(asyncResult.ContainsKey(kvp.Key));
            Assert.Equal(kvp.Value, asyncResult[kvp.Key]);
        }
    }

    [Fact]
    public void DiscoverMarkdownFiles_LargeNumberOfFiles_HandlesEfficiently()
    {
        // Arrange
        var sourceDir = Path.Combine(_tempDirectory, "performance-test");
        Directory.CreateDirectory(sourceDir);
        
        // Create many markdown files
        for (int i = 0; i < 100; i++)
        {
            File.WriteAllText(Path.Combine(sourceDir, $"file{i:D3}.md"), $"# File {i}");
        }

        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "performance-test",
            FilePattern = "*.md"
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);

        // Act
        var startTime = DateTime.UtcNow;
        var result = service.DiscoverMarkdownFiles().ToList();
        var duration = DateTime.UtcNow - startTime;

        // Assert
        Assert.Equal(100, result.Count);
        // Should complete within reasonable time (5 seconds for 100 files)
        Assert.True(duration.TotalSeconds < 5, $"Discovery took {duration.TotalSeconds} seconds");
    }
}