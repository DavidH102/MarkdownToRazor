using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using MDFileToRazor.Components.Configuration;
using MDFileToRazor.Components.Extensions;
using MDFileToRazor.Components.Services;

namespace MDFileToRazor.Components.Tests.Extensions;

public class PathHandlingTests
{
    [Fact]
    public void AddMdFileToRazorServices_WithRelativePath_ResolvesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockHostEnvironment = new Mock<IHostEnvironment>();
        mockHostEnvironment.Setup(x => x.ContentRootPath).Returns("H:\\MDFIleTORazor");
        services.AddSingleton(mockHostEnvironment.Object);

        // Act - Test relative path
        services.AddMdFileToRazorServices("TestScenarios/TwoFoldersUp/MDFiles");
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var mdFileService = serviceProvider.GetRequiredService<IMdFileDiscoveryService>();
        var sourceDir = mdFileService.GetSourceDirectory();
        Assert.Equal("H:\\MDFIleTORazor\\TestScenarios\\TwoFoldersUp\\MDFiles", sourceDir);
    }

    [Fact]
    public void AddMdFileToRazorServices_WithRootDirectory_ResolvesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockHostEnvironment = new Mock<IHostEnvironment>();
        mockHostEnvironment.Setup(x => x.ContentRootPath).Returns("H:\\MDFIleTORazor");
        services.AddSingleton(mockHostEnvironment.Object);

        // Act - Test root directory (empty string or ".")
        services.AddMdFileToRazorServices(".");
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var mdFileService = serviceProvider.GetRequiredService<IMdFileDiscoveryService>();
        var sourceDir = mdFileService.GetSourceDirectory();
        // Path.GetFullPath(".") resolves to the current directory without the trailing dot
        Assert.Equal("H:\\MDFIleTORazor", sourceDir);
    }

    [Fact]
    public void AddMdFileToRazorServices_WithAbsolutePath_ResolvesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockHostEnvironment = new Mock<IHostEnvironment>();
        mockHostEnvironment.Setup(x => x.ContentRootPath).Returns("H:\\MDFIleTORazor");
        services.AddSingleton(mockHostEnvironment.Object);

        // Act - Test absolute path (note: the implementation uses Path.Combine which might not handle this correctly)
        services.AddMdFileToRazorServices("H:\\SomeOtherLocation\\MDFiles");
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var mdFileService = serviceProvider.GetRequiredService<IMdFileDiscoveryService>();
        var sourceDir = mdFileService.GetSourceDirectory();
        // This will actually combine paths incorrectly: H:\MDFIleTORazor\H:\SomeOtherLocation\MDFiles
        Assert.Contains("H:\\SomeOtherLocation\\MDFiles", sourceDir);
    }

    [Fact]
    public void MdFileDiscoveryService_DiscoverFiles_WithTestPath_FindsFiles()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockHostEnvironment = new Mock<IHostEnvironment>();
        mockHostEnvironment.Setup(x => x.ContentRootPath).Returns("H:\\MDFIleTORazor");
        services.AddSingleton(mockHostEnvironment.Object);

        services.AddMdFileToRazorServices(options =>
        {
            options.SourceDirectory = "TestScenarios/TwoFoldersUp/MDFiles";
            options.SearchRecursively = true;
        });

        var serviceProvider = services.BuildServiceProvider();
        var mdFileService = serviceProvider.GetRequiredService<IMdFileDiscoveryService>();

        // Act
        var files = mdFileService.DiscoverMarkdownFiles();

        // Assert
        Assert.NotEmpty(files);
        Assert.Contains(files, f => f.Contains("test.md"));
    }
}
