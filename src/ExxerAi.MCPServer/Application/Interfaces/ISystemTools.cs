namespace ExxerAi.MCPServer.Application.Interfaces;

/// <summary>
/// Interface for system operations and health monitoring MCP tools
/// </summary>
public interface ISystemTools
{
	/// <summary>
	/// Gets comprehensive system information
	/// </summary>
	/// <returns>System information including OS, hardware, and runtime details</returns>
	Task<string> GetSystemInfoAsync();

	/// <summary>
	/// Gets the current date and time in various formats
	/// </summary>
	/// <param name="format">Time format (iso, readable, timestamp, utc)</param>
	/// <param name="timezone">Timezone for display (optional)</param>
	/// <returns>Current time in the specified format</returns>
	Task<string> GetCurrentTimeAsync(string format = "readable", string timezone = "");

	/// <summary>
	/// Lists files and directories in a specified path
	/// </summary>
	/// <param name="path">Directory path to list</param>
	/// <param name="includeHidden">Include hidden files and directories</param>
	/// <param name="maxItems">Maximum number of items to return</param>
	/// <returns>Directory listing with file information</returns>
	Task<string> ListFilesAsync(string path = ".", bool includeHidden = false, int maxItems = 50);

	/// <summary>
	/// Performs a mathematical calculation safely
	/// </summary>
	/// <param name="expression">Mathematical expression to evaluate</param>
	/// <returns>Calculation result</returns>
	Task<string> CalculateAsync(string expression);

	/// <summary>
	/// Gets memory usage information
	/// </summary>
	/// <returns>Memory usage statistics</returns>
	Task<string> GetMemoryUsageAsync();

	/// <summary>
	/// Checks the health status of the MCP server
	/// </summary>
	/// <returns>Health status information</returns>
	Task<string> CheckHealthAsync();
} 