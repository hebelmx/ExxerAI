$ErrorActionPreference = 'Stop'
#cd 'F:/Dynamic/ExxerAi/ExxerAI/src'

# Create solution
dotnet new sln --name ExxerAI

# Create main projects
dotnet new classlib --name ExxerAI.Domain --framework net9.0
dotnet sln add ExxerAI.Domain/ExxerAI.Domain.csproj

dotnet new classlib --name ExxerAI.Application --framework net9.0
dotnet sln add ExxerAI.Application/ExxerAI.Application.csproj

dotnet new classlib --name ExxerAI.Infrastructure --framework net9.0
dotnet sln add ExxerAI.Infrastructure/ExxerAI.Infrastructure.csproj

dotnet new classlib --name ExxerAI.Orchestration --framework net9.0
dotnet sln add ExxerAI.Orchestration/ExxerAI.Orchestration.csproj

dotnet new console --name ExxerAI.CLI --framework net9.0
dotnet sln add ExxerAI.CLI/ExxerAI.CLI.csproj

dotnet new webapi --auth Individual --use-local-db --use-controllers --name ExxerAI.Api --framework net9.0 --use-program-main
dotnet sln add ExxerAI.Api/ExxerAI.Api.csproj

#dotnet new mudblazor --interactivity Server --name ExxerAI.UI --all-interactive  server --auth Individual sqlserver --framework net9.0
dotnet new mudblazor --interactivity Server --name ExxerAI.UI -ai -au individual --framework net9.0
dotnet sln add ExxerAI.UI/ExxerAI.UI.csproj

# Create tests folder and test projects
mkdir tests

dotnet new xunit --name tests/ExxerAI.Domain.Tests --framework net9.0
dotnet sln add tests/ExxerAI.Domain.Tests/ExxerAI.Domain.Tests.csproj
dotnet add tests/ExxerAI.Domain.Tests/ExxerAI.Domain.Tests.csproj reference ExxerAI.Domain/ExxerAI.Domain.csproj

dotnet new xunit --name tests/ExxerAI.Application.Tests --framework net9.0
dotnet sln add tests/ExxerAI.Application.Tests/ExxerAI.Application.Tests.csproj
dotnet add tests/ExxerAI.Application.Tests/ExxerAI.Application.Tests.csproj reference ExxerAI.Application/ExxerAI.Application.csproj

dotnet new xunit --name tests/ExxerAI.Infrastructure.Tests --framework net9.0
dotnet sln add tests/ExxerAI.Infrastructure.Tests/ExxerAI.Infrastructure.Tests.csproj
dotnet add tests/ExxerAI.Infrastructure.Tests/ExxerAI.Infrastructure.Tests.csproj reference ExxerAI.Infrastructure/ExxerAI.Infrastructure.csproj

dotnet new xunit --name tests/ExxerAI.Orchestration.Tests --framework net9.0
dotnet sln add tests/ExxerAI.Orchestration.Tests/ExxerAI.Orchestration.Tests.csproj
dotnet add tests/ExxerAI.Orchestration.Tests/ExxerAI.Orchestration.Tests.csproj reference ExxerAI.Orchestration/ExxerAI.Orchestration.csproj

dotnet new xunit --name tests/ExxerAI.Api.Tests --framework net9.0

dotnet new xunit --name tests/ExxerAI.CLI.Tests --framework net9.0

dotnet new xunit --name tests/ExxerAI.UI.Tests --framework net9.0


