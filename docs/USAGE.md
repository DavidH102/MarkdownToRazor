# Using MDFileToRazor NuGet Packages

This guide explains how to install and use the MDFileToRazor NuGet packages from GitHub Packages in your Blazor projects.

## Overview

MDFileToRazor provides three NuGet packages:

- **`MDFileToRazor.Components`** - Blazor components for runtime markdown rendering
- **`MDFileToRazor.CodeGeneration`** - Build-time code generation tools
- **`MDFileToRazor.MSBuild`** - MSBuild integration for automatic code generation

## Two Usage Patterns

MDFileToRazor supports two distinct usage patterns:

### 1. Runtime Markdown Rendering (Recommended for most scenarios)

- Markdown files are rendered dynamically at runtime using the `MarkdownSection` component
- Routes are discovered automatically from markdown files
- **No OutputDirectory needed** - no files are generated
- Uses `IMdFileDiscoveryService` for route discovery

### 2. Build-Time Code Generation

- Converts markdown files to actual `.razor` files during build
- Generated pages become part of your compiled application
- **Requires both SourceDirectory AND OutputDirectory**
- Used via CodeGeneration project or MSBuild targets

## Prerequisites

- .NET 8.0 or later
- A Blazor Server or Blazor WebAssembly project
- Access to GitHub Packages (requires authentication)

## Authentication Setup

### 1. Create a GitHub Personal Access Token

1. Go to GitHub Settings → Developer settings → Personal access tokens → Tokens (classic)
2. Click "Generate new token (classic)"
3. Select the following scopes:
   - `read:packages` (to download packages)
   - `repo` (if working with private repositories)
4. Copy the generated token

### 2. Configure NuGet Authentication

#### Option A: Using nuget.config (Recommended)

Create a `nuget.config` file in your project root:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
    <add key="github" value="https://nuget.pkg.github.com/DavidMarsh-NOAA/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="YOUR_GITHUB_USERNAME" />
      <add key="ClearTextPassword" value="YOUR_GITHUB_TOKEN" />
    </github>
  </packageSourceCredentials>
</configuration>
```

Replace `YOUR_GITHUB_USERNAME` and `YOUR_GITHUB_TOKEN` with your values.

#### Option B: Using Environment Variables

Set the following environment variables:

```bash
# Windows
set NUGET_AUTH_TOKEN=your_github_token

# Linux/macOS
export NUGET_AUTH_TOKEN=your_github_token
```

#### Option C: Using dotnet CLI

```bash
dotnet nuget add source "https://nuget.pkg.github.com/DavidMarsh-NOAA/index.json" \
  --name "github" \
  --username "YOUR_GITHUB_USERNAME" \
  --password "YOUR_GITHUB_TOKEN" \
  --store-password-in-clear-text
```

## Installation

### For Runtime Markdown Rendering Only

If you only need to render markdown content at runtime:

```bash
dotnet add package MDFileToRazor.Components --source github
```

### For Build-Time Code Generation

If you want to convert markdown files to Razor pages during build:

```bash
# Add the main components package
dotnet add package MDFileToRazor.Components --source github

# Add the MSBuild integration
dotnet add package MDFileToRazor.MSBuild --source github
```

### Complete Installation (All Features)

For full functionality including build-time generation and runtime rendering:

```bash
dotnet add package MDFileToRazor.Components --source github
dotnet add package MDFileToRazor.CodeGeneration --source github
dotnet add package MDFileToRazor.MSBuild --source github
```

## Package Versions

The packages use semantic versioning with the following patterns:

- **Main branch**: `1.0.X` (stable releases)
- **Develop branch**: `1.0.X-alpha.Y` (pre-release)
- **Feature branches**: `1.0.X-feature-name.Y` (development)
- **Tags**: `1.0.X` or `1.0.X-beta.Y` (releases)

To install a specific version:

```bash
dotnet add package MDFileToRazor.Components --version 1.0.0-alpha.1 --source github
```

## Configuration

### 1. Register Services

Add the following to your `Program.cs`:

```csharp
using MDFileToRazor.Components.Services;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(); // For Blazor Server

// Add FluentUI services (required for styling)
builder.Services.AddFluentUIComponents();

// Add HttpClient for StaticAssetService
builder.Services.AddHttpClient();

// Register MDFileToRazor services for RUNTIME markdown rendering
// Note: OutputDirectory is NOT needed for runtime-only scenarios
builder.Services.AddMdFileToRazorServices(options =>
{
    options.SourceDirectory = "MDFilesToConvert"; // Where your .md files are located
    options.BaseRoutePath = "/docs";               // Optional base route path
    // No OutputDirectory needed - files are rendered dynamically at runtime
    // No DefaultLayout needed - component uses app's default layout automatically
});

