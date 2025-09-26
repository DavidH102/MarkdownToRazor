---
title: Welcome to MarkdownToRazor
route: /welcome
layout: MainLayout
showTitle: true
description: Welcome page demonstrating build-time markdown to Razor page generation
tags: [welcome, demo, build-time]
---

# Welcome to Build-Time Generation Demo!

This page was **automatically generated** from a markdown file during the build process. 

## How It Works

1. **Markdown Source**: This content is written in `MDFilesToConvert/welcome.md`
2. **Build-Time Processing**: During `dotnet build`, the MarkdownToRazor MSBuild task converts this to a `.razor` file
3. **Generated Page**: A physical `Pages/Generated/Welcome.razor` file is created
4. **Routing**: The page is automatically routed to `/welcome` based on the YAML frontmatter

## YAML Frontmatter Features

The YAML header at the top of this markdown file controls:
- **Route**: Custom URL path (`/welcome`)  
- **Title**: Page title and HTML `<title>` tag
- **Layout**: Which Blazor layout to use (`MainLayout`)
- **Description**: SEO meta description
- **Tags**: Categories for organizing content

## Code Example

```csharp
// This markdown becomes a real Blazor component!
@page "/welcome"
@layout MainLayout

<PageTitle>Welcome to MarkdownToRazor</PageTitle>

<div class="markdown-content">
    <!-- Your markdown content rendered as HTML -->
</div>
```

## Benefits of Build-Time Generation

✅ **Performance**: Pages are pre-generated, no runtime markdown processing  
✅ **SEO-Friendly**: Static HTML content is crawlable  
✅ **Type Safety**: Generated pages are compiled Blazor components  
✅ **Routing**: Automatic route registration in Blazor  
✅ **IntelliSense**: Full IDE support for generated pages  

---

*This demo shows MarkdownToRazor's build-time code generation capabilities for Blazor Server applications.*