# Batch 3: Fix CS8625 warnings - Change null to null! in assignments
param(
    [string]$TestsDirectory = "tests"
)

Write-Host "🎯 Batch 3: Fixing CS8625 warnings (null → null! in assignments)" -ForegroundColor Green
Write-Host "Target: Change null assignments to null! for non-nullable reference types" -ForegroundColor Cyan

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
    
    # Pattern 1: Variable assignments like: SomeType variable = null;
    if ($content -match '\w+\s+\w+\s*=\s*null;') {
        $content = $content -replace '(\w+\s+\w+\s*=\s*)null;', '$1null!;'
        $modified = $true
    }
    
    # Pattern 2: Direct assignments like: variable = null;
    if ($content -match '\w+\s*=\s*null;') {
        $content = $content -replace '(\w+\s*=\s*)null;', '$1null!;'
        $modified = $true
    }
    
    # Pattern 3: Method parameter assignments like: Method(param1, null, param3)
    # This is more complex, let's handle specific test patterns
    if ($content -match 'Should\.Throw.*\(\(\)\s*=>\s*new\s+\w+\([^)]*null[^)]*\)\)') {
        $content = $content -replace '(\(\(\)\s*=>\s*new\s+\w+\([^)]*)null([^)]*\)\))', '$1null!$2'
        $modified = $true
    }
    
    # Pattern 4: Constructor arguments with null like: new SomeClass(param1, null)
    if ($content -match 'new\s+\w+\([^)]*null[^)]*\)') {
        $content = $content -replace '(new\s+\w+\([^)]*)null([^)]*\))', '$1null!$2'
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
Write-Host "🎯 Batch 3 Complete!" -ForegroundColor Green
Write-Host "  📊 Files processed: $totalFiles" -ForegroundColor Cyan
Write-Host "  ✅ Files modified: $modifiedFiles" -ForegroundColor Green
Write-Host ""
Write-Host "🔨 Running compilation test..." -ForegroundColor Yellow 