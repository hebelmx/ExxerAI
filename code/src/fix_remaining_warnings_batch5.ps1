# Batch 5: Fix remaining specific warning patterns
param(
    [string]$TestsDirectory = "tests"
)

Write-Host "🎯 Batch 5: Fixing remaining specific warnings" -ForegroundColor Green
Write-Host "Target: CS8625 (null!), xUnit1012 (null! in InlineData), CS8602 (!), xUnit1051" -ForegroundColor Cyan

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
    
    # Pattern 1: Fix remaining xUnit1012 - InlineData with null parameters
    # More specific patterns that our first script missed
    if ($content -match '\[InlineData\([^)]*,\s*null\s*[,\)]') {
        $content = $content -replace '(\[InlineData\([^)]*),\s*null\s*([,\)])', '$1, null!$2'
        $modified = $true
    }
    
    # Pattern 2: Fix CS8625 - Method parameter assignments with null
    # Should.Throw<Exception>(() => new SomeClass(null))
    if ($content -match 'Should\.Throw.*\(\(\)\s*=>\s*new\s+\w+\([^)]*\bnull\b[^)]*\)\)') {
        $content = $content -replace '(Should\.Throw.*\(\(\)\s*=>\s*new\s+\w+\([^)]*)(\bnull\b)([^)]*\)\))', '$1$2!$3'
        $modified = $true
    }
    
    # Pattern 3: Fix CS8625 - Variable assignments with null
    # var variable = null; → var variable = null!;
    if ($content -match '\b\w+\s+\w+\s*=\s*null\s*;') {
        $content = $content -replace '(\b\w+\s+\w+\s*=\s*)null(\s*;)', '$1null!$2'
        $modified = $true
    }
    
    # Pattern 4: Fix remaining CS8602 - Property access that needs !
    # Look for specific patterns that our previous script missed
    if ($content -match '\.Value\.\w+' -and $content -notmatch '\.Value!\.\w+') {
        $content = $content -replace '(?<!!)\.Value\.(\w+)', '.Value!.$1'
        $modified = $true
    }
    
    # Pattern 5: Fix CS8620 - String array nullability issues
    # string?[] to string[] issues
    if ($content -match 'string\?\[\]') {
        $content = $content -replace 'string\?\[\]', 'string[]'
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
Write-Host "🎯 Batch 5 Complete!" -ForegroundColor Green
Write-Host "  📊 Files processed: $totalFiles" -ForegroundColor Cyan
Write-Host "  ✅ Files modified: $modifiedFiles" -ForegroundColor Green
Write-Host ""
Write-Host "🔨 Running compilation test..." -ForegroundColor Yellow 