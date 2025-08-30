using Microsoft.Extensions.DependencyInjection;
using Bunit;
using Xunit;
using MarkdownToRazor.Services;
using Microsoft.JSInterop;
using Microsoft.Extensions.Logging;
using System.Text;

namespace MarkdownToRazor.Tests.Integration;

/// <summary>
/// Integration tests for sample pages to ensure proper service-only usage in WASM
/// Validates that WASM sample follows service-only pattern without components
/// </summary>
public class SamplePageValidationTests
{
    [Fact]
    public void WasmSample_UsesServiceOnlyApproach()
    {
        // WASM sample should NOT contain any MarkdownSection components
        // It should only demonstrate the service-based approach for navigation

        var basePath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Environment.CurrentDirectory)));
        var samplePagesDir = Path.Combine(basePath!, "src", "MarkdownToRazor.Sample.BlazorWasm", "Pages");
        
        if (!Directory.Exists(samplePagesDir))
        {
            // Skip test if sample directory doesn't exist (e.g., in CI)
            return;
        }

        var pageFiles = Directory.GetFiles(samplePagesDir, "*.razor", SearchOption.TopDirectoryOnly);

        foreach (var pageFile in pageFiles)
        {
            var content = File.ReadAllText(pageFile);
            var fileName = Path.GetFileName(pageFile);

            // WASM sample should NOT use MarkdownSection components (actual component usage)
            Assert.DoesNotContain("<MarkdownSection", content);
            Assert.DoesNotContain("@using MarkdownToRazor.Components", content);
        }
    }

    [Fact]
    public void WasmSample_HasValidPageStructure()
    {
        var basePath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Environment.CurrentDirectory)));
        var samplePagesDir = Path.Combine(basePath!, "src", "MarkdownToRazor.Sample.BlazorWasm", "Pages");
        
        if (!Directory.Exists(samplePagesDir))
        {
            // Skip test if sample directory doesn't exist (e.g., in CI)
            return;
        }
        
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

            // WASM pages should focus on service demonstration, not component usage
            if (fileName.Equals("Index", StringComparison.OrdinalIgnoreCase))
            {
                // Index should explain the service-only approach
                Assert.Contains("IMdFileDiscoveryService", content);
            }
        }
    }

    [Fact]
    public void WasmSample_DoesNotUseProblematicPatterns()
    {
        // Validates that WASM sample does not use component-based patterns
        var basePath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Environment.CurrentDirectory)));
        var samplePagesDir = Path.Combine(basePath!, "src", "MarkdownToRazor.Sample.BlazorWasm", "Pages");
        
        if (!Directory.Exists(samplePagesDir))
        {
            // Skip test if sample directory doesn't exist (e.g., in CI)
            return;
        }
        
        var pageFiles = Directory.GetFiles(samplePagesDir, "*.razor", SearchOption.TopDirectoryOnly);

        var prohibitedPatterns = new[]
        {
            "<MarkdownSection",    // Component usage
            "<MarkdownFileExplorer",   // Component usage
            "FilePath=\"",         // Component property usage
            "@using MarkdownToRazor.Components"   // Component namespace import
        };

        foreach (var pageFile in pageFiles)
        {
            var content = File.ReadAllText(pageFile);
            var fileName = Path.GetFileName(pageFile);

            foreach (var prohibitedPattern in prohibitedPatterns)
            {
                Assert.DoesNotContain(prohibitedPattern, content);
            }
        }
    }

    [Fact]
    public void WasmSample_NavMenuUsesServicePattern()
    {
        // Test that NavMenu uses IMdFileDiscoveryService for navigation generation
        var navMenuPath = "h:\\MDFIleTORazor\\src\\MarkdownToRazor.Sample.BlazorWasm\\Shared\\NavMenu.razor";
        
        if (File.Exists(navMenuPath))
        {
            var content = File.ReadAllText(navMenuPath);
            
            // Should use the discovery service
            Assert.Contains("IMdFileDiscoveryService", content);
            Assert.Contains("DiscoverMarkdownFilesWithRoutesAsync", content);
            
            // Should NOT have static links to removed pages
            Assert.DoesNotContain("href=\"Documentation\"", content);
            Assert.DoesNotContain("href=\"Features\"", content);
            Assert.DoesNotContain("href=\"GettingStarted\"", content);
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

    public async Task<string?> GetAsync(string path)
    {
        await Task.Delay(1); // Simulate async operation

        if (_assets.TryGetValue(path, out var content))
        {
            return content;
        }

        return null; // Return null for not found instead of throwing
    }

    public async Task<string?> GetMarkdownAsync(string relativePath)
    {
        // For test purposes, delegate to GetAsync
        return await GetAsync(relativePath);
    }

    // Legacy methods for backward compatibility with existing tests
    public async Task<string> LoadAssetAsync(string assetPath)
    {
        var result = await GetAsync(assetPath);
        return result ?? throw new FileNotFoundException($"Test asset not found: {assetPath}");
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
