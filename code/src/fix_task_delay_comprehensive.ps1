# Comprehensive Fix: Repair all Task.Delay compilation errors
param(
    [string]$TestsDirectory = "tests"
)

Write-Host "🚨 Comprehensive Fix: Repairing all Task.Delay issues" -ForegroundColor Yellow
Write-Host "Target: Fix duplicates, convert int to TimeSpan, and clean up parameters" -ForegroundColor Cyan

# Get all test files
$testFiles = Get-ChildItem -Path $TestsDirectory -Recurse -Filter "*.cs"

$totalFiles = $testFiles.Count
$processedFiles = 0
$modifiedFiles = 0

foreach ($file in $testFiles) {
    $processedFiles++
    Write-Progress -Activity "Fixing Task.Delay issues" -Status "File $processedFiles of $totalFiles" -PercentComplete (($processedFiles / $totalFiles) * 100)
    
    $content = Get-Content -Path $file.FullName -Raw
    $originalContent = $content
    $modified = $false
    
    # Fix Pattern 1: Remove duplicate CancellationTokens like:
    # Task.Delay(ms, TestContext.Current.CancellationToken, TestContext.Current.CancellationToken)
    if ($content -match 'Task\.Delay\([^,]+,\s*TestContext\.Current\.CancellationToken,\s*TestContext\.Current\.CancellationToken\)') {
        $content = $content -replace 'Task\.Delay\(([^,]+),\s*TestContext\.Current\.CancellationToken,\s*TestContext\.Current\.CancellationToken\)', 'Task.Delay($1, TestContext.Current.CancellationToken)'
        $modified = $true
    }
    
    # Fix Pattern 2: Convert int milliseconds to TimeSpan when CancellationToken is present
    # Task.Delay(intValue, TestContext.Current.CancellationToken) → Task.Delay(TimeSpan.FromMilliseconds(intValue), TestContext.Current.CancellationToken)
    if ($content -match 'Task\.Delay\((?:CancellationTestConstants\.)?(\w+),\s*TestContext\.Current\.CancellationToken\)') {
        $content = $content -replace 'Task\.Delay\((CancellationTestConstants\.)?(\w+),\s*(TestContext\.Current\.CancellationToken)\)', 'Task.Delay(TimeSpan.FromMilliseconds($1$2), $3)'
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
Write-Host "🚨 Comprehensive Fix Complete!" -ForegroundColor Yellow
Write-Host "  📊 Files processed: $totalFiles" -ForegroundColor Cyan
Write-Host "  ✅ Files modified: $modifiedFiles" -ForegroundColor Green
Write-Host ""
Write-Host "🔨 Testing compilation..." -ForegroundColor Yellow 