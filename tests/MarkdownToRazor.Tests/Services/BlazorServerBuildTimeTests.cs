using Xunit;
using MarkdownToRazor.CodeGeneration;
using MarkdownToRazor.Services;
using MarkdownToRazor.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace MarkdownToRazor.Tests.Services;

/// <summary>
/// Comprehensive tests for Blazor Server build-time code generation scenarios.
/// Tests the MarkdownToRazorGenerator class and its integration with the MSBuild process.
/// </summary>
public class BlazorServerBuildTimeTests : IDisposable
{
    private readonly string _testSourceDir;
    private readonly string _testOutputDir;

    public BlazorServerBuildTimeTests()
    {
        var tempPath = Path.GetTempPath();
        var testId = Guid.NewGuid().ToString()[..8];
        _testSourceDir = Path.Combine(tempPath, $"MDFilesToConvert_{testId}");
        _testOutputDir = Path.Combine(tempPath, $"Generated_{testId}");

        Directory.CreateDirectory(_testSourceDir);
        Directory.CreateDirectory(_testOutputDir);
    }

    [Fact]
    public async Task BlazorServer_ServiceConfiguration_SupportsBuildTimeGeneration()
    {
        // Arrange
        CreateTestMarkdownFile("welcome.md", @"---
title: Welcome
route: /welcome
---

# Welcome to Build-Time Generation");

        // Act
        var generator = new MarkdownToRazorGenerator();
        await generator.GenerateRazorPagesAsync(_testSourceDir, _testOutputDir);

        // Assert
        Assert.True(Directory.Exists(_testOutputDir), "Output directory should be created");

        var generatedFiles = Directory.GetFiles(_testOutputDir, "*.razor", SearchOption.AllDirectories);
        Assert.Single(generatedFiles);

        var welcomeFile = Path.Combine(_testOutputDir, "Welcome.razor");
        Assert.True(File.Exists(welcomeFile), "Welcome.razor should be generated");
    }

    [Fact]
    public async Task BlazorServer_WithBuildTimeFiles_ServiceDiscoveryWorks()
    {
        // Arrange - Create multiple markdown files with different configurations
        CreateTestMarkdownFile("home.md", @"---
title: Home Page
route: /
layout: MainLayout
showTitle: true
description: Home page description
tags: [home, main]
---

# Welcome Home");

        CreateTestMarkdownFile("about.md", @"---
title: About Us
route: /about
layout: DefaultLayout
tags: [about, company]
---

# About Our Company");

        CreateTestMarkdownFile("contact.md", @"# Contact Us

Simple markdown without frontmatter");

        // Act
        var generator = new MarkdownToRazorGenerator();
        await generator.GenerateRazorPagesAsync(_testSourceDir, _testOutputDir);

        // Assert
        var generatedFiles = Directory.GetFiles(_testOutputDir, "*.razor", SearchOption.AllDirectories);
        Assert.Equal(3, generatedFiles.Length);

        // Verify specific files exist
        Assert.True(File.Exists(Path.Combine(_testOutputDir, "Home.razor")));
        Assert.True(File.Exists(Path.Combine(_testOutputDir, "About.razor")));
        Assert.True(File.Exists(Path.Combine(_testOutputDir, "Contact.razor")));
    }

    [Fact]
    public async Task BlazorServer_CodeGenerator_CreatesValidRazorPages()
    {
        // Arrange
        CreateTestMarkdownFile("example.md", @"---
title: Example Page
route: /example
layout: MainLayout
showTitle: true
description: Example page for testing
tags: [example, test, blazor]
---

# Example Content

This is a **test page** with various markdown features:

## Code Block
```csharp
public class Example 
{
    public string Name { get; set; } = ""Test"";
}
```

## List
- Item 1
- Item 2
- Item 3

## Links
[Visit our site](https://example.com)");

        // Act
        var generator = new MarkdownToRazorGenerator();
        await generator.GenerateRazorPagesAsync(_testSourceDir, _testOutputDir);

        // Assert
        var exampleFile = Path.Combine(_testOutputDir, "Example.razor");
        Assert.True(File.Exists(exampleFile));

        var content = await File.ReadAllTextAsync(exampleFile);

        // Verify Razor page structure
        Assert.Contains("@page \"/docs/example\"", content);
        Assert.Contains("<PageTitle>Example Page</PageTitle>", content);
        Assert.Contains("meta name=\"description\" content=\"Example page for testing\"", content);
        Assert.Contains("MarkdownSection FromAsset=\"MDFilesToConvert/example.md\"", content);

        // Verify tags section
        Assert.Contains("class=\"page-tags\"", content);
        Assert.Contains("example", content);
        Assert.Contains("test", content);
        Assert.Contains("blazor", content);
    }

    [Fact]
    public async Task BlazorServer_HandlesNestedDirectories()
    {
        // Arrange - Create nested directory structure
        var nestedDir = Path.Combine(_testSourceDir, "docs");
        Directory.CreateDirectory(nestedDir);

        var nestedFile = Path.Combine(nestedDir, "guide.md");
        await File.WriteAllTextAsync(nestedFile, @"---
title: User Guide
route: /docs/guide
---

# User Guide Content");

        // Act
        var generator = new MarkdownToRazorGenerator();
        await generator.GenerateRazorPagesAsync(_testSourceDir, _testOutputDir);

        // Assert - Generated files are flattened in output directory
        var guideFile = Path.Combine(_testOutputDir, "Guide.razor");
        Assert.True(File.Exists(guideFile));

        var content = await File.ReadAllTextAsync(guideFile);
        Assert.Contains("@page \"/docs/guide\"", content);
        Assert.Contains("FromAsset=\"MDFilesToConvert/docs/guide.md\"", content);
    }

    [Fact]
    public async Task BlazorServer_HandlesSpecialCharacters()
    {
        // Arrange - Test files with special characters
        CreateTestMarkdownFile("speci@l-ch@rs.md", @"---
title: Special Characters Test
route: /special-chars
---

# Special Characters: éñüñ & symbols!");

        CreateTestMarkdownFile("file with spaces.md", @"---
title: File With Spaces
route: /spaced-file
---

# File with spaces in name");

        // Act
        var generator = new MarkdownToRazorGenerator();
        await generator.GenerateRazorPagesAsync(_testSourceDir, _testOutputDir);

        // Assert - Special characters and spaces are preserved in filenames
        var specialFile = Path.Combine(_testOutputDir, "Speci@lCh@rs.razor");
        var spacedFile = Path.Combine(_testOutputDir, "File with spaces.razor");

        Assert.True(File.Exists(specialFile));
        Assert.True(File.Exists(spacedFile));

        var specialContent = await File.ReadAllTextAsync(specialFile);
        Assert.Contains("@page \"/docs/special-chars\"", specialContent);
    }

    [Fact]
    public async Task BlazorServer_HandlesHtmlCommentConfiguration()
    {
        // Arrange - Test HTML comment configuration
        CreateTestMarkdownFile("html-config.md", @"<!-- This is configuration data -->
<!-- @page ""/html-configured"" -->
<!-- title: HTML Comment Title -->
<!-- layout: CustomLayout -->
<!-- showTitle: false -->
<!-- description: Configured via HTML comments -->
<!-- tags: html, comments, config -->

# HTML Comment Configuration Test

This page is configured using HTML comments instead of YAML frontmatter.");

        // Act
        var generator = new MarkdownToRazorGenerator();
        await generator.GenerateRazorPagesAsync(_testSourceDir, _testOutputDir);

        // Assert
        var htmlConfigFile = Path.Combine(_testOutputDir, "HtmlConfig.razor");
        Assert.True(File.Exists(htmlConfigFile));

        var content = await File.ReadAllTextAsync(htmlConfigFile);
        Assert.Contains("@page \"/html-configured\"", content);
        Assert.Contains("<PageTitle>HTML Comment Title</PageTitle>", content);
        Assert.Contains("Configured via HTML comments", content);
        Assert.DoesNotContain("<h1>HTML Comment Title</h1>", content); // showTitle: false
    }

    [Fact]
    public async Task BlazorServer_GeneratesTagsSection()
    {
        // Arrange
        CreateTestMarkdownFile("tagged.md", @"---
title: Tagged Page
route: /tagged
tags: [web, blazor, markdown, build-time]
---

# Tagged Content");

        // Act
        var generator = new MarkdownToRazorGenerator();
        await generator.GenerateRazorPagesAsync(_testSourceDir, _testOutputDir);

        // Assert
        var taggedFile = Path.Combine(_testOutputDir, "Tagged.razor");
        var content = await File.ReadAllTextAsync(taggedFile);

        // Verify tags section is generated
        Assert.Contains("class=\"page-tags\"", content);
        Assert.Contains("<strong>Tags:</strong>", content);
        Assert.Contains("class=\"tag\"", content);
        Assert.Contains(">web</span>", content);
        Assert.Contains(">blazor</span>", content);
        Assert.Contains(">markdown</span>", content);
        Assert.Contains(">build-time</span>", content);
    }

    [Fact]
    public async Task BlazorServer_HandlesEmptySourceDirectory()
    {
        // Arrange - Create empty directory
        var emptySourceDir = Path.Combine(Path.GetTempPath(), $"Empty_{Guid.NewGuid():N}");
        Directory.CreateDirectory(emptySourceDir);

        try
        {
            // Act
            var generator = new MarkdownToRazorGenerator();
            await generator.GenerateRazorPagesAsync(emptySourceDir, _testOutputDir);

            // Assert - Should not throw exception
            var generatedFiles = Directory.GetFiles(_testOutputDir, "*.razor", SearchOption.AllDirectories);
            Assert.Empty(generatedFiles);
        }
        finally
        {
            if (Directory.Exists(emptySourceDir))
                Directory.Delete(emptySourceDir, true);
        }
    }

    [Fact]
    public async Task BlazorServer_HandlesInvalidYamlFrontmatter()
    {
        // Arrange - Create file with invalid YAML
        CreateTestMarkdownFile("invalid-yaml.md", @"---
title: Valid Title
route: /invalid
invalidYaml: [unclosed array
description: This has invalid YAML
---

# Content After Invalid YAML

This should still be processed despite invalid frontmatter.");

        // Act & Assert - Should not throw exception
        var generator = new MarkdownToRazorGenerator();
        await generator.GenerateRazorPagesAsync(_testSourceDir, _testOutputDir);

        // File should still be generated with defaults
        var invalidFile = Path.Combine(_testOutputDir, "InvalidYaml.razor");
        Assert.True(File.Exists(invalidFile));
    }

    [Fact]
    public async Task BlazorServer_ServiceIntegration_WorksWithDependencyInjection()
    {
        // Arrange - Set up service collection like a Blazor Server app would
        var services = new ServiceCollection();
        var mockHostEnvironment = new Mock<IHostEnvironment>();
        mockHostEnvironment.SetupGet(x => x.ContentRootPath).Returns(_testSourceDir);

        services.AddSingleton(mockHostEnvironment.Object);
        services.AddMarkdownToRazorServices(options =>
        {
            options.SourceDirectory = _testSourceDir;
        });

        CreateTestMarkdownFile("service-test.md", @"---
title: Service Test
route: /service-test
---

# Service Integration Test");

        // Act
        var generator = new MarkdownToRazorGenerator();
        await generator.GenerateRazorPagesAsync(_testSourceDir, _testOutputDir);

        var serviceProvider = services.BuildServiceProvider();
        var discoveryService = serviceProvider.GetRequiredService<IMdFileDiscoveryService>();

        // Assert
        var serviceTestFile = Path.Combine(_testOutputDir, "ServiceTest.razor");
        Assert.True(File.Exists(serviceTestFile));

        // Verify service can discover markdown files
        var files = await discoveryService.DiscoverMarkdownFilesAsync();
        Assert.NotEmpty(files);
    }

    [Fact]
    public async Task BlazorServer_PreservesDirectoryStructure()
    {
        // Arrange - Create complex nested structure
        var level1Dir = Path.Combine(_testSourceDir, "level1");
        var level2Dir = Path.Combine(level1Dir, "level2");
        Directory.CreateDirectory(level2Dir);

        var rootFile = Path.Combine(_testSourceDir, "root.md");
        var level1File = Path.Combine(level1Dir, "level1.md");
        var level2File = Path.Combine(level2Dir, "level2.md");

        await File.WriteAllTextAsync(rootFile, "# Root Level");
        await File.WriteAllTextAsync(level1File, "# Level 1");
        await File.WriteAllTextAsync(level2File, "# Level 2");

        // Act
        var generator = new MarkdownToRazorGenerator();
        await generator.GenerateRazorPagesAsync(_testSourceDir, _testOutputDir);

        // Assert - Files are flattened but references preserve paths
        Assert.True(File.Exists(Path.Combine(_testOutputDir, "Root.razor")));
        Assert.True(File.Exists(Path.Combine(_testOutputDir, "Level1.razor")));
        Assert.True(File.Exists(Path.Combine(_testOutputDir, "Level2.razor")));

        // Verify content references correct nested paths
        var level2Content = await File.ReadAllTextAsync(Path.Combine(_testOutputDir, "Level2.razor"));
        Assert.Contains("FromAsset=\"MDFilesToConvert/level1/level2/level2.md\"", level2Content);
    }

    private void CreateTestMarkdownFile(string fileName, string content)
    {
        var filePath = Path.Combine(_testSourceDir, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, content);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testSourceDir))
                Directory.Delete(_testSourceDir, true);

            if (Directory.Exists(_testOutputDir))
                Directory.Delete(_testOutputDir, true);
        }
        catch
        {
            // Best effort cleanup
        }
    }
}