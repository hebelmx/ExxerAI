cd F:\Dynamic\ExxerAi\ExxerAI\code\src
# Define the pattern to remove from Git tracking
$pattern = "Samples/local-ai-packaged/neo4j/data/transactions/**/*.db.0"

Write-Host "=== Step 1: Ensure files are ignored in .gitignore ==="
if (-not (Select-String -Path ".gitignore" -Pattern [regex]::Escape($pattern) -Quiet)) {
    Add-Content ".gitignore" "`n$pattern"
    Write-Host "Added to .gitignore"
} else {
    Write-Host "Already in .gitignore"
}

Write-Host "=== Step 2: Remove matching files from Git index (not disk) ==="
git rm --cached $pattern

Write-Host "=== Step 3: Stage .gitignore update ==="
git add .gitignore

Write-Host "=== Step 4: Commit changes ==="
git commit -m "Remove large Neo4j .db.0 files from Git tracking and ignore locally"

Write-Host "=== Step 5: You can now push safely ==="
Write-Host "Run: git push origin your-branch-name"
