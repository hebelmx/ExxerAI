# Define the path to the XML file
$xmlFilePath = "D:\Projects\IndTraceV2024\Src\Directory.Packages.props"

# Load the XML file
[xml]$xml = Get-Content $xmlFilePath

# Create a hashtable to track seen PackageReferences
$seen = @{}

# Process each PackageReference
foreach ($package in $xml.Project.ItemGroup.PackageReference) {
    $include = $package.Include
    if ($seen.ContainsKey($include)) {
        $package.ParentNode.RemoveChild($package) | Out-Null
    } else {
        $seen[$include] = $true
    }
}

# Save the updated XML back to the file
$xml.Save($xmlFilePath)

Write-Host "Duplicate PackageReference entries have been removed."
