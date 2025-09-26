# CI/CD Pipeline Setup

This document explains the complete CI/CD pipeline setup for MarkdownToRazor using GitHub Actions for automated building, testing, and publishing to NuGet.org.

## 🚀 Quick Release Process

**To release a new version:**

```bash
# 1. Make your changes and commit with semantic versioning
git commit -m "fix: resolve critical issue +semver: patch"

# 2. Push to main
git push

# 3. Create and push a version tag to trigger NuGet publishing
git tag v2.1.1  # Use the version GitVersion calculated
git push origin v2.1.1
```

**That's it!** The CI/CD pipeline will automatically build, test, and publish to NuGet.org.

## Overview

The project uses GitHub Actions for automated building, testing, and publishing of NuGet packages to **NuGet.org**. The pipeline includes:

- **Automated versioning** using GitVersion
- **Build and test** on every push/PR
- **Package generation** for the MarkdownToRazor package
- **Publishing to NuGet.org** ONLY when version tags are pushed
- **GitHub releases** with automatic release notes

## Pipeline Components

### GitHub Actions Workflow

**File:** `.github/workflows/ci-cd-release.yml`

The workflow is triggered by:

- Pushes to `main` and `develop` branches (build & test only)
- Git tags starting with `v*.*.*` (build, test & publish to NuGet)
- Pull requests (build and test only)

**Build & Test Job (runs on all pushes/PRs):**

1. Sets up .NET 8.0 and 9.0
2. Installs GitVersion tool
3. Determines version from Git history
4. Restores dependencies
5. Builds all projects
6. Runs tests
7. Creates NuGet packages
8. Uploads packages as artifacts

**Publish Job (only runs on version tags):**

1. Downloads build artifacts
2. Publishes packages to **NuGet.org** (not GitHub Packages)
3. Creates GitHub release with automated release notes
4. Requires `NUGET_API_KEY` secret for NuGet.org authentication

### Semantic Versioning

**File:** `GitVersion.yml`

Configures automatic version generation based on Git branches:

- **Main branch**: `1.0.x` (stable releases)
- **Develop branch**: `1.0.x-alpha.y` (pre-release)
- **Feature branches**: `1.0.x-feature-name.y` (development)
- **Release branches**: `1.0.x-beta.y` (release candidates)

### Centralized Configuration

**File:** `Directory.Build.props`

Provides common settings for all projects:

- Package metadata (author, description, license)
- Repository information
- Build configuration
- SourceLink integration for debugging
- Common dependencies

### Package Source Configuration

**File:** `NuGet.config`

Configures package sources for both consuming and publishing:

- Standard nuget.org for public packages
- GitHub Packages for private distribution
- Authentication setup for GitHub token

## Setting Up the Pipeline

### 1. Create GitHub Repository

```bash
# Create repository on GitHub: DavidMarsh-NOAA/MarkdownToRazor
# Then clone locally or add remote to existing repo

git remote add origin https://github.com/DavidMarsh-NOAA/MarkdownToRazor.git
git branch -M main
git push -u origin main
```

### 2. Configure Repository Secrets

In GitHub repository settings → Secrets and variables → Actions, add:

- **`GITHUB_TOKEN`** - Automatically provided by GitHub (no action needed)

The workflow uses the built-in `GITHUB_TOKEN` which has the necessary permissions for GitHub Packages.

### 3. Enable GitHub Packages

1. Go to repository Settings → Actions → General
2. Under "Workflow permissions", select "Read and write permissions"
3. Save changes

### 4. Test the Pipeline

Push code to trigger the workflow:

```bash
git add .
git commit -m "Initial setup with CI/CD pipeline"
git push origin main
```

The workflow will:

1. Build all projects
2. Run tests
3. Generate version 1.0.0
4. Create and publish NuGet packages

### 5. Creating Releases

**For stable releases:**

```bash
git tag v1.0.0
git push origin v1.0.0
```

**For pre-releases:**

```bash
git tag v1.0.0-beta.1
git push origin v1.0.0-beta.1
```

**For feature development:**

```bash
git checkout -b feature/new-feature
# Make changes
git push origin feature/new-feature
```

