# 🚀 Setting Up NuGet.org Publishing

This guide walks you through setting up automated publishing to both NuGet.org and GitHub Packages.

## ✅ **Prerequisites**

1. **NuGet.org Account**: Create account at [nuget.org](https://www.nuget.org)
2. **GitHub Repository**: Your repository on GitHub (already done! ✅)
3. **Repository Access**: Admin access to configure secrets

## 🔑 **Step 1: Create NuGet.org API Key**

1. Sign in to [nuget.org](https://www.nuget.org)
2. Click your username → **"API Keys"**
3. Click **"Create"** and configure:

   ```
   Key Name: MDFileToRazor-CI
   Select Scopes: ✅ Push new packages and package versions
   Select Packages: * (all packages)
   Glob Pattern: MDFileToRazor.*
   ```

4. **Copy the API key** (you'll only see it once!)

## 🔒 **Step 2: Add GitHub Secret**

1. Go to your GitHub repo → **Settings** → **Secrets and variables** → **Actions**
2. Click **"New repository secret"**
3. Configure:
   ```
   Name: NUGET_API_KEY
   Secret: [paste your NuGet API key from step 1]
   ```

## 🏷️ **Step 3: Create a Release Tag**

Your CI/CD pipeline publishes to NuGet.org only when you create a version tag. Here's how:

### Option A: Using Git Command Line

```bash
# Create and push a version tag
git tag v1.0.0
git push origin v1.0.0
```

### Option B: Using GitHub Web Interface

1. Go to your repo → **Releases** → **Create a new release**
2. **Tag version**: `v1.0.0`
3. **Release title**: `v1.0.0 - Initial Release`
4. **Description**: Add your release notes
5. Click **"Publish release"**

## 🔄 **What Happens When You Create a Tag:**

1. **GitHub Actions triggers** automatically
2. **Builds all packages** with the version from your tag
3. **Runs tests** to ensure quality
4. **Publishes to GitHub Packages** (always)
5. **Publishes to NuGet.org** (only for tags starting with `v`)
6. **Creates GitHub Release** with download links

## 📦 **Publishing Behavior:**

| Trigger             | GitHub Packages | NuGet.org    | GitHub Release |
| ------------------- | --------------- | ------------ | -------------- |
| Push to `main`      | ✅ Published    | ❌ No        | ❌ No          |
| Create tag `v*.*.*` | ✅ Published    | ✅ Published | ✅ Created     |
| Pull Request        | ❌ Build only   | ❌ No        | ❌ No          |

## 🧪 **Test the Pipeline:**

1. **Create a test tag:**

   ```bash
   git tag v0.1.0-test
   git push origin v0.1.0-test
   ```

2. **Watch the workflow:**

   - Go to **Actions** tab in your GitHub repo
   - Monitor the "Build and Publish NuGet Packages" workflow
   - Check for any errors

3. **Verify publishing:**
   - Check [nuget.org](https://www.nuget.org/profiles/YourUsername) for your packages
   - Check GitHub Packages in your repo

## 🚨 **Troubleshooting:**

### ❌ **"401 Unauthorized" error:**

- Your `NUGET_API_KEY` secret is incorrect or expired
- Create a new API key and update the secret

### ❌ **"Package already exists" error:**

- NuGet.org doesn't allow overwriting existing versions
- Increment your version number in the tag (e.g., `v1.0.1`)

### ❌ **Workflow doesn't trigger:**

- Ensure your tag starts with `v` (e.g., `v1.0.0`)
- Check the workflow file permissions

## 🎯 **Version Strategy:**

We use [GitVersion](https://gitversion.net/) for automatic versioning:

- **`v1.0.0`** - Major release
- **`v1.1.0`** - Minor release (new features)
- **`v1.0.1`** - Patch release (bug fixes)
- **`v1.0.0-beta.1`** - Pre-release

## 📋 **Checklist Before First Publish:**

- [ ] NuGet.org account created
- [ ] API key created and copied
- [ ] GitHub secret `NUGET_API_KEY` added
- [ ] All tests passing locally (`dotnet test`)
- [ ] Package metadata reviewed in `.csproj` files
- [ ] README.md updated with installation instructions
- [ ] Ready to create your first tag (`v1.0.0`)

## 🎉 **You're Ready!**

Once you complete the checklist above, create your first release tag and watch your packages get published automatically to both NuGet.org and GitHub Packages!

```bash
# Create your first official release
git tag v1.0.0
git push origin v1.0.0
```

Your packages will be available within minutes at:

- **NuGet.org**: https://www.nuget.org/packages/MDFileToRazor.Components
- **GitHub Packages**: In your repository's Packages section
