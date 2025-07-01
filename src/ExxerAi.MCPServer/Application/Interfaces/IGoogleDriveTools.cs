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
	/// <returns>Watch session information</returns>
	Task<string> StartFolderWatchAsync(string folderId, bool includeSubdirectories = true, bool autoProcess = true, int pollingIntervalSeconds = 60);

	/// <summary>
	/// Gets document changes for a specific watch session
	/// </summary>
	/// <param name="watchId">The watch session identifier</param>
	/// <returns>Document changes detected</returns>
	Task<string> GetDocumentChangesAsync(string watchId);

	/// <summary>
	/// Downloads a document from Google Drive
	/// </summary>
	/// <param name="documentId">The document identifier</param>
	/// <returns>Download result information</returns>
	Task<string> DownloadDocumentAsync(string documentId);

	/// <summary>
	/// Gets metadata for a specific document
	/// </summary>
	/// <param name="documentId">The document identifier</param>
	/// <returns>Document metadata information</returns>
	Task<string> GetDocumentMetadataAsync(string documentId);

	/// <summary>
	/// Checks the health status of the MCP server and Google Drive integration
	/// </summary>
	/// <returns>Health status information</returns>
	Task<string> CheckHealthStatusAsync();

	/// <summary>
	/// Lists all active watch sessions
	/// </summary>
	/// <returns>Active watch sessions information</returns>
	Task<string> GetActiveWatchesAsync();

	/// <summary>
	/// Stops watching a specific folder
	/// </summary>
	/// <param name="watchId">The watch session identifier</param>
	/// <returns>Stop operation result</returns>
	Task<string> StopWatchingAsync(string watchId);
} 