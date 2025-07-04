using System.ComponentModel;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ExxerAI.Domain;

using ExxerAi.MCPServer.Application.Interfaces;
using ModelContextProtocol.Server;

namespace ExxerAi.MCPServer.Application.Tools.System;

/// <summary>
/// MCP tools for system utilities and health monitoring
/// Provides basic system information, time, and file operations
/// </summary>
[McpServerToolType]
public class SystemTools : ISystemTools
{
    private readonly ILogger<SystemTools> _logger;

    /// <summary>
    /// Initializes a new instance of the SystemTools class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public SystemTools(ILogger<SystemTools> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets comprehensive system information
    /// </summary>
    /// <returns>System information including OS, hardware, and runtime details</returns>
    [McpServerTool, Description("Gets comprehensive system information including OS, hardware, and runtime details")]
    public Task<Result<string>> GetSystemInfoAsync()
    {
        _logger.LogInformation("Getting system information");

        try
        {
            var process = Process.GetCurrentProcess();
            var osVersion = Environment.OSVersion;
            var runtimeVersion = RuntimeInformation.FrameworkDescription;

            var systemInfo = $"🖥️ System Information:\n" +
                            $"💻 OS: {osVersion.Platform} {osVersion.Version}\n" +
                            $"🏗️ Architecture: {RuntimeInformation.OSArchitecture}\n" +
                            $"⚙️ Runtime: {runtimeVersion}\n" +
                            $"🔧 Process: {process.ProcessName} (PID: {process.Id})\n" +
                            $"🧠 Memory: {GC.GetTotalMemory(false) / 1024 / 1024:F2} MB\n" +
                            $"⏰ Uptime: {DateTime.UtcNow - process.StartTime.ToUniversalTime():dd\\.hh\\:mm\\:ss}\n" +
                            $"🖥️ Machine: {Environment.MachineName}\n" +
                            $"👤 User: {Environment.UserName}\n" +
                            $"📂 Working Dir: {Environment.CurrentDirectory}";

            _logger.LogInformation("System information retrieved successfully");
            return Task.FromResult(Result<string>.WithSuccess(systemInfo));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system information");
            return Task.FromResult(Result<string>.WithFailure($"Error getting system information: {ex.Message}"));
        }
    }

