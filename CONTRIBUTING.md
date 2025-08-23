# Contributing to MDFileToRazor

## Development Setup

### Prerequisites

- .NET 8 SDK or later
- Visual Studio 2022 or VS Code with C# extension

### Building the Solution

```bash
git clone <repository-url>
cd MDFileToRazor
dotnet restore
dotnet build
```

### Project Structure

- `src/MDFileToRazor.Components/` - Runtime Blazor components
- `src/MDFileToRazor.CodeGeneration/` - Build-time code generation
- `src/MDFileToRazor.MSBuild/` - MSBuild integration
- `samples/` - Example applications
- `tests/` - Unit and integration tests
- `docs/` - Documentation

### Testing

Run all tests:

```bash
dotnet test
```

Test the sample application:

```bash
cd samples/MDFileToRazor.Sample.BlazorServer
dotnet run
```

### Code Standards

- Follow .NET coding conventions
- Use file-scoped namespaces
- Add XML documentation for public APIs
- Write unit tests for new features
- Update documentation for changes

### Creating Packages

To create NuGet packages locally:

```bash
dotnet pack src/MDFileToRazor.Components -c Release -o nupkg
dotnet pack src/MDFileToRazor.CodeGeneration -c Release -o nupkg
dotnet pack src/MDFileToRazor.MSBuild -c Release -o nupkg
```

### Pull Request Process

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add/update tests
5. Update documentation
6. Submit a pull request

### Reporting Issues

Please use GitHub Issues to report bugs or request features. Include:

- .NET version
- Operating system
- Minimal reproduction case
- Expected vs. actual behavior
