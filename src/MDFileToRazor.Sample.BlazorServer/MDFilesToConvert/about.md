# About Our Application

Welcome to our **Markdown to Razor** demo application! This page was automatically generated from a markdown file during the build process.

## Features

Our application demonstrates several key capabilities:

- **Automatic Code Generation**: Markdown files are converted to Razor pages at build time
- **Syntax Highlighting**: Code blocks are automatically highlighted with copy functionality
- **FluentUI Integration**: All generated pages use Microsoft's FluentUI design system
- **Dynamic Routing**: Each markdown file becomes a routable Blazor page

## Sample Code

Here's an example of how our MarkdownSection component works:

```csharp
@page "/about"
@using MarkdownToRazor.Components

<MarkdownSection FromAsset="about.md" />
```

## Architecture

Our build system follows these steps:

1. **Scan** the `mdFilesToConvert` folder for `.md` files
2. **Generate** corresponding `.razor` pages with appropriate routes
3. **Compile** the generated pages into the application
4. **Serve** the pages as normal Blazor routes

## Benefits

- ✅ **Content-First Development**: Write content in markdown, get pages automatically
- ✅ **Maintainable**: Update content by editing markdown files
- ✅ **Consistent**: All pages use the same layout and styling
- ✅ **Developer Friendly**: Full IntelliSense and debugging support

> **Note**: This page was generated from `mdFilesToConvert/about.md` during the build process!
