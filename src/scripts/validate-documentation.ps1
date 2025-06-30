# XML Documentation Validation Script
# Validates that public members have XML documentation

param(
    [string]$ProjectPath = ".",
    [switch]$Detailed
)

Write-Host "🔍 XML Documentation Validation Report" -ForegroundColor Green
Write-Host "Project Path: $ProjectPath" -ForegroundColor Gray
Write-Host ""

$csFiles = Get-ChildItem -Path $ProjectPath -Recurse -Filter "*.cs" | Where-Object { 
    $_.FullName -notmatch "\\bin\\" -and 
    $_.FullName -notmatch "\\obj\\" -and
    $_.FullName -notmatch "GlobalSuppressions\.cs$"
}

$totalFiles = $csFiles.Count
$filesWithDocumentation = 0
$documentedMembers = 0
$undocumentedMembers = 0

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw
    
    # Check for XML documentation comments
    $xmlComments = [regex]::Matches($content, '/// <summary>')
    $publicMembers = [regex]::Matches($content, 'public (class|interface|record|enum|struct|\w+\s+\w+\s*\(|\w+\s+\w+\s*{)')
    
    if ($xmlComments.Count -gt 0) {
        $filesWithDocumentation++
    }
    
    $documentedMembers += $xmlComments.Count
    $undocumentedMembers += [Math]::Max(0, $publicMembers.Count - $xmlComments.Count)
    
    if ($Detailed -and $publicMembers.Count -gt 0) {
        $coveragePercent = if ($publicMembers.Count -gt 0) { 
            [Math]::Round(($xmlComments.Count / $publicMembers.Count) * 100, 1) 
        } else { 100 }
        
        $status = if ($coveragePercent -eq 100) { "✅" } 
                  elseif ($coveragePercent -ge 80) { "⚠️" } 
                  else { "❌" }
        
        Write-Host "$status $($file.Name): $coveragePercent% ($($xmlComments.Count)/$($publicMembers.Count))" -ForegroundColor $(
            if ($coveragePercent -eq 100) { "Green" }
            elseif ($coveragePercent -ge 80) { "Yellow" }
            else { "Red" }
        )
    }
}

# Summary Report
Write-Host ""
Write-Host "📊 Summary Report:" -ForegroundColor Green
Write-Host "  Total C# Files: $totalFiles" -ForegroundColor White
Write-Host "  Files with Documentation: $filesWithDocumentation ($([Math]::Round(($filesWithDocumentation / $totalFiles) * 100, 1))%)" -ForegroundColor White

$totalMembers = $documentedMembers + $undocumentedMembers
if ($totalMembers -gt 0) {
    $overallCoverage = [Math]::Round(($documentedMembers / $totalMembers) * 100, 1)
    Write-Host "  Documented Members: $documentedMembers" -ForegroundColor Green
    Write-Host "  Undocumented Members: $undocumentedMembers" -ForegroundColor Red
    Write-Host "  Overall Coverage: $overallCoverage%" -ForegroundColor $(
        if ($overallCoverage -ge 90) { "Green" }
        elseif ($overallCoverage -ge 70) { "Yellow" }
        else { "Red" }
    )
}

Write-Host ""
if ($overallCoverage -ge 90) {
    Write-Host "🎉 Excellent documentation coverage!" -ForegroundColor Green
} elseif ($overallCoverage -ge 70) {
    Write-Host "👍 Good documentation coverage, room for improvement" -ForegroundColor Yellow
} else {
    Write-Host "📝 Documentation needs improvement" -ForegroundColor Red
}

Write-Host ""
Write-Host "💡 To run detailed analysis: .\validate-documentation.ps1 -Detailed" -ForegroundColor Gray 