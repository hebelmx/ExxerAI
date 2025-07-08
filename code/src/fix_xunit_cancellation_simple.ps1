# Fix xUnit1051 CancellationToken warnings in test files - Simple targeted approach
param(
    [string]$TestsDirectory = "tests"
)

Write-Host "Starting targeted xUnit1051 CancellationToken warnings fixes..." -ForegroundColor Green

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
    
    # Ensure using Xunit is present if file contains test methods
    if ($content -match '\[Fact\]|\[Theory\]' -and $content -notmatch 'using Xunit;') {
        # Find the last using statement and add Xunit after it
        if ($content -match '(?m)^using [^;]+;') {
            $lastUsingMatch = [regex]::Matches($content, '(?m)^using [^;]+;') | Select-Object -Last 1
            $insertPosition = $lastUsingMatch.Index + $lastUsingMatch.Length
            $beforeInsert = $content.Substring(0, $insertPosition)
            $afterInsert = $content.Substring($insertPosition)
            $content = $beforeInsert + "`nusing Xunit;" + $afterInsert
            $modified = $true
        }
    }
    
    # Pattern 1: Replace CancellationToken.None with TestContext.Current.CancellationToken
    $newContent = $content -replace 'CancellationToken\.None', 'TestContext.Current.CancellationToken'
    if ($newContent -ne $content) {
        $content = $newContent
        $modified = $true
    }
    
    # Pattern 2: Replace default(CancellationToken) with TestContext.Current.CancellationToken
    $newContent = $content -replace 'default\(CancellationToken\)', 'TestContext.Current.CancellationToken'
    if ($newContent -ne $content) {
        $content = $newContent
        $modified = $true
    }
    
    # Pattern 3: Fix common async method patterns that are missing CancellationToken
    # These are the specific patterns causing the xUnit1051 warnings based on the build output
    
    # Service method calls with no parameters -> add CancellationToken
    $content = $content -replace '(await\s+\w+\.(?:InitializeAsync|ListModelsAsync|ValidateAsync|GetIngestionStatusAsync|StartWatchingFolderAsync|GetStatsAsync|GetActiveSessionsAsync))\(\)', '$1(TestContext.Current.CancellationToken)'
    
    # Service method calls with 1 parameter -> add CancellationToken as 2nd parameter
    $content = $content -replace '(await\s+\w+\.(?:CountTokensAsync|GetRateLimitInfoAsync|GetByIdAsync|DeleteByIdAsync|IngestDocumentAsync|DeleteEmbeddingAsync|UpdateEmbeddingAsync|GetExternalApiKeyAsync|StartWatchingFolderAsync|ValidateAsync|InitializeAsync))\(([^)]+)\)', '$1($2, TestContext.Current.CancellationToken)'
    
    # Service method calls with 2 parameters -> add CancellationToken as 3rd parameter
    $content = $content -replace '(await\s+\w+\.(?:EstimateCostAsync|GenerateCompletionAsync|GenerateChatCompletionAsync|SetExternalApiKeyAsync|StoreEmbeddingAsync|StoreBatchAsync|SearchSimilarAsync))\(([^,]+),\s*([^)]+)\)', '$1($2, $3, TestContext.Current.CancellationToken)'
    
    # Service method calls with 3 parameters -> add CancellationToken as 4th parameter  
    $content = $content -replace '(await\s+\w+\.(?:ProcessDocumentAsync|StoreEmbeddingAsync|SearchSimilarAsync|IngestDocumentAsync))\(([^,]+),\s*([^,]+),\s*([^)]+)\)', '$1($2, $3, $4, TestContext.Current.CancellationToken)'
    
    # Service method calls with 4 parameters -> add CancellationToken as 5th parameter
    $content = $content -replace '(await\s+\w+\.(?:StoreEmbeddingAsync|UpdateEmbeddingAsync))\(([^,]+),\s*([^,]+),\s*([^,]+),\s*([^)]+)\)', '$1($2, $3, $4, $5, TestContext.Current.CancellationToken)'
    
    # Repository method calls - common patterns
    $content = $content -replace '(await\s+\w+\.(?:AddAsync|UpdateAsync|DeleteAsync|GetByIdAsync|FindAsync|SaveChangesAsync|ExistsAsync|GetAllAsync|GetByStatusAsync|GetByTypeAsync|GetActiveAsync|GetOverdueAsync|GetByAgentIdAsync|GetTaskCountAsync))\(([^)]+)\)', '$1($2, TestContext.Current.CancellationToken)'
    
    # Task.Delay calls
    $content = $content -replace '(await\s+Task\.Delay)\(([^)]+)\)', '$1($2, TestContext.Current.CancellationToken)'
    
    # Check if any changes were made
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
Write-Host "Targeted xUnit1051 CancellationToken warnings fix completed!" -ForegroundColor Green 