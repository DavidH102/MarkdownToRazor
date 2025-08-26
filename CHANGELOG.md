# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2025-08-26

### 🎯 MAJOR RELEASE - Package Consolidation & Modernization

This is a **BREAKING CHANGE** release that consolidates the entire library architecture into a single, modern package.

### Added

- **Single unified package**: `MarkdownToRazor` replaces the previous 3-package architecture
- **Multi-targeting support**: Now supports both .NET 8.0 and .NET 9.0
- **Modernized package name**: Renamed from `MDFileToRazor` to `MarkdownToRazor` for better consistency
- **Unified namespace**: All components now use `MarkdownToRazor.*` namespace hierarchy
- **Enhanced MSBuild integration**: Rebuilt MSBuild targets for seamless single-package experience
- **Comprehensive testing**: All 22 unit tests passing with new consolidated architecture

### Changed

- **BREAKING**: Package name changed from `MDFileToRazor` to `MarkdownToRazor`
- **BREAKING**: All namespaces changed from `MDFileToRazor.*` to `MarkdownToRazor.*`
- **BREAKING**: Service registration method renamed from `AddMdFileToRazorServices()` to `AddMarkdownToRazorServices()`
- **BREAKING**: Configuration options renamed from `MdFileToRazorOptions` to `MarkdownToRazorOptions`
- **Improved architecture**: Consolidated all functionality into single package for easier dependency management
- **Enhanced project structure**: Streamlined organization with unified source tree
- **Updated documentation**: All references updated to reflect new package name and namespaces

### Removed

- **BREAKING**: Removed separate packages: `MDFileToRazor.Components`, `MDFileToRazor.CodeGeneration`, `MDFileToRazor.MSBuild`
- **BREAKING**: Dropped .NET Standard 2.1 support (incompatible with Blazor components)
- Cleaned up old project files and unnecessary folders
- Removed obsolete scripts and configuration files
- Eliminated generated page examples from sample project

### Migration Guide

To upgrade from v1.x to v2.0:

1. **Update package reference**:

   ```xml
   <!-- Old -->
   <PackageReference Include="MDFileToRazor.Components" Version="1.x.x" />
   <PackageReference Include="MDFileToRazor.MSBuild" Version="1.x.x" />
   
   <!-- New -->
   <PackageReference Include="MarkdownToRazor" Version="2.0.0" />
   ```

2. **Update namespace imports**:

   ```csharp
   // Old
   using MDFileToRazor.Components;
   using MDFileToRazor.Services;
   
   // New
   using MarkdownToRazor.Components;
   using MarkdownToRazor.Services;
   ```

3. **Update service registration**:

   ```csharp
   // Old
   builder.Services.AddMdFileToRazorServices(options => { ... });
   
   // New
   builder.Services.AddMarkdownToRazorServices(options => { ... });
   ```

4. **Update configuration**:

   ```csharp
   // Old
   services.Configure<MdFileToRazorOptions>(options => { ... });
   
   // New
   services.Configure<MarkdownToRazorOptions>(options => { ... });
   ```

## [1.2.0] - 2025-08-26

### Added

- Enhanced path handling in `MdFileToRazorOptions` with support for absolute paths
- Support for relative paths with `../..` patterns for accessing parent directories
- Cross-platform path normalization using `Path.GetFullPath()`
- `DiscoverMarkdownFilesWithRoutesAsync()` method to `IMdFileDiscoveryService` for file-to-route mapping
- Comprehensive path handling tests covering various scenarios
- Flexible source directory configuration examples in documentation

### Changed

- Improved `GetAbsoluteSourcePath()` and `GetAbsoluteOutputPath()` methods to handle absolute paths correctly
- Enhanced service registration to support complex directory structures
- Updated documentation with comprehensive path configuration examples

### Removed

- Cleaned up unnecessary test files (`UnitTest1.cs`)
- Removed empty configuration files (`GitVersion_new.yml`)

### Fixed

- Path resolution issues with absolute paths in `MdFileToRazorOptions`
- Cross-platform compatibility for path handling
- Test failures related to root directory path resolution

## [1.1.1] - 2025-08-25

### Fixed

- GitVersion configuration for proper CI/CD versioning
- NuGet package deployment pipeline issues
- Build and test pipeline stability

## [1.1.0] - 2025-08-25

### Added

- `IGeneratedPageDiscoveryService` for programmatic page discovery
- `IMdFileDiscoveryService` for markdown file discovery capabilities
- Comprehensive service registration with `AddMdFileToRazorServices()`
- Support for tag-based page filtering and organization
- Route pattern matching for page discovery
- Dynamic navigation menu generation capabilities

### Changed

- Enhanced service dependency injection architecture
- Improved documentation with service discovery examples
- Updated NuGet package metadata and descriptions

## [1.0.0] - 2025-08-24

### Added

- Initial release of MDFileToRazor library
- `MarkdownSection` component for runtime markdown rendering
- Build-time code generation from markdown to Razor pages
- YAML frontmatter support for page configuration
- HTML comment configuration parsing
- Syntax highlighting with highlight.js integration
- Copy-to-clipboard functionality for code blocks
- FluentUI integration for consistent styling
- MSBuild integration for automatic code generation
- GitHub Packages and NuGet.org publishing support

### Features

- Automatic routing generation from markdown files
- Support for custom layouts and page metadata
- Recursive directory scanning for markdown files
- Configurable base route paths
- Static asset loading from filesystem and URLs
- Cross-platform compatibility (.NET 8.0+)
- Comprehensive error handling and validation
