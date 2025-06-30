# ====================================================================================
# Region Cleaner Script - Replaces C# #region/#endregion blocks with comments
# Supports: DryRun (preview) or Apply (modify files)
# ====================================================================================

# -------------------------------
# Configurable Parameters
# -------------------------------

$mode = "Apply"  # Options: "DryRun" or "Apply"
$encoding = "utf8"
$rootPath = "E:\Dynamic\IndTrace\IndTraceV2025\Src\Tests\Core\Application.UnitTests"
# -------------------------------
# Smart Folder Detection
# -------------------------------

if (-not $rootPath -or [string]::IsNullOrWhiteSpace($rootPath)) {
    Write-Host "`nNo rootPath set. Using current directory..." -ForegroundColor Yellow
    $rootPath = Get-Location
}

# Optionally, allow manual input
if ($env:INTERACTIVE -eq "1") {
    $inputPath = Read-Host "Enter path to the code folder (leave blank for current)"
    if (-not [string]::IsNullOrWhiteSpace($inputPath)) {
        $rootPath = $inputPath
    }
}

# -------------------------------
# Execution Header
# -------------------------------

Write-Host "`n=== Region Cleanup Script ===" -ForegroundColor Whitee
Write-Host "Target Directory: $rootPath" -ForegroundColor Yellow
Write-Host "Execution Mode  : $mode" -ForegroundColor Yellow
Write-Host "-----------------------------`n"

# -------------------------------
# File Processing Logic
# -------------------------------

$files = Get-ChildItem -Path $rootPath -Recurse -Filter *.cs

foreach ($file in $files) {
    $originalLines = Get-Content $file.FullName
    $newLines = @()
    $changes = @()

    for ($i = 0; $i -lt $originalLines.Count; $i++) {
        $line = $originalLines[$i]

        if ($line -match '^\s*#region\s+(.*)$') {
            $newLine = "// $($matches[1])"
            $changes += "[${i+1}] - $line`n        + $newLine"
            $newLines += $newLine
        }
        elseif ($line -match '^\s*#endregion(?:\s+(.*))?\s*$') {
            if ($matches[1]) {
                $comment = "// $($matches[1])"
                $changes += "[${i+1}] - $line`n        + $comment"
                $newLines += $comment
            } else {
                $changes += "[${i+1}] - $line`n        + (removed)"
                # Skip appending to newLines
            }
        }
        else {
            $newLines += $line
        }
    }

    if ($changes.Count -gt 0) {
        Write-Host "File: $($file.FullName)" -ForegroundColor Cyan
        $changes | ForEach-Object { Write-Host $_ -ForegroundColor Gray }
        Write-Host ""

        if ($mode -eq "Apply") {
            $newLines | Set-Content -Path $file.FullName -Encoding $encoding
            Write-Host " => Changes applied to file." -ForegroundColor Green
            Write-Host ""
        }
    }
}
