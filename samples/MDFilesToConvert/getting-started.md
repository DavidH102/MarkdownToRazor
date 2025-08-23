# Getting Started Guide

This guide will help you get up and running with our **Markdown to Razor Code Generation** system.

## Prerequisites

Before you begin, make sure you have:

- .NET 8 SDK installed
- Visual Studio 2022 or VS Code
- Basic knowledge of Blazor and Markdown

## Quick Start

### Step 1: Create Your Markdown File

Create a new `.md` file in the `mdFilesToConvert` folder:

```markdown
# My New Page

This is my content!
```

### Step 2: Build the Project

Run the build command:

```bash
dotnet build
```

### Step 3: Access Your Generated Page

Your page will be available at `/my-new-page` (auto-generated from filename).

## Advanced Configuration

### Custom Routes

You can specify custom routes using frontmatter:

```yaml
---
route: /custom/path
title: My Custom Page
---
# Page Content

Your markdown content here...
```

### Page Templates

Generated pages follow this template structure:

```csharp
@page "/your-route"
@using MDFIleTORazor.Components

<PageTitle>Your Title</PageTitle>

<MarkdownSection FromAsset="your-file.md" />
```

## File Naming Conventions

| Markdown File        | Generated Route    | Page Title      |
| -------------------- | ------------------ | --------------- |
| `about.md`           | `/about`           | About           |
| `getting-started.md` | `/getting-started` | Getting Started |
| `api-docs.md`        | `/api-docs`        | Api Docs        |

## Supported Markdown Features

### Code Blocks with Highlighting

```python
def hello_world():
    print("Hello from generated page!")
    return "Success"

# This will be syntax highlighted automatically
hello_world()
```

### Tables and Lists

- ✅ Unordered lists
- ✅ Ordered lists
- ✅ Task lists
- ✅ Tables with sorting
- ✅ Blockquotes and callouts

### Interactive Elements

All code blocks include:

- **Copy to clipboard** functionality
- **Syntax highlighting** for 100+ languages
- **Line numbers** for better readability

## Troubleshooting

### Build Issues

If pages aren't generating:

1. Check that `.md` files are in `mdFilesToConvert/`
2. Verify the build target is running
3. Look for MSBuild errors in the output

### Runtime Issues

If pages return 404:

1. Check the generated route matches your URL
2. Verify the `.razor` file was created in `Pages/Generated/`
3. Restart the development server

## Next Steps

- Try creating your own markdown files
- Explore advanced frontmatter options
- Customize the page template
- Add your own markdown extensions

---

_This page was automatically generated from `getting-started.md`_
