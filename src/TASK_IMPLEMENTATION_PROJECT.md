Please complete this task,

Step 1: Make a round across all project files csproj

Make sure all follow this style:
Of course all with their own properties:

<Project Sdk="Microsoft.NET.Sdk.Worker">

  <!-- ============================================================================ -->
  <!-- CORE PROPERTIES -->
  <!-- ============================================================================ -->
  <PropertyGroup>
    <!-- Target Framework -->
    <TargetFramework>net10.0</TargetFramework>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    
    <!-- Language Features -->
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    
    <!-- Security & Configuration -->
    <UserSecretsId>dotnet-IndTrace.Hub-74d2a8b7-9374-4521-beab-897ed5726dbd</UserSecretsId>
    
    <!-- Application Startup -->
    <StartupObject>IndTrace.Hub.Server.Program</StartupObject>
    
    <!-- Application Information -->
    <AssemblyTitle>IndTrace HubServer</AssemblyTitle>
    <Description>IndTrace.HubServer</Description>
    <Company>Exxerpro Solutions</Company>
    <Product>IndTrace.HubServer</Product>
    <Title>IndTrace.HubServer</Title>
    
    <!-- Application Icon -->
    <ApplicationIcon>HubServer.ico</ApplicationIcon>
  </PropertyGroup>

  <!-- ============================================================================ -->
  <!-- PUBLISHING PROPERTIES -->
  <!-- ============================================================================ -->
  <PropertyGroup>
    <!-- Publishing Enhancements -->
    <EnableDynamicPgo>true</EnableDynamicPgo>
    <TieredCompilation>true</TieredCompilation>
    <PublishSingleFile>true</PublishSingleFile>
    <SelfContained>true</SelfContained>
  </PropertyGroup>

  <!-- ============================================================================ -->
  <!-- STATIC ASSETS -->
  <!-- ============================================================================ -->
  <ItemGroup Label="Application Assets">
    <Content Include="HubServer.ico" />
  </ItemGroup>

  <!-- ============================================================================ -->
  <!-- PACKAGE REFERENCES - SIGNALR -->
  <!-- ============================================================================ -->
  <ItemGroup Label="SignalR">
    <PackageReference Include="Microsoft.AspNet.SignalR.Client" />
    <PackageReference Include="Microsoft.AspNetCore.SignalR.Client.Core" />
    <PackageReference Include="Microsoft.AspNetCore.SignalR.Client" />
  </ItemGroup>

  <!-- ============================================================================ -->
  <!-- PACKAGE REFERENCES - CORE FRAMEWORK -->
  <!-- ============================================================================ -->
  <ItemGroup Label="Core Framework">
    <PackageReference Include="Microsoft.Extensions.Hosting" />
  </ItemGroup>

  <!-- ============================================================================ -->
  <!-- PACKAGE REFERENCES - LOGGING -->
  <!-- ============================================================================ -->
  <ItemGroup Label="Logging">
    <PackageReference Include="Serilog.Enrichers.Demystifier" />
  </ItemGroup>

  <!-- ============================================================================ -->
  <!-- PROJECT REFERENCES -->
  <!-- ============================================================================ -->
  <ItemGroup Label="Infrastructure References">
    <ProjectReference Include="..\IndTrace.Dependencies\IndTrace.Dependencies.csproj" />
  </ItemGroup>

  <!-- ============================================================================ -->
  <!-- LAUNCH SETTINGS -->
  <!-- ============================================================================ -->
  <ItemGroup Label="Development Configuration">
    <None Update="Properties\launchSettings.json">
      <CopyToOutputDirectory>Never</CopyToOutputDirectory>
      <ExcludeFromSingleFile>true</ExcludeFromSingleFile>
      <CopyToPublishDirectory>Never</CopyToPublishDirectory>
    </None>
  </ItemGroup>

</Project>



Step 2: Please make sure all projects all classes all public class and methods and properties have xml comments

Step 3: Please make sure all projects all classes all public class and methods and properties have unit testing coverage

Step 4: Please Read the ExxerAI Intelligence System Design and made comparation against the code implementation 

Step 5: Update the ExxerAI MCP Server Implementation Report

Step 6 : Make a detailed plans to complete the implementation o add new features acording to the design document 

Step 7: implement this features, with all the good practices

Step 8: Do a due diligence check, be sure all projects are compiling, the test are passing, the documentation is complete, the coverage is good

Step 9: Optional Do a Stryker test and, Analyze the results, and implement the recomendations 

Step 10:  Update the ExxerAI MCP Server Implementation Report and go to the Step 2
 


