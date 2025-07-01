# VS-Extension-Sync: Update Extension List

# Get the script's directory
$scriptPath = $MyInvocation.MyCommand.Path
$scriptDir = Split-Path $scriptPath
$outputFile = Join-Path $scriptDir "extension-list.csv"

# Visual Studio default extension location (can be adapted per version)
$vsixDir = "$env:ProgramData\Microsoft\VisualStudio\Packages"

if (Test-Path $outputFile) { Remove-Item $outputFile -Force }

# Scan installed extensions and export basic metadata
Get-ChildItem -Directory $vsixDir | ForEach-Object {
    $id = $_.Name
    $manifest = Get-ChildItem $_.FullName -Recurse -Include extension.vsixmanifest | Select-Object -First 1
    if ($manifest) {
        [xml]$xml = Get-Content $manifest.FullName
        $name = $xml.PackageManifest.Metadata.Identity.Id
        $publisher = $xml.PackageManifest.Metadata.Identity.Publisher
        $version = $xml.PackageManifest.Metadata.Identity.Version

        "$name,$publisher,$id,$version" | Out-File -Append -FilePath $outputFile
    }
}

Write-Host "Extension list saved to $outputFile" -ForegroundColor Green