# PowerShell script to create and push a release tag
param(
    [Parameter(Mandatory=$true)]
    [string]$Version,
    [string]$Message = "Release $Version"
)

# Validate version format
if ($Version -notmatch '^v\d+\.\d+\.\d+(-.*)?$') {
    Write-Error "Version must be in format 'v1.0.0' or 'v1.0.0-beta.1'"
    exit 1
}

Write-Host "🏷️  Creating release tag: $Version" -ForegroundColor Green
Write-Host "📝 Message: $Message" -ForegroundColor Cyan

# Ensure we're on main branch
$currentBranch = git branch --show-current
if ($currentBranch -ne "main") {
    Write-Warning "Current branch is '$currentBranch'. It's recommended to create releases from 'main'."
    $continue = Read-Host "Continue anyway? (y/N)"
    if ($continue.ToLower() -ne "y") {
        Write-Host "Aborted." -ForegroundColor Yellow
        exit 0
    }
}

# Check for uncommitted changes
$status = git status --porcelain
if ($status) {
    Write-Error "You have uncommitted changes. Please commit or stash them first."
    git status
    exit 1
}

# Pull latest changes
Write-Host "📥 Pulling latest changes..." -ForegroundColor Blue
git pull origin main

# Create and push tag
try {
    Write-Host "🏷️  Creating tag..." -ForegroundColor Blue
    git tag -a $Version -m $Message
    
    Write-Host "🚀 Pushing tag to origin..." -ForegroundColor Blue
    git push origin $Version
    
    Write-Host "✅ Success! Tag $Version created and pushed." -ForegroundColor Green
    Write-Host ""
    Write-Host "🔍 Monitor the workflow at:" -ForegroundColor Cyan
    Write-Host "   https://github.com/DavidH102/MDFileToRazor/actions"
    Write-Host ""
    Write-Host "📦 Your packages will be published to:" -ForegroundColor Cyan
    Write-Host "   - NuGet.org: https://www.nuget.org/"
    Write-Host "   - GitHub Packages: https://github.com/DavidH102/MDFileToRazor/packages"
}
catch {
    Write-Error "Failed to create or push tag: $_"
    exit 1
}
