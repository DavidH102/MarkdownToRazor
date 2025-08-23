# HTML Comment Configuration

Starting with version 1.2.0, MDFileToRazor supports configuration through HTML comments as an alternative to YAML frontmatter. This provides more flexibility for markdown authoring and can be useful when you want to avoid YAML syntax or maintain compatibility with other markdown processors.

## Syntax

To use HTML comment configuration, place your configuration at the very beginning of your markdown file using this format:

```markdown
<!-- This is configuration data -->
<!-- @page "/your-route" -->
<!-- title: Your Page Title -->
<!-- layout: YourLayout -->
<!-- showTitle: true -->
<!-- description: Your page description for SEO -->
<!-- tags: tag1, tag2, tag3 -->

# Your Markdown Content

Your regular markdown content goes here...
```

## Configuration Options

All the same configuration options available in YAML frontmatter are supported in HTML comments:

### Required Configuration Marker

The first line must be the configuration marker:

```html
<!-- This is configuration data -->
```

This tells the parser that the following HTML comments contain configuration data.

### @page Directive

Specify the route for your page:

```html
<!-- @page "/example" -->
<!-- @page "/products/{id:int}" -->
<!-- @page "/blog/{slug}" -->
```

### Page Properties

Configure page metadata:

```html
<!-- title: My Page Title -->
<!-- layout: MyCustomLayout -->
<!-- showTitle: true -->
<!-- description: This is a description for SEO purposes -->
<!-- tags: blazor, markdown, documentation -->
```

## Precedence Rules

When both YAML frontmatter and HTML comment configuration are present in the same file:

1. **HTML comment configuration takes precedence** over YAML frontmatter
2. Individual properties from HTML comments override the same properties in YAML
3. Properties only specified in one format are still included

Example of mixed configuration:

```markdown
---
title: YAML Title
description: YAML Description
layout: YamlLayout
---

<!-- This is configuration data -->
<!-- @page "/html-route" -->
<!-- title: HTML Title -->

# Content
```

Result:

- Route: `/html-route` (from HTML comment)
- Title: `HTML Title` (from HTML comment)
- Description: `YAML Description` (from YAML, not overridden)
- Layout: `YamlLayout` (from YAML, not overridden)

## Benefits

### Compatibility

- Works with any markdown processor that ignores HTML comments
- Doesn't interfere with other markdown tools or editors
- Can be used alongside existing YAML frontmatter

### Flexibility

- No need to learn YAML syntax
- More familiar HTML comment style
- Can be mixed with YAML frontmatter for gradual migration

### Maintenance

- Easy to spot configuration in markdown files
- Comments are naturally ignored by most markdown renderers
- Configuration is clearly separated from content

## Migration from YAML

To migrate existing pages from YAML frontmatter to HTML comments:

### Before (YAML):
```markdown
---
route: /example
title: Example Page
layout: MainLayout
showTitle: true
description: Example description
tags: [example, demo]
---

# Example Page
Content here...
```

### After (HTML Comments):
```markdown
<!-- This is configuration data -->
<!-- @page "/example" -->
<!-- title: Example Page -->
<!-- layout: MainLayout -->
<!-- showTitle: true -->
<!-- description: Example description -->
<!-- tags: example, demo -->

# Example Page
Content here...
```

## Examples

### Simple Page
```markdown
<!-- This is configuration data -->
<!-- @page "/about" -->
<!-- title: About Us -->

# About Us
Welcome to our company...
```

### Complex Page with Parameters
```markdown
<!-- This is configuration data -->
<!-- @page "/products/{category}/{id:int}" -->
<!-- title: Product Details -->
<!-- layout: ProductLayout -->
<!-- description: View detailed information about our products -->
<!-- tags: products, catalog, shopping -->

# Product Details
[Content would be here...]
```

### Blog Post
```markdown
<!-- This is configuration data -->
<!-- @page "/blog/2024/new-features" -->
<!-- title: New Features in Version 2.0 -->
<!-- layout: BlogLayout -->
<!-- showTitle: true -->
<!-- description: Learn about the exciting new features in our latest release -->
<!-- tags: blog, features, release -->

# New Features in Version 2.0
Today we're excited to announce...
```

## Technical Notes

- HTML comments must be at the very beginning of the file (before any other content)
- The configuration marker `<!-- This is configuration data -->` must be the first comment
- Each configuration property should be on its own line within HTML comments
- Properties use the same names and syntax as YAML frontmatter
- Tag lists can be comma-separated: `<!-- tags: tag1, tag2, tag3 -->`
- Boolean values: `<!-- showTitle: true -->` or `<!-- showTitle: false -->`

## Troubleshooting

### Configuration Not Being Parsed
- Ensure `<!-- This is configuration data -->` is the very first line
- Check that there's no whitespace or other content before the configuration marker
- Verify that each configuration property is in its own HTML comment

### Route Not Working
- Make sure the `@page` directive includes the forward slash: `<!-- @page "/route" -->`
- Check for valid route syntax according to ASP.NET Core routing rules
- Ensure route parameters use correct syntax: `{parameter}` or `{parameter:type}`

### Mixed Configuration Issues
- HTML comment configuration overrides YAML frontmatter properties
- Both configurations are merged, with HTML comments taking precedence
- If you have both, ensure they don't conflict in unexpected ways
