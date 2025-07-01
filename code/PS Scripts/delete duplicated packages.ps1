# Define the path to the source directory
$sourcePath = "D:\Projects\IndTraceV2024\Src"

# Get all .csproj files in the specified directory and its subdirectories
$projectFiles = Get-ChildItem -Path $sourcePath -Recurse -Filter *.csproj

foreach ($file in $projectFiles) {
    # Read the content of the .csproj file
    $content = Get-Content $file.FullName
    $seen = @{}

    # Process each line and filter out duplicate PackageReference entries
    $newContent = foreach ($line in $content) {
        if ($line -match '<PackageReference Include="([^"]+)" Version="[^"]+"/>') {
            $package = $matches[1]
            if ($seen[$package]) {
                continue
            } else {
                $seen[$package] = $true
            }
        }
        $line
    }

    # Write the updated content back to the .csproj file
    Set-Content -Path $file.FullName -Value $newContent
}

Write-Host "Duplicate PackageReference entries have been removed."
