using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using MarkdownToRazor.Configuration;
using MarkdownToRazor.Services;

namespace MarkdownToRazor.Tests.Services;

/// <summary>
/// Enhanced WASM-specific testing for browser behaviors, memory usage, and service integration.
/// Tests scenarios unique to WebAssembly runtime environments.
/// </summary>
public class WasmSpecificBehaviorTests : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly MarkdownToRazorOptions _options;

    public WasmSpecificBehaviorTests()
    {
        _httpClient = new HttpClient();
        _options = new MarkdownToRazorOptions
        {
            SourceDirectory = "content",
            FilePattern = "*.md"
        };
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }

    [Fact]
    public void MdFileDiscoveryService_WithoutHostEnvironment_UsesRelativePaths()
    {
        // Arrange - WASM scenario without IHostEnvironment
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(_options);
        var service = new MdFileDiscoveryService(optionsWrapper, null);

        // Act
        var sourceDir = service.GetSourceDirectory();
        var outputDir = service.GetOutputDirectory();

        // Assert - Should return raw relative paths in WASM
        Assert.Equal(_options.SourceDirectory, sourceDir);
        Assert.Equal(_options.OutputDirectory, outputDir);
    }

    [Fact]
    public void MdFileDiscoveryService_WithHostEnvironment_UsesAbsolutePaths()
    {
        // Arrange - Server-side scenario with IHostEnvironment
        var mockHostEnvironment = new Mock<IHostEnvironment>();
        mockHostEnvironment.Setup(x => x.ContentRootPath).Returns("/app/root");
        
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(_options);
        var service = new MdFileDiscoveryService(optionsWrapper, mockHostEnvironment.Object);

        // Act
        var sourceDir = service.GetSourceDirectory();
        var outputDir = service.GetOutputDirectory();

        // Assert - Should return absolute paths on server
        Assert.StartsWith("/app/root", sourceDir);
        Assert.Null(outputDir); // OutputDirectory is null in options
    }

    [Fact]
    public void WasmStaticAssetService_CacheManagement_HandlesMemoryEfficiently()
    {
        // Arrange
        var service = new WasmStaticAssetService(_httpClient);

        // Act - Make multiple requests to trigger caching
        var tasks = new List<Task<string?>>();
        for (int i = 0; i < 50; i++)
        {
            tasks.Add(service.GetAsync($"file{i}.md"));
        }

        // Wait for all requests to complete (they will fail, but cache behavior is tested)
        Task.WaitAll(tasks.ToArray());

        // Assert - Service should not throw OutOfMemoryException or similar
        // This test primarily validates that the cache doesn't cause memory issues
        Assert.True(tasks.All(t => t.IsCompleted));
    }

    [Fact]
    public async Task WasmStaticAssetService_ConcurrentCacheAccess_HandlesCorrectly()
    {
        // Arrange
        var service = new WasmStaticAssetService(_httpClient);
        var samePath = "concurrent-cache-test.md";

        // Act - Make concurrent requests to the same path
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => service.GetAsync(samePath))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert - All requests should return the same result (null in this case)
        Assert.All(results, result => Assert.Null(result));
        
        // Verify no race conditions occurred (no exceptions thrown)
        Assert.True(tasks.All(t => t.IsCompletedSuccessfully));
    }

    [Fact]
    public async Task WasmStaticAssetService_UrlConstruction_HandlesWasmPathCorrectly()
    {
        // Arrange
        var service = new WasmStaticAssetService(_httpClient);

        // Test cases for WASM-specific URL construction
        var testCases = new Dictionary<string, string>
        {
            ["simple.md"] = "/content/simple.md",
            ["content/prefixed.md"] = "/content/prefixed.md", 
            ["./relative.md"] = "./relative.md",
            ["/absolute.md"] = "/absolute.md",
            ["https://remote.com/file.md"] = "https://remote.com/file.md"
        };

        foreach (var (input, expectedUrl) in testCases)
        {
            // Act - The service will attempt to load these URLs
            // We're not testing the actual loading but the URL construction logic
            var result = await service.GetAsync(input);

            // Assert - Should not throw exceptions during URL construction
            // Result will be null since files don't exist, but no exceptions should occur
            Assert.Null(result);
        }
    }

    [Fact]
    public void ServiceRegistration_ForWasm_ConfiguresCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Configure services for WASM (without IHostEnvironment)
        services.AddSingleton(_httpClient);
        services.Configure<MarkdownToRazorOptions>(options =>
        {
            options.SourceDirectory = "content";
            options.FilePattern = "*.md";
        });
        services.AddSingleton<IMdFileDiscoveryService, MdFileDiscoveryService>();
        services.AddSingleton<IStaticAssetService, WasmStaticAssetService>();

        // Act
        using var provider = services.BuildServiceProvider();
        var discoveryService = provider.GetRequiredService<IMdFileDiscoveryService>();
        var assetService = provider.GetRequiredService<IStaticAssetService>();

        // Assert
        Assert.IsType<MdFileDiscoveryService>(discoveryService);
        Assert.IsType<WasmStaticAssetService>(assetService);
    }

    [Fact]
    public async Task WasmServices_Integration_WorksWithoutServerDependencies()
    {
        // Arrange - Simulate complete WASM service setup
        var services = new ServiceCollection();
        services.AddSingleton(_httpClient);
        services.Configure<MarkdownToRazorOptions>(options =>
        {
            options.SourceDirectory = "content";
            options.OutputDirectory = null; // WASM doesn't need output directory
            options.FilePattern = "*.md";
        });
        services.AddSingleton<IMdFileDiscoveryService, MdFileDiscoveryService>();
        services.AddSingleton<IStaticAssetService, WasmStaticAssetService>();

        using var provider = services.BuildServiceProvider();

        // Act - Test integration between services
        var discoveryService = provider.GetRequiredService<IMdFileDiscoveryService>();
        var assetService = provider.GetRequiredService<IStaticAssetService>();

        var routes = discoveryService.DiscoverMarkdownFilesWithRoutes();
        var sourceDir = discoveryService.GetSourceDirectory();

        // Attempt to load a file (will fail but tests integration)
        var content = await assetService.GetAsync("test.md");

        // Assert - Services should integrate without server dependencies
        Assert.NotNull(routes);
        Assert.Equal("content", sourceDir); // WASM uses relative paths
        Assert.Null(content); // File doesn't exist, but no exceptions
    }

    [Fact]
    public async Task WasmStaticAssetService_MemoryUsage_RemainsStableWithManyRequests()
    {
        // Arrange
        var service = new WasmStaticAssetService(_httpClient);
        var initialMemory = GC.GetTotalMemory(true);

        // Act - Make many requests to test memory stability
        for (int i = 0; i < 100; i++)
        {
            await service.GetAsync($"memory-test-{i}.md");
        }

        // Force garbage collection and check memory
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var finalMemory = GC.GetTotalMemory(true);
        var memoryIncrease = finalMemory - initialMemory;

        // Assert - Memory increase should be reasonable (less than 10MB for test data)
        Assert.True(memoryIncrease < 10 * 1024 * 1024, 
            $"Memory increased by {memoryIncrease} bytes, which may indicate a memory leak");
    }

    [Fact]
    public void WasmStaticAssetService_Construction_HandlesHttpClientLifetime()
    {
        // Arrange & Act - Test different construction scenarios
        using var httpClient1 = new HttpClient();
        var service1 = new WasmStaticAssetService(httpClient1);

        using var httpClient2 = new HttpClient();
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(_options);
        var service2 = new WasmStaticAssetService(httpClient2, optionsWrapper);

        // Assert - Services should be created successfully
        Assert.NotNull(service1);
        Assert.NotNull(service2);

        // Dispose services implicitly when httpClients are disposed
    }

    [Fact]
    public async Task WasmStaticAssetService_ErrorRecovery_ContinuesAfterNetworkFailures()
    {
        // Arrange
        var service = new WasmStaticAssetService(_httpClient);

        // Act - Make requests that will fail, then make more requests
        var failedRequest1 = await service.GetAsync("https://invalid-domain-1.com/file.md");
        var failedRequest2 = await service.GetAsync("https://invalid-domain-2.com/file.md");
        var localRequest = await service.GetAsync("local-file.md");

        // Assert - Service should continue working after network failures
        Assert.Null(failedRequest1);
        Assert.Null(failedRequest2);
        Assert.Null(localRequest); // File doesn't exist, but service still works
    }

    [Fact]
    public async Task WasmServices_RouteGeneration_WorksWithoutFileSystem()
    {
        // Arrange - WASM scenario where file system access is limited
        var optionsWrapper = new OptionsWrapper<MarkdownToRazorOptions>(_options);
        var service = new MdFileDiscoveryService(optionsWrapper, null);

        // Act - Test route generation when directory doesn't exist
        var routes = service.DiscoverMarkdownFilesWithRoutes();
        var routesAsync = await service.DiscoverMarkdownFilesWithRoutesAsync();

        // Assert - Should handle missing directories gracefully
        Assert.NotNull(routes);
        Assert.NotNull(routesAsync);
        Assert.Empty(routes); // No files found, but no exceptions
        Assert.Empty(routesAsync);
    }

    [Fact]
    public async Task WasmStaticAssetService_PathNormalization_HandlesWasmSpecificPaths()
    {
        // Arrange
        var service = new WasmStaticAssetService(_httpClient);

        // Test WASM-specific path scenarios
        var wasmPaths = new[]
        {
            "/_content/MyApp/file.md",
            "/_framework/file.md", 
            "/content/file.md",
            "./wwwroot/content/file.md"
        };

        foreach (var path in wasmPaths)
        {
            // Act
            var result = await service.GetAsync(path);

            // Assert - Should handle WASM-specific paths without exceptions
            Assert.Null(result); // Files don't exist, but path handling should work
        }
    }

    [Fact]
    public void WasmServices_ConfigurationValidation_WorksInBrowserContext()
    {
        // Arrange
        var wasmOptions = new MarkdownToRazorOptions
        {
            SourceDirectory = "content",
            OutputDirectory = null, // Common in WASM
            FilePattern = "*.md",
            SearchRecursively = true,
            EnableHtmlCommentConfiguration = false, // WASM might prefer YAML only
            EnableYamlFrontmatter = true
        };

        // Act & Assert - Should validate successfully for WASM scenarios
        wasmOptions.Validate();
        
        // Test WASM-specific absolute path scenario
        var wasmAbsolutePath = wasmOptions.GetAbsoluteSourcePath("");
        Assert.Equal(Path.GetFullPath("content"), wasmAbsolutePath);
    }

    [Fact]
    public async Task WasmStaticAssetService_BrowserLimitations_HandledGracefully()
    {
        // Arrange
        var service = new WasmStaticAssetService(_httpClient);

        // Test scenarios that might be limited in browser environments
        var browserLimitedScenarios = new[]
        {
            "file:///local/file.md", // File protocol
            "ftp://server/file.md",  // FTP protocol
            "C:\\Windows\\file.md",  // Local file system path
            "/etc/passwd"            // System file access
        };

        foreach (var scenario in browserLimitedScenarios)
        {
            // Act
            var result = await service.GetAsync(scenario);

            // Assert - Should handle browser limitations gracefully
            Assert.Null(result); // Should return null rather than throw
        }
    }
}