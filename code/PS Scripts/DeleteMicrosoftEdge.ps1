# Navigate to the base directory where Edge versions are installed
$basePath = "C:\Program Files (x86)\Microsoft\Edge\Application"

# Check if the base directory exists
if (Test-Path $basePath) {
    # Get all version directories using regex to match version patterns
    $versionDirs = Get-ChildItem -Path $basePath -Directory | Where-Object { $_.Name -match '^\d+\.\d+\.\d+\.\d+$' }

    # Uninstall each detected version
    foreach ($versionDir in $versionDirs) {
        $installerPath = "$($versionDir.FullName)\Installer"
        if (Test-Path $installerPath) {
            Write-Host "Uninstalling Edge version $($versionDir.Name)..."
            Start-Process -FilePath "$installerPath\setup.exe" -ArgumentList "--uninstall --system-level --verbose-logging --force-uninstall" -Wait
        } else {
            Write-Host "Installer path not found for version $($versionDir.Name)."
        }
    }
} else {
    Write-Host "Edge base directory not found."
}

# Block Edge Reinstallation
$edgePath = "$env:WinDir\SystemApps\Microsoft.MicrosoftEdge_8wekyb3d8bbwe"

if (Test-Path $edgePath) {
    takeown /f $edgePath /A /R
    icacls $edgePath /grant administrators:F /T
    icacls $edgePath /deny system:(OI)(CI)F
    Remove-Item -Path $edgePath -Recurse -Force
    Write-Host "Microsoft Edge has been successfully blocked."
} else {
    Write-Host "Microsoft Edge is already blocked or not found."
}

# Disable Edge Update Tasks
$tasks = @(
    "Microsoft\EdgeUpdate\EdgeUpdate",
    "Microsoft\EdgeUpdate\EdgeUpdateTaskMachineCore",
    "Microsoft\EdgeUpdate\EdgeUpdateTaskMachineUA"
)

foreach ($task in $tasks) {
    if (Get-ScheduledTask -TaskPath "\Microsoft\EdgeUpdate\" -TaskName $task) {
        Disable-ScheduledTask -TaskPath "\Microsoft\EdgeUpdate\" -TaskName $task
        Write-Host "$task has been disabled."
    } else {
        Write-Host "$task not found."
    }
}

# Disable Edge Installation via Registry (equivalent to Group Policy)
$registryPath = "HKLM:\SOFTWARE\Policies\Microsoft\EdgeUpdate"
New-Item -Path $registryPath -Force | Out-Null
Set-ItemProperty -Path $registryPath -Name "AutoUpdateCheckPeriodMinutes" -Value 0
Set-ItemProperty -Path $registryPath -Name "InstallDefault" -Value 0
Write-Host "Edge installation policy set via Registry."
