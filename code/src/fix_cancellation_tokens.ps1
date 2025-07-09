# Fix CancellationToken usage in test files for xUnit1051 warnings
param(
    [string]$TestsDirectory = "tests"
)

Write-Host "Starting CancellationToken fixes for xUnit1051 warnings..." -ForegroundColor Green

# Get all test files
$testFiles = Get-ChildItem -Path $TestsDirectory -Recurse -Filter "*.cs" | Where-Object { $_.Directory.Name -like "*Test*" }

$totalFiles = $testFiles.Count
$processedFiles = 0
$modifiedFiles = 0

foreach ($file in $testFiles) {
    $processedFiles++
    Write-Progress -Activity "Processing test files" -Status "File $processedFiles of $totalFiles" -PercentComplete (($processedFiles / $totalFiles) * 100)
    
    $content = Get-Content -Path $file.FullName -Raw
    $originalContent = $content
    $modified = $false
    
    # Add using Xunit if not present
    if ($content -notmatch "using Xunit;") {
        # Find the last using statement and add after it
        if ($content -match "(using [^;]+;)\s*(?:\r?\n)+\s*namespace") {
            $content = $content -replace "(using [^;]+;)(\s*(?:\r?\n)+\s*namespace)", "`$1`r`nusing Xunit;`$2"
            $modified = $true
        }
    }
    
    # Skip files that already have CancellationToken parameters to avoid double-fixing
    if ($content -match "TestContext\.Current\.CancellationToken") {
        Write-Host "Skipping already processed file: $($file.FullName)" -ForegroundColor Cyan
        continue
    }
    
    # Fix specific async method patterns - be more specific to avoid over-matching
    # Pattern 1: Basic async methods without parameters (most common)
    $content = $content -replace '(\w+)\.(\w+Async)\(\)\s*(?![.\(])', '$1.$2(TestContext.Current.CancellationToken)'
    
    # Pattern 2: Specific common NSubstitute patterns
    $content = $content -replace '(\w+)\.(\w+Async)\(\)\.Returns\(', '$1.$2(Arg.Any<CancellationToken>()).Returns('
    $content = $content -replace 'Received\(1\)\.(\w+Async)\(\)', 'Received(1).$1(Arg.Any<CancellationToken>())'
    
    if ($content -ne $originalContent) {
        $modified = $true
    }
    
    # Write back if modified
    if ($modified) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        $modifiedFiles++
        Write-Host "Modified: $($file.FullName)" -ForegroundColor Yellow
    }
}

Write-Host "`nCompleted processing $totalFiles files" -ForegroundColor Green
Write-Host "Modified $modifiedFiles files" -ForegroundColor Green
Write-Host "Fix CancellationToken script completed!" -ForegroundColor Green 