// Alternative configurations for runtime scenarios:
// builder.Services.AddMdFileToRazorServices(); // Use defaults (source: "MDFilesToConvert")

// Relative paths from project root:
// builder.Services.AddMdFileToRazorServices("content");
// builder.Services.AddMdFileToRazorServices("docs/markdown");
// builder.Services.AddMdFileToRazorServices("../../../SharedDocs"); // Multiple folders up

// Project root directory:
// builder.Services.AddMdFileToRazorServices(".");

// Absolute paths (useful for shared content):
// builder.Services.AddMdFileToRazorServices(@"C:\SharedDocumentation\ProjectDocs");

// ONLY for build-time code generation (not typical runtime scenarios):
// builder.Services.AddMdFileToRazorServices(options =>
// {
//     options.SourceDirectory = "content";
//     options.OutputDirectory = "Pages/Generated"; // Required for build-time generation
// });

var app = builder.Build();

// Configure the pipeline
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub(); // For Blazor Server
app.MapFallbackToPage("/_Host"); // For Blazor Server

app.Run();
```

### 2. Update \_Imports.razor

Add the following to your `_Imports.razor`:

```razor
@using MDFileToRazor.Components
@using MDFileToRazor.Components.Services
@using Microsoft.FluentUI.AspNetCore.Components
```

### 3. Include CSS and JavaScript

Add to your `_Host.cshtml` (Blazor Server) or `index.html` (Blazor WebAssembly):

```html
<head>
  <!-- FluentUI CSS -->
  <link
    href="https://cdn.jsdelivr.net/npm/@fluentui/web-components/dist/themes/fluent.css"
    rel="stylesheet"
  />

  <!-- Highlight.js for syntax highlighting -->
  <link
    rel="stylesheet"
    href="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/default.min.css"
  />
  <script src="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/highlight.min.js"></script>

  <!-- Your app styles -->
  <link href="css/app.css" rel="stylesheet" />
  <link href="YourApp.styles.css" rel="stylesheet" />
</head>
```

## Usage Examples

### Runtime Markdown Rendering

Use the `MarkdownSection` component to render markdown content:

````razor
@page "/example"

<h1>Markdown Examples</h1>

<!-- Render inline markdown -->
<MarkdownSection Content="@inlineMarkdown" />

<!-- Load markdown from a file -->
<MarkdownSection FromAsset="content/example.md" />

@code {
    private string inlineMarkdown = @"
# Hello World

This is **bold** text and this is *italic* text.

```csharp
public class Example
{
    public string Message { get; set; } = ""Hello, World!"";
}
````

";
}

`````

### File Discovery and Route Mapping

The `IMdFileDiscoveryService` provides several methods to discover markdown files and understand their generated routes:

````razor
@page "/file-explorer"
@inject IMdFileDiscoveryService MdFileDiscovery

<h1>Markdown File Explorer</h1>

<!-- Basic file discovery -->
<h2>Available Files</h2>
@foreach (var file in markdownFiles)
{
    <p>@file</p>
}

<!-- File to route mapping -->
<h2>File Route Mapping</h2>
@foreach (var (filename, route) in fileRouteMap)
{
    <div class="file-route-item">
        <strong>@filename</strong> → <code>@route</code>
        <a href="@route" target="_blank">Visit Page</a>
    </div>
}

@code {
    private IEnumerable<string> markdownFiles = Enumerable.Empty<string>();
    private Dictionary<string, string> fileRouteMap = new();

    protected override async Task OnInitializedAsync()
    {
        // Get all markdown files
        markdownFiles = await MdFileDiscovery.DiscoverMarkdownFilesAsync();

        // Get files with their generated routes
        fileRouteMap = await MdFileDiscovery.DiscoverMarkdownFilesWithRoutesAsync();
    }
}
`````

Available methods:

- `DiscoverMarkdownFiles()` - Returns file paths
- `DiscoverMarkdownFilesAsync()` - Async version of file discovery
- `DiscoverMarkdownFilesWithRoutes()` - Returns Dictionary<filename, route>
- `DiscoverMarkdownFilesWithRoutesAsync()` - Async version with route mapping

The route mapping follows these conventions:

- `index.md` → `/` (root route)
- `getting-started.md` → `/getting-started`
- `user_guide.md` → `/user-guide` (underscores become hyphens)
- `My Document.md` → `/my-document` (spaces and special chars normalized)

