# MarkdownToRazor Features

Discover the powerful features that make MarkdownToRazor the perfect choice for your Blazor applications.

## 🚀 Core Features

### Runtime Markdown Rendering

Transform your Blazor applications with dynamic markdown rendering:

- **Zero Build Dependencies**: No code generation required
- **Hot Reload Support**: Changes reflect immediately during development
- **Dynamic Content**: Perfect for CMS scenarios and user-generated content
- **Azure Static Web Apps Ready**: Optimized for static hosting environments

### Advanced Markdown Support

Built on the robust Markdig library with extensive extensions:

```markdown
# Headers with auto-anchors

## Subheadings work perfectly

**Bold text** and _italic text_ styling

- Bullet lists
- [x] Task lists with checkboxes
- [ ] Incomplete tasks

| Feature | Supported | Notes                |
| ------- | --------- | -------------------- |
| Tables  | ✅        | Full styling support |
| Code    | ✅        | Syntax highlighting  |
| Images  | ✅        | Responsive images    |
```

### Syntax Highlighting

Beautiful code syntax highlighting powered by highlight.js:

```csharp
public class ExampleService
{
    public async Task<string> GetDataAsync()
    {
        // Beautiful C# syntax highlighting
        return await httpClient.GetStringAsync("/api/data");
    }
}
```

```javascript
// JavaScript highlighting
function greetUser(name) {
  console.log(`Hello, ${name}!`);
  return `Welcome to MarkdownToRazor!`;
}
```

```html
<!-- HTML highlighting -->
<div class="example">
  <h1>MarkdownToRazor</h1>
  <p>Amazing markdown rendering!</p>
</div>
```

## 🎨 Styling & Customization

### Responsive Design

MarkdownToRazor content is fully responsive out of the box:

- **Mobile-First**: Optimized for mobile devices
- **Flexible Layouts**: Adapts to any container size
- **Bootstrap Compatible**: Works seamlessly with Bootstrap
- **Custom CSS**: Easy to override with your own styles

### YAML Frontmatter

Rich metadata support for advanced scenarios:

```yaml
---
title: "Custom Page Title"
description: "SEO-friendly description"
route: "/custom-url-path"
layout: "SpecialLayout"
showTitle: false
tags: ["blazor", "markdown", "web"]
author: "John Doe"
publishDate: "2024-01-15"
---
```

### Copy-to-Clipboard

Every code block includes a convenient copy button:

- **One-Click Copying**: Users can easily copy code examples
- **Visual Feedback**: Success indicators for better UX
- **Accessibility**: Keyboard navigation support
- **Customizable**: Style the copy button to match your theme

## 🔧 Developer Experience

### Simple Integration

Add MarkdownToRazor to any Blazor project in minutes:

```csharp
// Program.cs - One line to get started
builder.Services.AddMarkdownToRazor();

// Use anywhere in your components
<MarkdownSection FilePath="content/my-file.md" />
```

### File System Flexibility

Support for multiple content sources:

- **Local Files**: Files from your project directory
- **HTTP Resources**: Remote markdown files via HTTP
- **Embedded Resources**: Markdown files embedded in assemblies
- **Custom Providers**: Implement your own content sources

### Error Handling

Robust error handling with graceful degradation:

- **Custom Error Templates**: Define how errors are displayed
- **Fallback Content**: Show alternative content when files are missing
- **Development Debugging**: Detailed error information during development
- **Production Safety**: Clean error messages for end users

## 🌐 Deployment Features

### Azure Static Web Apps Optimized

Perfect for modern static hosting:

- **No Server Dependencies**: Pure client-side rendering
- **Fast Loading**: Optimized for static content delivery
- **CDN Compatible**: Works great with content delivery networks
- **Offline Support**: Service worker compatibility

### Build Integration

Optional build-time features for advanced scenarios:

- **MSBuild Targets**: Integrate with your build process
- **Code Generation**: Convert markdown to compiled Razor pages
- **Asset Optimization**: Optimize images and resources
- **Cache Busting**: Automatic cache invalidation

## 📊 Performance Features

### Optimized Rendering

Built for performance from the ground up:

- **Efficient Parsing**: Fast markdown processing with Markdig
- **Smart Caching**: Automatic content caching where possible
- **Minimal Re-renders**: Optimized component lifecycle
- **Bundle Size**: Lightweight library with minimal dependencies

### Memory Management

Careful attention to memory usage:

- **Disposal Patterns**: Proper cleanup of resources
- **Event Unsubscription**: Prevents memory leaks
- **Efficient DOM Updates**: Minimal DOM manipulation
- **Garbage Collection**: GC-friendly patterns

## 🔍 Accessibility Features

### WCAG Compliance

Built with accessibility in mind:

- **Semantic HTML**: Proper heading hierarchy and structure
- **Keyboard Navigation**: Full keyboard accessibility
- **Screen Reader Support**: ARIA labels and descriptions
- **Color Contrast**: High contrast for readability

### User Experience

Enhanced UX for all users:

- **Focus Management**: Proper focus handling
- **Skip Links**: Navigation shortcuts
- **Alternative Text**: Image accessibility
- **Error Announcements**: Screen reader error notifications

## 🛡️ Security Features

### Content Sanitization

Safe rendering of user content:

- **XSS Protection**: Automatic script sanitization
- **HTML Filtering**: Safe HTML subset support
- **Link Validation**: Optional link checking
- **Content Security Policy**: CSP header compatibility

### Input Validation

Robust input handling:

- **Path Validation**: Secure file path handling
- **Content Limits**: Configurable size limits
- **Type Checking**: Strict file type validation
- **Encoding Support**: Proper character encoding

## 📈 Analytics & Monitoring

### Usage Tracking

Optional analytics integration:

- **Page Views**: Track markdown page visits
- **Performance Metrics**: Monitor rendering performance
- **Error Tracking**: Capture and report errors
- **User Interactions**: Track copy events and navigation

### Debugging Tools

Development and troubleshooting features:

- **Verbose Logging**: Detailed operation logs
- **Performance Profiling**: Identify bottlenecks
- **Content Validation**: Verify markdown syntax
- **Link Checking**: Validate internal and external links

Ready to get started? Check out our [getting started](getting-started) guide!
