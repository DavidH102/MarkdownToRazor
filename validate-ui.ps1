#!/usr/bin/env pwsh

# Validation script to check MarkdownToRazor UI components and sample pages
# Ensures proper FilePath usage and prevents regression

Write-Host "=== MarkdownToRazor UI Validation Script ===" -ForegroundColor Green
Write-Host ""

$ErrorCount = 0
$WarningCount = 0

# Function to check file content for patterns
function Test-FileContent {
    param(
        [string]$FilePath,
        [string[]]$RequiredPatterns,
        [string[]]$ProhibitedPatterns,
        [string]$Description
    )
    
    Write-Host "Checking: $Description" -ForegroundColor Cyan
    
    if (-not (Test-Path $FilePath)) {
        Write-Host "  ❌ File not found: $FilePath" -ForegroundColor Red
        return $false
    }
    
    $content = Get-Content $FilePath -Raw
    $success = $true
    
    # Check required patterns
    foreach ($pattern in $RequiredPatterns) {
        if ($content -match [regex]::Escape($pattern)) {
            Write-Host "  ✅ Found required: $pattern" -ForegroundColor Green
        } else {
            Write-Host "  ❌ Missing required: $pattern" -ForegroundColor Red
            $script:ErrorCount++
            $success = $false
        }
    }
    
    # Check prohibited patterns
    foreach ($pattern in $ProhibitedPatterns) {
        if ($content -match [regex]::Escape($pattern)) {
            Write-Host "  ❌ Found prohibited: $pattern" -ForegroundColor Red
            $script:ErrorCount++
            $success = $false
        } else {
            Write-Host "  ✅ Correctly avoided: $pattern" -ForegroundColor Green
        }
    }
    
    return $success
}

Write-Host "1. Validating Sample Page FilePath Usage" -ForegroundColor Yellow
Write-Host "===========================================" -ForegroundColor Yellow

# Test Features.razor
Test-FileContent -FilePath "src/MarkdownToRazor.Sample.BlazorWasm/Pages/Features.razor" `
    -RequiredPatterns @('FilePath="content/features.md"') `
    -ProhibitedPatterns @('FilePath="features.md"', 'FilePath="../', 'FilePath="./') `
    -Description "Features.razor - Correct content path usage"

# Test GettingStarted.razor
Test-FileContent -FilePath "src/MarkdownToRazor.Sample.BlazorWasm/Pages/GettingStarted.razor" `
    -RequiredPatterns @('FilePath="content/getting-started.md"') `
    -ProhibitedPatterns @('FilePath="getting-started.md"', 'FilePath="../', 'FilePath="./') `
    -Description "GettingStarted.razor - Correct content path usage"

# Test Documentation.razor
Test-FileContent -FilePath "src/MarkdownToRazor.Sample.BlazorWasm/Pages/Documentation.razor" `
    -RequiredPatterns @('FilePath="content/documentation.md"') `
    -ProhibitedPatterns @('FilePath="documentation.md"', 'FilePath="../', 'FilePath="./') `
    -Description "Documentation.razor - Correct content path usage"

Write-Host ""
Write-Host "2. Validating Component Implementation" -ForegroundColor Yellow
Write-Host "======================================" -ForegroundColor Yellow

# Test MarkdownSection component supports FilePath parameter
Test-FileContent -FilePath "src/MarkdownToRazor/Components/MarkdownSection.razor.cs" `
    -RequiredPatterns @('[Parameter] public string? FilePath') `
    -ProhibitedPatterns @() `
    -Description "MarkdownSection.razor.cs - FilePath parameter support"

Write-Host ""
Write-Host "3. Validating Test Coverage" -ForegroundColor Yellow
Write-Host "===========================" -ForegroundColor Yellow

# Test that UI validation tests exist
Test-FileContent -FilePath "tests/MarkdownToRazor.Tests/Components/MarkdownSectionUiTests.cs" `
    -RequiredPatterns @('MarkdownSection_WithValidFilePath_RendersCorrectly', 'MarkdownSection_WithInvalidRelativePath_ShowsError') `
    -ProhibitedPatterns @() `
    -Description "MarkdownSectionUiTests.cs - UI component validation tests"

Test-FileContent -FilePath "tests/MarkdownToRazor.Tests/Integration/SamplePageValidationTests.cs" `
    -RequiredPatterns @('NoSamplePages_UseRelativeFilePaths', 'SamplePages_UseCorrectContentPaths') `
    -ProhibitedPatterns @() `
    -Description "SamplePageValidationTests.cs - Sample page validation tests"

Write-Host ""
Write-Host "4. Validating WASM Performance Optimizations" -ForegroundColor Yellow
Write-Host "=============================================" -ForegroundColor Yellow

# Check WASM service implementations
Test-FileContent -FilePath "src/MarkdownToRazor/Services/StaticAssetService.cs" `
    -RequiredPatterns @('WasmStaticAssetService', 'ConcurrentDictionary') `
    -ProhibitedPatterns @() `
    -Description "StaticAssetService.cs - WASM optimizations"

Test-FileContent -FilePath "src/MarkdownToRazor/Services/MdFileDiscoveryService.cs" `
    -RequiredPatterns @('WasmFileDiscoveryService', 'Parallel') `
    -ProhibitedPatterns @() `
    -Description "MdFileDiscoveryService.cs - WASM parallel discovery"

Write-Host ""
Write-Host "=== Validation Summary ===" -ForegroundColor Green

if ($ErrorCount -eq 0) {
    Write-Host "✅ All validations passed! UI components are properly configured." -ForegroundColor Green
    Write-Host ""
    Write-Host "Key validations completed:" -ForegroundColor White
    Write-Host "  • Sample pages use correct 'content/' prefixed paths" -ForegroundColor White
    Write-Host "  • No relative FilePath patterns found" -ForegroundColor White
    Write-Host "  • MarkdownSection component supports FilePath parameter" -ForegroundColor White
    Write-Host "  • UI validation tests are in place" -ForegroundColor White
    Write-Host "  • WASM performance optimizations confirmed" -ForegroundColor White
    Write-Host ""
    Write-Host "The MarkdownToRazor library is ready for WASM deployment with proper UI validation!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "❌ Found $ErrorCount error(s) that need attention." -ForegroundColor Red
    Write-Host ""
    Write-Host "Please review the output above and fix any issues before deployment." -ForegroundColor Yellow
    exit 1
}
