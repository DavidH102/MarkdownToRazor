---
title: Build-Time Features
route: /features
layout: MainLayout
showTitle: true
description: Comprehensive list of MarkdownToRazor build-time code generation features
tags: [features, build-time, msbuild, code-generation]
---

# Build-Time Code Generation Features

MarkdownToRazor provides powerful build-time capabilities for converting markdown files into Blazor Razor pages.

## 🚀 Core Features

### YAML Frontmatter Support
- **Route Customization**: Define custom routes with `route: /custom-path`
- **Layout Selection**: Choose layouts with `layout: MyCustomLayout`  
- **SEO Metadata**: Set titles, descriptions, and meta tags
- **Content Control**: Toggle title display with `showTitle: true/false`
- **Tagging System**: Organize content with `tags: [tag1, tag2]`

### HTML Comment Configuration
Alternative to YAML frontmatter using HTML comments:

```html
<!-- This is configuration data -->
<!-- @page "/custom-route" -->
<!-- title: My Page Title -->
<!-- layout: MainLayout -->
<!-- description: SEO description -->
```

### Advanced Routing
- **Filename-based Routes**: Automatic routes from filenames
- **Custom Route Paths**: Override with frontmatter or HTML comments
- **Route Validation**: Ensures valid Blazor routing patterns
- **Nested Directories**: Supports hierarchical content organization

## 🛠️ MSBuild Integration

### Automatic Generation
```xml
<!-- Runs before compilation -->
<Target Name="GenerateMarkdownPages" BeforeTargets="CoreCompile">
```

### Configurable Paths
```xml
<PropertyGroup>
  <MarkdownSourceDirectory>$(MSBuildProjectDirectory)/docs</MarkdownSourceDirectory>
  <GeneratedPagesDirectory>$(MSBuildProjectDirectory)/Pages/Auto</GeneratedPagesDirectory>
</PropertyGroup>
```

### Build Performance
- **Incremental Processing**: Only processes changed files
- **Parallel Generation**: Multi-threaded for large projects
- **Memory Efficient**: Optimized for large markdown collections

## 📝 Content Processing

### Markdown Extensions
- **Tables**: Full GitHub Flavored Markdown table support
- **Code Blocks**: Syntax highlighting with language detection
- **Task Lists**: Checkbox todo items
- **Strikethrough**: ~~Cross out~~ text support
- **Autolinks**: Automatic URL detection and linking

### Code Syntax Highlighting
Supports 40+ programming languages:

```csharp
public class Example
{
    public string Property { get; set; } = "Hello World";
}
```

```javascript
const example = {
    message: "JavaScript syntax highlighting",
    isAwesome: true
};
```

```python
def example_function():
    """Python syntax highlighting example"""
    return "This code is highlighted!"
```

## 🎨 Styling & Themes

### FluentUI Integration
- **Consistent Design**: Matches Microsoft Fluent Design System
- **Responsive Layout**: Mobile-first responsive components
- **Dark/Light Mode**: Automatic theme detection
- **Accessibility**: WCAG 2.1 compliant generated HTML

### Custom CSS Support
- **Scoped Styles**: Component-specific styling
- **Global Themes**: Site-wide styling customization
- **CSS Variables**: Dynamic theming support

## 🔧 Development Experience

### IDE Integration
- **IntelliSense**: Full code completion for generated pages
- **Debugging**: Standard Blazor debugging support
- **Hot Reload**: Changes reflect immediately during development
- **Error Reporting**: Clear error messages for invalid markdown

### Build Integration
- **Visual Studio**: MSBuild integration in VS 2022+
- **VS Code**: Full support with C# extension
- **CLI Support**: Works with `dotnet build` and `dotnet watch`
- **CI/CD Ready**: Integrates with GitHub Actions, Azure DevOps

---

## Next Steps

Try modifying this markdown file and running `dotnet build` to see the changes reflected in the generated Razor page!

*Build-time generation provides the perfect balance of authoring convenience and runtime performance.*