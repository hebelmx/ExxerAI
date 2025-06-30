# Export-FullDiskBitLockerKeys.ps1
# Gets all volumes (with or without drive letters), checks for BitLocker,
# and exports recovery keys for internal and external drives.

$outputFile = "$env:USERPROFILE\Documents\AllBitLockerKeys_$(Get-Date -Format yyyyMMdd_HHmmss).txt"

"BitLocker Recovery Keys Backup - Export Date: $(Get-Date)" | Out-File -FilePath $outputFile -Encoding UTF8

$volumes = Get-Volume | Where-Object { $_.FileSystemLabel -ne $null }

foreach ($vol in $volumes) {
    $mount = $vol.DriveLetter
    $mountPoint = if ($mount) { "${mount}:\\" } else { $vol.Path }

    try {
        $bitlocker = Get-BitLockerVolume -MountPoint $mountPoint -ErrorAction Stop

        if ($bitlocker.KeyProtector) {
            Add-Content -Path $outputFile -Value "======================================="
            Add-Content -Path $outputFile -Value "Volume: $mountPoint"
            Add-Content -Path $outputFile -Value "Volume Type : $($bitlocker.VolumeType)"
            Add-Content -Path $outputFile -Value "Protection Status: $($bitlocker.ProtectionStatus)"

            foreach ($key in $bitlocker.KeyProtector) {
                if ($key.KeyProtectorType -eq 'RecoveryPassword') {
                    Add-Content -Path $outputFile -Value "Key Protector ID : $($key.KeyProtectorId)"
                    Add-Content -Path $outputFile -Value "Recovery Password: $($key.RecoveryPassword)"
                    Add-Content -Path $outputFile -Value "---------------------------------------"
                }
            }
        }
    }
    catch {
        Write-Warning "Skipping volume $mountPoint (not protected or inaccessible)"
    }
}

Add-Content -Path $outputFile -Value "======================================="
Add-Content -Path $outputFile -Value "End of Export"

Start-Process notepad.exe $outputFile
