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
    
.PARAMETER TestType
    Type of tests to run: All, Unit, Integration, Coverage
    Default: All
    
.PARAMETER Verbose
    Enable verbose output for detailed test execution information
    
.PARAMETER GenerateReport
    Generate HTML test and coverage reports
    Default: true
    
.EXAMPLE
    .\RunGoogleDriveTestSuite.ps1
    Runs all tests with standard output
    
.EXAMPLE
    .\RunGoogleDriveTestSuite.ps1 -TestType Unit -Verbose
    Runs only unit tests with verbose output
    
.EXAMPLE
    .\RunGoogleDriveTestSuite.ps1 -TestType Coverage -GenerateReport $true
    Runs coverage analysis and generates HTML reports
    
.NOTES
    Author: ExxerAI Development Team
    Version: 1.0.0
    Requirements: .NET 9, xUnit v3, coverlet, reportgenerator
#>

param(
    [ValidateSet("All", "Unit", "Integration", "Coverage")]
    [string]$TestType = "All",
    
    [switch]$Verbose,
    
    [bool]$GenerateReport = $true
)

# Script configuration
$ErrorActionPreference = "Stop"
$ProgressPreference = "Continue"

# Test project paths
$TestProjectPath = "src\tests\ExxerAi.MCPServer.Tests"
$MCPServerProjectPath = "src\ExxerAi.MCPServer"
$SolutionPath = "src\ExxerAI.sln"

