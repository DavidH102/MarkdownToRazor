# MarkdownToRazor

[![.NET](https://img.shields.io/badge/.NET-8.0%20%7C%209.0-blue)](https://dotnet.microsoft.com/download/dotnet)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/nuget/v/MarkdownToRazor?label=NuGet)](https://www.nuget.org/packages/MarkdownToRazor)
[![GitHub Packages](https://img.shields.io/badge/GitHub%20Packages-Available-blue)](https://github.com/DavidH102/MDFileToRazor/packages)

**Transform your Markdown files into beautiful Blazor pages with automatic routing and syntax highlighting.**

MarkdownToRazor is a powerful .NET library that bridges the gap between Markdown content and Blazor applications. Whether you're building documentation sites, blogs, or content-driven applications, this library provides everything you need to seamlessly integrate Markdown into your Blazor projects.

## ✨ What Can You Do?

- 📝 **Runtime Rendering**: Display markdown content dynamically in your Blazor components
- 🏗️ **Build-Time Generation**: Automatically convert markdown files to Razor pages during compilation
- 🧭 **Dynamic UI Generation**: Build navigation menus and content browsers with page discovery service
- 🎨 **Beautiful Styling**: Integrated with Microsoft FluentUI design system
- 💡 **Syntax Highlighting**: Code blocks with highlight.js integration and copy-to-clipboard functionality
- 🔗 **Automatic Routing**: Generate routable pages from your markdown files with YAML frontmatter or HTML comment configuration support
- 📁 **Flexible Content**: Load from files, URLs, or provide inline markdown content

## 🆕 What's New in v2.0.0 - MAJOR RELEASE

🎯 **Single Unified Package** - We've consolidated everything into one powerful package!

### 🔥 **BREAKING CHANGES - Migration Required**

**Package Consolidation**:

- **Old**: 3 separate packages (`MDFileToRazor.Components`, `MDFileToRazor.CodeGeneration`, `MDFileToRazor.MSBuild`)
- **New**: Single `MarkdownToRazor` package with everything included!

**Modernized Naming**:

- **Package**: `MDFileToRazor` → `MarkdownToRazor`
- **Namespaces**: `MDFileToRazor.*` → `MarkdownToRazor.*`
- **Services**: `AddMarkdownToRazorServices()` → `AddMarkdownToRazorServices()`

**Framework Support**:

- ✅ Added .NET 9.0 support
- ✅ Maintained .NET 8.0 support
- ❌ Removed .NET Standard 2.1 (incompatible with Blazor)

### � **Quick Migration**

```csharp
// Before v2.0
builder.Services.AddMarkdownToRazorServices("../content");

// After v2.0
builder.Services.AddMarkdownToRazorServices("../content");
```

### 🛠️ **Enhanced Path Handling & File Discovery**

```csharp
// Get file-to-route mapping for dynamic navigation
var fileRoutes = await MdFileService.DiscoverMarkdownFilesWithRoutesAsync();
foreach (var (fileName, route) in fileRoutes)
{
    Console.WriteLine($"File: {fileName} → Route: {route}");
}
```

### 📁 **Flexible Source Directory Configuration**

```csharp
// Relative paths from project root
builder.Services.AddMarkdownToRazorServices("content/docs");

// Multiple folders up (perfect for shared content)
builder.Services.AddMarkdownToRazorServices("../../../SharedDocumentation");

// Absolute paths (cross-project content sharing)
builder.Services.AddMarkdownToRazorServices(@"C:\SharedContent\ProjectDocs");

// Project root directory
builder.Services.AddMarkdownToRazorServices(".");
```

### 🧪 **Comprehensive Test Coverage**

- **22 passing tests** covering all scenarios
- **Cross-platform path handling** with proper normalization
- **Edge case coverage** for various directory structures

## 🆕 What's New in v1.1.0

✨ **IGeneratedPageDiscoveryService** - Programmatically discover and work with your generated Razor pages!

```csharp
// Inject the service into your components
@inject IGeneratedPageDiscoveryService PageDiscovery

// Get all pages with metadata
var pages = await PageDiscovery.GetAllPagesAsync();

// Filter by tags
var blogPosts = await PageDiscovery.GetPagesByTagAsync("blog");

// Find pages by route pattern
var apiDocs = await PageDiscovery.GetPagesByRoutePatternAsync("/api/*");

// Build dynamic navigation menus
foreach (var page in pages)
{
    Console.WriteLine($"Route: {page.Route}, Title: {page.Title}");
}
```

Perfect for building:

- 📋 **Dynamic sitemaps** from your content
- 🧭 **Automatic navigation menus** that update as you add pages
- 🏷️ **Tag-based content filtering** and organization
- 📊 **Content management dashboards** with page metadata

## 📦 Installation

**Single package with everything included!**

### NuGet.org (Stable Releases)

```bash
dotnet add package MarkdownToRazor
```

### GitHub Packages (Pre-release & Latest)

```bash
dotnet add package MarkdownToRazor --source https://nuget.pkg.github.com/DavidH102/index.json
```

## 🚀 Quick Start

### 1. Build-Time Page Generation (Recommended)

**This is the primary use case** - automatically convert markdown files to routable Blazor pages during build:

```bash
# Single package with all features included
dotnet add package MarkdownToRazor
```

**Two approaches for build-time generation:**

#### Option A: MSBuild Integration (Automatic)

**Add to your .csproj:**

```xml
<PropertyGroup>
  <MarkdownSourceDirectory>$(MSBuildProjectDirectory)\content</MarkdownSourceDirectory>
  <GeneratedPagesDirectory>$(MSBuildProjectDirectory)\Pages\Generated</GeneratedPagesDirectory>
</PropertyGroup>

<Target Name="GenerateMarkdownPages" BeforeTargets="Build">
  <Exec Command="dotnet run --project path/to/MarkdownToRazor.CodeGeneration -- &quot;$(MarkdownSourceDirectory)&quot; &quot;$(GeneratedPagesDirectory)&quot;" />
</Target>
```

#### Option B: Manual Code Generation

```bash
# Run code generation manually
cd CodeGeneration
dotnet run -- "../content" "../Pages/Generated"
```

**Service Registration for Dynamic Navigation:**

Even for build-time scenarios, you often want service registration to build dynamic navigation menus from discovered routes:

```csharp
using MarkdownToRazor.Components.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Register services to enable dynamic UI generation from discovered routes
builder.Services.AddMarkdownToRazorServices(options =>
{
    options.SourceDirectory = "content"; // Where your markdown files are located
    options.BaseRoutePath = "/docs"; // Optional route prefix for generated pages
    // OutputDirectory NOT needed - files are generated at build-time, not runtime
});

var app = builder.Build();
// ... rest of configuration
```

**Dynamic Navigation Example:**

```razor
@inject IMdFileDiscoveryService MdFileDiscovery

<FluentNavGroup Title="Documentation" Icon="@(new Icons.Regular.Size24.DocumentText())">
    @{
        var documentationRoutes = await MdFileDiscovery.DiscoverMarkdownFilesWithRoutesAsync();
        var orderedFiles = documentationRoutes.OrderBy(kvp => GetFileDisplayName(kvp.Key)).ToList();
    }

    @foreach (var (filename, route) in orderedFiles)
    {
        var displayName = GetFileDisplayName(filename);
        <FluentNavLink Href="@route" Match="NavLinkMatch.All"
                       Icon="@GetDocumentationIcon(displayName)">
            @displayName
        </FluentNavLink>
    }
</FluentNavGroup>
```

### 2. Runtime Markdown Rendering (Advanced Use Case)

For displaying dynamic markdown content without generating pages:

**Your Blazor Page:**

````razor
@page "/docs"
@using MarkdownToRazor.Components
@inject IMdFileDiscoveryService MdFileDiscovery

<MarkdownSection Content="@markdownContent" />

<!-- Optional: Display discovered files -->
<MarkdownFileExplorer />

@code {
    private string markdownContent = @"
# Welcome to My Documentation

This is **bold text** and this is *italic text*.

```cs
public class Example
{
    public string Name { get; set; } = ""Hello World"";
}
```

";
    }
}
```

### 2. Build-Time Page Generation

Automatically convert markdown files to routable Blazor pages:

```bash
dotnet add package MarkdownToRazor
````

**Create markdown files with YAML frontmatter:**

`content/about.md:`

```markdown
---
title: About Us
route: /about
layout: MainLayout
---

# About Our Company

We build amazing software...
```

**Or use HTML comment configuration (new in v1.2.0):**

`content/about.md:`

```markdown
<!-- This is configuration data -->
<!-- @page "/about" -->
<!-- title: About Us -->
<!-- layout: MainLayout -->

# About Our Company

We build amazing software...
```

> **💡 Tip**: HTML comment configuration takes precedence over YAML frontmatter when both are present. This provides flexibility for different authoring preferences and tool compatibility.

**Add to your .csproj:**

```xml
<PropertyGroup>
  <MarkdownSourceDirectory>$(MSBuildProjectDirectory)\content</MarkdownSourceDirectory>
  <GeneratedPagesDirectory>$(MSBuildProjectDirectory)\Pages\Generated</GeneratedPagesDirectory>
</PropertyGroup>

<Target Name="GenerateMarkdownPages" BeforeTargets="Build">
  <Exec Command="dotnet run --project path/to/MarkdownToRazor.CodeGeneration -- &quot;$(MarkdownSourceDirectory)&quot; &quot;$(GeneratedPagesDirectory)&quot;" />
</Target>
```

**Result:** Automatic `/about` route with your markdown content as a Blazor page!

## 🏗️ Architecture Overview: When to Use What

### ✅ Build-Time Generation (Primary Use Case - 95% of scenarios)

**What happens:**

1. **Build process** converts `content/about.md` → `Pages/Generated/About.razor`
2. **Generated `.razor` files** have `@page "/about"` directives
3. **Blazor Router** automatically discovers these routes during compilation
4. **Service registration** enables dynamic navigation menus from discovered markdown files

**When to use:** Documentation sites, blogs, content-driven applications where routes are known at build time.

**Key insight:** Service registration is NOT for generating files - it's for building dynamic UI from the files that exist.

```csharp
// Service registration enables dynamic navigation, NOT file generation
builder.Services.AddMarkdownToRazorServices(options =>
{
    options.SourceDirectory = "content"; // Where markdown files are
    options.BaseRoutePath = "/docs";     // Route prefix for generated pages
    // OutputDirectory NOT needed - files generated by build tools
});
```

**Dynamic Navigation Example:**

```csharp
@inject IMdFileDiscoveryService DiscoveryService

<FluentNavGroup Title="Documentation">
    @foreach (var route in markdownRoutes)
    {
        <FluentNavLink Href="@route.Route">@route.Title</FluentNavLink>
    }
</FluentNavGroup>

@code {
    private MarkdownRouteInfo[] markdownRoutes = Array.Empty<MarkdownRouteInfo>();

    protected override async Task OnInitializedAsync()
    {
        markdownRoutes = await DiscoveryService.DiscoverMarkdownFilesWithRoutesAsync();
    }
}
```

### 🔧 Runtime Rendering (Advanced Use Case - 5% of scenarios)

**What happens:**

1. **No file generation** - markdown rendered dynamically by `MarkdownSection` component
2. **Manual route handling** - you create pages that use `<MarkdownSection FromAsset="file.md" />`
3. **Service registration** provides file discovery and content loading

**When to use:** Dynamic content systems, CMS-like scenarios, when content changes frequently.

**Important:** Runtime scenarios do NOT automatically create routable pages - you must create the pages manually.

## 📁 How Markdown File Discovery Works

MarkdownToRazor follows convention-over-configuration principles to automatically discover and process your markdown files:

### 📂 **Flexible Source Directory Configuration**

The `AddMarkdownToRazorServices` method supports various path configurations:

```csharp
// Relative paths from project root
services.AddMarkdownToRazorServices("docs/content");
services.AddMarkdownToRazorServices("../../../SharedDocumentation");

// Project root directory
services.AddMarkdownToRazorServices(".");

// Absolute paths (useful for shared content across projects)
services.AddMarkdownToRazorServices(@"C:\SharedDocs\ProjectDocs");

// Advanced configuration with recursive search
services.AddMarkdownToRazorServices(options => {
    options.SourceDirectory = "content/posts";
    options.SearchRecursively = true; // Finds files in all subdirectories
    options.FilePattern = "*.md";
});
```

### 🎯 **Convention-Based Discovery**

**Default Behavior:**

- **Source Directory**: `MDFilesToConvert/` (relative to your project root)
- **Output Directory**: `Pages/Generated/` (relative to your project root)
- **File Pattern**: All `*.md` files are discovered recursively

**Directory Structure Example:**

```text
YourProject/
├── MDFilesToConvert/           ← Source markdown files
│   ├── about.md               ← Becomes /about route
│   ├── docs/
│   │   ├── getting-started.md ← Becomes /docs/getting-started route
│   │   └── api-reference.md   ← Becomes /docs/api-reference route
│   └── blog/
│       └── 2024/
│           └── news.md        ← Becomes /blog/2024/news route
├── Pages/
│   └── Generated/             ← Auto-generated Razor pages
│       ├── About.razor        ← Generated from about.md
│       ├── DocsGettingStarted.razor
│       ├── DocsApiReference.razor
│       └── Blog2024News.razor
└── YourProject.csproj
```

### ⚙️ **Configuration Options**

**For Build-Time Generation (Most Common):**

```csharp
// Program.cs - Service registration for dynamic navigation only
builder.Services.AddMarkdownToRazorServices(options =>
{
    options.SourceDirectory = "content";          // Where to find .md files
    options.BaseRoutePath = "/docs";               // Optional route prefix
    options.DefaultLayout = "MainLayout";         // Default layout component
    options.EnableYamlFrontmatter = true;         // Enable YAML frontmatter
    // OutputDirectory NOT needed - files generated by build tools
});
```

**For Runtime Rendering (Advanced scenarios):**

```csharp
// Program.cs - Service registration for file loading and rendering
builder.Services.AddMarkdownToRazorServices(options =>
{
    options.SourceDirectory = "content";          // Where to find .md files
    options.FilePattern = "*.md";                 // File pattern to search for
    options.SearchRecursively = true;             // Search subdirectories
    options.EnableHtmlCommentConfiguration = true; // Enable HTML comment config
    options.EnableYamlFrontmatter = true;         // Enable YAML frontmatter
    // OutputDirectory NOT used for runtime scenarios
});
```

**MSBuild Configuration (Build-Time Only):**

```xml
<PropertyGroup>
  <!-- Customize source directory for code generation -->
  <MarkdownSourceDirectory>$(MSBuildProjectDirectory)\docs</MarkdownSourceDirectory>

  <!-- Customize output directory for generated .razor files -->
  <GeneratedPagesDirectory>$(MSBuildProjectDirectory)\Pages\Auto</GeneratedPagesDirectory>
</PropertyGroup>
```

**Simple Service Registration Shortcuts:**

```csharp
// Use defaults for navigation discovery
builder.Services.AddMarkdownToRazorServices();

// Custom source directory only
builder.Services.AddMarkdownToRazorServices("content");
```

### 🧭 **Dynamic Navigation Discovery**

Use service registration to build navigation menus and UI from your markdown files:

**Choose Your Service:**

- `IMdFileDiscoveryService` - Basic route discovery (faster, simpler)
- `IGeneratedPageDiscoveryService` - Rich metadata with titles, descriptions, tags (more features)

```csharp
@inject IMdFileDiscoveryService FileDiscovery

@code {
    private MarkdownRouteInfo[] navigationRoutes = Array.Empty<MarkdownRouteInfo>();

    protected override async Task OnInitializedAsync()
    {
        // Get files with their generated routes for navigation
        navigationRoutes = await FileDiscovery.DiscoverMarkdownFilesWithRoutesAsync();
    }
}

// Build navigation UI
<FluentNavGroup Title="Documentation">
    @foreach (var route in navigationRoutes)
    {
        <FluentNavLink Href="@route.Route">@route.Title</FluentNavLink>
    }
</FluentNavGroup>
```

**Legacy File Discovery (if needed):**

```csharp
// Get all markdown file paths (less common)
var markdownFiles = await FileDiscovery.DiscoverMarkdownFilesAsync();
```

#### Example: Build Navigation Menu from Generated Routes

```razor
<!-- Components/DocumentationNav.razor -->
<FluentNavGroup Title="Documentation">
    @foreach (var route in documentationRoutes.Where(r => r.Route.StartsWith("/docs")))
    {
        <FluentNavLink Href="@route.Route"
                       Title="@route.Description">
            @route.Title
        </FluentNavLink>
    }
</FluentNavGroup>

<FluentNavGroup Title="Blog Posts">
    @foreach (var route in documentationRoutes.Where(r => r.Route.StartsWith("/blog")))
    {
        <FluentNavLink Href="@route.Route">@route.Title</FluentNavLink>
    }
</FluentNavGroup>

@code {
    [Inject] private IMdFileDiscoveryService FileDiscovery { get; set; } = default!;

    private MarkdownRouteInfo[] documentationRoutes = Array.Empty<MarkdownRouteInfo>();

    protected override async Task OnInitializedAsync()
    {
        documentationRoutes = await FileDiscovery.DiscoverMarkdownFilesWithRoutesAsync();
    }
}
```

**Route Generation Examples:**

- `index.md` → `/` (root route)
- `getting-started.md` → `/getting-started`
- `user_guide.md` → `/user-guide` (underscores become hyphens)
- `API Reference.md` → `/api-reference` (spaces normalized)

**Available Services:**

- `IMdFileDiscoveryService` - Discover markdown files based on configuration
- `IStaticAssetService` - Load markdown content from configured directories
- `IGeneratedPageDiscoveryService` - Discover generated Razor pages with routes and metadata (new!)
- `MarkdownToRazorOptions` - Access current configuration settings

### 🧭 **Dynamic UI Generation with Page Discovery**

_New in v1.3.0!_ The `IGeneratedPageDiscoveryService` allows you to build dynamic navigation and UI components by discovering all generated Razor pages with their routes and metadata.

**Perfect for:**

- 📋 Dynamic navigation menus
- 🔍 Site maps and content indexes
- 📊 Content management dashboards
- 🏷️ Tag-based content filtering

#### Basic Usage

```csharp
@inject IGeneratedPageDiscoveryService PageDiscovery

@code {
    private List<GeneratedPageInfo> pages = new();

    protected override async Task OnInitializedAsync()
    {
        pages = (await PageDiscovery.DiscoverGeneratedPagesAsync()).ToList();
    }
}
```

#### Dynamic Navigation Component

```razor
<!-- Components/DynamicNavigation.razor -->
@inject IGeneratedPageDiscoveryService PageDiscovery

<FluentNavGroup Title="Documentation">
    @foreach (var page in documentationPages.Where(p => p.Route.StartsWith("/docs") && p.ShowTitle))
    {
        <FluentNavLink Href="@page.Route"
                       Title="@page.Description">
            @page.Title
        </FluentNavLink>
    }
</FluentNavGroup>

<FluentNavGroup Title="Blog Posts">
    @foreach (var page in documentationPages.Where(p => p.Route.StartsWith("/blog")))
    {
        <FluentNavLink Href="@page.Route">@page.Title</FluentNavLink>
    }
</FluentNavGroup>

@code {
    private GeneratedPageInfo[] documentationPages = Array.Empty<GeneratedPageInfo>();

    protected override async Task OnInitializedAsync()
    {
        documentationPages = (await PageDiscovery.DiscoverGeneratedPagesAsync()).ToArray();
    }
}
```

#### Tag-Based Content Browser

```razor
<!-- Components/ContentBrowser.razor -->
@inject IGeneratedPageDiscoveryService PageDiscovery

<FluentSelect Items="@allTags" @bind-SelectedOption="@selectedTag">
    <OptionTemplate>@context</OptionTemplate>
</FluentSelect>

@if (!string.IsNullOrEmpty(selectedTag))
{
    <div class="content-grid">
        @foreach (var page in filteredPages)
        {
            <FluentCard>
                <FluentAnchor Href="@page.Route">@page.Title</FluentAnchor>
                <p>@page.Description</p>
                <div class="tags">
                    @foreach (var tag in page.Tags)
                    {
                        <FluentBadge>@tag</FluentBadge>
                    }
                </div>
            </FluentCard>
        }
    </div>
}

@code {
    private List<GeneratedPageInfo> allPages = new();
    private List<string> allTags = new();
    private string selectedTag = "";
    private List<GeneratedPageInfo> filteredPages = new();

    protected override async Task OnInitializedAsync()
    {
        allPages = (await PageDiscovery.DiscoverGeneratedPagesAsync()).ToList();
        allTags = allPages.SelectMany(p => p.Tags).Distinct().OrderBy(t => t).ToList();
    }

    private void OnTagSelected()
    {
        filteredPages = allPages.Where(p => p.Tags.Contains(selectedTag)).ToList();
    }
}
```

#### GeneratedPageInfo Properties

```csharp
public class GeneratedPageInfo
{
    public string Route { get; set; }           // Page route (e.g., "/docs/getting-started")
    public string Title { get; set; }           // Page title from frontmatter or filename
    public string? Description { get; set; }    // Meta description
    public string[] Tags { get; set; }          // Tags for categorization
    public bool ShowTitle { get; set; }         // Whether to display title
    public string? Layout { get; set; }         // Layout component name
    public string FilePath { get; set; }        // Original markdown file path
}
```

#### Advanced Filtering

```csharp
// Filter by tag
var docPages = await PageDiscovery.DiscoverGeneratedPagesAsync("documentation");

// Get all pages
var allPages = await PageDiscovery.DiscoverGeneratedPagesAsync();

// Filter programmatically
var blogPosts = allPages.Where(p => p.Route.StartsWith("/blog/"));
var recentPosts = allPages.Where(p => p.Tags.Contains("recent"));
```

**2. Using MSBuild Package (Zero Configuration):**

```bash
dotnet add package MarkdownToRazor --source https://nuget.pkg.github.com/DavidH102/index.json
```

> **✨ Zero Config**: The MSBuild package automatically uses conventions and runs during build!

**3. Manual Tool Execution:**

```bash
dotnet run --project MarkdownToRazor.CodeGeneration -- "source-dir" "output-dir"
```

### 🔄 **Processing Behavior**

**What Gets Processed:**

- ✅ All `.md` files in source directory (recursive)
- ✅ Files with YAML frontmatter configuration
- ✅ Files with HTML comment configuration (new!)
- ✅ Plain markdown files (use filename for route)

**What Gets Generated:**

- 🎯 **Razor Pages**: One `.razor` file per markdown file
- 🔗 **Automatic Routing**: Based on file path or `@page` directive
- 🏷️ **Page Metadata**: Title, description, layout from configuration
- 🎨 **Runtime Rendering**: Uses `MarkdownSection` component for content

**Route Generation Examples:**

```text
Source File                    →  Generated Route
about.md                      →  /about
docs/getting-started.md       →  /docs/getting-started
blog/2024/my-post.md         →  /blog/2024/my-post

With @page directive:
docs/quick-start.md           →  /quick-start (if @page "/quick-start")
```

### 🚀 **Best Practices**

**1. Organize by Content Type:**

```text
MDFilesToConvert/
├── docs/          ← Documentation pages
├── blog/          ← Blog posts
├── guides/        ← User guides
└── legal/         ← Legal pages (privacy, terms)
```

**2. Use Meaningful File Names:**

```text
✅ getting-started.md     → /getting-started
✅ api-reference.md       → /api-reference
❌ page1.md              → /page1 (not descriptive)
```

**3. Include in Version Control:**

```xml
<!-- Include markdown files in your project -->
<ItemGroup>
  <Content Include="MDFilesToConvert\**\*.md">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

**4. Configure Build Integration:**

```xml
<!-- Automatic generation on build -->
<Target Name="GenerateMarkdownPages" BeforeTargets="Build">
  <!-- Your generation command -->
</Target>

<!-- Clean generated files -->
<Target Name="CleanGeneratedPages" BeforeTargets="Clean">
  <RemoveDir Directories="$(GeneratedPagesDirectory)" />
</Target>
```

## ✨ Features

- **🎨 Runtime Rendering**: Display markdown content dynamically in your Blazor applications
- **⚡ Build-Time Generation**: Convert markdown files to routable Blazor pages automatically
- **🎯 YAML Frontmatter & HTML Comments**: Control page routing, layout, title, and metadata using either YAML frontmatter or HTML comment configuration
- **🔥 Syntax Highlighting**: Beautiful code syntax highlighting with copy-to-clipboard
- **📱 Responsive Design**: FluentUI integration for modern, mobile-friendly layouts
- **🔧 MSBuild Integration**: Seamless build-time processing with zero configuration
- **📦 Modular Packages**: Choose only the components you need

## 💡 Use Cases

- **📚 Documentation Sites**: Convert markdown docs to navigable Blazor pages
- **📝 Blog Platforms**: Build content-driven sites with dynamic routing
- **❓ Help Systems**: Embed rich help content directly in your applications
- **🔧 Content Management**: Mix static markdown with dynamic Blazor components
- **📖 Technical Writing**: Author in markdown, publish as interactive web pages

## 🏗️ Architecture

### Runtime Components

- **MarkdownSection.razor**: Main component for dynamic markdown rendering
- **StaticAssetService**: Service for loading content from files or URLs
- **Markdig Extensions**: Custom extensions for enhanced code block rendering

### Build-Time Tools

- **MarkdownToRazorGenerator**: Core engine for converting markdown to Blazor pages
- **MSBuild Tasks**: Automated integration with your build process
- **YAML Parser**: Frontmatter processing for page metadata and routing

### Generated Output

- **Routable Pages**: Automatic Blazor page creation with proper routing
- **Layout Integration**: Seamless integration with your existing Blazor layouts
- **Metadata Handling**: SEO-friendly titles, descriptions, and meta tags

## 📖 Documentation

For complete guides and examples, visit our documentation:

- [**Getting Started**](docs/getting-started.md) - Step-by-step setup instructions
- [**HTML Comment Configuration**](docs/HTML-COMMENT-CONFIGURATION.md) - Alternative to YAML frontmatter using HTML comments
- [**API Reference**](docs/api-reference.md) - Complete component documentation
- [**Examples**](examples/) - Real-world usage patterns and recipes
- [**Sample Applications**](src/MarkdownToRazor.Sample.BlazorServer/) - Working demo projects

## 🤝 Contributing

We welcome contributions! Here's how to get involved:

1. **Fork the repository** and create a feature branch
2. **Follow our coding standards** and ensure tests pass
3. **Submit a pull request** with a clear description of changes
4. **Join the discussion** in issues and help improve the library

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

Built with these amazing open-source projects:

- [**Markdig**](https://github.com/xoofx/markdig) - Fast, powerful markdown processor for .NET
- [**YamlDotNet**](https://github.com/aaubry/YamlDotNet) - .NET library for YAML parsing
- [**FluentUI Blazor**](https://github.com/microsoft/fluentui-blazor) - Microsoft's Fluent UI components for Blazor
- [**highlight.js**](https://highlightjs.org/) - Syntax highlighting for the web

---

⭐ **Found this helpful?** Give us a star and share with your fellow developers!
