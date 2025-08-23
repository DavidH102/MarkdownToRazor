# Troubleshooting Guide

This guide provides solutions to common issues when using MDFileToRazor packages.

## Authentication Issues

### Unable to load the service index

**Symptoms:**
- Error message: "Unable to load the service index for source 'https://nuget.pkg.github.com/DavidMarsh-NOAA/index.json'"
- Package restore fails

**Solutions:**

- Verify your GitHub token has `read:packages` scope
- Check that your username and token are correct in nuget.config
- Ensure the repository owner name is correct in the package source URL

### Package source 'github' was not found

**Symptoms:**
- Error during package installation
- NuGet cannot find the configured source

**Solutions:**

- Make sure you have a `nuget.config` file in your project root
- Verify the package source is configured correctly

## Package Not Found

### Package 'MDFileToRazor.Components' is not found

**Symptoms:**
- Package installation fails
- Error indicates package doesn't exist

**Solutions:**

- Check that the package has been published to GitHub Packages
- Verify you're using the correct package name and version
- Ensure you have access to the repository

## Build Issues

### Cannot find type 'MarkdownSection'

**Symptoms:**
- Compilation errors referencing MarkdownSection
- IntelliSense doesn't recognize components

**Solutions:**

- Verify `MDFileToRazor.Components` is installed
- Check that using statements are added to `_Imports.razor`
- Ensure services are registered in `Program.cs`

### Generated pages not found

**Symptoms:**
- Navigating to generated routes returns 404
- Generated pages don't appear in routing

**Solutions:**

- Check that markdown files are in the `MDFilesToConvert` directory
- Verify the `MarkdownSourceDirectory` property in your `.csproj`
- Run `dotnet build -t:GenerateMarkdownPages` manually

## Runtime Issues

### Styles not applied correctly

**Symptoms:**
- Markdown content appears unstyled
- FluentUI components don't render properly

**Solutions:**

- Ensure FluentUI CSS is included in your layout
- Check that `AddFluentUIComponents()` is called in `Program.cs`
- Verify highlight.js CSS and JavaScript are loaded

### Syntax highlighting not working

**Symptoms:**
- Code blocks appear without syntax highlighting
- JavaScript errors in browser console

**Solutions:**

- Confirm highlight.js scripts are loaded
- Check browser console for JavaScript errors
- Verify code blocks use proper language tags

## Common Configuration Mistakes

### Missing service registration

Add the following to `Program.cs`:

```csharp
builder.Services.AddFluentUIComponents();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IStaticAssetService, StaticAssetService>();
```

### Incorrect _Imports.razor

Ensure these using statements are present:

```razor
@using MDFileToRazor.Components
@using MDFileToRazor.Components.Services
@using Microsoft.FluentUI.AspNetCore.Components
```

### Missing CSS/JS references

Include in your layout file:

```html
<link href="https://cdn.jsdelivr.net/npm/@fluentui/web-components/dist/themes/fluent.css" rel="stylesheet" />
<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/default.min.css">
<script src="https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/highlight.min.js"></script>
```

## Getting Help

If you continue to experience issues:

1. Check the [GitHub Issues](https://github.com/DavidMarsh-NOAA/MDFileToRazor/issues) for similar problems
2. Review the complete [Usage Guide](USAGE.md)
3. Create a new issue with:
   - Your project configuration
   - Steps to reproduce the problem
   - Error messages and stack traces
   - Package versions being used
