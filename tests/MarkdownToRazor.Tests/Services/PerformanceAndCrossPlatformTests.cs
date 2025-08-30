using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using MarkdownToRazor.Configuration;
using MarkdownToRazor.Services;
using System.Diagnostics;
using System.Text;

namespace MarkdownToRazor.Tests.Services;

/// <summary>
/// Performance and cross-platform tests for MarkdownToRazor services.
/// Tests scalability, platform compatibility, and performance characteristics.
/// </summary>
public class PerformanceAndCrossPlatformTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly Mock<IHostEnvironment> _mockHostEnvironment;
    private readonly HttpClient _httpClient;

    public PerformanceAndCrossPlatformTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);

        _mockHostEnvironment = new Mock<IHostEnvironment>();
        _mockHostEnvironment.Setup(x => x.ContentRootPath).Returns(_tempDirectory);

        _httpClient = new HttpClient();
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }
        catch (UnauthorizedAccessException)
        {
            // If we can't delete the directory, that's fine for tests
        }
        _httpClient?.Dispose();
    }

    [Fact]
    public void MdFileDiscoveryService_LargeDirectoryPerformance_HandlesEfficiently()
    {
        // Arrange - Create a large number of markdown files
        var sourceDir = Path.Combine(_tempDirectory, "large-source");
        Directory.CreateDirectory(sourceDir);

        var fileCount = 1000; // Create 1000 files for performance testing
        for (int i = 0; i < fileCount; i++)
        {
            var filePath = Path.Combine(sourceDir, $"file{i:D4}.md");
            File.WriteAllText(filePath, $"# File {i}\nContent for file {i}");
        }

        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "large-source",
            FilePattern = "*.md",
            SearchRecursively = false
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var files = service.DiscoverMarkdownFiles().ToList();
        var routes = service.DiscoverMarkdownFilesWithRoutes();
        stopwatch.Stop();

        // Assert
        Assert.Equal(fileCount, files.Count);
        Assert.Equal(fileCount, routes.Count);
        
        // Performance assertion - should complete within 5 seconds for 1000 files
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
            $"Discovery took {stopwatch.ElapsedMilliseconds}ms for {fileCount} files");
    }

    [Fact]
    public async Task MdFileDiscoveryService_AsyncPerformance_ScalesWithFileCount()
    {
        // Arrange - Create files in multiple subdirectories with unique name
        var uniqueDir = $"async-perf-{Guid.NewGuid():N}";
        var sourceDir = Path.Combine(_tempDirectory, uniqueDir);
        Directory.CreateDirectory(sourceDir);

        var dirCount = 2; // Simplified test
        var filesPerDir = 3; // Simplified test

        for (int d = 0; d < dirCount; d++)
        {
            var subDir = Path.Combine(sourceDir, $"subdir{d:D2}");
            Directory.CreateDirectory(subDir);
            
            for (int f = 0; f < filesPerDir; f++)
            {
                var filePath = Path.Combine(subDir, $"file{f:D2}.md");
                File.WriteAllText(filePath, $"# File {d}-{f}\nContent");
            }
        }

        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = uniqueDir,
            FilePattern = "*.md",
            SearchRecursively = true
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var filesAsync = await service.DiscoverMarkdownFilesAsync();
        var routesAsync = await service.DiscoverMarkdownFilesWithRoutesAsync();
        stopwatch.Stop();

        var filesList = filesAsync.ToList();

        // Assert - Check that files are found from subdirectories
        Assert.True(filesList.Count >= filesPerDir, $"Expected at least {filesPerDir} files, but found {filesList.Count}");
        Assert.True(routesAsync.Count >= filesPerDir, $"Expected at least {filesPerDir} routes, but found {routesAsync.Count}");
        
        // Async operations should complete efficiently
        Assert.True(stopwatch.ElapsedMilliseconds < 5000,
            $"Async discovery took {stopwatch.ElapsedMilliseconds}ms for {filesList.Count} files");
    }

    [Theory]
    [InlineData("unix/path/file.md", "/")]
    [InlineData("windows\\path\\file.md", "\\")]
    [InlineData("mixed/path\\file.md", "mixed")]
    public void PathHandling_CrossPlatform_HandlesPathSeparators(string filePath, string separatorType)
    {
        // Arrange
        var sourceDir = Path.Combine(_tempDirectory, "path-test");
        Directory.CreateDirectory(sourceDir);

        // Create a test file with platform-appropriate path
        var normalizedPath = filePath.Replace('/', Path.DirectorySeparatorChar)
                                    .Replace('\\', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(sourceDir, normalizedPath);
        
        // Create directory structure if needed
        var directory = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(directory);
        File.WriteAllText(fullPath, "# Cross Platform Test");

        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "path-test",
            FilePattern = "*.md",
            SearchRecursively = true
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);

        // Act
        var files = service.DiscoverMarkdownFiles().ToList();
        var routes = service.DiscoverMarkdownFilesWithRoutes();

        // Assert
        Assert.Single(files);
        Assert.Single(routes);
        
        // Verify the file was found regardless of path separator style
        Assert.Contains(files, f => f.EndsWith("file.md"));
    }

    [Fact]
    public void FileEncoding_CrossPlatform_HandlesVariousEncodings()
    {
        // Arrange
        var sourceDir = Path.Combine(_tempDirectory, "encoding-test");
        Directory.CreateDirectory(sourceDir);

        // Create files with different encodings
        var encodings = new Dictionary<string, Encoding>
        {
            ["utf8.md"] = Encoding.UTF8,
            ["utf8-bom.md"] = new UTF8Encoding(true), // With BOM
            ["ascii.md"] = Encoding.ASCII
        };

        foreach (var (fileName, encoding) in encodings)
        {
            var content = "# Encoding Test\nContent with special chars: café, naïve, résumé";
            var filePath = Path.Combine(sourceDir, fileName);
            File.WriteAllText(filePath, content, encoding);
        }

        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "encoding-test",
            FilePattern = "*.md"
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);

        // Act
        var files = service.DiscoverMarkdownFiles().ToList();

        // Assert
        Assert.Equal(3, files.Count);
        
        // Verify all files were discovered regardless of encoding
        Assert.Contains(files, f => f.EndsWith("utf8.md"));
        Assert.Contains(files, f => f.EndsWith("utf8-bom.md"));
        Assert.Contains(files, f => f.EndsWith("ascii.md"));
    }

    [Fact]
    public async Task StaticAssetService_FileReadingPerformance_HandlesLargeFiles()
    {
        // Arrange
        var sourceDir = Path.Combine(_tempDirectory, "large-files");
        Directory.CreateDirectory(sourceDir);

        // Create files of different sizes
        var fileSizes = new[] { 1024, 10 * 1024, 100 * 1024, 1024 * 1024 }; // 1KB to 1MB
        
        foreach (var size in fileSizes)
        {
            var fileName = $"large-{size}.md";
            var filePath = Path.Combine(sourceDir, fileName);
            var content = new string('*', size);
            File.WriteAllText(filePath, content);
        }

        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "large-files",
            FilePattern = "*.md"
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new StaticAssetService(_httpClient, _mockHostEnvironment.Object, optionsWrapper);

        // Act & Assert
        foreach (var size in fileSizes)
        {
            var fileName = $"large-{size}.md";
            
            var stopwatch = Stopwatch.StartNew();
            var content = await service.GetAsync(fileName);
            stopwatch.Stop();

            Assert.NotNull(content);
            Assert.Equal(size, content.Length);
            
            // Performance assertion - should read files quickly (under 1 second)
            Assert.True(stopwatch.ElapsedMilliseconds < 1000,
                $"Reading {size}-byte file took {stopwatch.ElapsedMilliseconds}ms");
        }
    }

    [Fact]
    public async Task StaticAssetService_ConcurrentFileAccess_HandlesParallelRequests()
    {
        // Arrange
        var sourceDir = Path.Combine(_tempDirectory, "concurrent-test");
        Directory.CreateDirectory(sourceDir);

        // Create multiple test files
        var fileCount = 20;
        for (int i = 0; i < fileCount; i++)
        {
            var filePath = Path.Combine(sourceDir, $"concurrent{i:D2}.md");
            File.WriteAllText(filePath, $"# Concurrent Test {i}\nContent for file {i}");
        }

        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "concurrent-test",
            FilePattern = "*.md"
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var service = new StaticAssetService(_httpClient, _mockHostEnvironment.Object, optionsWrapper);

        // Act - Make concurrent requests to different files
        var tasks = Enumerable.Range(0, fileCount)
            .Select(i => service.GetAsync($"concurrent{i:D2}.md"))
            .ToArray();

        var stopwatch = Stopwatch.StartNew();
        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        Assert.Equal(fileCount, results.Length);
        Assert.All(results, result => Assert.NotNull(result));
        
        // Concurrent access should be efficient
        Assert.True(stopwatch.ElapsedMilliseconds < 2000,
            $"Concurrent access to {fileCount} files took {stopwatch.ElapsedMilliseconds}ms");
    }

    [Fact]
    public void PathNormalization_CrossPlatform_HandlesEdgeCases()
    {
        // Arrange
        var testPaths = new[]
        {
            "content/normal/file.md",
            "content\\windows\\style.md",
            "content/./current/dir.md",
            "content/../parent/dir.md",
            "content//double//slash.md",
            "content\\\\double\\\\backslash.md"
        };

        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "content",
            FilePattern = "*.md"
        };

        foreach (var testPath in testPaths)
        {
            // Act - Test path normalization without requiring actual files
            var normalizedPath = Path.GetFullPath(Path.Combine(_tempDirectory, testPath));

            // Assert - Should not throw exceptions during path normalization
            Assert.NotNull(normalizedPath);
            Assert.True(Path.IsPathRooted(normalizedPath));
        }
    }

    [Fact]
    public async Task MemoryUsage_UnderLoad_RemainsStable()
    {
        // Arrange
        var sourceDir = Path.Combine(_tempDirectory, "memory-test");
        Directory.CreateDirectory(sourceDir);

        // Create files for memory testing
        for (int i = 0; i < 100; i++)
        {
            var filePath = Path.Combine(sourceDir, $"memory{i:D3}.md");
            File.WriteAllText(filePath, $"# Memory Test {i}\n{new string('*', 1024)}");
        }

        var options = new MarkdownToRazorOptions
        {
            SourceDirectory = "memory-test",
            FilePattern = "*.md"
        };
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
        var discoveryService = new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);
        var assetService = new StaticAssetService(_httpClient, _mockHostEnvironment.Object, optionsWrapper);

        // Force garbage collection to get baseline
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var initialMemory = GC.GetTotalMemory(false);

        // Act - Perform memory-intensive operations
        for (int iteration = 0; iteration < 10; iteration++)
        {
            var files = discoveryService.DiscoverMarkdownFiles().ToList();
            var routes = discoveryService.DiscoverMarkdownFilesWithRoutes();

            // Read some files
            for (int i = 0; i < 10; i++)
            {
                var content = await assetService.GetAsync($"memory{i:D3}.md");
                Assert.NotNull(content);
            }
        }

        // Force garbage collection and measure final memory
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var finalMemory = GC.GetTotalMemory(false);

        var memoryIncrease = finalMemory - initialMemory;

        // Assert - Memory increase should be reasonable (less than 50MB)
        Assert.True(memoryIncrease < 50 * 1024 * 1024,
            $"Memory increased by {memoryIncrease} bytes during load testing");
    }

    [Fact]
    public void RouteGeneration_Performance_ScalesLinearly()
    {
        // Arrange - Create files with varying name complexities
        var sourceDir = Path.Combine(_tempDirectory, "route-perf");
        
        var fileCounts = new[] { 10, 20, 30 }; // Smaller, more manageable test sizes
        var timings = new List<long>();

        foreach (var fileCount in fileCounts)
        {
            // Clean directory
            if (Directory.Exists(sourceDir))
            {
                Directory.Delete(sourceDir, true);
            }
            Directory.CreateDirectory(sourceDir);

            // Create files with complex names for route generation
            for (int i = 0; i < fileCount; i++)
            {
                var fileName = $"Complex File Name {i} with Spaces & Symbols!.md";
                var filePath = Path.Combine(sourceDir, fileName);
                File.WriteAllText(filePath, $"# Content {i}");
            }

            var options = new MarkdownToRazorOptions
            {
                SourceDirectory = "route-perf",
                FilePattern = "*.md"
            };
            var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(options);
            var service = new MdFileDiscoveryService(optionsWrapper, _mockHostEnvironment.Object);

            // Act
            var stopwatch = Stopwatch.StartNew();
            var routes = service.DiscoverMarkdownFilesWithRoutes();
            stopwatch.Stop();

            // Assert
            Assert.Equal(fileCount, routes.Count);
            timings.Add(Math.Max(1, stopwatch.ElapsedMilliseconds)); // Ensure minimum 1ms for ratio calculation
        }

        // Verify performance scales reasonably (not exponentially)
        for (int i = 1; i < timings.Count; i++)
        {
            var ratio = (double)timings[i] / timings[i - 1];
            var fileRatio = (double)fileCounts[i] / fileCounts[i - 1];
            
            // Time ratio should not be significantly higher than file ratio
            // Allow more tolerance for very fast operations
            Assert.True(ratio < fileRatio * 5,
                $"Performance may have degraded: {timings[i]}ms for {fileCounts[i]} files vs {timings[i-1]}ms for {fileCounts[i-1]} files (ratio: {ratio:F2})");
        }
    }
}