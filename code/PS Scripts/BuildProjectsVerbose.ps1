param (
    [switch]$Test
)

$LogDir = Join-Path $RootDir "logs"
if (!(Test-Path $LogDir)) {
    New-Item -ItemType Directory -Path $LogDir | Out-Null
}

$RootDir = "F:\Dynamic\ExxerAi\ExxerAI\code"

$Projects = @(
    "src\ExxerAI.Domain\ExxerAI.Domain.csproj",
    "src\ExxerAI.Application\ExxerAI.Application.csproj",
    "src\ExxerAI.Infrastructure\ExxerAI.Infrastructure.csproj",
    "src\ExxerAI.Api\ExxerAI.Api.csproj",
    "src\ExxerAI.CLI\ExxerAI.CLI.csproj",
    "src\ExxerAi.MCPServer\ExxerAi.MCPServer.csproj",
    "src\ExxerAI.Orchestration\ExxerAI.Orchestration.csproj",
    "src\ExxerAI.UI\ExxerAI.UI.csproj",
    "src\tests\ExxerAI.Domain.Tests\ExxerAI.Domain.Tests.csproj",
    "src\tests\ExxerAI.Application.Tests\ExxerAI.Application.Tests.csproj",
    "src\tests\ExxerAI.Api.Tests\ExxerAI.Api.Tests.csproj",
    "src\tests\ExxerAI.Infrastructure.Tests\ExxerAI.Infrastructure.Tests.csproj",
    "src\tests\ExxerAI.IntegrationTests\ExxerAI.IntegrationTests.csproj",
    "src\tests\ExxerAI.CLI.Tests\ExxerAI.CLI.Tests.csproj",
    "src\tests\ExxerAi.MCPServer.Tests\ExxerAi.MCPServer.Tests.csproj",
    "src\tests\ExxerAI.Orchestration.Tests\ExxerAI.Orchestration.Tests.csproj",
    "src\tests\ExxerAI.Architecture.Tests\ExxerAI.Architecture.Tests.csproj"
)

Write-Host "🧹 Clearing NuGet caches..."
dotnet nuget locals all --clear

foreach ($proj in $Projects) {
    $FullPath = Join-Path $RootDir $proj
    Write-Host "`n📦 Restoring: $proj"
    dotnet restore $FullPath -v:diag
//    dotnet restore $FullPath -v:diag 2>&1 | Tee-Object -FilePath "$LogDir\restore-$($proj.Replace('\','_')).log"

    Write-Host "`n🔨 Building: $proj"
    dotnet build $FullPath -v:diag
}

if ($Test) {
    foreach ($proj in $Projects) {
        if ($proj -like "*Tests.csproj") {
            $TestPath = Join-Path $RootDir $proj
            Write-Host "`n🧪 Testing: $proj"
            dotnet test $TestPath -v:diag
        }
    }
}
