#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Comprehensive Test Suite Runner for Google Drive MCP Integration
    
.DESCRIPTION
    Executes the complete test suite for ExxerAI Google Drive MCP integration including:
    - Unit tests for GoogleDriveService
    - Unit tests for GoogleDriveTools (MCP layer)
    - Integration tests for end-to-end workflows
    - Coverage analysis and reporting
    - Performance benchmarks
    
.PARAMETER TestType
    Type of tests to run: All, Unit, Integration, Coverage, Performance
    Default: All
    
.PARAMETER Verbose
    Enable verbose output for detailed test execution information
    
.PARAMETER GenerateReport
    Generate HTML test and coverage reports
    Default: true
    
.PARAMETER IncludePerformance
    Include performance benchmarks in test execution
    Default: false
    
.EXAMPLE
    .\RunTestSuite.ps1
    Runs all tests with standard output
    
.EXAMPLE
    .\RunTestSuite.ps1 -TestType Unit -Verbose
    Runs only unit tests with verbose output
    
.EXAMPLE
    .\RunTestSuite.ps1 -TestType Coverage -GenerateReport $true
    Runs coverage analysis and generates HTML reports
    
.NOTES
    Author: ExxerAI Development Team
    Version: 1.0.0
    Requirements: .NET 9, xUnit v3, coverlet, reportgenerator
#>

param(
    [ValidateSet("All", "Unit", "Integration", "Coverage", "Performance")]
    [string]$TestType = "All",
    
    [switch]$Verbose,
    
    [bool]$GenerateReport = $true,
    
    [bool]$IncludePerformance = $false
)

# Script configuration
$ErrorActionPreference = "Stop"
$ProgressPreference = "Continue"

# Test project paths
$TestProjectPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$MCPServerProjectPath = "..\..\ExxerAi.MCPServer\ExxerAi.MCPServer.csproj"
$SolutionPath = "..\..\ExxerAI.sln"

# Output directories
$OutputDir = Join-Path $TestProjectPath "TestResults"
$CoverageDir = Join-Path $OutputDir "Coverage"
$ReportsDir = Join-Path $OutputDir "Reports"

# Ensure directories exist
@($OutputDir, $CoverageDir, $ReportsDir) | ForEach-Object {
    if (-not (Test-Path $_)) {
        New-Item -Path $_ -ItemType Directory -Force | Out-Null
    }
}

# Test execution timestamp
$TestRunId = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
$TestRunDir = Join-Path $OutputDir "TestRun_$TestRunId"
New-Item -Path $TestRunDir -ItemType Directory -Force | Out-Null

# Logging functions
function Write-TestHeader {
    param([string]$Title)
    Write-Host "`n" -NoNewline
    Write-Host "="*80 -ForegroundColor Cyan
    Write-Host " $Title" -ForegroundColor Yellow
    Write-Host "="*80 -ForegroundColor Cyan
    Write-Host ""
}

function Write-TestSection {
    param([string]$Section)
    Write-Host "`n🔹 $Section" -ForegroundColor Green
    Write-Host "-"*60 -ForegroundColor Gray
}

function Write-TestResult {
    param([string]$Test, [string]$Result, [string]$Color = "White")
    Write-Host "  ✓ $Test" -ForegroundColor $Color -NoNewline
    Write-Host " → $Result" -ForegroundColor Gray
}

function Write-TestError {
    param([string]$Test, [string]$Error)
    Write-Host "  ✗ $Test" -ForegroundColor Red -NoNewline
    Write-Host " → $Error" -ForegroundColor DarkRed
}