This generates packages like `1.0.0-new-feature.1`.

## Package Versioning Strategy

### Branch-Based Versioning

| Branch Type | Version Pattern                                           | Example               | Use Case            |
| ----------- | --------------------------------------------------------- | --------------------- | ------------------- |
| `main`      | `{Major}.{Minor}.{Patch}`                                 | `1.0.0`               | Stable releases     |
| `develop`   | `{Major}.{Minor}.{Patch}-alpha.{PreReleaseNumber}`        | `1.0.1-alpha.1`       | Integration testing |
| `feature/*` | `{Major}.{Minor}.{Patch}-{BranchName}.{PreReleaseNumber}` | `1.0.1-new-feature.1` | Feature development |
| `release/*` | `{Major}.{Minor}.{Patch}-beta.{PreReleaseNumber}`         | `1.0.0-beta.1`        | Release candidates  |

### Version Increments

- **Patch**: Bug fixes, minor updates
- **Minor**: New features, backward compatible
- **Major**: Breaking changes

GitVersion automatically increments versions based on:

- Commit messages (using conventional commits)
- Branch merges
- Manual version bumps in configuration

## Security Considerations

### Token Permissions

The `GITHUB_TOKEN` used in the workflow has:

- Read access to repository content
- Write access to GitHub Packages
- No access to repository settings or other sensitive data

### Package Visibility

GitHub Packages published from this repository are:

- Private by default (only accessible to repository collaborators)
- Requires authentication to download
- Can be made public if needed

### Dependency Management

- All dependencies are pinned to specific versions
- Security scanning enabled through GitHub (Dependabot)
- Regular updates recommended for security patches

## Monitoring and Maintenance

### Workflow Status

Monitor pipeline health:

1. Check GitHub Actions tab in repository
2. Review build logs for warnings or errors
3. Monitor package publication success

### Package Management

View published packages:

1. Go to repository page
2. Click "Packages" tab on the right sidebar
3. Review package versions and download statistics

### Troubleshooting

Common issues and solutions:

**Build failures:**

- Check .NET version compatibility
- Verify all dependencies are available
- Review compiler warnings and errors

**Publishing failures:**

- Verify token permissions
- Check package naming conventions
- Ensure repository URL is correct

**Version conflicts:**

- Review GitVersion configuration
- Check for conflicting tags
- Verify branch naming conventions

## Manual Operations

### Local Package Generation

Generate packages locally for testing:

```bash
# Clean previous builds
dotnet clean

# Restore dependencies
dotnet restore

# Build all projects
dotnet build --configuration Release

# Create packages
dotnet pack --configuration Release --output ./nupkg
```

### Manual Publishing

Publish packages manually (for testing):

```bash
# Set GitHub token
$env:GITHUB_TOKEN = "your_github_token"

# Push packages
dotnet nuget push "./nupkg/*.nupkg" --source "github" --api-key $env:GITHUB_TOKEN
```

### Local Testing

Test packages locally before publishing:

```bash
# Create local package source
dotnet nuget add source "./nupkg" --name "local"

# Install from local source
dotnet add package MarkdownToRazor.Components --source "local"
```

## Best Practices

### Commit Messages

Use conventional commit format for automatic version management:

```bash
git commit -m "feat: add new markdown renderer feature"     # Minor version bump
git commit -m "fix: resolve syntax highlighting issue"      # Patch version bump
git commit -m "feat!: redesign component API"               # Major version bump
git commit -m "docs: update installation instructions"      # No version bump
```

### Branch Management

- Keep `main` branch stable and deployable
- Use `develop` for integration of new features
- Create feature branches for individual changes
- Use pull requests for code review

### Testing Strategy

- Run tests locally before pushing
- Use pull requests to trigger CI validation
- Test package installation in separate projects
- Validate documentation accuracy

### Release Process

1. **Development**: Work in feature branches
2. **Integration**: Merge to `develop` branch
3. **Testing**: Validate alpha packages
4. **Release**: Merge to `main` for stable release
5. **Tagging**: Create release tags for versioning
6. **Documentation**: Update changelog and docs

This CI/CD setup provides a robust foundation for private NuGet package distribution with automated versioning and comprehensive testing.
