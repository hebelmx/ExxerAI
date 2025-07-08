# Batch 1: Fix xUnit1012 warnings - Change null to null! in InlineData attributes
param(
    [string]$TestsDirectory = "tests"
)

Write-Host "🎯 Batch 1: Fixing xUnit1012 warnings (null → null! in InlineData)" -ForegroundColor Green
Write-Host "Target: Change [InlineData(null)] to [InlineData(null!)]" -ForegroundColor Cyan

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
    
    # Pattern 1: [InlineData(null)] → [InlineData(null!)]
    # Only change null when it's a standalone parameter in InlineData
    if ($content -match '\[InlineData\([^\]]*\bnull\b[^\]]*\)\]') {
        $content = $content -replace '\[InlineData\(([^)]*)\bnull\b([^)]*)\)\]', '[InlineData($1null!$2)]'
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
Write-Host "🎯 Batch 1 Complete!" -ForegroundColor Green
Write-Host "  📊 Files processed: $totalFiles" -ForegroundColor Cyan
Write-Host "  ✅ Files modified: $modifiedFiles" -ForegroundColor Green
Write-Host ""
Write-Host "🔨 Running compilation test..." -ForegroundColor Yellow 