using Xunit;
using System.Diagnostics;
using System.Text;

namespace MarkdownToRazor.Tests.Integration;

/// <summary>
/// Integration tests for Blazor Server MSBuild integration.
/// Tests the complete build-time code generation workflow.
/// </summary>
public class BlazorServerMSBuildIntegrationTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _testProjectPath;

    public BlazorServerMSBuildIntegrationTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
        _testProjectPath = Path.Combine(_tempDirectory, "TestBlazorServer");
    }

    [Fact]
    public void MSBuild_BlazorServer_GeneratesRazorPagesFromMarkdown()
    {
        // Arrange - Create a minimal Blazor Server project
        CreateTestBlazorServerProject();
        CreateTestMarkdownFiles();

        // Act - Build the project
        var buildResult = RunDotNetBuild();

        // Assert
        Assert.True(buildResult.Success, $"Build failed: {buildResult.Output}");

        // Verify generated pages exist
        var generatedPagesDir = Path.Combine(_testProjectPath, "Pages", "Generated");
        Assert.True(Directory.Exists(generatedPagesDir), "Generated pages directory should exist");

        // Check specific generated files
        var welcomePagePath = Path.Combine(generatedPagesDir, "Welcome.razor");
        Assert.True(File.Exists(welcomePagePath), "Welcome.razor should be generated");

        var welcomeContent = File.ReadAllText(welcomePagePath);
        Assert.Contains("@page \"/welcome\"", welcomeContent);
        Assert.Contains("MarkdownSection FromAsset=\"MDFilesToConvert/welcome.md\"", welcomeContent);
        Assert.Contains("Welcome to MarkdownToRazor", welcomeContent);
    }

    [Fact]
    public void MSBuild_BlazorServer_HandlesYamlFrontmatter()
    {
        // Arrange
        CreateTestBlazorServerProject();
        CreateMarkdownWithYamlFrontmatter();

        // Act
        var buildResult = RunDotNetBuild();

        // Assert
        Assert.True(buildResult.Success, $"Build failed: {buildResult.Output}");

        var generatedPagePath = Path.Combine(_testProjectPath, "Pages", "Generated", "CustomRoute.razor");
        Assert.True(File.Exists(generatedPagePath));

        var content = File.ReadAllText(generatedPagePath);
        Assert.Contains("@page \"/custom/path\"", content);
        Assert.Contains("@layout CustomLayout", content);
        Assert.Contains("<PageTitle>Custom Title</PageTitle>", content);
        Assert.Contains("<meta name=\"description\" content=\"Custom description\"", content);
    }

    [Fact]
    public void MSBuild_BlazorServer_HandlesNestedDirectories()
    {
        // Arrange
        CreateTestBlazorServerProject();
        CreateNestedMarkdownFiles();

        // Act
        var buildResult = RunDotNetBuild();

        // Assert
        Assert.True(buildResult.Success, $"Build failed: {buildResult.Output}");

        // Verify nested structure is preserved
        var nestedPagePath = Path.Combine(_testProjectPath, "Pages", "Generated", "Docs", "GettingStarted.razor");
        Assert.True(File.Exists(nestedPagePath), "Nested page should be generated");

        var content = File.ReadAllText(nestedPagePath);
        Assert.Contains("FromAsset=\"MDFilesToConvert/docs/getting-started.md\"", content);
    }

    [Fact]
    public void MSBuild_BlazorServer_IncrementalBuild_OnlyProcessesChangedFiles()
    {
        // Arrange
        CreateTestBlazorServerProject();
        CreateTestMarkdownFiles();

        // First build
        var firstBuild = RunDotNetBuild();
        Assert.True(firstBuild.Success);

        var welcomePagePath = Path.Combine(_testProjectPath, "Pages", "Generated", "Welcome.razor");
        var originalModifyTime = File.GetLastWriteTime(welcomePagePath);

        // Wait to ensure different timestamps
        Thread.Sleep(1000);

        // Modify one markdown file
        var markdownPath = Path.Combine(_testProjectPath, "MDFilesToConvert", "about.md");
        File.AppendAllText(markdownPath, "\n\n## Updated Content");

        // Act - Second build
        var secondBuild = RunDotNetBuild();

        // Assert
        Assert.True(secondBuild.Success);

        // Welcome.razor should not have been regenerated
        var newModifyTime = File.GetLastWriteTime(welcomePagePath);
        Assert.Equal(originalModifyTime, newModifyTime);

        // About.razor should have been regenerated
        var aboutPagePath = Path.Combine(_testProjectPath, "Pages", "Generated", "About.razor");
        var aboutContent = File.ReadAllText(aboutPagePath);
        Assert.Contains("Updated Content", aboutContent);
    }

    [Fact]
    public void MSBuild_BlazorServer_CleanTarget_RemovesGeneratedFiles()
    {
        // Arrange
        CreateTestBlazorServerProject();
        CreateTestMarkdownFiles();

        // Build first
        var buildResult = RunDotNetBuild();
        Assert.True(buildResult.Success);

        var generatedPagesDir = Path.Combine(_testProjectPath, "Pages", "Generated");
        Assert.True(Directory.Exists(generatedPagesDir));

        // Act - Clean
        var cleanResult = RunDotNetClean();

        // Assert
        Assert.True(cleanResult.Success, $"Clean failed: {cleanResult.Output}");

        // Generated pages should be removed
        if (Directory.Exists(generatedPagesDir))
        {
            var remainingFiles = Directory.GetFiles(generatedPagesDir, "*.razor", SearchOption.AllDirectories);
            Assert.Empty(remainingFiles);
        }
    }

    [Fact]
    public void MSBuild_BlazorServer_WithHtmlCommentConfiguration()
    {
        // Arrange
        CreateTestBlazorServerProject();
        CreateMarkdownWithHtmlComments();

        // Act
        var buildResult = RunDotNetBuild();

        // Assert
        Assert.True(buildResult.Success, $"Build failed: {buildResult.Output}");

        var generatedPagePath = Path.Combine(_testProjectPath, "Pages", "Generated", "ConfiguredPage.razor");
        var content = File.ReadAllText(generatedPagePath);

        // Verify HTML comment configuration is applied
        Assert.Contains("@page \"/configured\"", content);
        Assert.Contains("@layout SpecialLayout", content);
        Assert.Contains("Special Title", content);
    }

    private void CreateTestBlazorServerProject()
    {
        Directory.CreateDirectory(_testProjectPath);

        // Create project file
        var projectContent = @"<Project Sdk=""Microsoft.NET.Sdk.Web"">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""MarkdownToRazor"" Version=""*"" />
  </ItemGroup>

  <PropertyGroup>
    <MarkdownSourceDirectory>MDFilesToConvert</MarkdownSourceDirectory>
    <MarkdownOutputDirectory>Pages/Generated</MarkdownOutputDirectory>
  </PropertyGroup>

</Project>";

        File.WriteAllText(Path.Combine(_testProjectPath, "TestBlazorServer.csproj"), projectContent);

        // Create Program.cs
        var programContent = @"using MarkdownToRazor.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMarkdownToRazor(options =>
{
    options.SourceDirectory = ""MDFilesToConvert"";
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(""/Error"");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.MapBlazorHub();
app.MapFallbackToPage(""/_Host"");

app.Run();";

        File.WriteAllText(Path.Combine(_testProjectPath, "Program.cs"), programContent);

        // Create _Imports.razor
        var importsContent = @"@using System.Net.Http
@using Microsoft.AspNetCore.Authorization
@using Microsoft.AspNetCore.Components.Authorization
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.JSInterop
@using MarkdownToRazor.Components";

        File.WriteAllText(Path.Combine(_testProjectPath, "_Imports.razor"), importsContent);
    }

    private void CreateTestMarkdownFiles()
    {
        var markdownDir = Path.Combine(_testProjectPath, "MDFilesToConvert");
        Directory.CreateDirectory(markdownDir);

        // Welcome.md
        var welcomeContent = @"---
title: Welcome to MarkdownToRazor  
route: /welcome
layout: MainLayout
showTitle: true
description: Welcome page for MarkdownToRazor
tags: [welcome, demo, build-time]
---

# Welcome to Build-Time Generation Demo!

This page was **automatically generated** from a markdown file during the build process.

## How It Works

1. **Author Content**: Write markdown files with YAML frontmatter
2. **Build Process**: Run `dotnet build` to trigger code generation  
3. **Generated Pages**: Physical `.razor` files are created
4. **Automatic Routing**: Pages are routed based on frontmatter";

        File.WriteAllText(Path.Combine(markdownDir, "welcome.md"), welcomeContent);

        // About.md
        var aboutContent = @"---
title: About This Demo
route: /about
layout: MainLayout
tags: [about, demo]
---

# About This Blazor Server Demo

This demo showcases **MarkdownToRazor's build-time code generation** capabilities.";

        File.WriteAllText(Path.Combine(markdownDir, "about.md"), aboutContent);
    }

    private void CreateMarkdownWithYamlFrontmatter()
    {
        var markdownDir = Path.Combine(_testProjectPath, "MDFilesToConvert");
        Directory.CreateDirectory(markdownDir);

        var content = @"---
title: Custom Title
route: /custom/path
layout: CustomLayout
showTitle: false
description: Custom description
tags: [custom, yaml, test]
---

# Custom Page Content

This page tests YAML frontmatter processing.";

        File.WriteAllText(Path.Combine(markdownDir, "custom-route.md"), content);
    }

    private void CreateNestedMarkdownFiles()
    {
        var docsDir = Path.Combine(_testProjectPath, "MDFilesToConvert", "docs");
        Directory.CreateDirectory(docsDir);

        var content = @"---
title: Getting Started
route: /docs/getting-started
---

# Getting Started Guide

This is nested content to test directory structure preservation.";

        File.WriteAllText(Path.Combine(docsDir, "getting-started.md"), content);
    }

    private void CreateMarkdownWithHtmlComments()
    {
        var markdownDir = Path.Combine(_testProjectPath, "MDFilesToConvert");
        Directory.CreateDirectory(markdownDir);

        var content = @"<!-- route: /configured -->
<!-- layout: SpecialLayout -->
<!-- title: Special Title -->
<!-- showTitle: true -->

# Configured Page

This page uses HTML comment configuration instead of YAML frontmatter.";

        File.WriteAllText(Path.Combine(markdownDir, "configured-page.md"), content);
    }

    private BuildResult RunDotNetBuild()
    {
        return RunDotNetCommand("build");
    }

    private BuildResult RunDotNetClean()
    {
        return RunDotNetCommand("clean");
    }

    private BuildResult RunDotNetCommand(string command)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = command,
            WorkingDirectory = _testProjectPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(processStartInfo);
        if (process == null)
        {
            return new BuildResult { Success = false, Output = "Failed to start dotnet process" };
        }

        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (sender, e) => { if (e.Data != null) output.AppendLine(e.Data); };
        process.ErrorDataReceived += (sender, e) => { if (e.Data != null) error.AppendLine(e.Data); };

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        var fullOutput = output.ToString();
        if (error.Length > 0)
        {
            fullOutput += "\nERROR:\n" + error.ToString();
        }

        return new BuildResult
        {
            Success = process.ExitCode == 0,
            Output = fullOutput
        };
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
        {
            try
            {
                Directory.Delete(_tempDirectory, true);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }

    private class BuildResult
    {
        public bool Success { get; set; }
        public string Output { get; set; } = string.Empty;
    }
}