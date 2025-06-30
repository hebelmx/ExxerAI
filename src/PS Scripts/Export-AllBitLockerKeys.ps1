# Export-AllBitLockerKeys.ps1
# Exports BitLocker recovery keys from all internal and external drives
# and saves them into a secure text file.

$outputFile = "$env:USERPROFILE\Documents\AllBitLockerKeys_$(Get-Date -Format yyyyMMdd_HHmmss).txt"

"BitLocker Recovery Keys Backup - Export Date: $(Get-Date)" | Out-File -FilePath $outputFile -Encoding UTF8

$bitLockerVolumes = Get-BitLockerVolume | Where-Object { $_.KeyProtector }

foreach ($volume in $bitLockerVolumes) {
    Add-Content -Path $outputFile -Value "======================================="
    Add-Content -Path $outputFile -Value "Drive Letter: $($volume.MountPoint)"
    Add-Content -Path $outputFile -Value "Volume Type : $($volume.VolumeType)"
    Add-Content -Path $outputFile -Value "Protection Status: $($volume.ProtectionStatus)"

    foreach ($key in $volume.KeyProtector) {
        if ($key.KeyProtectorType -eq 'RecoveryPassword') {
            Add-Content -Path $outputFile -Value "Key Protector ID : $($key.KeyProtectorId)"
            Add-Content -Path $outputFile -Value "Recovery Password: $($key.RecoveryPassword)"
            Add-Content -Path $outputFile -Value "---------------------------------------"
        }
    }
}

Add-Content -Path $outputFile -Value "======================================="
Add-Content -Path $outputFile -Value "End of Export"

# Open file automatically to verify
Start-Process notepad.exe $outputFile