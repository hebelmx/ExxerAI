$baseDir = "E:\Dynamic\IndTrace\IndTraceV2025\Src\Tests\Core\Aggregation.Tests"  # Change to your test root directory

$structure = @{
    "Dto" = "Domain\Dtos"
    "Event" = "Domain\Events"
    "Created" = "Domain\Events"
    "Updated" = "Domain\Events"
    "Handler" = "Features"
    "Validator" = "Validators"
    "ViewModel" = "ViewModels"
    "Vm" = "ViewModels"
    "Query" = "Queries"
    "Command" = "Commands"
    "Middleware" = "Middleware"
    "Behavior" = "Middleware"
    "Exception" = "Middleware"
    "Configuration" = "Configuration"
    "Service" = "Infrastructure"
    "Utility" = "Utilities"
}

$featureGroups = @("Barcode", "ConfigApp", "ConfigStation", "Cycle", "Machine", "Performance", "Plc", "Product", "Register", "Reports", "Setting", "Shift", "Variable", "WorkFlow")

Get-ChildItem -Path $baseDir -Filter *.cs | ForEach-Object {
    $file = $_
    $targetDir = ""

    foreach ($feature in $featureGroups) {
        if ($file.Name -like "*$feature*") {
            $targetDir = "Features\$feature"
            break
        }
    }

    if (-not $targetDir) {
        foreach ($key in $structure.Keys) {
            if ($file.Name -like "*$key*") {
                $targetDir = $structure[$key]
                break
            }
        }
    }

    if (-not $targetDir) {
        $targetDir = "Uncategorized"
    }

    $fullTargetPath = Join-Path $baseDir $targetDir
    if (-not (Test-Path $fullTargetPath)) {
        New-Item -Path $fullTargetPath -ItemType Directory -Force
    }

    Move-Item -Path $file.FullName -Destination $fullTargetPath
}
