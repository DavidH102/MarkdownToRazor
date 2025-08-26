using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using MarkdownToRazor.Configuration;
using MarkdownToRazor.Extensions;
using MarkdownToRazor.Services;

namespace MDFileToRazor.Components.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMarkdownToRazorServices_RegistersAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Register required dependencies manually for test
        var mockHostEnvironment = new Mock<IHostEnvironment>();
        services.AddSingleton(mockHostEnvironment.Object);

        // Act
        services.AddMarkdownToRazorServices(options =>
        {
            options.SourceDirectory = "source";
            options.OutputDirectory = "output";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IStaticAssetService>());
        Assert.NotNull(serviceProvider.GetService<IMdFileDiscoveryService>());
        Assert.NotNull(serviceProvider.GetService<IGeneratedPageDiscoveryService>());
        Assert.NotNull(serviceProvider.GetService<IOptions<MarkdownToRazorOptions>>());

        var options = serviceProvider.GetRequiredService<IOptions<MarkdownToRazorOptions>>().Value;
        Assert.Equal("source", options.SourceDirectory);
        Assert.Equal("output", options.OutputDirectory);
    }

    [Fact]
    public void AddMarkdownToRazorServices_WithSourceDirectory_ConfiguresCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockHostEnvironment = new Mock<IHostEnvironment>();
        services.AddSingleton(mockHostEnvironment.Object);

        // Act
        services.AddMarkdownToRazorServices("test-source");
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<MarkdownToRazorOptions>>().Value;
        Assert.Equal("test-source", options.SourceDirectory);
    }

    [Fact]
    public void AddMarkdownToRazorServices_WithSourceAndOutputDirectory_ConfiguresCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockHostEnvironment = new Mock<IHostEnvironment>();
        services.AddSingleton(mockHostEnvironment.Object);

        // Act
        services.AddMarkdownToRazorServices("test-source", "test-output");
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<MarkdownToRazorOptions>>().Value;
        Assert.Equal("test-source", options.SourceDirectory);
        Assert.Equal("test-output", options.OutputDirectory);
    }

    [Fact]
    public void AddMarkdownToRazorServices_RegistersServicesAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockHostEnvironment = new Mock<IHostEnvironment>();
        services.AddSingleton(mockHostEnvironment.Object);
        services.AddMarkdownToRazorServices();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        using var scope1 = serviceProvider.CreateScope();
        using var scope2 = serviceProvider.CreateScope();

        var service1a = scope1.ServiceProvider.GetRequiredService<IGeneratedPageDiscoveryService>();
        var service1b = scope1.ServiceProvider.GetRequiredService<IGeneratedPageDiscoveryService>();
        var service2 = scope2.ServiceProvider.GetRequiredService<IGeneratedPageDiscoveryService>();

        // Assert
        Assert.Same(service1a, service1b); // Same instance within scope
        Assert.NotSame(service1a, service2); // Different instances across scopes
    }

    [Fact]
    public void AddMarkdownToRazorServices_WithAbsolutePath_HandlesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockHostEnvironment = new Mock<IHostEnvironment>();
        var mockContentRoot = Path.Combine(Path.GetTempPath(), "ProjectRoot");
        var absolutePath = Path.Combine(Path.GetTempPath(), "OtherLocation", "MDFiles");
        mockHostEnvironment.Setup(x => x.ContentRootPath).Returns(mockContentRoot);
        services.AddSingleton(mockHostEnvironment.Object);

        // Act - Test absolute path
        services.AddMarkdownToRazorServices(absolutePath);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<MarkdownToRazorOptions>>().Value;
        var sourceDir = options.GetAbsoluteSourcePath(mockContentRoot);
        Assert.Equal(absolutePath, sourceDir);
    }

    [Fact]
    public void AddMarkdownToRazorServices_WithRelativePath_HandlesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockHostEnvironment = new Mock<IHostEnvironment>();
        mockHostEnvironment.Setup(x => x.ContentRootPath).Returns(@"C:\ProjectRoot");
        services.AddSingleton(mockHostEnvironment.Object);

        // Act - Test relative path including ".." for going up directories
        services.AddMarkdownToRazorServices(@"..\..\..\SharedDocs");
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<MarkdownToRazorOptions>>().Value;
        var sourceDir = options.GetAbsoluteSourcePath(@"C:\ProjectRoot");
        // Path.GetFullPath will resolve the .. properly
        Assert.EndsWith("SharedDocs", sourceDir);
        Assert.True(Path.IsPathRooted(sourceDir));
    }
}
