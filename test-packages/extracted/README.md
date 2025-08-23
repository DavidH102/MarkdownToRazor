# MDFileToRazor - Markdown to Blazor Component Library

A comprehensive .NET 8 library that provides both runtime markdown rendering components and build-time code generation to convert Markdown files into Blazor Razor pages with automatic routing.

## 🚀 Project Vision & NuGet Roadmap

This project is being structured for **NuGet package distribution** to provide the community with powerful markdown-to-Blazor tooling. The ultimate goal is to offer multiple NuGet packages:

- **`MDFileToRazor.Components`** - Runtime markdown rendering components
- **`MDFileToRazor.CodeGeneration`** - Build-time markdown-to-Razor page generation
- **`MDFileToRazor.MSBuild`** - MSBuild targets and tasks for seamless integration

### 📦 Planned NuGet Packages

| Package                        | Purpose                                          | Target Audience                                                   |
| ------------------------------ | ------------------------------------------------ | ----------------------------------------------------------------- |
| `MDFileToRazor.Components`     | Blazor components for runtime markdown rendering | Developers who want to display markdown content dynamically       |
| `MDFileToRazor.CodeGeneration` | Build-time code generation tools                 | Developers building documentation sites, blogs, or static content |
| `MDFileToRazor.MSBuild`        | MSBuild integration and targets                  | Projects wanting automatic markdown-to-page conversion            |
| `MDFileToRazor.Templates`      | Project templates and scaffolding                | Quick-start templates for new projects                            |

## ✅ Project Status Update

**🎉 NuGet Package Structure Successfully Implemented!**

The project restructuring is complete and all packages build successfully:

```bash
✅ MDFileToRazor.CodeGeneration.1.0.0-preview.1.nupkg - Build-time code generation
✅ MDFileToRazor.Components.1.0.0-preview.1.nupkg - Runtime Blazor components
✅ MDFileToRazor.MSBuild.1.0.0-preview.1.nupkg - MSBuild integration
```

**What's Working:**

- ✅ Multi-project solution structure following .NET best practices
- ✅ All projects build successfully in Debug and Release modes
- ✅ NuGet packages created with proper metadata and dependencies
- ✅ MSBuild integration for automatic code generation during build
- ✅ Proper separation of concerns across runtime and build-time components
- ✅ Package references and dependencies correctly resolved

**Ready for:** Testing, documentation improvements, and public NuGet publication!

## 🏗️ Project Structure

This solution follows .NET library best practices with clear separation of concerns:

```text
MDFileToRazor/
├── src/                                    # Source code (for NuGet packages)
│   ├── MDFileToRazor.Components/          # 📦 Runtime Blazor components
│   │   ├── Components/                     # Blazor components
│   │   │   ├── MarkdownSection.razor      # Main markdown rendering component
│   │   │   ├── MarkdownSection.razor.cs   # Component logic
│   │   │   ├── MarkdownSection.razor.css  # Component styles
│   │   │   └── MarkdownSection.razor.js   # JavaScript interop
│   │   ├── Extensions/                     # Markdig extensions
│   │   │   ├── MarkdownSectionPreCodeExtension.cs
│   │   │   ├── MarkdownSectionPreCodeRenderer.cs
│   │   │   └── MarkdownSectionPreCodeRendererOptions.cs
│   │   ├── Services/                       # Services and interfaces
│   │   │   ├── IStaticAssetService.cs
│   │   │   └── StaticAssetService.cs
│   │   └── MDFileToRazor.Components.csproj
│   │
│   ├── MDFileToRazor.CodeGeneration/      # 📦 Build-time code generation
│   │   ├── Generators/
│   │   │   └── MarkdownToRazorGenerator.cs # Core generation logic
│   │   ├── Models/                         # Data models
│   │   ├── Templates/                      # Razor page templates
│   │   └── MDFileToRazor.CodeGeneration.csproj
│   │
│   └── MDFileToRazor.MSBuild/             # 📦 MSBuild integration
│       ├── Tasks/                          # MSBuild tasks
│       ├── Targets/                        # MSBuild targets
│       └── MDFileToRazor.MSBuild.csproj
│
├── samples/                                # Example applications
│   ├── MDFileToRazor.Sample.BlazorServer/ # Demo Blazor Server app
│   │   ├── Components/                     # Sample components
│   │   ├── Pages/                          # Demo pages
│   │   │   └── Generated/                  # Auto-generated pages
│   │   ├── Shared/                         # Layouts
│   │   ├── MDFilesToConvert/              # Source markdown files
│   │   └── MDFileToRazor.Sample.BlazorServer.csproj
│   │
│   └── MDFileToRazor.Sample.WebAssembly/  # Demo Blazor WASM app
│
├── tests/                                  # Unit and integration tests
│   ├── MDFileToRazor.Components.Tests/
│   ├── MDFileToRazor.CodeGeneration.Tests/
│   └── MDFileToRazor.MSBuild.Tests/
│
├── tools/                                  # Development tools
│   └── CodeGeneration/                     # Standalone code generation tool
│
├── docs/                                   # Documentation
│   ├── getting-started.md
│   ├── api-reference.md
│   └── examples/
│
└── build/                                  # Build scripts and configuration
    ├── pack.ps1                           # NuGet packing script
    ├── version.props                       # Version management
    └── Directory.Build.props               # Global MSBuild properties
```

