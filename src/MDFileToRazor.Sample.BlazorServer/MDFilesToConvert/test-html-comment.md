<!-- This is configuration data -->
<!-- @page "/example" -->
<!-- title: Example Page with HTML Comments -->
<!-- description: This page demonstrates HTML comment configuration -->

# Example Page

This markdown file uses HTML comment configuration instead of YAML frontmatter.

The configuration is specified in HTML comments at the top of the file:

- The route is set to `/example` using `@page "/example"`
- The title is set using `title: Example Page with HTML Comments`
- The description is set for SEO purposes

## Features

This approach is useful when:

- You want to avoid YAML frontmatter syntax
- You prefer HTML comment style configuration
- You need to maintain compatibility with other markdown processors

## Code Example

```csharp
public class ExampleService
{
    public string GetMessage()
    {
        return "Hello from HTML comment configured page!";
    }
}
```

The HTML comment configuration takes precedence over YAML frontmatter if both are present.
