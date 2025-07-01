using ExxerAI.Domain;

namespace ExxerAi.MCPServer.Application.Interfaces;

/// <summary>
/// Interface for Google Drive MCP tools providing document monitoring and processing capabilities
/// </summary>
public interface IGoogleDriveTools
{
	/// <summary>
	/// Starts watching a Google Drive folder for document changes
	/// </summary>
	/// <param name="folderId">The Google Drive folder ID to watch</param>
	/// <param name="includeSubdirectories">Whether to include subdirectories</param>
	/// <param name="autoProcess">Whether to automatically process detected files</param>
	/// <param name="pollingIntervalSeconds">Polling interval in seconds</param>
	/// <returns>Result containing watch session information</returns>
	Task<Result<string>> StartFolderWatchAsync(string folderId, bool includeSubdirectories = true, bool autoProcess = true, int pollingIntervalSeconds = 60);

	/// <summary>
	/// Gets document changes for a specific watch session
	/// </summary>
	/// <param name="watchId">The watch session identifier</param>
	/// <returns>Result containing document changes detected</returns>
	Task<Result<string>> GetDocumentChangesAsync(string watchId);

	/// <summary>
	/// Downloads a document from Google Drive
	/// </summary>
	/// <param name="documentId">The document identifier</param>
	/// <returns>Result containing download result information</returns>
	Task<Result<string>> DownloadDocumentAsync(string documentId);

	/// <summary>
	/// Gets metadata for a specific document
	/// </summary>
	/// <param name="documentId">The document identifier</param>
	/// <returns>Result containing document metadata information</returns>
	Task<Result<string>> GetDocumentMetadataAsync(string documentId);

	/// <summary>
	/// Checks the health status of the MCP server and Google Drive integration
	/// </summary>
	/// <returns>Result containing health status information</returns>
	Task<Result<string>> CheckHealthStatusAsync();

	/// <summary>
	/// Lists all active watch sessions
	/// </summary>
	/// <returns>Result containing active watch sessions information</returns>
	Task<Result<string>> GetActiveWatchesAsync();

	/// <summary>
	/// Stops watching a specific folder
	/// </summary>
	/// <param name="watchId">The watch session identifier</param>
	/// <returns>Result containing stop operation result</returns>
	Task<Result<string>> StopWatchingAsync(string watchId);
} 