# Test execution functions
function Test-Prerequisites {
    Write-TestSection "Checking Prerequisites"
    
    # Check .NET version
    try {
        $dotnetVersion = dotnet --version
        Write-TestResult ".NET SDK" "Version $dotnetVersion" "Green"
    }
    catch {
        Write-TestError ".NET SDK" "Not found or not accessible"
        throw "Required .NET SDK not found"
    }
    
    # Check solution file
    if (Test-Path $SolutionPath) {
        Write-TestResult "Solution File" "Found at $SolutionPath" "Green"
    } else {
        Write-TestError "Solution File" "Not found at $SolutionPath"
        throw "Solution file not found"
    }
    
    # Check test project
    if (Test-Path (Join-Path $TestProjectPath "ExxerAi.MCPServer.Tests.csproj")) {
        Write-TestResult "Test Project" "Found and accessible" "Green"
    } else {
        Write-TestError "Test Project" "Project file not found"
        throw "Test project file not found"
    }
    
    # Check MCP Server project
    if (Test-Path $MCPServerProjectPath) {
        Write-TestResult "MCP Server Project" "Found and accessible" "Green"
    } else {
        Write-TestError "MCP Server Project" "Project file not found"
        throw "MCP Server project file not found"
    }
}

function Invoke-BuildSolution {
    Write-TestSection "Building Solution"
    
    try {
        Write-Host "  Building solution..." -ForegroundColor Yellow
        $buildOutput = dotnet build $SolutionPath --configuration Debug --verbosity minimal 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-TestResult "Solution Build" "Success" "Green"
        } else {
            Write-TestError "Solution Build" "Failed"
            Write-Host $buildOutput -ForegroundColor Red
            throw "Solution build failed"
        }
    }
    catch {
        Write-TestError "Solution Build" $_.Exception.Message
        throw
    }
}

function Invoke-UnitTests {
    Write-TestSection "Running Unit Tests"
    
    $testFilter = "FullyQualifiedName~GoogleDriveServiceTests|FullyQualifiedName~GoogleDriveToolsTests"
    if ($TestType -eq "Unit") {
        $testFilter += "&Category!=Integration"
    }
    
    try {
        Write-Host "  Executing unit tests..." -ForegroundColor Yellow
        
        $testCommand = @(
            "test"
            $TestProjectPath
            "--configuration", "Debug"
            "--filter", $testFilter
            "--logger", "trx;LogFileName=unit-tests-$TestRunId.trx"
            "--results-directory", $TestRunDir
            "--verbosity", $(if ($Verbose) { "detailed" } else { "normal" })
        )
        
        $testOutput = & dotnet @testCommand 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            # Parse test results
            $passed = ($testOutput | Select-String "Passed:.*?(\d+)" | ForEach-Object { $_.Matches[0].Groups[1].Value }) -join ""
            $failed = ($testOutput | Select-String "Failed:.*?(\d+)" | ForEach-Object { $_.Matches[0].Groups[1].Value }) -join ""
            $skipped = ($testOutput | Select-String "Skipped:.*?(\d+)" | ForEach-Object { $_.Matches[0].Groups[1].Value }) -join ""
            
            Write-TestResult "Unit Tests Passed" "$passed tests" "Green"
            if ($failed -and $failed -ne "0") {
                Write-TestError "Unit Tests Failed" "$failed tests"
            }
            if ($skipped -and $skipped -ne "0") {
                Write-TestResult "Unit Tests Skipped" "$skipped tests" "Yellow"
            }
        } else {
            Write-TestError "Unit Tests" "Execution failed"
            if ($Verbose) {
                Write-Host $testOutput -ForegroundColor Red
            }
            throw "Unit tests failed"
        }
    }
    catch {
        Write-TestError "Unit Tests" $_.Exception.Message
        throw
    }
}

function Invoke-IntegrationTests {
    Write-TestSection "Running Integration Tests"
    
    $testFilter = "FullyQualifiedName~GoogleDriveIntegrationTests"
    
    try {
        Write-Host "  Executing integration tests..." -ForegroundColor Yellow
        
        $testCommand = @(
            "test"
            $TestProjectPath
            "--configuration", "Debug"
            "--filter", $testFilter
            "--logger", "trx;LogFileName=integration-tests-$TestRunId.trx"
            "--results-directory", $TestRunDir
            "--verbosity", $(if ($Verbose) { "detailed" } else { "normal" })
        )
        
        $testOutput = & dotnet @testCommand 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            # Parse test results
            $passed = ($testOutput | Select-String "Passed:.*?(\d+)" | ForEach-Object { $_.Matches[0].Groups[1].Value }) -join ""
            $failed = ($testOutput | Select-String "Failed:.*?(\d+)" | ForEach-Object { $_.Matches[0].Groups[1].Value }) -join ""
            
            Write-TestResult "Integration Tests Passed" "$passed tests" "Green"
            if ($failed -and $failed -ne "0") {
                Write-TestError "Integration Tests Failed" "$failed tests"
            }
        } else {
            Write-TestError "Integration Tests" "Execution failed"
            if ($Verbose) {
                Write-Host $testOutput -ForegroundColor Red
            }
            throw "Integration tests failed"
        }
    }
    catch {
        Write-TestError "Integration Tests" $_.Exception.Message
        throw
    }
}

