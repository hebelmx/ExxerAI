
# === CONFIGURATION ===
$Database = "YourDatabase"
$BackupDir = "D:\Backups"
$BackupName = "$Database-$(Get-Date -Format 'yyyyMMdd_HHmmss').bak"
$BackupPath = Join-Path $BackupDir $BackupName
$SQLInstance = "localhost\SQLEXPRESS"  # Change as needed
$CertificateName = "BackupCert"
$ECCRedundancy = 10  # ECC percentage

# === SQL BACKUP WITH CHECKSUM AND ENCRYPTION ===
$sql = @"
BACKUP DATABASE [$Database]
TO DISK = N'$BackupPath'
WITH FORMAT,
     INIT,
     COMPRESSION,
     CHECKSUM,
     ENCRYPTION(ALGORITHM = AES_256, SERVER CERTIFICATE = $CertificateName),
     STATS = 10;
"@

Invoke-Sqlcmd -ServerInstance $SQLInstance -Query $sql
Write-Host "✅ Backup completed: $BackupPath"

# === VERIFY BACKUP ===
$sqlVerify = @"
RESTORE VERIFYONLY 
FROM DISK = N'$BackupPath'
WITH CHECKSUM;
"@

Invoke-Sqlcmd -ServerInstance $SQLInstance -Query $sqlVerify
Write-Host "✅ Backup verification succeeded."

# === GENERATE ECC USING PAR2 (MultiPar CLI required) ===
$par2Path = "par2j.exe"
$par2Args = "c -r${ECCRedundancy} `"$BackupPath.par2`" `"$BackupPath`""
Start-Process -FilePath $par2Path -ArgumentList $par2Args -NoNewWindow -Wait
Write-Host "✅ ECC parity files created for error correction."

# === OPTIONAL: CLEANUP OLD BACKUPS (>30 days) ===
Get-ChildItem -Path $BackupDir -Filter "*.bak" | Where-Object { $_.LastWriteTime -lt (Get-Date).AddDays(-30) } | Remove-Item
Write-Host "♻️  Old backups cleaned up."
