# Getting Started with MarkdownToRazor

Welcome to MarkdownToRazor! This guide will help you get up and running quickly.

## Installation

Install the MarkdownToRazor NuGet package in your Blazor project:

```bash
dotnet add package MarkdownToRazor
```

## Basic Setup

### 1. Configure Services

Add MarkdownToRazor services to your `Program.cs`:

```csharp
using MarkdownToRazor.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

// Add HttpClient
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Add MarkdownToRazor services
builder.Services.AddMarkdownToRazor(options =>
{
    options.SourceDirectory = "wwwroot/content";
});

await builder.Build().RunAsync();
```

### 2. Use the MarkdownSection Component

Add markdown content to your Blazor pages:

```razor
@page "/my-page"

<MarkdownSection FilePath="my-content.md" />
```

## Key Features

- ✅ **Runtime Rendering**: Render markdown files dynamically
- ✅ **YAML Frontmatter**: Support for metadata and configuration
- ✅ **Syntax Highlighting**: Beautiful code syntax highlighting
- ✅ **Copy to Clipboard**: Easy code copying functionality
- ✅ **Responsive Design**: Works great on all devices
- ✅ **Azure Static Web Apps**: Perfect for static hosting

## Next Steps

1. Explore the [documentation](documentation) for advanced features
2. Check out the [features](features) page for detailed capabilities
3. Use the [file explorer](explorer) to browse markdown files interactively

Happy coding! 🚀