### 🎯 Current Phase: Restructuring for NuGet

#### Phase 1 - Project Restructuring (Current)

- [ ] Create separate projects for each NuGet package
- [ ] Move components to `src/MDFileToRazor.Components`
- [ ] Move code generation to `src/MDFileToRazor.CodeGeneration`
- [ ] Create MSBuild integration project
- [ ] Set up proper solution structure

#### Phase 2 - Package Preparation

- [ ] Add proper package metadata and descriptions
- [ ] Create comprehensive XML documentation
- [ ] Set up versioning strategy
- [ ] Create project templates

#### Phase 3 - Testing & Documentation

- [ ] Comprehensive unit test coverage
- [ ] Integration tests for MSBuild tasks
- [ ] API documentation with DocFX
- [ ] Sample applications demonstrating usage

#### Phase 4 - NuGet Publishing

- [ ] Set up CI/CD pipeline for automated builds
- [ ] Configure NuGet publishing workflow
- [ ] Create prerelease packages for testing
- [ ] Official NuGet package release

## ✨ Features

- **Markdown Processing**: Uses the Markdig library for robust markdown parsing
- **Syntax Highlighting**: Integrated highlight.js for code block syntax highlighting
- **Copy Functionality**: Click-to-copy buttons for all code blocks
- **FluentUI Integration**: Styled with Microsoft FluentUI design system
- **File Loading**: Load markdown content from files or provide inline content

## Getting Started

1. **Install Dependencies**: The project requires these NuGet packages:

   - `Markdig` (version 0.38.0 or higher)
   - `Microsoft.FluentUI.AspNetCore.Components` (version 4.12.1 or higher)

2. **Run the Application**:

   ```bash
   dotnet run
   ```

3. **View the Demo**: Navigate to `/markdown-demo` to see the component in action

## Usage

### Basic Usage with Inline Content

```razor
<MarkdownSection Content="@markdownContent" />

@code {
    private string markdownContent = "# Hello World\nThis is **bold** text.";
}
```

### Loading from File

```razor
<MarkdownSection FromAsset="myfile.md" />
```

### Component Parameters

- **Content**: Direct markdown content as a string
- **FromAsset**: Path to a markdown file in wwwroot folder or a URL

## Architecture

The component consists of:

- `MarkdownSection.razor`: Main component for rendering
- `MarkdownSection.razor.cs`: Code-behind with markdown processing logic
- `MarkdownSectionPreCodeExtension.cs`: Custom Markdig extension for enhanced code blocks
- `MarkdownSectionPreCodeRenderer.cs`: Custom HTML renderer for code blocks
- `StaticAssetService.cs`: Service for loading content from files/URLs
- `MarkdownSection.razor.js`: JavaScript module for syntax highlighting

## Dependencies

- **Markdig**: Markdown processing engine
- **highlight.js**: Syntax highlighting (loaded via CDN)
- **FluentUI**: Microsoft's design system for Blazor

## Credits

This implementation is based on the MarkdownSection component from Microsoft's FluentUI Blazor repository:
[Microsoft FluentUI Blazor](https://github.com/microsoft/fluentui-blazor)

## Contributing

We welcome contributions! Please see our contributing guidelines for more information on how to get involved in making this project ready for NuGet distribution.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
