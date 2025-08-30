# Script to delete GitHub packages for MarkdownToRazor project
# You'll need to run this with a token that has delete:packages scope

$packages = @(
    "MDFileToRazor.CodeGeneration",
    "MDFileToRazor.Components", 
    "MDFileToRazor.MSBuild",
    "MarkdownToRazor"
)

Write-Host "This script will delete all GitHub packages for the MarkdownToRazor project" -ForegroundColor Yellow
Write-Host "Make sure you have a GitHub token with 'delete:packages' scope" -ForegroundColor Yellow
Write-Host ""

$confirm = Read-Host "Are you sure you want to continue? (y/N)"
if ($confirm -ne "y" -and $confirm -ne "Y") {
    Write-Host "Operation cancelled." -ForegroundColor Green
    exit
}

foreach ($package in $packages) {
    Write-Host "Deleting package: $package" -ForegroundColor Cyan
    
    try {
        # Delete the entire package (all versions)
        gh api --method DELETE "user/packages/nuget/$package" | Out-Null
        Write-Host "✓ Successfully deleted $package" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ Failed to delete $package" -ForegroundColor Red
        Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Package cleanup completed!" -ForegroundColor Green
Write-Host "Remember to update your documentation to remove GitHub Packages references." -ForegroundColor Yellow
