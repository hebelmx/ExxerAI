# Define the path to your solution and the packages file
$solutionPath = "D:\Projects\IndTraceV2024\Src"
$packagesFile = "D:\Projects\IndTraceV2024\Src\Directory.Packages.props"

# Define output file paths
$usedPackagesFile = "D:\Projects\IndTraceV2024\used-packages.xml"
$unusedPackagesFile =  "D:\Projects\IndTraceV2024\unused-packages.xml"

# Get all package references from Directory.Packages.props
[xml]$xml = Get-Content $packagesFile
$packages = $xml.Project.ItemGroup.PackageReference | ForEach-Object {
    [PSCustomObject]@{
        Include = $_.Include
        Version = $_.Version
    }
}

# Function to search for package usage in the solution
function Find-PackageUsage {
    param (
        [string]$package
    )
    $found = $false
    $files = Get-ChildItem -Path $solutionPath -Recurse -Include *.cs
    foreach ($file in $files) {
        $content = Get-Content $file.FullName
        if ($content -match $package) {
            $found = $true
            break
        }
    }
    return $found
}

# Initialize lists for used and unused packages
$usedPackages = @()
$unusedPackages = @()

# Check each package for usage
foreach ($package in $packages) {
    $isUsed = Find-PackageUsage -package $package.Include
    if ($isUsed) {
        Write-Output "Used: $($package.Include)"
        $usedPackages += $package
    } else {
        Write-Output "Unused: $($package.Include)"
        $unusedPackages += $package
    }
}

# Function to create XML content
function Create-XmlContent {
    param (
        [array]$packages
    )
    $xmlContent = [xml]'<Project></Project>'
    $itemGroup = $xmlContent.CreateElement('ItemGroup')
    foreach ($package in $packages) {
        $packageReference = $xmlContent.CreateElement('PackageReference')
        $packageReference.SetAttribute('Include', $package.Include)
        $packageReference.SetAttribute('Version', $package.Version)
        $itemGroup.AppendChild($packageReference) | Out-Null
    }
    $xmlContent.Project.AppendChild($itemGroup) | Out-Null
    return $xmlContent
}

# Create XML contents for used and unused packages
$usedPackagesXml = Create-XmlContent -packages $usedPackages
$unusedPackagesXml = Create-XmlContent -packages $unusedPackages


# Save the XML contents to files
$usedPackagesXml.Save($usedPackagesFile)
$unusedPackagesXml.Save($unusedPackagesFile)

Write-Output "Used packages XML saved to: $usedPackagesFile"
Write-Output "Unused packages XML saved to: $unusedPackagesFile"