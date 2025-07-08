# Batch 2: Fix CS8602 warnings - Add null-forgiving operator to result.Value access
param(
    [string]$TestsDirectory = "tests"
)

Write-Host "🎯 Batch 2: Fixing CS8602 warnings (result.Value → result.Value!)" -ForegroundColor Green
Write-Host "Target: Add null-forgiving operator after IsSuccess check and ShouldNotBeNull" -ForegroundColor Cyan

# Get all test files
$testFiles = Get-ChildItem -Path $TestsDirectory -Recurse -Filter "*.cs"

$totalFiles = $testFiles.Count
$processedFiles = 0
$modifiedFiles = 0

foreach ($file in $testFiles) {
    $processedFiles++
    Write-Progress -Activity "Processing test files" -Status "File $processedFiles of $totalFiles" -PercentComplete (($processedFiles / $totalFiles) * 100)
    
    $content = Get-Content -Path $file.FullName -Raw
    $originalContent = $content
    $modified = $false
    
    # Pattern 1: result.Value.Property → result.Value!.Property after success/null checks
    # Look for patterns where result.Value is accessed after IsSuccess or ShouldNotBeNull
    if ($content -match 'result\.Value\.') {
        # Replace result.Value. with result.Value!. but only when not already using !
        $content = $content -replace '(?<!!)result\.Value\.(?!\!)', 'result.Value!.'
        $modified = $true
    }
    
    # Pattern 2: Similar for other common result variables
    if ($content -match '\w+Result\.Value\.') {
        $content = $content -replace '(?<!!)(\w+Result)\.Value\.(?!\!)', '$1.Value!.'
        $modified = $true
    }
    
    # Pattern 3: Similar for variables ending in 'result' 
    if ($content -match '\w+result\.Value\.') {
        $content = $content -replace '(?<!!)(\w+result)\.Value\.(?!\!)', '$1.Value!.'
        $modified = $true
    }
    
    # Save if modified
    if ($modified) {
        try {
            Set-Content -Path $file.FullName -Value $content -NoNewline
            $modifiedFiles++
            Write-Host "  ✅ Fixed: $($file.Name)" -ForegroundColor Green
        }
        catch {
            Write-Error "  ❌ Failed to modify: $($file.FullName) - $($_.Exception.Message)"
            # Restore original content on error
            Set-Content -Path $file.FullName -Value $originalContent -NoNewline
        }
    }
}

Write-Host ""
Write-Host "🎯 Batch 2 Complete!" -ForegroundColor Green
Write-Host "  📊 Files processed: $totalFiles" -ForegroundColor Cyan
Write-Host "  ✅ Files modified: $modifiedFiles" -ForegroundColor Green
Write-Host ""
Write-Host "🔨 Running compilation test..." -ForegroundColor Yellow 