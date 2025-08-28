# MarkdownToRazor Documentation

Comprehensive documentation for the MarkdownToRazor library.

## Overview

MarkdownToRazor is a powerful library that enables you to seamlessly integrate Markdown content into your Blazor applications. It supports both build-time code generation and runtime rendering scenarios.

## Architecture

The library follows a dual-purpose architecture:

### Runtime Rendering (Recommended for WASM)

Perfect for Blazor WebAssembly and Azure Static Web Apps:

- Dynamic markdown rendering using the `MarkdownSection` component
- No build-time code generation required
- Files are loaded and rendered on-demand
- Ideal for content management scenarios

### Build-Time Generation

For scenarios requiring compiled pages:

- Converts markdown files to actual `.razor` files during build
- Generated pages become part of your compiled application
- Better performance for static content

## Configuration Options

### MarkdownToRazorOptions

```csharp
public class MarkdownToRazorOptions
{
    /// <summary>
    /// Source directory containing markdown files
    /// </summary>
    public string SourceDirectory { get; set; } = "wwwroot/content";

    /// <summary>
    /// Output directory for generated Razor files (build-time only)
    /// </summary>
    public string? OutputDirectory { get; set; }

    /// <summary>
    /// Default layout for generated pages
    /// </summary>
    public string DefaultLayout { get; set; } = "MainLayout";
}
```

## Service Registration

### Basic Registration

```csharp
builder.Services.AddMarkdownToRazor();
```

### With Configuration

```csharp
builder.Services.AddMarkdownToRazor(options =>
{
    options.SourceDirectory = "content";
    options.DefaultLayout = "MyLayout";
});
```

## Components

### MarkdownSection

The primary component for rendering markdown content:

```razor
<!-- Basic usage -->
<MarkdownSection FilePath="my-file.md" />

<!-- With custom CSS class -->
<MarkdownSection FilePath="my-file.md" CssClass="custom-markdown" />

<!-- With error handling -->
<MarkdownSection FilePath="my-file.md">
    <ErrorContent>
        <p>Failed to load content.</p>
    </ErrorContent>
</MarkdownSection>
```

### MarkdownFileExplorer

Interactive file browser component:

```razor
<MarkdownFileExplorer />
```

## YAML Frontmatter

MarkdownToRazor supports YAML frontmatter for metadata:

```yaml
---
title: "My Page Title"
description: "Page description for SEO"
route: "/custom-route"
layout: "CustomLayout"
showTitle: true
tags: ["blazor", "markdown", "documentation"]
---
# Your Markdown Content

Content goes here...
```

### Supported Properties

- `title`: Page title for HTML head and display
- `description`: Meta description for SEO
- `route`: Custom route path (overrides filename-based routing)
- `layout`: Blazor layout component to use
- `showTitle`: Boolean to control title display in content
- `tags`: Array of tags for categorization

## Markdown Extensions

The library uses Markdig with advanced extensions:

- **Tables**: Full table support with styling
- **Code Blocks**: Syntax highlighting with copy functionality
- **Task Lists**: GitHub-style task lists
- **Emphasis**: Bold, italic, strikethrough
- **Links**: Auto-linking and reference links
- **Images**: Image support with alt text
- **Custom HTML**: Inline HTML support

## Styling

### Default Styling

MarkdownToRazor includes built-in responsive styling that works well with Bootstrap and other CSS frameworks.

### Custom Styling

Override the default styles by targeting the `.markdown-content` class:

```css
.markdown-content {
  /* Your custom styles */
}

.markdown-content h1 {
  color: #333;
  border-bottom: 2px solid #007bff;
}

.markdown-content pre {
  background-color: #f8f9fa;
  border-radius: 0.375rem;
}
```

## Best Practices

### For Azure Static Web Apps

1. Use runtime rendering (no OutputDirectory)
2. Place markdown files in `wwwroot/content`
3. Include content files in your project deployment
4. Use relative paths for images and links

### Performance Optimization

1. Cache markdown content when possible
2. Use efficient file loading patterns
3. Minimize unnecessary component re-renders
4. Consider lazy loading for large content

### SEO Optimization

1. Use YAML frontmatter for metadata
2. Include descriptive titles and descriptions
3. Structure content with proper headings
4. Use semantic HTML elements

## Troubleshooting

### Common Issues

**Files not loading in production:**

- Ensure markdown files are included in deployment
- Check file paths are correct (case-sensitive on Linux)
- Verify HttpClient base address configuration

**Syntax highlighting not working:**

- Ensure highlight.js is loaded
- Check network connectivity for CDN resources
- Verify code block language specifications

**Styling issues:**

- Check CSS class names and specificity
- Ensure Bootstrap or base CSS framework is loaded
- Test responsive behavior on different screen sizes

For more troubleshooting tips, check the [GitHub repository issues](https://github.com/YourOrg/MarkdownToRazor/issues).
