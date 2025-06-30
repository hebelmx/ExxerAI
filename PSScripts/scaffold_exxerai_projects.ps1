$ErrorActionPreference = 'Stop'
cd 'F:/Dynamic/ExxerAi/ExxerAI/src'
dotnet new sln --name ExxerAI
dotnet new classlib -n ExxerAI.Domain
dotnet sln add ExxerAI.Domain/ExxerAI.Domain.csproj
dotnet new xunit.v3 -n ExxerAI.Domain.Tests
dotnet sln add ExxerAI.Domain.Tests/ExxerAI.Domain.Tests.csproj
dotnet add ExxerAI.Domain.Tests/ExxerAI.Domain.Tests.csproj reference ExxerAI.Domain/ExxerAI.Domain.csproj
dotnet new classlib -n ExxerAI.Application
dotnet sln add ExxerAI.Application/ExxerAI.Application.csproj
dotnet new xunit.v3 -n ExxerAI.Application.Tests
dotnet sln add ExxerAI.Application.Tests/ExxerAI.Application.Tests.csproj
dotnet add ExxerAI.Application.Tests/ExxerAI.Application.Tests.csproj reference ExxerAI.Application/ExxerAI.Application.csproj
dotnet new classlib -n ExxerAI.Infrastructure
dotnet sln add ExxerAI.Infrastructure/ExxerAI.Infrastructure.csproj
dotnet new xunit.v3 -n ExxerAI.Infrastructure.Tests
dotnet sln add ExxerAI.Infrastructure.Tests/ExxerAI.Infrastructure.Tests.csproj
dotnet add ExxerAI.Infrastructure.Tests/ExxerAI.Infrastructure.Tests.csproj reference ExxerAI.Infrastructure/ExxerAI.Infrastructure.csproj
dotnet new classlib -n ExxerAI.Orchestration
dotnet sln add ExxerAI.Orchestration/ExxerAI.Orchestration.csproj
dotnet new xunit.v3 -n ExxerAI.Orchestration.Tests
dotnet sln add ExxerAI.Orchestration.Tests/ExxerAI.Orchestration.Tests.csproj
dotnet add ExxerAI.Orchestration.Tests/ExxerAI.Orchestration.Tests.csproj reference ExxerAI.Orchestration/ExxerAI.Orchestration.csproj
dotnet new classlib -n ExxerAI.UI
dotnet sln add ExxerAI.UI/ExxerAI.UI.csproj
dotnet new xunit.v3 -n ExxerAI.UI.Tests
dotnet sln add ExxerAI.UI.Tests/ExxerAI.UI.Tests.csproj
dotnet add ExxerAI.UI.Tests/ExxerAI.UI.Tests.csproj reference ExxerAI.UI/ExxerAI.UI.csproj