function Invoke-CoverageAnalysis {
    Write-TestSection "Running Coverage Analysis"
    
    try {
        Write-Host "  Analyzing code coverage..." -ForegroundColor Yellow
        
        $coverageFile = Join-Path $CoverageDir "coverage-$TestRunId.cobertura.xml"
        
        $coverageCommand = @(
            "test"
            $TestProjectPath
            "--configuration", "Debug"
            "--collect", "XPlat Code Coverage"
            "--results-directory", $CoverageDir
            "--verbosity", "minimal"
            "--"
            "DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura"
        )
        
        $coverageOutput = & dotnet @coverageCommand 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-TestResult "Coverage Analysis" "Completed successfully" "Green"
            
            # Find the generated coverage file
            $generatedCoverage = Get-ChildItem -Path $CoverageDir -Filter "*.cobertura.xml" -Recurse | Sort-Object LastWriteTime | Select-Object -Last 1
            
            if ($generatedCoverage) {
                Copy-Item $generatedCoverage.FullName $coverageFile -Force
                Write-TestResult "Coverage Report" "Saved to $coverageFile" "Green"
                
                # Parse coverage percentage
                if (Test-Path $coverageFile) {
                    try {
                        [xml]$coverageXml = Get-Content $coverageFile
                        $lineRate = [math]::Round([decimal]$coverageXml.coverage.'line-rate' * 100, 2)
                        $branchRate = [math]::Round([decimal]$coverageXml.coverage.'branch-rate' * 100, 2)
                        
                        Write-TestResult "Line Coverage" "$lineRate%" $(if ($lineRate -ge 80) { "Green" } elseif ($lineRate -ge 60) { "Yellow" } else { "Red" })
                        Write-TestResult "Branch Coverage" "$branchRate%" $(if ($branchRate -ge 80) { "Green" } elseif ($branchRate -ge 60) { "Yellow" } else { "Red" })
                    }
                    catch {
                        Write-TestResult "Coverage Parsing" "Unable to parse coverage data" "Yellow"
                    }
                }
            } else {
                Write-TestError "Coverage Report" "Coverage file not generated"
            }
        } else {
            Write-TestError "Coverage Analysis" "Failed to execute"
            if ($Verbose) {
                Write-Host $coverageOutput -ForegroundColor Red
            }
        }
    }
    catch {
        Write-TestError "Coverage Analysis" $_.Exception.Message
    }
}

function New-HtmlReports {
    if (-not $GenerateReport) {
        return
    }
    
    Write-TestSection "Generating HTML Reports"
    
    try {
        # Install ReportGenerator if not available
        Write-Host "  Checking ReportGenerator tool..." -ForegroundColor Yellow
        $reportGenCheck = dotnet tool list --global | Select-String "reportgenerator"
        
        if (-not $reportGenCheck) {
            Write-Host "  Installing ReportGenerator..." -ForegroundColor Yellow
            dotnet tool install --global dotnet-reportgenerator-globaltool | Out-Null
        }
        
        # Find coverage files
        $coverageFiles = Get-ChildItem -Path $CoverageDir -Filter "*.cobertura.xml" -Recurse
        
        if ($coverageFiles) {
            Write-Host "  Generating HTML coverage report..." -ForegroundColor Yellow
            
            $reportPath = Join-Path $ReportsDir "Coverage_$TestRunId"
            
            $reportCommand = @(
                "reportgenerator"
                "-reports:$($coverageFiles[0].FullName)"
                "-targetdir:$reportPath"
                "-reporttypes:Html;HtmlSummary"
                "-verbosity:Info"
            )
            
            & dotnet @reportCommand | Out-Null
            
            if ($LASTEXITCODE -eq 0) {
                $indexFile = Join-Path $reportPath "index.html"
                Write-TestResult "HTML Coverage Report" "Generated at $indexFile" "Green"
                
                if ($IsWindows) {
                    Write-Host "  Opening coverage report in browser..." -ForegroundColor Yellow
                    Start-Process $indexFile
                }
            } else {
                Write-TestError "HTML Report Generation" "Failed"
            }
        } else {
            Write-TestError "HTML Report Generation" "No coverage files found"
        }
    }
    catch {
        Write-TestError "HTML Report Generation" $_.Exception.Message
    }
}

