using Microsoft.Extensions.DependencyInjection;
using Bunit;
using Xunit;
using MarkdownToRazor.Components;
using MarkdownToRazor.Services;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using System.Text;

namespace MarkdownToRazor.Tests.Components;

/// <summary>
/// Tests to validate UI component rendering and proper usage patterns
/// Ensures MarkdownSection component works correctly and prevents improper usage
/// </summary>
public class MarkdownSectionUiTests : TestContext
{
    public MarkdownSectionUiTests()
    {
        // Register required services for component testing
        Services.AddSingleton<IStaticAssetService, TestStaticAssetService>();
        Services.AddLogging();

        // Mock IJSRuntime for component initialization
        var jsRuntime = Services.AddMockJSRuntime();
    }

    [Fact]
    public void MarkdownSection_WithValidFilePath_RendersCorrectly()
    {
        // Arrange
        var component = RenderComponent<MarkdownSection>(parameters => parameters
            .Add(p => p.FilePath, "content/features.md"));

        // Act & Assert
        Assert.NotNull(component.Find(".markdown-content"));
        var content = component.Markup;
        Assert.Contains("# Features", content);
        Assert.Contains("markdown-content", content);
    }

    [Fact]
    public void MarkdownSection_WithInvalidRelativePath_ShowsError()
    {
        // Arrange & Act
        var component = RenderComponent<MarkdownSection>(parameters => parameters
            .Add(p => p.FilePath, "features.md")); // Invalid - missing content/ prefix

        // Assert
        var content = component.Markup;
        // Component should handle the error gracefully
        Assert.True(content.Contains("error") || content.Contains("not found") || string.IsNullOrEmpty(content.Trim()));
    }

    [Fact]
    public void MarkdownSection_WithContentParameter_RendersDirectly()
    {
        // Arrange
        var markdownContent = "# Test Content\n\nThis is a test.";

        // Act
        var component = RenderComponent<MarkdownSection>(parameters => parameters
            .Add(p => p.Content, markdownContent));

        // Assert
        var content = component.Markup;
        Assert.Contains("Test Content", content);
        Assert.Contains("This is a test", content);
    }

    [Theory]
    [InlineData("features.md")] // Invalid - missing content prefix
    [InlineData("../features.md")] // Invalid - relative path
    [InlineData("./features.md")] // Invalid - relative path
    public void MarkdownSection_WithInvalidPaths_HandlesGracefully(string invalidPath)
    {
        // Arrange & Act
        var component = RenderComponent<MarkdownSection>(parameters => parameters
            .Add(p => p.FilePath, invalidPath));

        // Assert - Should not throw exception
        Assert.NotNull(component);
        // Content should be empty or show error message
        var markup = component.Markup.Trim();
        Assert.True(string.IsNullOrEmpty(markup) ||
                   markup.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                   markup.Contains("not found", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("content/features.md")]
    [InlineData("content/getting-started.md")]
    [InlineData("content/documentation.md")]
    public void MarkdownSection_WithValidContentPaths_LoadsSuccessfully(string validPath)
    {
        // Arrange & Act
        var component = RenderComponent<MarkdownSection>(parameters => parameters
            .Add(p => p.FilePath, validPath));

        // Assert
        Assert.NotNull(component);
        var content = component.Markup;
        Assert.Contains("markdown-content", content);
    }

    [Fact]
    public void MarkdownSection_CodeBlocks_HaveCopyButtons()
    {
        // Arrange
        var markdownWithCode = @"# Code Example

```csharp
public class Test
{
    public string Name { get; set; }
}
```";

        // Act
        var component = RenderComponent<MarkdownSection>(parameters => parameters
            .Add(p => p.Content, markdownWithCode));

        // Assert
        var content = component.Markup;
        Assert.Contains("copy-btn", content);
        Assert.Contains("pre", content);
    }

    [Fact]
    public void MarkdownSection_WithoutParameters_ShowsEmptyState()
    {
        // Arrange & Act
        var component = RenderComponent<MarkdownSection>();

        // Assert
        var content = component.Markup.Trim();
        Assert.True(string.IsNullOrEmpty(content) || content == "<div class=\"markdown-content\"></div>");
    }
}

/// <summary>
/// Test implementation of IStaticAssetService for unit testing
/// Simulates loading markdown files from the content directory
/// </summary>
public class TestStaticAssetService : IStaticAssetService
{
    private readonly Dictionary<string, string> _testFiles = new()
    {
        ["content/features.md"] = @"# Features

## Core Features
- Markdown to Razor conversion
- Build-time code generation
- Runtime markdown rendering
- YAML frontmatter support

## WASM Performance
- Optimized for WebAssembly
- Efficient caching
- Parallel file discovery",

        ["content/getting-started.md"] = @"# Getting Started

## Installation
Install the MarkdownToRazor package from NuGet.

## Basic Usage
1. Add the service to your DI container
2. Configure source directory
3. Use MarkdownSection component",

        ["content/documentation.md"] = @"# Documentation

## Overview
Complete documentation for MarkdownToRazor library.

## Configuration
Configure your markdown processing options.

## Components
Use the provided Blazor components."
    };

    public async Task<string> LoadAssetAsync(string assetPath)
    {
        await Task.Delay(1); // Simulate async operation

        if (_testFiles.TryGetValue(assetPath, out var content))
        {
            return content;
        }

        throw new FileNotFoundException($"Asset not found: {assetPath}");
    }

    public async Task<bool> AssetExistsAsync(string assetPath)
    {
        await Task.Delay(1); // Simulate async operation
        return _testFiles.ContainsKey(assetPath);
    }

    public async Task<Stream> LoadAssetStreamAsync(string assetPath)
    {
        var content = await LoadAssetAsync(assetPath);
        return new MemoryStream(Encoding.UTF8.GetBytes(content));
    }
}
