# MDFileToRazor

[![.NET](https://img.shields.io/badge/.NET-8.0-blue)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![NuGet](https://img.shields.io/badge/NuGet-Available%20on%20GitHub%20Packages-blue)](https://github.com/DavidH102/MDFileToRazor/packages)

**Transform your Markdown files into beautiful Blazor pages with automatic routing and syntax highlighting.**

MDFileToRazor is a powerful .NET 8 library that bridges the gap between Markdown content and Blazor applications. Whether you're building documentation sites, blogs, or content-driven applications, this library provides everything you need to seamlessly integrate Markdown into your Blazor projects.

## ✨ What Can You Do?

- 📝 **Runtime Rendering**: Display markdown content dynamically in your Blazor components
- 🏗️ **Build-Time Generation**: Automatically convert markdown files to Razor pages during compilation
- 🎨 **Beautiful Styling**: Integrated with Microsoft FluentUI design system
- 💡 **Syntax Highlighting**: Code blocks with highlight.js integration and copy-to-clipboard functionality
- 🔗 **Automatic Routing**: Generate routable pages from your markdown files with YAML frontmatter or HTML comment configuration support
- 📁 **Flexible Content**: Load from files, URLs, or provide inline markdown content

## 📦 Available Packages

| Package                          | Purpose                                           | Install                                                                                                      |
| -------------------------------- | ------------------------------------------------- | ------------------------------------------------------------------------------------------------------------ |
| **MDFileToRazor.Components**     | Runtime Blazor components for markdown rendering  | `dotnet add package MDFileToRazor.Components --source https://nuget.pkg.github.com/DavidH102/index.json`     |
| **MDFileToRazor.CodeGeneration** | Build-time markdown-to-Razor page generation      | `dotnet add package MDFileToRazor.CodeGeneration --source https://nuget.pkg.github.com/DavidH102/index.json` |
| **MDFileToRazor.MSBuild**        | MSBuild integration for automatic page generation | `dotnet add package MDFileToRazor.MSBuild --source https://nuget.pkg.github.com/DavidH102/index.json`        |

## 🚀 Quick Start

### 1. Runtime Markdown Rendering

Perfect for displaying dynamic markdown content in your Blazor applications:

```bash
dotnet add package MDFileToRazor.Components --source https://nuget.pkg.github.com/DavidH102/index.json
```

**Program.cs:**

```csharp
using MDFileToRazor.Components.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add MDFileToRazor services
builder.Services.AddHttpClient();
builder.Services.AddScoped<StaticAssetService>();

var app = builder.Build();
// ... rest of configuration
```

**Your Blazor Page:**

````razor
@page "/docs"
@using MDFileToRazor.Components

<MarkdownSection Content="@markdownContent" />

@code {
    private string markdownContent = @"
# Welcome to My Documentation

This is **bold text** and this is *italic text*.

```csharp
public class Example
{
    public string Name { get; set; } = ""Hello World"";
}
````

";
}

````

### 2. Build-Time Page Generation

Automatically convert markdown files to routable Blazor pages:

```bash
dotnet add package MDFileToRazor.MSBuild --source https://nuget.pkg.github.com/DavidH102/index.json
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
  <Exec Command="dotnet run --project path/to/MDFileToRazor.CodeGeneration -- &quot;$(MarkdownSourceDirectory)&quot; &quot;$(GeneratedPagesDirectory)&quot;" />
</Target>
```

**Result:** Automatic `/about` route with your markdown content as a Blazor page!

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
- [**Sample Applications**](src/MDFileToRazor.Sample.BlazorServer/) - Working demo projects

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
