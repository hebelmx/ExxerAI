# Keywords derived from deleted branch names
$branchKeywords = @(
    "Acceptation-Test", "BatchUpload", "DevShouldly", "DevShouldly-backup", "KeyedSingletonHandlers",
    "LibrariesUpdated", "MachineDescription", "MultitaskingPlcAndHub", "OeeTesting", "ParallelPLCDB",
    "PruebasRam", "Ram", "RamTesting", "ReadOnlyRepos", "WarmUpOptimization", "bumper",
    "dynamic-configuration-testing", "gateway-task-abstraction", "reference"
)

# Normalize to lowercase for matching
$keywords = $branchKeywords | ForEach-Object { $_.ToLower() }

# Output file
$outputFile = "GroupedDeletedCommits.txt"
if (Test-Path $outputFile) { Remove-Item $outputFile }

# Initialize mapping
$matchMap = @{}
foreach ($keyword in $keywords) { $matchMap[$keyword] = @() }
$unmatched = @()

# Get unreachable commit SHAs
$unreachable = git fsck --full --no-reflogs --unreachable | Select-String "commit" | ForEach-Object {
    ($_ -split " ")[2]
}

# Process each commit
foreach ($sha in $unreachable) {
    $info = git show --no-patch --pretty=format:"%h | %an | %ad | %s" $sha 2>$null
    if (-not $info) { continue }

    $found = $false
    foreach ($keyword in $keywords) {
        if ($info.ToLower().Contains($keyword)) {
            $matchMap[$keyword] += $info
            $found = $true
            break
        }
    }

    if (-not $found) {
        $unmatched += $info
    }
}

# Output matched branches
foreach ($keyword in $keywords) {
    $entries = $matchMap[$keyword]
    if ($entries.Count -gt 0) {
        Add-Content -Path $outputFile -Value "Branch Match: $keyword"
        $entries | ForEach-Object { Add-Content -Path $outputFile -Value "  $_" }
        Add-Content -Path $outputFile -Value ""
    }
}

# Output unmatched
if ($unmatched.Count -gt 0) {
    Add-Content -Path $outputFile -Value "Unmatched Commits:"
    $unmatched | ForEach-Object { Add-Content -Path $outputFile -Value "  $_" }
}

Write-Host "Saved grouped unreachable commits to '$outputFile'" -ForegroundColor Green
