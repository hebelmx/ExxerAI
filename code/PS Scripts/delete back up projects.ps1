param (
    [string]$basePath = "D:\Projects\IndTraceV2024\Src"
)

# Function to send files to the Recycle Bin
function Move-ToRecycleBin {
    param (
        [string]$path
    )

    # Find all .csproj files containing 'Backup' in their name
    $backupFiles = Get-ChildItem -Path $path -Recurse -File -Filter "*Backup*.csproj"

    # Move each file to the Recycle Bin
    foreach ($file in $backupFiles) {
        try {
            # Use Shell.Application to move files to Recycle Bin
            $shell = New-Object -ComObject Shell.Application
            $recycleBin = $shell.NameSpace(10)
            $fileItem = $shell.NameSpace((Get-Item $file.FullName).DirectoryName).ParseName((Get-Item $file.FullName).Name)
            $recycleBin.MoveHere($fileItem)
            Write-Host "Moved to Recycle Bin: $($file.FullName)"
        } catch {
            Write-Host "Failed to move to Recycle Bin: $($file.FullName) - $($_.Exception.Message)"
        }
    }
}

# Check if the base path exists
if (Test-Path -Path $basePath) {
    Move-ToRecycleBin -path $basePath
} else {
    Write-Host "The specified path '$basePath' does not exist."
}
