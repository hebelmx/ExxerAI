# Save-BitLockerKeys.ps1
# Script to securely store BitLocker recovery keys to a file

$keys = @(
    @{ID='A5FAC1EE'; RecoveryKey='644622-016951-373637-099605-597432-030492-172502-237985'},
    @{ID='145BCB31'; RecoveryKey='417890-408947-068486-183502-592075-412500-325006-194073'},
    @{ID='1AB360DC'; RecoveryKey='665478-162173-277409-658603-189112-121869-654951-259842'}
)

$outputPath = "$env:USERPROFILE\Documents\BitLockerRecoveryKeys_$(Get-Date -Format yyyyMMdd).txt"

"BitLocker Recovery Keys Backup" | Out-File -FilePath $outputPath -Encoding UTF8
"Export Date: $(Get-Date)" | Out-File -FilePath $outputPath -Append

foreach ($key in $keys) {
    "--------------------------------------------" | Out-File -FilePath $outputPath -Append
    "Key ID: $($key.ID)" | Out-File -FilePath $outputPath -Append
    "Recovery Password: $($key.RecoveryKey)" | Out-File -FilePath $outputPath -Append
}

"--------------------------------------------" | Out-File -FilePath $outputPath -Append

# Opens the file after creation for verification
Start-Process notepad.exe $outputPath
