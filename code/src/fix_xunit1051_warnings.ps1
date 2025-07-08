# Batch 4: Fix xUnit1051 warnings - Add TestContext.Current.CancellationToken to async calls
param(
    [string]$TestsDirectory = "tests"
)

Write-Host "🎯 Batch 4: Fixing xUnit1051 warnings (CancellationToken)" -ForegroundColor Green
Write-Host "Target: Add TestContext.Current.CancellationToken to async method calls" -ForegroundColor Cyan

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
    
    # Ensure using Xunit is present for TestContext
    if ($content -match '\[Fact\]|\[Theory\]' -and $content -notmatch 'using Xunit;') {
        # Add using Xunit at the top with other usings
        $content = $content -replace '(using [^;]+;\s*)+', "$&using Xunit;`r`n"
        $modified = $true
    }
    
    # Pattern 1: Task.Delay calls without CancellationToken
    if ($content -match 'Task\.Delay\([^)]+\)(?!\s*,\s*TestContext\.Current\.CancellationToken)') {
        $content = $content -replace 'Task\.Delay\(([^)]+)\)(?!\s*,\s*TestContext\.Current\.CancellationToken)', 'Task.Delay($1, TestContext.Current.CancellationToken)'
        $modified = $true
    }
    
    # Pattern 2: await SomeMethodAsync() calls that should include CancellationToken
    # This is trickier - let's focus on specific common patterns
    
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
Write-Host "🎯 Batch 4 Complete!" -ForegroundColor Green
Write-Host "  📊 Files processed: $totalFiles" -ForegroundColor Cyan
Write-Host "  ✅ Files modified: $modifiedFiles" -ForegroundColor Green
Write-Host ""
Write-Host "🔨 Running compilation test..." -ForegroundColor Yellow 