# PowerShell script to deploy diagnostic scripts to Ubuntu
# Copy the scripts below to your Ubuntu machine and run them

Write-Host "🚀 ExxerAI Ubuntu Deployment Scripts" -ForegroundColor Green
Write-Host "Copy these scripts to your Ubuntu machine (192.168.0.40)" -ForegroundColor Yellow

Write-Host "`n=== Step 1: Run Diagnostic Script ===" -ForegroundColor Cyan
Write-Host "Create file: ~/diagnose.sh" -ForegroundColor White
Write-Host "Then run: chmod +x ~/diagnose.sh && ./diagnose.sh" -ForegroundColor Yellow

Write-Host "`n=== Step 2: Fix Ollama Permissions ===" -ForegroundColor Cyan  
Write-Host "Create file: ~/fix_ollama.sh" -ForegroundColor White
Write-Host "Then run: chmod +x ~/fix_ollama.sh && sudo ./fix_ollama.sh" -ForegroundColor Yellow

Write-Host "`n=== Connection Methods ===" -ForegroundColor Magenta
Write-Host "Option 1: Copy via USB drive or file share" -ForegroundColor White
Write-Host "Option 2: Use browser to copy text content" -ForegroundColor White  
Write-Host "Option 3: Install Windows SSH client:" -ForegroundColor White
Write-Host "  Add-WindowsCapability -Online -Name OpenSSH.Client~~~~0.0.1.0" -ForegroundColor Gray

# Display script contents for easy copying
Write-Host "`n=== DIAGNOSTIC SCRIPT CONTENT ===" -ForegroundColor Yellow
Write-Host "(Copy everything between the lines to ~/diagnose.sh)" -ForegroundColor Gray
Write-Host "----------------------------------------" -ForegroundColor DarkGray
Get-Content -Path "diagnose_ubuntu_setup.sh" -Raw
Write-Host "----------------------------------------" -ForegroundColor DarkGray

Write-Host "`n=== OLLAMA FIX SCRIPT CONTENT ===" -ForegroundColor Yellow  
Write-Host "(Copy everything between the lines to ~/fix_ollama.sh)" -ForegroundColor Gray
Write-Host "----------------------------------------" -ForegroundColor DarkGray
Get-Content -Path "fix_ollama_permissions.sh" -Raw
Write-Host "----------------------------------------" -ForegroundColor DarkGray

Write-Host "`nAfter copying and running these scripts, your Ubuntu agent node will be ready!" -ForegroundColor Green 