using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using MDFileToRazor.Components.Configuration;
using MDFileToRazor.Components.Extensions;
using MDFileToRazor.Components.Services;

namespace MDFileToRazor.Components.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddMdFileToRazorServices_RegistersAllRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Register required dependencies manually for test
        var mockHostEnvironment = new Mock<IHostEnvironment>();
        services.AddSingleton(mockHostEnvironment.Object);

        // Act
        services.AddMdFileToRazorServices(options =>
        {
            options.SourceDirectory = "source";
            options.OutputDirectory = "output";
        });

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<IStaticAssetService>());
        Assert.NotNull(serviceProvider.GetService<IMdFileDiscoveryService>());
        Assert.NotNull(serviceProvider.GetService<IGeneratedPageDiscoveryService>());
        Assert.NotNull(serviceProvider.GetService<IOptions<MdFileToRazorOptions>>());

        var options = serviceProvider.GetRequiredService<IOptions<MdFileToRazorOptions>>().Value;
        Assert.Equal("source", options.SourceDirectory);
        Assert.Equal("output", options.OutputDirectory);
    }

    [Fact]
    public void AddMdFileToRazorServices_WithSourceDirectory_ConfiguresCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockHostEnvironment = new Mock<IHostEnvironment>();
        services.AddSingleton(mockHostEnvironment.Object);

        // Act
        services.AddMdFileToRazorServices("test-source");
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<MdFileToRazorOptions>>().Value;
        Assert.Equal("test-source", options.SourceDirectory);
    }

    [Fact]
    public void AddMdFileToRazorServices_WithSourceAndOutputDirectory_ConfiguresCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockHostEnvironment = new Mock<IHostEnvironment>();
        services.AddSingleton(mockHostEnvironment.Object);

        // Act
        services.AddMdFileToRazorServices("test-source", "test-output");
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var options = serviceProvider.GetRequiredService<IOptions<MdFileToRazorOptions>>().Value;
        Assert.Equal("test-source", options.SourceDirectory);
        Assert.Equal("test-output", options.OutputDirectory);
    }

    [Fact]
    public void AddMdFileToRazorServices_RegistersServicesAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockHostEnvironment = new Mock<IHostEnvironment>();
        services.AddSingleton(mockHostEnvironment.Object);
        services.AddMdFileToRazorServices();

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
}
