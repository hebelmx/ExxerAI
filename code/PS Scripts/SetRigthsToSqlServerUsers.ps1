# Define the directory and the SQL Server service account
$directoryPath = "C:\Users\Exxerpro_Precision55\Documents\GitHub\IndTraceV2024\Src\Databases"
$serviceAccount = "NT SERVICE\MSSQLSERVER"

# Check if the directory exists
if (-Not (Test-Path -Path $directoryPath)) {
    Write-Host "The directory path does not exist: $directoryPath"
    exit
}

# Get the current ACL for the directory
$acl = Get-Acl $directoryPath

# Define the new access rule
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($serviceAccount, "ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow")

# Add the new access rule to the ACL
$acl.SetAccessRule($accessRule)

# Apply the updated ACL to the directory
Set-Acl $directoryPath $acl

Write-Host "Read permissions granted to $serviceAccount on $directoryPath"
