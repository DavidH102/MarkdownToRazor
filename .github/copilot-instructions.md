# MDFileToRazor Project Instructions

## Project Overview

This project is a .NET 8 Blazor Server application that implements build-time code generation to convert Markdown files into Razor pages with automatic routing. The system processes Markdown files with YAML frontmatter and generates corresponding Blazor pages that are integrated into the application's routing system.

## Project Structure

- `/Components/` - Custom Blazor components including MarkdownSection for runtime markdown rendering
- `/Services/` - Service layer including StaticAssetService for file loading operations
- `/Pages/` - Blazor pages and routing components
- `/Pages/Generated/` - Auto-generated Razor pages created by the build system (build output)
- `/CodeGeneration/` - Separate console application project for build-time code generation
- `/MDFilesToConvert/` - Source markdown files with optional YAML frontmatter for processing
- `/wwwroot/` - Static web assets and client-side resources
- `/.github/` - Repository configuration and Copilot instructions

## Key Technologies and Libraries

- .NET 8 with Blazor Server for web application framework
- Microsoft.FluentUI.AspNetCore.Components (4.12.1+) for UI components
- Markdig (0.38.0+) for markdown processing and HTML conversion
- YamlDotNet (15.1.4+) for parsing YAML frontmatter in markdown files
- highlight.js (via CDN) for syntax highlighting in code blocks
- MSBuild targets for build-time code generation integration

## Architecture Patterns

The project follows a dual-purpose architecture:

1. **Build-Time Code Generation**: Converts markdown files to Razor pages during build
2. **Runtime Markdown Rendering**: MarkdownSection component for dynamic content display

Key components:

- `MarkdownToRazorGenerator` - Core code generation engine with YAML frontmatter support
- `MarkdownSection` - Runtime component for rendering markdown content with syntax highlighting
- `StaticAssetService` - Service for loading markdown files from filesystem or URLs

## Coding Standards

- Use file-scoped namespace declarations (C# 10+)
- Follow Blazor component patterns with proper lifecycle management
- Use async/await for all asynchronous operations
- Implement proper error handling and validation for file operations
- Use PascalCase for public members, camelCase for private fields
- Prefer explicit component properties over @bind directives
- Use [Inject] attribute for dependency injection in Blazor components

## File Organization Conventions

**Blazor Components**:

- `[ComponentName].razor` - Component markup and directives
- `[ComponentName].razor.cs` - Code-behind logic and lifecycle methods
- `[ComponentName].razor.css` - Component-specific styles
- `[ComponentName].razor.js` - Component-specific JavaScript modules

**Project References**:

- Main project: `MDFIleTORazor.csproj` (Blazor Server application)
- Code generation: `CodeGeneration/CodeGeneration.csproj` (Console application)
- Source files: `MDFilesToConvert/` directory (capitalized, not camelCase)
- Generated output: `Pages/Generated/` directory

## Build Integration

The project uses MSBuild targets for code generation:

- `GenerateMarkdownPages` target converts markdown files to Razor pages
- `CleanGeneratedPages` target removes generated files during clean operations
- Generated pages are automatically included in compilation by the .NET SDK

## Markdown Processing Features

**Supported YAML Frontmatter Properties**:

- `route` - Custom route path (overrides filename-based routing)
- `title` - Page title for HTML head and display
- `layout` - Blazor layout component to use
- `showTitle` - Boolean to control title display
- `description` - Meta description for SEO
- `tags` - Array of tags for categorization

**Markdown Features**:

- Standard markdown syntax with Markdig advanced extensions
- Syntax highlighting for code blocks using highlight.js
- Copy-to-clipboard functionality for code blocks
- Support for tables, lists, links, images, and custom HTML
- FluentUI styling integration for consistent appearance

## Development Workflow

**Code Generation Commands**:

```bash
# Manual code generation
cd CodeGeneration
dotnet run -- "../MDFilesToConvert" "../Pages/Generated"

# Build with code generation
dotnet build -t:GenerateMarkdownPages

# Clean generated files
dotnet build -t:CleanGeneratedPages
```

**Testing and Validation**:

- Use `dotnet run` to start the Blazor Server application
- Access generated pages at their configured routes
- Test markdown rendering with `/MarkdownDemo` page
- Verify syntax highlighting and copy functionality work correctly

## Error Handling Requirements

- Validate markdown file paths before processing
- Implement try-catch blocks for file I/O operations
- Provide meaningful error messages for build failures
- Handle missing frontmatter gracefully with sensible defaults
- Validate YAML frontmatter syntax and provide clear error messages

## Performance Considerations

- Cache processed markdown content when possible
- Use efficient file loading patterns in StaticAssetService
- Minimize unnecessary component re-renders with proper StateHasChanged usage
- Optimize build-time generation for large numbers of markdown files
- Use ShouldRender override for performance-critical components
