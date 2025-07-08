# Fix xUnit1051 CancellationToken warnings in test files
param(
    [string]$TestsDirectory = "tests"
)

Write-Host "Starting xUnit1051 CancellationToken warnings fixes..." -ForegroundColor Green

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
    
    # Pattern 3: Replace new CancellationToken() with TestContext.Current.CancellationToken
    $newContent = $content -replace 'new CancellationToken\(\)', 'TestContext.Current.CancellationToken'
    if ($newContent -ne $content) {
        $content = $newContent
        $modified = $true
    }
    
    # Pattern 4: Common async method calls that should include cancellation token
    # This pattern looks for method calls ending with Async() and adds cancellation token
    $asyncMethodPatterns = @(
        # Repository methods
        '(\w+\.(?:Add|Update|Delete|Get|Find|Save|Load|Create|Remove|Execute|Process|Store|Retrieve)Async)\(\)',
        
        # Service methods  
        '(\w+\.(?:Initialize|Start|Stop|Process|Handle|Execute|Run|Generate|Calculate|Validate|Send|Receive)Async)\(\)',
        
        # Common async patterns
        '(\w+\.(?:Wait|Delay|Task|Complete|Finish)Async)\(\)',
        
        # HTTP and external calls
        '(\w+\.(?:Post|Get|Put|Delete|Send|Call|Invoke|Request)Async)\(\)'
    )
    
    foreach ($pattern in $asyncMethodPatterns) {
        $newContent = $content -replace $pattern, '$1(TestContext.Current.CancellationToken)'
        if ($newContent -ne $content) {
            $content = $newContent
            $modified = $true
        }
    }
    
    # Pattern 5: Fix specific async method calls with multiple parameters
    # Add cancellation token as last parameter to common async calls
    $multiParamPatterns = @(
        # Methods with 1 parameter that need CancellationToken
        '(\w+\.(?:ProcessDocumentAsync|IngestDocumentAsync|GetByIdAsync|DeleteByIdAsync|UpdateAsync|CreateAsync|AddAsync|RemoveAsync|StoreAsync|LoadAsync|FindAsync|SearchAsync|QueryAsync|SaveChangesAsync|ExecuteAsync|RunAsync|InitializeAsync|StartAsync|StopAsync|ValidateAsync|GenerateAsync|CalculateAsync|HandleAsync|SendAsync|ReceiveAsync|CompleteAsync|FinishAsync))\(([^)]+)\)(?!\s*,\s*[^)]*CancellationToken)',
        
        # Methods with 2 parameters that need CancellationToken  
        '(\w+\.(?:ProcessDocumentAsync|IngestDocumentAsync|UpdateAsync|CreateAsync|AddAsync|StoreAsync|SearchAsync|QueryAsync|ExecuteAsync|RunAsync|HandleAsync|SendAsync|ReceiveAsync))\(([^,]+),\s*([^)]+)\)(?!\s*,\s*[^)]*CancellationToken)',
        
        # Methods with 3 parameters that need CancellationToken
        '(\w+\.(?:ProcessDocumentAsync|UpdateAsync|CreateAsync|ExecuteAsync|HandleAsync|SendAsync))\(([^,]+),\s*([^,]+),\s*([^)]+)\)(?!\s*,\s*[^)]*CancellationToken)'
    )
    
    # Apply multi-parameter patterns
    $newContent = $content -replace '(\w+\.(?:ProcessDocumentAsync|IngestDocumentAsync|GetByIdAsync|DeleteByIdAsync|UpdateAsync|CreateAsync|AddAsync|RemoveAsync|StoreAsync|LoadAsync|FindAsync|SearchAsync|QueryAsync|SaveChangesAsync|ExecuteAsync|RunAsync|InitializeAsync|StartAsync|StopAsync|ValidateAsync|GenerateAsync|CalculateAsync|HandleAsync|SendAsync|ReceiveAsync|CompleteAsync|FinishAsync))\(([^)]+)\)(?!\s*,\s*[^)]*CancellationToken)', '$1($2, TestContext.Current.CancellationToken)'
    if ($newContent -ne $content) {
        $content = $newContent
        $modified = $true
    }
    
    $newContent = $content -replace '(\w+\.(?:ProcessDocumentAsync|IngestDocumentAsync|UpdateAsync|CreateAsync|AddAsync|StoreAsync|SearchAsync|QueryAsync|ExecuteAsync|RunAsync|HandleAsync|SendAsync|ReceiveAsync))\(([^,]+),\s*([^)]+)\)(?!\s*,\s*[^)]*CancellationToken)', '$1($2, $3, TestContext.Current.CancellationToken)'
    if ($newContent -ne $content) {
        $content = $newContent
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
Write-Host "xUnit1051 CancellationToken warnings fix completed!" -ForegroundColor Green 