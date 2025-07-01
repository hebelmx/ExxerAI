# ==========================================
# Export-BitLockerKeys.ps1
# Exports all BitLocker recovery keys and saves them to a secure file.
# Optionally enables auto-unlock for fixed and removable drives.
# ==========================================

$exportPath = "$env:USERPROFILE\Documents\BitLockerKeys_$(Get-Date -Format yyyyMMdd_HHmmss).txt"

"Exporting BitLocker Recovery Keys..." | Out-File -FilePath $exportPath -Encoding UTF8

$bitlockerVolumes = Get-BitLockerVolume | Where-Object { $_.KeyProtector }

foreach ($volume in $bitlockerVolumes) {
    $driveLetter = $volume.MountPoint
    $keyProtectors = $volume.KeyProtector

    Add-Content -Path $exportPath -Value "------------------------------------"
    Add-Content -Path $exportPath -Value "Drive: $driveLetter"

    foreach ($protector in $keyProtectors) {
        if ($protector.KeyProtectorType -eq 'RecoveryPassword') {
            $id = $protector.KeyProtectorId
            $key = (Get-BitLockerKeyProtector -MountPoint $driveLetter -KeyProtectorId $id).RecoveryPassword
            Add-Content -Path $exportPath -Value "ID: $id"
            Add-Content -Path $exportPath -Value "Recovery Key: $key"
        }
    }

    # Optional: Enable auto-unlock for non-system volumes
    if ($volume.VolumeType -ne 'OperatingSystem') {
        Enable-BitLockerAutoUnlock -MountPoint $driveLetter -ErrorAction SilentlyContinue
        Add-Content -Path $exportPath -Value "Auto-Unlock Enabled: Yes"
    }
}

Add-Content -Path $exportPath -Value "------------------------------------"
Add-Content -Path $exportPath -Value "Export complete: $(Get-Date)"

Start-Process notepad.exe $exportPath


Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope Process .\Export-BitLockerKeys.ps1

cd "E:\Dynamic\IndTrace\IndTraceV2025\Src\PS Scripts"