using ExxerAI.Domain;
using ExxerAI.Domain.Operations;

namespace ExxerAI.MCPServer.Application.Interfaces;

/// <summary>
/// Interface for system operations and health monitoring MCP tools
/// </summary>
public interface ISystemTools
{
	/// <summary>
	/// Gets comprehensive system information
	/// </summary>
	/// <returns>Result containing system information including OS, hardware, and runtime details</returns>
	Task<Result<string>> GetSystemInfoAsync();

	/// <summary>
	/// Gets the current date and time in various formats
	/// </summary>
	/// <param name="format">Time format (iso, readable, timestamp, utc)</param>
	/// <param name="timezone">Timezone for display (optional)</param>
	/// <returns>Result containing current time in the specified format</returns>
	Task<Result<string>> GetCurrentTimeAsync(string format = "readable", string timezone = "");

	/// <summary>
	/// Lists files and directories in a specified path
	/// </summary>
	/// <param name="path">Directory path to list</param>
	/// <param name="includeHidden">Include hidden files and directories</param>
	/// <param name="maxItems">Maximum number of items to return</param>
	/// <returns>Result containing directory listing with file information</returns>
	Task<Result<string>> ListFilesAsync(string path = ".", bool includeHidden = false, int maxItems = 50);

	/// <summary>
	/// Performs a mathematical calculation safely
	/// </summary>
	/// <param name="expression">Mathematical expression to evaluate</param>
	/// <returns>Result containing calculation result</returns>
	Task<Result<string>> CalculateAsync(string expression);

	/// <summary>
	/// Gets memory usage information
	/// </summary>
	/// <returns>Result containing memory usage statistics</returns>
	Task<Result<string>> GetMemoryUsageAsync();

	/// <summary>
	/// Checks the health agentStatus of the MCP server
	/// </summary>
	/// <returns>Result containing health agentStatus information</returns>
	Task<Result<string>> CheckHealthAsync();
} 