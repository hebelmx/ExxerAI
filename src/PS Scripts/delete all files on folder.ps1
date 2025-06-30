Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
# Check if the folder exists
if (Test-Path -Path "C:\app") {
    # Try to remove all contents in the folder
    try {
        Remove-Item -Path "C:\app\*" -Recurse -Force
        Write-Output "All contents in C:\app\ have been deleted successfully."
    } catch {
        # Catch any exceptions that occur during the removal process
        Write-Output "An error occurred while trying to delete the contents in C:\app\: $_"
    }
} else {
    # If the folder doesn't exist, output a message indicating this
    Write-Output "The folder C:\app\ does not exist."
}
