# Fix compilation errors introduced by overly aggressive CancellationToken fixes
param(
    [string]$TestsDirectory = "tests"
)

Write-Host "Reverting incorrect CancellationToken modifications..." -ForegroundColor Green

# Get all test files
$testFiles = Get-ChildItem -Path $TestsDirectory -Recurse -Filter "*.cs"

$totalFiles = $testFiles.Count
$processedFiles = 0
$modifiedFiles = 0

foreach ($file in $testFiles) {
    $processedFiles++
    Write-Progress -Activity "Fixing compilation errors" -Status "File $processedFiles of $totalFiles" -PercentComplete (($processedFiles / $totalFiles) * 100)
    
    $content = Get-Content -Path $file.FullName -Raw
    $originalContent = $content
    $modified = $false
    
    # Fix Task.Delay calls that got broken
    # Pattern: Task.Delay(int, cancellationToken, TestContext.Current.CancellationToken) -> Task.Delay(int, TestContext.Current.CancellationToken)
    $content = $content -replace 'Task\.Delay\(([^,]+),\s*[^,]+,\s*TestContext\.Current\.CancellationToken\)', 'Task.Delay($1, TestContext.Current.CancellationToken)'
    
    # Fix method calls that don't actually accept CancellationToken but had it added incorrectly
    # These are methods where we added CT but they don't support it
    
    # Fix InitializeAsync calls that only take 1 parameter, not 2
    $content = $content -replace '(\.InitializeAsync)\(([^,]+),\s*TestContext\.Current\.CancellationToken\)', '$1($2)'
    
    # Fix SearchSimilarAsync calls with incorrect parameter count  
    $content = $content -replace '(\.SearchSimilarAsync)\(([^,]+),\s*([^,]+),\s*TestContext\.Current\.CancellationToken\)(?=\s*;)', '$1($2, $3)'
    
    # Fix StoreEmbeddingAsync calls with too many parameters
    $content = $content -replace '(\.StoreEmbeddingAsync)\(([^,]+),\s*([^,]+),\s*([^,]+),\s*([^,]+),\s*([^,]+),\s*TestContext\.Current\.CancellationToken\)', '$1($2, $3, $4, $5)'
    
    # Fix method calls where CancellationToken was added but method signature doesn't support it
    # For GetExternalApiKeyAsync, SetExternalApiKeyAsync that only take specific parameters
    $content = $content -replace '(\.GetExternalApiKeyAsync)\(([^,]+),\s*TestContext\.Current\.CancellationToken\)', '$1($2)'
    $content = $content -replace '(\.SetExternalApiKeyAsync)\(([^,]+),\s*([^,]+),\s*TestContext\.Current\.CancellationToken\)', '$1($2, $3)'
    
    # Fix IngestDocumentAsync calls with wrong parameter count
    $content = $content -replace '(\.IngestDocumentAsync)\(([^,]+),\s*([^,]+),\s*([^,]+),\s*TestContext\.Current\.CancellationToken\)', '$1($2, $3, $4)'
    
    # Fix calls where we have "argument missing" errors - these are methods where we added CT in wrong position
    # Remove any double CancellationToken parameters
    $content = $content -replace '(TestContext\.Current\.CancellationToken),\s*TestContext\.Current\.CancellationToken', '$1'
    
    # Remove trailing commas before closing parentheses that might have been introduced
    $content = $content -replace ',\s*\)', ')'
    
    # Check if any changes were made
    if ($content -ne $originalContent) {
        $modified = $true
        Set-Content -Path $file.FullName -Value $content -NoNewline
        $modifiedFiles++
        Write-Host "Fixed: $($file.FullName)" -ForegroundColor Yellow
    }
}

Write-Host "`nCompleted processing $totalFiles files" -ForegroundColor Green
Write-Host "Fixed $modifiedFiles files" -ForegroundColor Green
Write-Host "Compilation error fixes completed!" -ForegroundColor Green 