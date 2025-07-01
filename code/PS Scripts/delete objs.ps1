
#Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# PowerShell Script to recursively delete 'bin' and 'obj' folders from the solution directory

# Define function to recursively delete 'bin' and 'obj' directories
function DeleteBuildFolders([string]$path) {
    # Search for 'bin' folders
    $binFolders = Get-ChildItem -Path $path -Recurse -Directory -Filter 'bin'
    
    # Search for 'obj' folders
    $obFolders = Get-ChildItem -Path $path -Recurse -Directory -Filter 'ob'
    
     # Search for 'obj' folders
    $objFolders = Get-ChildItem -Path $path -Recurse -Directory -Filter 'obj'
    # Combine the two lists
    $foldersToDelete = $binFolders+ $obFolders + $objFolders
    
    foreach ($folder in $foldersToDelete) {
        Write-Host ("Deleting: " + $folder.FullName)
        # Delete the folder
        Remove-Item -Path $folder.FullName -Recurse -Force
    }
}

# Main Execution

# Use '.' to denote the current directory, or replace it with the specific path where your .NET solution is located.
$rootPath = "C:\Users\hebel\OneDrive\Documentos\GitHub\IndTraceV2023\Src"

DeleteBuildFolders -path $rootPath

Write-Host "Deleted all bin and obj folders."


# Main Execution

# Use '.' to denote the current directory, or replace it with the specific path where your .NET solution is located.
$rootPath = "C:\App\"

DeleteBuildFolders -path $rootPath
