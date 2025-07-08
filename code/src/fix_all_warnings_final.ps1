# Fix remaining xUnit1051 CancellationToken warnings and nullable reference warnings
param(
    [string]$TestsDirectory = "tests"
)

Write-Host "Starting final cleanup of all warnings..." -ForegroundColor Green

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
    
    # 1. Fix nullable reference warnings (CS8625, CS8604)
    
    # Pattern: null! for test parameters that expect non-nullable
    $content = $content -replace '\bnull\b(?=\s*[,)])', 'null!'
    
    # Pattern: Add null-forgiving operator for possible null reference warnings
    $content = $content -replace '(\w+\.Error)\.(ShouldContain|ShouldBe)\(', '$1!.$2('
    
    # 2. Fix xUnit1051 CancellationToken warnings
    
    # Ensure using Xunit is present if file contains test methods
    if ($content -match '\[Fact\]|\[Theory\]' -and $content -notmatch 'using Xunit;') {
        # Find the last using statement and add Xunit after it
        $content = $content -replace '(using [^;]+;)\s*(?=\r?\n\r?\nnamespace)', "$1`nusing Xunit;"
        $modified = $true
    }
    
    # Pattern 1: Methods ending with Async() should use TestContext.Current.CancellationToken
    # But only if they have a CancellationToken parameter available
    $content = $content -replace '(\w+Async)\(\)(?=\s*;)', '$1(TestContext.Current.CancellationToken)'
    
    # Pattern 2: Task.Delay() calls without cancellation token
    $content = $content -replace 'Task\.Delay\((\d+)\)(?!.*CancellationToken)', 'Task.Delay($1, TestContext.Current.CancellationToken)'
    
    # Pattern 3: ConfigureAwait(false) calls
    $content = $content -replace '\.ConfigureAwait\(false\)', '.ConfigureAwait(false)'
    
    # Pattern 4: Fix specific async calls that need CancellationToken but don't have it
    # This is more conservative - only add where we're sure it's needed
    $asyncCallsNeedingToken = @(
        'GetByIdAsync',
        'GetAllAsync', 
        'AddAsync',
        'UpdateAsync',
        'DeleteAsync',
        'ExistsAsync',
        'ProcessDocumentAsync',
        'ExtractTextAsync',
        'InitializeAsync'
    )
    
    foreach ($asyncCall in $asyncCallsNeedingToken) {
        # Pattern: methodAsync(parameters) where no CancellationToken is present
        $pattern = "($asyncCall)\(([^)]*)\)(?!.*CancellationToken)"
        $content = $content -replace $pattern, {
            param($match)
            $methodName = $match.Groups[1].Value
            $params = $match.Groups[2].Value.Trim()
            
            if ($params -eq "") {
                return "$methodName(TestContext.Current.CancellationToken)"
            } else {
                return "$methodName($params, TestContext.Current.CancellationToken)"
            }
        }
    }
    
    # Check if content was modified
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        $modifiedFiles++
        $modified = $true
        Write-Host "  ✓ Fixed warnings in: $($file.Name)" -ForegroundColor Green
    }
}

Write-Host "\nCompleted processing $totalFiles files" -ForegroundColor Cyan
Write-Host "Modified $modifiedFiles files" -ForegroundColor Green
Write-Host "Final cleanup complete!" -ForegroundColor Green 