function Invoke-PerformanceBenchmarks {
    if (-not $IncludePerformance) {
        return
    }
    
    Write-TestSection "Running Performance Benchmarks"
    
    try {
        Write-Host "  Executing performance tests..." -ForegroundColor Yellow
        
        # Performance tests would be implemented separately
        # This is a placeholder for future performance testing
        
        Write-TestResult "Performance Benchmarks" "Not implemented yet" "Yellow"
    }
    catch {
        Write-TestError "Performance Benchmarks" $_.Exception.Message
    }
}

function Write-TestSummary {
    param([datetime]$StartTime)
    
    $EndTime = Get-Date
    $Duration = $EndTime - $StartTime
    
    Write-TestHeader "TEST EXECUTION SUMMARY"
    
    Write-Host "🕐 Execution Time: " -NoNewline -ForegroundColor Yellow
    Write-Host "$($Duration.ToString('hh\:mm\:ss'))" -ForegroundColor White
    
    Write-Host "📁 Test Results: " -NoNewline -ForegroundColor Yellow
    Write-Host "$TestRunDir" -ForegroundColor White
    
    Write-Host "📊 Coverage Reports: " -NoNewline -ForegroundColor Yellow
    Write-Host "$CoverageDir" -ForegroundColor White
    
    if ($GenerateReport) {
        Write-Host "🌐 HTML Reports: " -NoNewline -ForegroundColor Yellow
        Write-Host "$ReportsDir" -ForegroundColor White
    }
    
    Write-Host "`n✅ Google Drive MCP Test Suite Completed Successfully!" -ForegroundColor Green
    Write-Host "Ready for Phase 2 autonomous operations! 🚀" -ForegroundColor Cyan
}

# Main execution
try {
    $StartTime = Get-Date
    
    Write-TestHeader "EXXERAI GOOGLE DRIVE MCP - COMPREHENSIVE TEST SUITE"
    Write-Host "🎯 Test Type: $TestType" -ForegroundColor Cyan
    Write-Host "📅 Started: $($StartTime.ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor Cyan
    Write-Host "🆔 Run ID: $TestRunId" -ForegroundColor Cyan
    
    # Execute test phases
    Test-Prerequisites
    Invoke-BuildSolution
    
    switch ($TestType) {
        "All" {
            Invoke-UnitTests
            Invoke-IntegrationTests
            Invoke-CoverageAnalysis
            New-HtmlReports
            Invoke-PerformanceBenchmarks
        }
        "Unit" {
            Invoke-UnitTests
        }
        "Integration" {
            Invoke-IntegrationTests
        }
        "Coverage" {
            Invoke-UnitTests
            Invoke-IntegrationTests
            Invoke-CoverageAnalysis
            New-HtmlReports
        }
        "Performance" {
            Invoke-PerformanceBenchmarks
        }
    }
    
    Write-TestSummary $StartTime
}
catch {
    Write-Host "`n❌ TEST SUITE FAILED" -ForegroundColor Red
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($Verbose) {
        Write-Host "`nStack Trace:" -ForegroundColor Red
        Write-Host $_.Exception.StackTrace -ForegroundColor DarkRed
    }
    
    exit 1
} 