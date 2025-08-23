# Sample Markdown File

This markdown file is loaded from the wwwroot folder.

## Features Demonstrated

### Syntax Highlighting

```javascript
function greetUser(name) {
  console.log(`Hello, ${name}!`);
  return `Welcome to our application, ${name}!`;
}

greetUser("World");
```

### Python Example

```python
def fibonacci(n):
    if n <= 1:
        return n
    return fibonacci(n-1) + fibonacci(n-2)

# Generate first 10 Fibonacci numbers
for i in range(10):
    print(f"F({i}) = {fibonacci(i)}")
```

### Tables

| Feature             | Status      | Description                   |
| ------------------- | ----------- | ----------------------------- |
| Markdown Parsing    | ✅ Complete | Uses Markdig library          |
| Syntax Highlighting | ✅ Complete | Uses highlight.js             |
| Copy Functionality  | ✅ Complete | JavaScript clipboard API      |
| FluentUI Styling    | ✅ Complete | Integrated with design system |

### Mathematical Expressions

If math extensions are enabled:

- Inline math: $E = mc^2$
- Block math: $$\sum_{i=1}^{n} i = \frac{n(n+1)}{2}$$

### Task Lists

- [x] Implement MarkdownSection component
- [x] Add syntax highlighting
- [x] Add copy functionality
- [ ] Add math support (optional)
- [ ] Add mermaid diagram support (optional)

> **Note**: This component provides a solid foundation for rendering markdown content in Blazor applications with FluentUI styling.
