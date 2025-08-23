# Copilot Instructions for MDFileToRazor

## Project Information

- **Project Name**: MDFileToRazor
- **Technology**: .NET 8 Blazor Server App
- **Primary Purpose**: Build-time code generation system that converts Markdown files to Razor pages with automatic routing
- **Key Libraries**: Microsoft FluentUI for Blazor, Markdig for markdown processing, YamlDotNet for frontmatter parsing
- **Architecture**: Build-time code generation with separate CodeGeneration project, custom MarkdownSection component for rendering

## Project Structure Overview

**Root Directory Structure**:

```
MDFIleTORazor/
├── Components/               # Blazor components (MarkdownSection, etc.)
├── Services/                # Service layer (StaticAssetService, etc.)
├── Pages/                   # Blazor pages and demos
│   └── Generated/           # Auto-generated Razor pages (build output)
├── CodeGeneration/          # Separate project for build-time code generation
├── MDFilesToConvert/        # Source markdown files for processing
├── wwwroot/                 # Static web assets
└── MDFIleTORazor.csproj     # Main Blazor Server project
```

**CodeGeneration Project**:

```
CodeGeneration/
├── MarkdownToRazorGenerator.cs  # Core code generation logic
├── Program.cs                   # Console app entry point
└── CodeGeneration.csproj        # Independent build project
```

## Core Architecture: Build-Time Code Generation

**Primary Workflow**:

1. **Source**: Markdown files in `MDFilesToConvert/` with optional YAML frontmatter
2. **Processing**: `MarkdownToRazorGenerator` converts MD → Razor pages
3. **Output**: Generated Razor pages in `Pages/Generated/` with automatic routing
4. **Integration**: MSBuild targets automate generation during build process

**Key Components**:

- **MarkdownToRazorGenerator.cs** - Core code generation engine with YAML frontmatter parsing
- **MarkdownSection.razor** - Runtime component for rendering markdown content
- **StaticAssetService.cs** - Service for loading markdown files at runtime
- **MSBuild Targets** - Build integration for automatic page generation
- **Services/** - Service layer including StaticAssetService for file loading
- **Pages/** - Blazor pages and demos
- **wwwroot/** - Static files including sample markdown files
- **mdFilesToConvert/** - Source markdown files for processing

**Key Components**:

- **MarkdownSection.razor** - Main component for rendering markdown as HTML
- **MarkdownSectionPreCodeExtension.cs** - Custom Markdig extension for enhanced code blocks
- **MarkdownSectionPreCodeRenderer.cs** - Custom HTML renderer for code blocks with copy functionality
- **StaticAssetService.cs** - Service for loading content from files or URLs

## Blazor Code Style and Structure

- **Always place `@page` directive at the very top of Razor files** for routable components
- **Razor file structure order**:
  1. `@page` directive (if routable)
  2. `@using` statements
  3. `@inject` directives
  4. Other directives
  5. HTML markup
  6. `@code` block
- Use `async`/`await` for asynchronous operations
- **Prefer explicit component properties**: Use `Value=@model.Property ValueChanged=@OnPropertyChanged` instead of `@bind-Value=@model.Property`
- Use `[Inject]` for dependency injection in code blocks
- Use file-scoped namespace declarations (C# 10+)

## MarkdownSection Component Usage

**Basic Usage Patterns**:

```razor
<!-- Inline markdown content -->
<MarkdownSection Content="@markdownString" />

<!-- Load from file in wwwroot -->
<MarkdownSection FromAsset="myfile.md" />

<!-- Load from URL -->
<MarkdownSection FromAsset="https://example.com/content.md" />
```

**Component Parameters**:

- **Content** (string?): Direct markdown content to render
- **FromAsset** (string?): Path to markdown file in wwwroot or external URL

**Features**:

- Markdown-to-HTML conversion using Markdig
- Syntax highlighting with highlight.js
- Copy-to-clipboard functionality for code blocks
- FluentUI styling integration
- Support for tables, lists, links, images, and code blocks

## Key Dependencies and Libraries

**NuGet Packages**:

- `Microsoft.FluentUI.AspNetCore.Components` (4.12.1+) - FluentUI components
- `Markdig` (0.38.0+) - Markdown processing engine

**External Dependencies**:

- `highlight.js` - Loaded via CDN for syntax highlighting
- FluentUI Web Components - For consistent styling

## Service Integration Patterns

**Dependency Injection**:

```csharp
// In Program.cs
builder.Services.AddFluentUIComponents();
builder.Services.AddScoped<IStaticAssetService, StaticAssetService>();
builder.Services.AddHttpClient();
```

**StaticAssetService Usage**:

- Loads content from wwwroot files or external URLs
- Handles HTTP requests for remote markdown content
- Provides fallback mechanisms for file loading

## Development Workflows

**Building & Running**:

- Main command: `dotnet run` from project root
- Access at `https://localhost:7064` (or configured port)
- Demo page available at `/markdown-demo`

**Adding New Markdown Features**:

1. Extend Markdig pipeline in `MarkdownSection.razor.cs`
2. Add custom renderers if needed (follow PreCodeRenderer pattern)
3. Update JavaScript module for client-side enhancements
4. Test with sample content in `/markdown-demo`

## File Organization Conventions

**Component Structure**:

- `[ComponentName].razor` - Main component markup
- `[ComponentName].razor.cs` - Code-behind logic
- `[ComponentName].razor.css` - Component-specific styles
- `[ComponentName].razor.js` - Component-specific JavaScript

**Naming Conventions**:

- Use PascalCase for component names and public members
- Use camelCase for private fields and parameters
- Prefix interfaces with "I"
- Namespace: `MDFIleTORazor.[Folder]`

## Error Handling and Validation

- Implement error boundaries for Blazor components
- Use try-catch blocks for file loading operations
- Validate markdown content before processing
- Provide user feedback for loading states and errors

## Performance Optimization

- Use `StateHasChanged()` efficiently
- Minimize unnecessary component re-renders
- Cache processed markdown when possible
- Use `ShouldRender()` for performance-critical components

## Testing and Debugging

- Test markdown rendering with various content types
- Verify syntax highlighting works for different languages
- Test file loading from both local and remote sources
- Use browser dev tools for JavaScript debugging
- Test copy functionality across different browsers

## FluentUI Integration Best Practices

- **Before implementing new UI patterns**: Check the microsoft/fluentui-blazor repository via DeepWiki MCP for established patterns
- **For component styling**: Use FluentUI design tokens and CSS custom properties
- **For component behavior**: Follow FluentUI accessibility and interaction patterns
- **For complex layouts**: Reference FluentUI demo applications and examples in the repository
- **For theming**: Use FluentUI's theming system rather than custom CSS when possible

## Build Validation

**Development Process**:

1. Use `dotnet build` to validate changes
2. Test component functionality in `/markdown-demo`
3. Verify markdown processing with different content types
4. Check syntax highlighting and copy functionality
5. Validate responsive design and accessibility

**Common Issues**:

- Ensure highlight.js is loaded before component initialization
- Verify markdown file paths are correct for StaticAssetService
- Check FluentUI component dependencies are properly registered
- Validate Markdig extensions are configured correctly
