using Microsoft.Extensions.DependencyInjection;
using Bunit;
using Xunit;
using MarkdownToRazor.Services;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using System.Text;

namespace MarkdownToRazor.Tests.Integration;

/// <summary>
/// Integration tests for sample pages to ensure proper FilePath usage
/// Validates that sample pages use correct content paths and render properly
/// </summary>
public class SamplePageValidationTests : TestContext
{
    public SamplePageValidationTests()
    {
        // Register required services
        Services.AddSingleton<IStaticAssetService, TestStaticAssetService>();
        Services.AddLogging();
        Services.AddMockJSRuntime();
    }

    [Theory]
    [InlineData("Features", "content/features.md")]
    [InlineData("GettingStarted", "content/getting-started.md")]
    [InlineData("Documentation", "content/documentation.md")]
    public void SamplePages_UseCorrectContentPaths(string pageName, string expectedPath)
    {
        // This test ensures sample pages use proper FilePath format
        // with "content/" prefix, not relative paths like "features.md"

        var pageFilePath = $"h:\\MDFIleTORazor\\src\\MarkdownToRazor.Sample.BlazorWasm\\Pages\\{pageName}.razor";

        // Read the actual page file content
        var pageContent = File.ReadAllText(pageFilePath);

        // Assert correct FilePath usage
        Assert.Contains($"FilePath=\"{expectedPath}\"", pageContent);

        // Assert INCORRECT usage is NOT present
        var incorrectPath = expectedPath.Replace("content/", "");
        Assert.DoesNotContain($"FilePath=\"{incorrectPath}\"", pageContent);
    }

    [Fact]
    public void AllSamplePages_HaveValidPageDirectives()
    {
        var samplePagesDir = "h:\\MDFIleTORazor\\src\\MarkdownToRazor.Sample.BlazorWasm\\Pages";
        var pageFiles = Directory.GetFiles(samplePagesDir, "*.razor", SearchOption.TopDirectoryOnly);

        foreach (var pageFile in pageFiles)
        {
            var content = File.ReadAllText(pageFile);
            var fileName = Path.GetFileNameWithoutExtension(pageFile);

            // Skip if this is not a routable page
            if (!content.Contains("@page"))
                continue;

            // Ensure page has proper route directive
            Assert.Contains("@page", content);

            // If it uses MarkdownSection with FilePath, ensure proper format
            if (content.Contains("MarkdownSection") && content.Contains("FilePath="))
            {
                // Must use content/ prefix for WASM compatibility
                Assert.Contains("FilePath=\"content/", content);
            }
        }
    }

    [Fact]
    public void NoSamplePages_UseRelativeFilePaths()
    {
        // Validates that no sample pages use problematic relative paths
        var samplePagesDir = "h:\\MDFIleTORazor\\src\\MarkdownToRazor.Sample.BlazorWasm\\Pages";
        var pageFiles = Directory.GetFiles(samplePagesDir, "*.razor", SearchOption.TopDirectoryOnly);

        var invalidPatterns = new[]
        {
            "FilePath=\"features.md\"",
            "FilePath=\"getting-started.md\"",
            "FilePath=\"documentation.md\"",
            "FilePath=\"../",
            "FilePath=\"./"
        };

        foreach (var pageFile in pageFiles)
        {
            var content = File.ReadAllText(pageFile);
            var fileName = Path.GetFileName(pageFile);

            foreach (var invalidPattern in invalidPatterns)
            {
                Assert.DoesNotContain(invalidPattern, content,
                    $"Page {fileName} contains invalid FilePath pattern: {invalidPattern}");
            }
        }
    }

    [Fact]
    public void SamplePages_ComponentsCanRender()
    {
        // Test that sample pages can actually render their MarkdownSection components
        var testCases = new[]
        {
            ("Features", "content/features.md"),
            ("GettingStarted", "content/getting-started.md"),
            ("Documentation", "content/documentation.md")
        };

        foreach (var (pageName, expectedPath) in testCases)
        {
            try
            {
                // Create a test component that mimics the sample page
                var component = RenderComponent<MarkdownToRazor.Components.MarkdownSection>(parameters => parameters
                    .Add(p => p.FilePath, expectedPath));

                // Should render without throwing
                Assert.NotNull(component);
                var markup = component.Markup;

                // Should contain markdown content div
                Assert.Contains("markdown-content", markup);
            }
            catch (Exception ex)
            {
                Assert.True(false, $"Sample page {pageName} component failed to render: {ex.Message}");
            }
        }
    }
}

/// <summary>
/// Test asset service that provides sample content for validation
/// </summary>
public class TestStaticAssetService : IStaticAssetService
{
    private readonly Dictionary<string, string> _assets = new()
    {
        ["content/features.md"] = @"# Features

This is the features documentation for MarkdownToRazor.

## Core Features
- Build-time code generation
- Runtime markdown rendering
- YAML frontmatter support

## Performance Features
- WASM optimizations
- Efficient caching
- Parallel processing",

        ["content/getting-started.md"] = @"# Getting Started

Quick start guide for MarkdownToRazor.

## Installation
```bash
dotnet add package MarkdownToRazor
```

## Basic Usage
1. Configure services
2. Add components
3. Start using",

        ["content/documentation.md"] = @"# Documentation

Complete documentation for MarkdownToRazor.

## Configuration Options
- Source directory
- Output directory
- Build integration

## Component Usage
Use the MarkdownSection component properly."
    };

    public async Task<string> LoadAssetAsync(string assetPath)
    {
        await Task.Delay(1); // Simulate async operation

        if (_assets.TryGetValue(assetPath, out var content))
        {
            return content;
        }

        throw new FileNotFoundException($"Test asset not found: {assetPath}");
    }

    public async Task<bool> AssetExistsAsync(string assetPath)
    {
        await Task.Delay(1);
        return _assets.ContainsKey(assetPath);
    }

    public async Task<Stream> LoadAssetStreamAsync(string assetPath)
    {
        var content = await LoadAssetAsync(assetPath);
        return new MemoryStream(Encoding.UTF8.GetBytes(content));
    }
}
