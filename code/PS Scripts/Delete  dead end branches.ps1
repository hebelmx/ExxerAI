

cd D:\IndTrace\Projects\IndTraceV2025
# List of branches to keep
$keepBranches = @(
    "dynamic",
    "dev",    
    "dynamicMapper",
    "release-2025-05-18",
    "release",
    "stellantis-deploy-2025-may-08",
    "Bumper.2025.4.9",
    "main"
)

# Convert to a hashtable for fast lookup
$keepHash = @{}
foreach ($b in $keepBranches) { $keepHash[$b] = $true }

# Fetch all branches
git fetch --all

# Get all remote branches
$remoteBranches = git branch -r | Where-Object { $_ -notmatch '->' } | ForEach-Object {
    ($_ -replace '^\s*origin/', '').Trim()
}

# Filter out branches to delete
$branchesToDelete = $remoteBranches | Where-Object { -not $keepHash.ContainsKey($_) }

if (-not $branchesToDelete) {
    Write-Host "No branches to delete." -ForegroundColor Green
    exit
}

# Confirm with user
Write-Host "The following branches will be deleted from 'origin':" -ForegroundColor Yellow
$branchesToDelete | ForEach-Object { Write-Host " - $_" }

$confirmation = Read-Host "Do you want to proceed with deletion? (y/n)"
if ($confirmation -ne 'y') {
    Write-Host "Aborted by user." -ForegroundColor Red
    exit
}

# Proceed to delete
foreach ($branch in $branchesToDelete) {
    Write-Host "Deleting remote branch: $branch" -ForegroundColor Cyan
    git push origin --delete $branch
}

Write-Host "Branch cleanup complete." -ForegroundColor Green
