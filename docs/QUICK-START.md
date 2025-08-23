# Quick Start Guide

Get up and running with MDFileToRazor in 5 minutes.

## Prerequisites

- .NET 8.0 or later
- A Blazor Server or WebAssembly project
- GitHub account with access to DavidMarsh-NOAA/MDFileToRazor repository

## Step 1: Authentication

Create a GitHub Personal Access Token:

1. Go to GitHub Settings → Developer settings → Personal access tokens → Tokens (classic)
2. Generate new token with `read:packages` scope
3. Save the token securely

## Step 2: Configure Package Source

Create `nuget.config` in your project root:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
    <add key="github" value="https://nuget.pkg.github.com/DavidMarsh-NOAA/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="YOUR_GITHUB_USERNAME" />
      <add key="ClearTextPassword" value="YOUR_GITHUB_TOKEN" />
    </github>
  </packageSourceCredentials>
</configuration>
```

Replace `YOUR_GITHUB_USERNAME` and `YOUR_GITHUB_TOKEN` with your values.

## Step 3: Install Packages

```bash
# For runtime markdown rendering only
dotnet add package MDFileToRazor.Components --source github

# For build-time code generation (recommended)
dotnet add package MDFileToRazor.Components --source github
dotnet add package MDFileToRazor.MSBuild --source github
```

## Step 4: Configure Services

Update your `Program.cs`:

```csharp
using MDFileToRazor.Components.Services;
using Microsoft.FluentUI.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddFluentUIComponents();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IStaticAssetService, StaticAssetService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
```

## Step 5: Add Imports

Update `_Imports.razor`:

```razor
@using MDFileToRazor.Components
@using MDFileToRazor.Components.Services
@using Microsoft.FluentUI.AspNetCore.Components
```

## Step 6: Include CSS/JS

Add to your `_Host.cshtml` or `index.html`:

```html
<head>
    <!-- FluentUI CSS -->
    <link href="https://cdn.jsdelivr.net/npm/@fluentui/web-components/dist/themes/fluent.css" rel="stylesheet" />
    
    <!-- Highlight.js -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/default.min.css">
    <script src="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/highlight.min.js"></script>
</head>
```

## Step 7: Create Markdown Content

### Option A: Runtime Rendering

Create a page:

```razor
@page "/example"

<h1>My Page</h1>

<MarkdownSection Content="@markdownContent" />

@code {
    private string markdownContent = @"
# Hello World

This is **bold** text.

```csharp
Console.WriteLine(""Hello, World!"");
```
";
}
```

### Option B: Build-Time Generation

1. Create folder: `MDFilesToConvert/`

2. Add a markdown file: `MDFilesToConvert/hello.md`

```markdown
---
title: Hello World
---

# Hello World

This page was generated from markdown!
```

3. Build your project:

```bash
dotnet build
```

4. Navigate to `/docs/hello`

## Step 8: Test

Run your application:

```bash
dotnet run
```

Visit your routes to see the markdown content rendered with FluentUI styling and syntax highlighting.

## Next Steps

- Read the complete [Usage Guide](USAGE.md) for advanced features
- Check the [Troubleshooting Guide](TROUBLESHOOTING.md) if you encounter issues
- Explore YAML frontmatter options for custom routing and layouts
- Customize styling and syntax highlighting themes

## Getting Help

- [Usage Guide](USAGE.md) - Complete documentation
- [Troubleshooting](TROUBLESHOOTING.md) - Common issues and solutions
- [GitHub Issues](https://github.com/DavidMarsh-NOAA/MDFileToRazor/issues) - Report bugs or request features
