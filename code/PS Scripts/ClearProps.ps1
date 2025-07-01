# Define the root of your source code
$sourceRoot = "E:\Dynamic\IndTrace\IndTraceV2025\Src"# Define the root of your source code


# Extract all NuGet packages declared in .csproj files
$allPackages = Get-ChildItem -Recurse -Filter *.csproj -Path $sourceRoot |
    ForEach-Object {
        Select-String -Path $_.FullName -Pattern '<PackageReference Include="([^"]+)"' |
        ForEach-Object {
            $_.Matches.Groups[1].Value
        }
    } | Sort-Object -Unique

# Get all .cs files
$codeFiles = Get-ChildItem -Recurse -Include *.cs -Path $sourceRoot

# Analyze usage of each package
$unusedPackages = @()
foreach ($pkg in $allPackages) {
    $found = $false
    foreach ($file in $codeFiles) {
        if (Select-String -Path $file.FullName -Pattern "\b$pkg\b" -Quiet) {
            $found = $true
            break
        }
    }
    if (-not $found) {
        $unusedPackages += $pkg
    }
}

# Output unused packages
Write-Output "Unused NuGet Packages:"
$unusedPackages
