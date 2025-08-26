# Getting Started with MarkdownToRazor

## Quick Start

### 1. Install the NuGet Package

For runtime markdown rendering:

```bash
dotnet add package MarkdownToRazor.Components
```

For build-time code generation:

```bash
dotnet add package MarkdownToRazor.MSBuild
```

### 2. Configure Your Project

Add to your `_Imports.razor`:

```razor
@using MarkdownToRazor.Components
@using MarkdownToRazor.Components.Services
```

Register services in `Program.cs`:

```csharp
builder.Services.AddScoped<IStaticAssetService, StaticAssetService>();
```

### 3. Use the MarkdownSection Component

```razor
<MarkdownSection Content="@markdownContent" />

@code {
    private string markdownContent = "# Hello World\nThis is **bold** text.";
}
```

### 4. Build-Time Code Generation

Create a `MDFilesToConvert` folder in your project root and add markdown files with YAML frontmatter:

```markdown
---
title: "My Page"
route: "/my-custom-route"
layout: "CustomLayout"
---

# My Page Content

This will be converted to a Razor page automatically during build.
```

The MSBuild integration will automatically generate Razor pages in `Pages/Generated/` during compilation.

## Next Steps

- [API Reference](api-reference.md)
- [Examples](examples/)
- [Configuration Options](configuration.md)
