# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
