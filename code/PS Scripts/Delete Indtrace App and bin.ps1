##//[string]$basePath = "D:\Projects\IndTraceV2024\Src"

param (
    
      [string]$basePath = "C:\Users\Exxerpro_Precision55\Documents\GitHub\IndTraceV2024"
   ## [string]$basePath = "D:\Projects\IndTraceV2024\Src"
)

# Function to delete specific folders
function Delete-SpecificFolders {
    param (
        [string]$path,
        [string[]]$foldersToDelete
    )

    # Iterate through each directory in the base path
    $directories = Get-ChildItem -Path $path -Directory
    foreach ($dir in $directories) {
        # Check if the directory name matches any of the specified folder names
        if ($foldersToDelete -contains $dir.Name) {
            try {
                Remove-Item -Path $dir.FullName -Recurse -Force
                Write-Host "Deleted: $($dir.FullName)"
            } catch {
                Write-Host "Failed to delete: $($dir.FullName) - $($_.Exception.Message)"
            }
        }
    }

    # Recursively call the function for each subdirectory
    foreach ($dir in $directories) {
        Delete-SpecificFolders -path $dir.FullName -foldersToDelete $foldersToDelete
    }
}

# Define the folder names to delete
$foldersToDelete = @("IndTraceV2023Dir", "IndTraceApp", "bin", "obj")

# Check if the base path exists
if (Test-Path -Path $basePath) {
    Write-Host "Starting to delete specified folders..."
    Delete-SpecificFolders -path $basePath -foldersToDelete $
    Write-Host "Completed deleting specified folders."
} else {
    Write-Host "The specified path '$basePath' does not exist."
}