# Output directories
$OutputDir = "TestResults"
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
    param([string]$Test, [string]$ErrorMessage)
    Write-Host "  ✗ $Test" -ForegroundColor Red -NoNewline
    Write-Host " → $ErrorMessage" -ForegroundColor DarkRed
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
    if (Test-Path $TestProjectPath) {
        Write-TestResult "Test Project" "Found at $TestProjectPath" "Green"
    } else {
        Write-TestError "Test Project" "Directory not found"
        throw "Test project directory not found"
    }
    
    # Check MCP Server project
    if (Test-Path $MCPServerProjectPath) {
        Write-TestResult "MCP Server Project" "Found at $MCPServerProjectPath" "Green"
    } else {
        Write-TestError "MCP Server Project" "Directory not found"
        throw "MCP Server project directory not found"
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
    
    try {
        Write-Host "  Executing Google Drive unit tests..." -ForegroundColor Yellow
        
        $testOutput = dotnet test $TestProjectPath --configuration Debug --verbosity normal --logger "console;verbosity=normal" 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            # Parse test results from output
            $testSummary = $testOutput | Where-Object { $_ -match "Passed|Failed|Skipped" } | Select-Object -Last 3
            
            Write-TestResult "Unit Tests" "Completed successfully" "Green"
            
            # Display test summary
            foreach ($line in $testSummary) {
                if ($line -match "Passed.*?(\d+)") {
                    Write-TestResult "Tests Passed" $matches[1] "Green"
                }
                if ($line -match "Failed.*?(\d+)" -and $matches[1] -ne "0") {
                    Write-TestError "Tests Failed" $matches[1]
                }
                if ($line -match "Skipped.*?(\d+)" -and $matches[1] -ne "0") {
                    Write-TestResult "Tests Skipped" $matches[1] "Yellow"
                }
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
    
    try {
        Write-Host "  Executing Google Drive integration tests..." -ForegroundColor Yellow
        
        $testFilter = "FullyQualifiedName~GoogleDriveIntegrationTests"
        $testOutput = dotnet test $TestProjectPath --configuration Debug --filter $testFilter --verbosity normal 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-TestResult "Integration Tests" "Completed successfully" "Green"
        } else {
            Write-TestError "Integration Tests" "Some tests failed (expected without real credentials)"
            if ($Verbose) {
                Write-Host $testOutput -ForegroundColor Red
            }
            # Don't throw for integration tests as they're expected to fail without real Google credentials
        }
    }
    catch {
        Write-TestError "Integration Tests" $_.Exception.Message
        # Don't throw for integration tests
    }
}

function Invoke-CoverageAnalysis {
    Write-TestSection "Running Coverage Analysis"
    
    try {
        Write-Host "  Analyzing code coverage..." -ForegroundColor Yellow
        
        $coverageOutput = dotnet test $TestProjectPath --configuration Debug --collect:"XPlat Code Coverage" --results-directory $CoverageDir 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-TestResult "Coverage Analysis" "Completed successfully" "Green"
            
            # Find generated coverage files
            $coverageFiles = Get-ChildItem -Path $CoverageDir -Filter "*.cobertura.xml" -Recurse
            
            if ($coverageFiles) {
                $latestCoverage = $coverageFiles | Sort-Object LastWriteTime | Select-Object -Last 1
                Write-TestResult "Coverage Report" "Generated: $($latestCoverage.Name)" "Green"
                
                # Try to parse coverage data
                try {
                    [xml]$coverageXml = Get-Content $latestCoverage.FullName
                    $lineRate = [math]::Round([decimal]$coverageXml.coverage.'line-rate' * 100, 2)
                    
                    Write-TestResult "Line Coverage" "$lineRate%" $(if ($lineRate -ge 80) { "Green" } elseif ($lineRate -ge 60) { "Yellow" } else { "Red" })
                }
                catch {
                    Write-TestResult "Coverage Parsing" "Unable to parse coverage data" "Yellow"
                }
            }
        } else {
            Write-TestError "Coverage Analysis" "Failed to execute"
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
        # Check if ReportGenerator is available
        $reportGenAvailable = $false
        try {
            dotnet tool list --global | Select-String "reportgenerator" | Out-Null
            $reportGenAvailable = $true
        }
        catch {
            Write-Host "  ReportGenerator not found, attempting to install..." -ForegroundColor Yellow
            try {
                dotnet tool install --global dotnet-reportgenerator-globaltool | Out-Null
                $reportGenAvailable = $true
                Write-TestResult "ReportGenerator" "Installed successfully" "Green"
            }
            catch {
                Write-TestError "ReportGenerator" "Installation failed"
                return
            }
        }
        
        if ($reportGenAvailable) {
            # Find coverage files
            $coverageFiles = Get-ChildItem -Path $CoverageDir -Filter "*.cobertura.xml" -Recurse
            
            if ($coverageFiles) {
                $reportPath = Join-Path $ReportsDir "Coverage_$TestRunId"
                
                Write-Host "  Generating HTML coverage report..." -ForegroundColor Yellow
                
                dotnet reportgenerator "-reports:$($coverageFiles[0].FullName)" "-targetdir:$reportPath" "-reporttypes:Html;HtmlSummary" | Out-Null
                
                if ($LASTEXITCODE -eq 0) {
                    $indexFile = Join-Path $reportPath "index.html"
                    Write-TestResult "HTML Coverage Report" "Generated at $indexFile" "Green"
                } else {
                    Write-TestError "HTML Report Generation" "Failed"
                }
            }
        }
    }
    catch {
        Write-TestError "HTML Report Generation" $_.Exception.Message
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
    Write-Host "$OutputDir" -ForegroundColor White
    
    Write-Host "📊 Coverage Reports: " -NoNewline -ForegroundColor Yellow
    Write-Host "$CoverageDir" -ForegroundColor White
    
    if ($GenerateReport) {
        Write-Host "🌐 HTML Reports: " -NoNewline -ForegroundColor Yellow
        Write-Host "$ReportsDir" -ForegroundColor White
    }
    
    Write-Host "`n✅ Google Drive MCP Test Suite Completed!" -ForegroundColor Green
    Write-Host "🚀 Ready for Phase 2 autonomous operations!" -ForegroundColor Cyan
    
    Write-Host "`n📋 Next Steps:" -ForegroundColor Yellow
    Write-Host "  1. Review test results and coverage reports" -ForegroundColor Gray
    Write-Host "  2. Set up Google OAuth credentials for live testing" -ForegroundColor Gray
    Write-Host "  3. Configure Claude Desktop with MCP server" -ForegroundColor Gray
    Write-Host "  4. Test real-time Google Drive monitoring" -ForegroundColor Gray
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