    /// <summary>
    /// Gets the current date and time in various formats
    /// </summary>
    /// <param name="format">Time format (iso, readable, timestamp, utc)</param>
    /// <param name="timezone">Timezone for display (optional)</param>
    /// <returns>Current time in the specified format</returns>
    [McpServerTool, Description("Gets current date and time in various formats")]
    public Task<Result<string>> GetCurrentTimeAsync(
        [Description("Time format: iso, readable, timestamp, utc")] string format = "readable",
        [Description("Timezone for display (optional)")] string timezone = "")
    {
        _logger.LogInformation("Getting current time in format {Format}", format);

        try
        {
            var now = DateTime.UtcNow;
            var localNow = DateTime.Now;

            var timeInfo = format.ToLowerInvariant() switch
            {
                "iso" => $"🕐 Current Time (ISO): {now:yyyy-MM-ddTHH:mm:ss.fffZ}",
                "timestamp" => $"🕐 Current Time (Timestamp): {((DateTimeOffset)now).ToUnixTimeSeconds()}",
                "utc" => $"🕐 Current Time (UTC): {now:yyyy-MM-dd HH:mm:ss} UTC",
                _ => $"🕐 Current Time:\n" +
                     $"📅 Local: {localNow:yyyy-MM-dd HH:mm:ss}\n" +
                     $"🌍 UTC: {now:yyyy-MM-dd HH:mm:ss} UTC\n" +
                     $"📍 Timezone: {TimeZoneInfo.Local.DisplayName}"
            };

            if (!string.IsNullOrWhiteSpace(timezone))
            {
                try
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(timezone);
                    var tzTime = TimeZoneInfo.ConvertTimeFromUtc(now, tz);
                    timeInfo += $"\n🌏 {timezone}: {tzTime:yyyy-MM-dd HH:mm:ss}";
                }
                catch
                {
                    timeInfo += $"\n❌ Invalid timezone: {timezone}";
                }
            }

            _logger.LogInformation("Current time retrieved in format {Format}", format);
            return Task.FromResult(Result<string>.WithSuccess(timeInfo));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current time");
            return Task.FromResult(Result<string>.WithFailure($"Error getting current time: {ex.Message}"));
        }
    }

    /// <summary>
    /// Lists files and directories in a specified path
    /// </summary>
    /// <param name="path">Directory path to list</param>
    /// <param name="includeHidden">Include hidden files and directories</param>
    /// <param name="maxItems">Maximum number of items to return</param>
    /// <returns>Directory listing with file information</returns>
    [McpServerTool, Description("Lists files and directories in a specified path")]
    public Task<Result<string>> ListFilesAsync(
        [Description("Directory path to list")] string path = ".",
        [Description("Include hidden files and directories")] bool includeHidden = false,
        [Description("Maximum number of items to return")] int maxItems = 50)
    {
        _logger.LogInformation("Listing files in directory {Path}", path);

        try
        {
            if (!Directory.Exists(path))
            {
                return Task.FromResult(Result<string>.WithFailure($"Directory not found: {path}"));
            }

            var directoryInfo = new DirectoryInfo(path);
            var entries = new List<string>();

            // Get directories
            var directories = directoryInfo.GetDirectories()
                .Where(d => includeHidden || !d.Attributes.HasFlag(FileAttributes.Hidden))
                .Take(maxItems / 2)
                .OrderBy(d => d.Name);

            foreach (var dir in directories)
            {
                entries.Add($"📁 {dir.Name}/ (Modified: {dir.LastWriteTime:yyyy-MM-dd HH:mm})");
            }

            // Get files
            var files = directoryInfo.GetFiles()
                .Where(f => includeHidden || !f.Attributes.HasFlag(FileAttributes.Hidden))
                .Take(maxItems - entries.Count)
                .OrderBy(f => f.Name);

            foreach (var file in files)
            {
                var sizeKB = file.Length / 1024.0;
                var sizeStr = sizeKB < 1024 ? $"{sizeKB:F1} KB" : $"{sizeKB / 1024:F1} MB";
                entries.Add($"📄 {file.Name} ({sizeStr}, Modified: {file.LastWriteTime:yyyy-MM-dd HH:mm})");
            }

            var result = $"📂 Directory Listing: {Path.GetFullPath(path)}\n" +
                         $"📊 Items: {entries.Count} (Max: {maxItems})\n\n" +
                         string.Join("\n", entries);

            if (entries.Count >= maxItems)
            {
                result += $"\n\n⚠️ Listing truncated to {maxItems} items";
            }

            _logger.LogInformation("Listed {Count} items in directory {Path}", entries.Count, path);
            return Task.FromResult(Result<string>.WithSuccess(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files in directory {Path}", path);
            return Task.FromResult(Result<string>.WithFailure($"Error listing files: {ex.Message}"));
        }
    }

    /// <summary>
    /// Performs a mathematical calculation safely
    /// </summary>
    /// <param name="expression">Mathematical expression to evaluate</param>
    /// <returns>Calculation result</returns>
    [McpServerTool, Description("Performs safe mathematical calculations")]
    public Task<Result<string>> CalculateAsync(
        [Description("Mathematical expression to evaluate (supports +, -, *, /, %, parentheses)")] string expression)
    {
        _logger.LogInformation("Calculating expression: {Expression}", expression);

        try
        {
            // Simple validation for safety
            var allowedChars = "0123456789+-*/.()% ";
            if (!expression.All(c => allowedChars.Contains(c)))
            {
                return Task.FromResult(Result<string>.WithFailure("Expression contains invalid characters. Only numbers, +, -, *, /, %, (), and spaces are allowed."));
            }

            if (string.IsNullOrWhiteSpace(expression))
            {
                return Task.FromResult(Result<string>.WithFailure("Expression cannot be empty"));
            }

            // Simple expression evaluation (production would use a proper parser)
            var result = EvaluateExpression(expression);

            var calculation = $"🧮 Calculation Result:\n" +
                             $"📝 Expression: {expression}\n" +
                             $"🔢 Result: {result}";

            _logger.LogInformation("Calculation completed: {Expression} = {Result}", expression, result);
            return Task.FromResult(Result<string>.WithSuccess(calculation));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating expression {Expression}", expression);
            return Task.FromResult(Result<string>.WithFailure($"Error calculating expression: {ex.Message}"));
        }
    }

    /// <summary>
    /// Gets memory usage information
    /// </summary>
    /// <returns>Memory usage statistics</returns>
    [McpServerTool, Description("Gets current memory usage information")]
    public Task<Result<string>> GetMemoryUsageAsync()
    {
        _logger.LogInformation("Getting memory usage information");

        try
        {
            var process = Process.GetCurrentProcess();
            var gcMemory = GC.GetTotalMemory(false);
            var workingSet = process.WorkingSet64;
            var privateMemory = process.PrivateMemorySize64;

            var memoryInfo = $"🧠 Memory Usage Information:\n" +
                            $"📊 GC Memory: {gcMemory / 1024 / 1024:F2} MB\n" +
                            $"🔧 Working Set: {workingSet / 1024 / 1024:F2} MB\n" +
                            $"🔒 Private Memory: {privateMemory / 1024 / 1024:F2} MB\n" +
                            $"🗑️ GC Collections (Gen 0): {GC.CollectionCount(0)}\n" +
                            $"🗑️ GC Collections (Gen 1): {GC.CollectionCount(1)}\n" +
                            $"🗑️ GC Collections (Gen 2): {GC.CollectionCount(2)}";

            _logger.LogInformation("Memory usage information retrieved");
            return Task.FromResult(Result<string>.WithSuccess(memoryInfo));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting memory usage information");
            return Task.FromResult(Result<string>.WithFailure($"Error getting memory usage: {ex.Message}"));
        }
    }

    /// <summary>
    /// Checks the health agentStatus of the MCP server
    /// </summary>
    /// <returns>Health agentStatus information</returns>
    [McpServerTool, Description("Checks the health agentStatus of the MCP server")]
    public Task<Result<string>> CheckHealthAsync()
    {
        _logger.LogInformation("Checking MCP server health");

        try
        {
            var process = Process.GetCurrentProcess();
            var uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime();
            var memoryUsage = GC.GetTotalMemory(false) / 1024 / 1024;

            var isHealthy = uptime.TotalMinutes > 0 && memoryUsage < 1000; // Basic health checks

            var healthStatus = $"🏥 MCP Server Health AgentStatus:\n" +
                              $"✅ AgentStatus: {(isHealthy ? "Healthy" : "Warning")}\n" +
                              $"⏰ Uptime: {uptime:dd\\.hh\\:mm\\:ss}\n" +
                              $"🧠 Memory: {memoryUsage:F2} MB\n" +
                              $"🔖 Version: 1.0.0\n" +
                              $"🕐 Check Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n" +
                              $"🌡️ AgentStatus: {(isHealthy ? "All systems operational" : "Performance degradation detected")}";

            _logger.LogInformation("Health check completed: {AgentStatus}", isHealthy ? "Healthy" : "Warning");
            return Task.FromResult(Result<string>.WithSuccess(healthStatus));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking server health");
            return Task.FromResult(Result<string>.WithFailure($"Error checking health: {ex.Message}"));
        }
    }

    /// <summary>
    /// Simple expression evaluator (basic implementation)
    /// </summary>
    /// <param name="expression">The expression to evaluate</param>
    /// <returns>The evaluation result</returns>
    private static double EvaluateExpression(string expression)
    {
        // This is a very basic implementation. Production code should use a proper expression parser.
        var dataTable = new System.Data.DataTable();
        var result = dataTable.Compute(expression, null);
        return Convert.ToDouble(result);
    }
}