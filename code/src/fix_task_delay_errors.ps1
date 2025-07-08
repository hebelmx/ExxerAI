# Quick Fix: Repair Task.Delay compilation errors caused by Batch 4
param(
    [string]$TestsDirectory = "tests"
)

Write-Host "🚨 Quick Fix: Repairing Task.Delay compilation errors" -ForegroundColor Yellow
Write-Host "Target: Convert Task.Delay(ms, token) to Task.Delay(TimeSpan.FromMilliseconds(ms), token)" -ForegroundColor Cyan

# Get all test files
$testFiles = Get-ChildItem -Path $TestsDirectory -Recurse -Filter "*.cs"

$totalFiles = $testFiles.Count
$processedFiles = 0
$modifiedFiles = 0

foreach ($file in $testFiles) {
    $processedFiles++
    Write-Progress -Activity "Fixing Task.Delay errors" -Status "File $processedFiles of $totalFiles" -PercentComplete (($processedFiles / $totalFiles) * 100)
    
    $content = Get-Content -Path $file.FullName -Raw
    $originalContent = $content
    $modified = $false
    
    # Fix Task.Delay(int, CancellationToken) → Task.Delay(TimeSpan.FromMilliseconds(int), CancellationToken)
    if ($content -match 'Task\.Delay\(\d+, TestContext\.Current\.CancellationToken\)') {
        $content = $content -replace 'Task\.Delay\((\d+), (TestContext\.Current\.CancellationToken)\)', 'Task.Delay(TimeSpan.FromMilliseconds($1), $2)'
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
Write-Host "🚨 Quick Fix Complete!" -ForegroundColor Yellow
Write-Host "  📊 Files processed: $totalFiles" -ForegroundColor Cyan
Write-Host "  ✅ Files modified: $modifiedFiles" -ForegroundColor Green
Write-Host ""
Write-Host "🔨 Testing compilation..." -ForegroundColor Yellow 