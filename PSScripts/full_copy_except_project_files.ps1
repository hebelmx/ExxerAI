$ErrorActionPreference = 'Stop'
$sourceRoot = 'E:/Dynamic/ExxerAi/ExxerAI/src'
$destinationRoot = 'F:/Dynamic/ExxerAi/ExxerAI/src'
$logFile = 'F:/Dynamic/ExxerAi/ExxerAI/migration_log.txt'
if (Test-Path $logFile) { Remove-Item $logFile -Force }
Add-Content $logFile '=== Migration Log ==='
Add-Content $logFile ('Start: ' + (Get-Date))

Get-ChildItem -Path $sourceRoot -Recurse | Where-Object {
    $_.Extension -notin '.csproj', '.json', '.sln'
} | ForEach-Object {
    $relativePath = $_.FullName.Substring($sourceRoot.Length)
    $destinationPath = Join-Path $destinationRoot $relativePath
    $destinationDir = Split-Path $destinationPath
    if (-not (Test-Path $destinationDir)) {
        New-Item -Path $destinationDir -ItemType Directory -Force | Out-Null
    }
    if (-not (Test-Path $destinationPath)) {
        Copy-Item -Path $_.FullName -Destination $destinationPath -Force
        Add-Content $logFile ('COPIED: ' + $relativePath)
    } else {
        Add-Content $logFile ('SKIPPED (already exists): ' + $relativePath + '  <-- REVIEW')
    }
}
Add-Content $logFile ('End: ' + (Get-Date))