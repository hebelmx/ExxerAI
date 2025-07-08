# Fix specific nullable reference warnings (CS8602, CS8601) in test files
param(
    [string]$TestsDirectory = "tests"
)

Write-Host "Starting targeted nullable reference warnings fixes..." -ForegroundColor Green

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
    
    # Pattern 1: Fix result.Data.Property after IsSuccess and ShouldNotBeNull checks
    # Match pattern where we have both IsSuccess and ShouldNotBeNull checks followed by Data access
    if ($content -match 'result\.IsSuccess\.ShouldBeTrue\(\);\s*result\.Data\.ShouldNotBeNull\(\);') {
        # Replace subsequent result.Data. with result.Data!. (but not ShouldNotBeNull or ShouldBeNull)
        $content = $content -replace '(\s+result\.Data\.ShouldNotBeNull\(\);\s+)result\.Data\.(?!ShouldBeNull|ShouldNotBeNull)', '$1result.Data!.'
        $modified = $true
    }
    
    # Pattern 2: Fix VARIABLENAME.Data.Property after success checks
    $variablePattern = '(\w+Result)\.IsSuccess\.ShouldBeTrue\(\);\s*(.*\n)?\s*\1\.Data\.(?!ShouldBeNull|ShouldNotBeNull)'
    if ($content -match $variablePattern) {
        $content = $content -replace $variablePattern, '$1.IsSuccess.ShouldBeTrue();$2$1.Data!.'
        $modified = $true
    }
    
    # Pattern 3: Fix FirstOrDefault calls that can return null
    $content = $content -replace '(\w+\.FirstOrDefault\([^)]+\))\.(\w+)', '$1!.$2'
    if ($content -ne $originalContent) { $modified = $true }
    
    # Pattern 4: Fix Error property access on Result objects
    $content = $content -replace '(result\.Error)\.ShouldContain', '$1!.ShouldContain'
    if ($content -ne $originalContent) { $modified = $true }
    
    # Pattern 5: Fix .Value property access that could be null
    $content = $content -replace '(\w+Result\.Value)\.(\w+)', '$1!.$2'
    if ($content -ne $originalContent) { $modified = $true }
    
    # Write back if modified
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        $modifiedFiles++
        Write-Host "Modified: $($file.FullName)" -ForegroundColor Yellow
    }
}

Write-Host "`nCompleted processing $totalFiles files" -ForegroundColor Green
Write-Host "Modified $modifiedFiles files" -ForegroundColor Green
Write-Host "Targeted nullable reference warnings fix completed!" -ForegroundColor Green 