# Conservative script to fix specific warnings for "warnings as errors" compilation
param(
    [string]$TestsDirectory = "tests"
)

Write-Host "Starting conservative warning fixes..." -ForegroundColor Green
Write-Host "Target: CS8625, CS8620, xUnit1051 warnings" -ForegroundColor Cyan

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
    
    # 1. Fix CS8625: Cannot convert null literal to non-nullable reference type
    # Only target very specific, safe patterns
    
    # Pattern 1: Method calls with null as parameter (ResultExtensions patterns)
    $content = $content -replace '\b(ResultExtensions\.\w+)\(null\b', '$1(null!'
    
    # Pattern 2: Direct null assignments in test assertions
    $content = $content -replace '\bnew\s+\w+\s*\{\s*\w+\s*=\s*null\b', { param($match) $match.Value -replace '\bnull\b', 'null!' }
    
    # Pattern 3: Test method parameters that are explicitly null for testing
    $content = $content -replace '(\.Create\w*\()\s*null\s*([,)])', '$1null!$2'
    
    # 2. Fix CS8620: Nullability differences (string?[] vs IEnumerable<string>)
    # Target specific pattern: string?[] to string[]
    $content = $content -replace 'string\?\[\]', 'string[]'
    
    # 3. Fix xUnit1051: CancellationToken warnings - VERY CONSERVATIVE
    # Only add to specific methods where we're confident it's safe
    
    # Pattern 1: Task.Delay with hardcoded values and no cancellation token
    $content = $content -replace 'Task\.Delay\((\d+)\)(?!\s*[,)].*CancellationToken)', 'Task.Delay($1, TestContext.Current.CancellationToken)'
    
    # Pattern 2: Very specific async method calls in test contexts (conservative list)
    $safeAsyncMethods = @('GetByIdAsync', 'GetAllAsync', 'AddAsync', 'UpdateAsync', 'DeleteAsync', 'ExistsAsync')
    
    foreach ($method in $safeAsyncMethods) {
        # Only add CancellationToken if method call has parameters but no CancellationToken
        $pattern = "($method)\(([^)]+)\)(?![^;]*CancellationToken)"
        $content = $content -replace $pattern, {
            param($match)
            $methodName = $match.Groups[1].Value
            $params = $match.Groups[2].Value.Trim()
            
            # Very conservative: only add if it looks like a simple parameter list
            if ($params -match '^[^={}]+$' -and $params -notmatch 'TestContext\.Current\.CancellationToken') {
                return "$methodName($params, TestContext.Current.CancellationToken)"
            } else {
                return $match.Value  # Don't change if it looks complex
            }
        }
        
        # Handle methods with no parameters
        $content = $content -replace "($method)\(\)(?![^;]*CancellationToken)", '$1(TestContext.Current.CancellationToken)'
    }
    
    # 4. Ensure using Xunit is present for TestContext.Current usage
    if ($content -match 'TestContext\.Current' -and $content -notmatch 'using Xunit;') {
        # Find insertion point after other using statements
        if ($content -match '(using [^;]+;)(\s*namespace)') {
            $content = $content -replace '(using [^;]+;)(\s*namespace)', "$1`nusing Xunit;$2"
            $modified = $true
        }
    }
    
    # Check if content was modified
    if ($content -ne $originalContent) {
        # Validate that we didn't break anything obvious
        $brokenPatterns = @(
            'null!!'    # Double null-forgiving
            'CancellationToken.*CancellationToken'  # Duplicate parameters
            'using Xunit;.*using Xunit;'  # Duplicate using
        )
        
        $contentValid = $true
        foreach ($brokenPattern in $brokenPatterns) {
            if ($content -match $brokenPattern) {
                Write-Host "  ⚠️  Skipping $($file.Name) - detected potential issue with pattern: $brokenPattern" -ForegroundColor Yellow
                $contentValid = $false
                break
            }
        }
        
        if ($contentValid) {
            Set-Content -Path $file.FullName -Value $content -NoNewline
            $modifiedFiles++
            Write-Host "  ✓ Fixed warnings in: $($file.Name)" -ForegroundColor Green
        }
    }
}

Write-Host "`nCompleted processing $totalFiles files" -ForegroundColor Cyan
Write-Host "Modified $modifiedFiles files" -ForegroundColor Green

# Test compilation after changes
Write-Host "`nTesting compilation..." -ForegroundColor Yellow
$buildResult = & dotnet build --verbosity quiet 2>&1
$buildExitCode = $LASTEXITCODE

if ($buildExitCode -eq 0) {
    Write-Host "✅ Build successful!" -ForegroundColor Green
    
    # Count remaining warnings
    $warningCount = ($buildResult | Where-Object { $_ -match "warning" }).Count
    Write-Host "Remaining warnings: $warningCount" -ForegroundColor $(if ($warningCount -eq 0) { "Green" } else { "Yellow" })
    
    if ($warningCount -gt 0) {
        Write-Host "`nRemaining warning types:" -ForegroundColor Yellow
        $buildResult | Where-Object { $_ -match "warning" } | ForEach-Object {
            if ($_ -match "warning (CS\d+|xUnit\d+)") {
                $matches.Values[0]
            }
        } | Sort-Object | Get-Unique | ForEach-Object {
            Write-Host "  - $_" -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "❌ Build failed! Rolling back changes..." -ForegroundColor Red
    & git checkout -- $TestsDirectory 2>$null
    Write-Host "Changes rolled back. Please check the build manually." -ForegroundColor Red
}

Write-Host "`nConservative warning fix completed!" -ForegroundColor Green 