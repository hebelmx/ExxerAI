# Fix nullable reference warnings in test files (CS8602, CS8601)
param(
    [string]$TestsDirectory = "tests"
)

Write-Host "Starting nullable reference warnings fixes..." -ForegroundColor Green

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
    
    # Fix Result<T>.Data access patterns after IsSuccess check
    # Pattern: result.IsSuccess.ShouldBeTrue(); ... result.Data.Property
    $patterns = @(
        # Most common pattern: result.Data.Property after IsSuccess check
        @{
            Pattern = '(result\.IsSuccess\.ShouldBeTrue\(\);\s*(?:.*\n)?\s*)result\.Data\.(?!ShouldBeNull|ShouldNotBeNull)'
            Replacement = '$1result.Data!.'
        },
        
        # Pattern: result.Data.ShouldNotBeNull(); result.Data.Property
        @{
            Pattern = '(result\.Data\.ShouldNotBeNull\(\);\s*(?:.*\n)?\s*)result\.Data\.(?!ShouldBeNull|ShouldNotBeNull)'
            Replacement = '$1result.Data!.'
        },
        
        # Pattern: Multiple Data accesses in same test after null check
        @{
            Pattern = '(\w+Result)\.Data\.(?!ShouldBeNull|ShouldNotBeNull)(\w+\.ShouldBe\()'
            Replacement = '$1.Data!.$2'
        },
        
        # Fix direct null assignment warnings
        @{
            Pattern = '= null;(\s*// .+)?$'
            Replacement = '= null!;$1'
        }
    )
    
    # Apply patterns
    foreach ($pattern in $patterns) {
        $newContent = $content -replace $pattern.Pattern, $pattern.Replacement
        if ($newContent -ne $content) {
            $content = $newContent
            $modified = $true
        }
    }
    
    # Additional specific fixes for common nullable warnings
    
    # Fix LINQ FirstOrDefault calls that could return null
    $content = $content -replace '(\w+\.FirstOrDefault\([^)]+\))\.(\w+)', '$1!.$2'
    
    # Fix Error property access
    $content = $content -replace '(result\.Error)\.ShouldContain', '$1!.ShouldContain'
    
    # Fix specific warning patterns from the warning list
    if ($content -match '\.Value\.') {
        $content = $content -replace '(\w+)\.Value\.(\w+)', '$1.Value!.$2'
        $modified = $true
    }
    
    # Write back if modified
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        $modifiedFiles++
        Write-Host "Modified: $($file.FullName)" -ForegroundColor Yellow
    }
}

Write-Host "`nCompleted processing $totalFiles files" -ForegroundColor Green
Write-Host "Modified $modifiedFiles files" -ForegroundColor Green
Write-Host "Nullable reference warnings fix completed!" -ForegroundColor Green 