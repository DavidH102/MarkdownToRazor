---
title: About This Demo
route: /about
layout: MainLayout
showTitle: true
description: Learn about the MarkdownToRazor build-time code generation demo
tags: [about, demo, build-time, blazor-server]
---

# About This Blazor Server Demo

This demo showcases **MarkdownToRazor's build-time code generation** capabilities for Blazor Server applications.

## What Makes This Different

Unlike the WASM demo (which shows runtime file discovery), this demo demonstrates:

### 🏗️ Build-Time Processing
- Markdown files are converted to `.razor` pages during `dotnet build`
- Generated pages become **physical files** in your project
- No runtime markdown processing overhead

### 🎯 MSBuild Integration
- Automatic integration via MSBuild targets
- Configurable source and output directories
- Incremental builds supported

### 📁 Project Structure
```
MarkdownToRazor.Sample.BlazorServer/
├── MDFilesToConvert/          ← Source markdown files
│   ├── welcome.md
│   ├── about.md
│   └── features.md
├── Pages/Generated/           ← Auto-generated Razor pages
│   ├── Welcome.razor
│   ├── About.razor
│   └── Features.razor
└── MarkdownToRazor.Sample.BlazorServer.csproj
```

## Configuration

The project is configured in the `.csproj` file:

```xml
<PropertyGroup>
  <MarkdownSourceDirectory>$(MSBuildProjectDirectory)/MDFilesToConvert</MarkdownSourceDirectory>
  <GeneratedPagesDirectory>$(MSBuildProjectDirectory)/Pages/Generated</GeneratedPagesDirectory>
</PropertyGroup>
```

## MSBuild Targets

Two key targets are available:

1. **GenerateMarkdownPages**: Converts markdown to Razor (runs before compile)
2. **CleanGeneratedPages**: Removes generated files (runs during clean)

## Use Cases

Perfect for:
- **Documentation sites** with static content
- **Blogs** with markdown posts  
- **Product pages** authored in markdown
- **Knowledge bases** with technical content
- **Company websites** with mixed content types

---

*Experience the power of build-time markdown processing with MarkdownToRazor!*