### Build-Time Code Generation

#### 1. Project Structure

Create the following structure in your project:

```

```

YourProject/
├── MDFilesToConvert/
│ ├── about.md
│ ├── getting-started.md
│ └── features.md
├── Pages/
│ └── Generated/ (auto-created)
└── YourProject.csproj

```

```

#### 2. Configure MSBuild

Update your `.csproj` file:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>

    <!-- Configure paths for code generation -->
    <GeneratedPagesDirectory>$(MSBuildProjectDirectory)\Pages\Generated</GeneratedPagesDirectory>
    <MarkdownSourceDirectory>$(MSBuildProjectDirectory)\MDFilesToConvert</MarkdownSourceDirectory>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="MDFileToRazor.Components" Version="1.0.0" />
    <PackageReference Include="MDFileToRazor.MSBuild" Version="1.0.0" />
    <PackageReference Include="Microsoft.FluentUI.AspNetCore.Components" Version="4.12.1" />
  </ItemGroup>

  <!-- Include markdown files as content -->
  <ItemGroup>
    <Content Include="MDFilesToConvert\**\*.md">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </Content>
  </ItemGroup>

</Project>
```

#### 3. Create Markdown Files

Create markdown files with optional YAML frontmatter:

**MDFilesToConvert/about.md:**

```markdown
---
title: About Us
route: /company/about
layout: MainLayout
showTitle: true
description: Learn more about our company
tags: [company, about]
---

# About Our Company

This is the about page content...
```

**MDFilesToConvert/getting-started.md:**

```markdown
---
title: Getting Started Guide
showTitle: false
---

# Quick Start

Follow these steps to get started...
```

#### 4. Build Your Project

The markdown files will be automatically converted to Razor pages during build:

```bash
dotnet build
```

This generates pages accessible at:

- `/docs/about` (from about.md)
- `/docs/getting-started` (from getting-started.md)

Note: All generated routes automatically get a `/docs/` prefix.

#### 5. Manual Generation (Optional)

You can also manually trigger code generation:

```bash
dotnet build -t:GenerateMarkdownPages
```

Or clean generated files:

```bash
dotnet build -t:CleanGeneratedPages
```

## YAML Frontmatter Options

The following YAML frontmatter properties are supported:

```yaml
---
# Custom route (overrides filename-based routing)
route: /custom/path

# Page title for HTML head and display
title: My Page Title

# Blazor layout component to use
layout: MainLayout

# Whether to show the title in the page
showTitle: true

# Meta description for SEO
description: Page description for search engines

# Tags for categorization
tags: [tag1, tag2, tag3]
---
```

## Troubleshooting

For detailed troubleshooting instructions, see the [Troubleshooting Guide](TROUBLESHOOTING.md).

Common issues include:

- Authentication problems with GitHub Packages
- Package installation failures
- Build and runtime configuration issues
- Missing dependencies or incorrect service registration

## Advanced Configuration

### Custom CSS Styling

Override the default FluentUI styles by adding custom CSS:

```css
/* Custom markdown styling */
.markdown-content h1 {
  color: #2c3e50;
  border-bottom: 2px solid #3498db;
}

.markdown-content code {
  background-color: #f8f9fa;
  padding: 2px 4px;
  border-radius: 3px;
}

.markdown-content pre {
  background-color: #282c34;
  color: #abb2bf;
  padding: 1rem;
  border-radius: 4px;
  overflow-x: auto;
}
```

### Syntax Highlighting Themes

Change the highlight.js theme by updating the CSS link:

```html
<!-- Different highlight.js themes -->
<link
  rel="stylesheet"
  href="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/github.min.css"
/>
<link
  rel="stylesheet"
  href="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/vs2015.min.css"
/>
<link
  rel="stylesheet"
  href="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/atom-one-dark.min.css"
/>
```

## Sample Project

For a complete working example, see the sample project in the repository:
[https://github.com/DavidMarsh-NOAA/MDFileToRazor/tree/main/samples](https://github.com/DavidMarsh-NOAA/MDFileToRazor/tree/main/samples)

## Support

- **Issues**: [GitHub Issues](https://github.com/DavidMarsh-NOAA/MDFileToRazor/issues)
- **Discussions**: [GitHub Discussions](https://github.com/DavidMarsh-NOAA/MDFileToRazor/discussions)
- **Documentation**: [Project Wiki](https://github.com/DavidMarsh-NOAA/MDFileToRazor/wiki)
