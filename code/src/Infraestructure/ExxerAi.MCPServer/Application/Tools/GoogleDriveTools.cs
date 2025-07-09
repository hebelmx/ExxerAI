using System.ComponentModel;
using ExxerAi.MCPServer.Application.Interfaces;
using ExxerAI.Domain.Operations;
using ModelContextProtocol.Server;

namespace ExxerAi.MCPServer.Application.Tools;

/// <summary>
/// MCP tools for Google Drive integration implementing advanced document intelligence pipeline
/// Provides real-time document monitoring, processing, and change detection
/// </summary>
[McpServerToolType]
public class GoogleDriveTools : IGoogleDriveTools
{
    private readonly ILogger<GoogleDriveTools> _logger;
    private readonly IGoogleDriveService _googleDriveService;

    /// <summary>
    /// Initializes a new instance of the GoogleDriveTools class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="googleDriveService">The Google Drive service for real API integration</param>
    public GoogleDriveTools(ILogger<GoogleDriveTools> logger, IGoogleDriveService googleDriveService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _googleDriveService = googleDriveService ?? throw new ArgumentNullException(nameof(googleDriveService));
    }

    /// <summary>
    /// Starts watching a Google Drive folder for document changes
    /// </summary>
    /// <param name="folderId">The Google Drive folder ID to watch</param>
    /// <param name="includeSubdirectories">Whether to include subdirectories</param>
    /// <param name="autoProcess">Whether to automatically process detected files</param>
    /// <param name="pollingIntervalSeconds">Polling interval in seconds</param>
    /// <returns>Watch session information</returns>
    [McpServerTool, Description("Starts watching a Google Drive folder for document changes and automatically processes new files")]
    public async Task<Result<string>> StartFolderWatchAsync(
        [Description("The Google Drive folder ID to monitor")] string folderId,
        [Description("Include subdirectories in monitoring")] bool includeSubdirectories = true,
        [Description("Automatically process detected files")] bool autoProcess = true,
        [Description("Polling interval in seconds")] int pollingIntervalSeconds = 60)
    {
        _logger.LogInformation("🚀 Starting real Google Drive folder watch for folder {FolderId}", folderId);

        try
        {
            var result = await _googleDriveService.StartFolderWatchAsync(folderId, includeSubdirectories, autoProcess, pollingIntervalSeconds);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Successfully started real Google Drive folder watch for {FolderId}", folderId);
            }
            else
            {
                _logger.LogError("❌ Failed to start Google Drive folder watch: {Error}", result.Error);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error starting Google Drive folder watch for {FolderId}", folderId);
            return Result<string>.WithFailure($"Error starting folder watch: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets document changes for a specific watch session
    /// </summary>
    /// <param name="watchId">The watch session identifier</param>
    /// <returns>Document changes detected</returns>
    [McpServerTool, Description("Retrieves document changes detected by an active watch session")]
    public async Task<Result<string>> GetDocumentChangesAsync(
        [Description("The watch session ID")] string watchId)
    {
        _logger.LogInformation("📋 Getting document changes for watch {WatchId}", watchId);

        try
        {
            // Get active watches which includes detected changes
            var watchesResult = await _googleDriveService.GetActiveWatchesAsync();

            if (watchesResult.IsSuccess)
            {
                var result = $"📋 Document Changes for Watch {watchId}:\n" +
                           $"ℹ️ Use GetActiveWatches to see detailed change information\n" +
                           $"🕐 Last Check: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n\n" +
                           $"{watchesResult.Value}";

                _logger.LogInformation("✅ Retrieved document changes for watch {WatchId}", watchId);
                return Result<string>.WithSuccess(result);
            }
            else
            {
                return Result<string>.WithFailure($"Failed to get document changes: {watchesResult.Error}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting document changes for watch {WatchId}", watchId);
            return Result<string>.WithFailure($"Error getting document changes: {ex.Message}");
        }
    }

    /// <summary>
    /// Downloads a document from Google Drive
    /// </summary>
    /// <param name="documentId">The document identifier</param>
    /// <returns>Download result information</returns>
    [McpServerTool, Description("Downloads a document from Google Drive and returns file information")]
    public async Task<Result<string>> DownloadDocumentAsync(
        [Description("The Google Drive document ID")] string documentId)
    {
        _logger.LogInformation("📥 Downloading document {DocumentId} from Google Drive", documentId);

        try
        {
            // Get metadata first
            var metadataResult = await _googleDriveService.GetDocumentMetadataAsync(documentId);
            if (!metadataResult.IsSuccess)
            {
                return Result<string>.WithFailure($"Failed to get document metadata: {metadataResult.Error}");
            }

            // Download document
            var downloadResult = await _googleDriveService.DownloadDocumentAsync(documentId);
            if (!downloadResult.IsSuccess)
            {
                return Result<string>.WithFailure($"Failed to download document: {downloadResult.Error}");
            }

            var metadata = metadataResult.Value!;
            var fileSize = downloadResult.Value!.Length;

            var result = $"✅ Downloaded: {metadata.Name}\n" +
                        $"📄 Document ID: {documentId}\n" +
                        $"📋 Type: {metadata.MimeType}\n" +
                        $"📦 Size: {fileSize:N0} bytes ({fileSize / 1024.0:F1} KB)\n" +
                        $"📅 Modified: {metadata.ModifiedTime:yyyy-MM-dd HH:mm:ss} UTC\n" +
                        $"🔗 View Link: {metadata.WebViewLink ?? "Not available"}\n" +
                        $"✅ AgentStatus: Download completed successfully\n" +
                        $"🔄 Ready for document processing pipeline";

            _logger.LogInformation("✅ Successfully downloaded document {DocumentId} ({Size} bytes)", documentId, fileSize);
            return Result<string>.WithSuccess(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error downloading document {DocumentId}", documentId);
            return Result<string>.WithFailure($"Error downloading document: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets metadata for a specific document
    /// </summary>
    /// <param name="documentId">The document identifier</param>
    /// <returns>Document metadata information</returns>
    [McpServerTool, Description("Retrieves detailed metadata for a Google Drive document")]
    public async Task<Result<string>> GetDocumentMetadataAsync(
        [Description("The Google Drive document ID")] string documentId)
    {
        _logger.LogInformation("📄 Getting metadata for document {DocumentId}", documentId);

        try
        {
            var metadataResult = await _googleDriveService.GetDocumentMetadataAsync(documentId);

            if (!metadataResult.IsSuccess)
            {
                return Result<string>.WithFailure($"Failed to get document metadata: {metadataResult.Error}");
            }

            var metadata = metadataResult.Value!;
            var result = $"📄 Document Metadata:\n" +
                        $"🏷️ Name: {metadata.Name}\n" +
                        $"🆔 ID: {metadata.Id}\n" +
                        $"📋 MIME Type: {metadata.MimeType}\n" +
                        $"📦 Size: {metadata.Size:N0} bytes ({metadata.Size / 1024.0:F1} KB)\n" +
                        $"📅 Created: {metadata.CreatedTime:yyyy-MM-dd HH:mm:ss} UTC\n" +
                        $"🔄 Modified: {metadata.ModifiedTime:yyyy-MM-dd HH:mm:ss} UTC\n" +
                        $"🔗 View Link: {metadata.WebViewLink ?? "Not available"}\n" +
                        $"📥 Download URL: {(string.IsNullOrEmpty(metadata.DownloadUrl) ? "Direct API access required" : "Available")}";

            _logger.LogInformation("✅ Retrieved real metadata for document {DocumentId}", documentId);
            return Result<string>.WithSuccess(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting metadata for document {DocumentId}", documentId);
            return Result<string>.WithFailure($"Error getting document metadata: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks the health agentStatus of the MCP server and Google Drive integration
    /// </summary>
    /// <returns>Health agentStatus information</returns>
    [McpServerTool, Description("Checks the health agentStatus of Google Drive MCP integration")]
    public async Task<Result<string>> CheckHealthStatusAsync()
    {
        _logger.LogInformation("🏥 Checking Google Drive health agentStatus");

        try
        {
            // Try to initialize the Google Drive service to check connectivity
            var initResult = await _googleDriveService.InitializeAsync();

            var connectionStatus = initResult.IsSuccess ? "✅ Connected" : "❌ Failed";
            var authStatus = initResult.IsSuccess ? "✅ Valid" : "❌ Invalid";

            // Get active watches
            var watchesResult = await _googleDriveService.GetActiveWatchesAsync();
            var activeWatches = watchesResult.IsSuccess ? "Available" : "Error getting watch info";

            var result = $"🏥 Google Drive MCP Health AgentStatus:\n" +
                        $"✅ AgentStatus: {(initResult.IsSuccess ? "Healthy" : "Unhealthy")}\n" +
                        $"🔗 API Connection: {connectionStatus}\n" +
                        $"🔑 Authentication: {authStatus}\n" +
                        $"🔖 Version: 1.0.0 (Native C# Implementation)\n" +
                        $"📊 Active Watches: {activeWatches}\n" +
                        $"🕐 Last Check: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n" +
                        $"📈 AgentStatus: {(initResult.IsSuccess ? "All systems operational" : $"Error: {initResult.Error}")}\n" +
                        $"🔧 Implementation: Native Google APIs for .NET";

            if (!initResult.IsSuccess)
            {
                _logger.LogWarning("⚠️ Google Drive health check failed: {Error}", initResult.Error);
            }
            else
            {
                _logger.LogInformation("✅ Google Drive health check completed successfully");
            }

            return Result<string>.WithSuccess(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error checking Google Drive health agentStatus");
            return Result<string>.WithFailure($"Error checking health agentStatus: {ex.Message}");
        }
    }

    /// <summary>
    /// Lists all active watch sessions
    /// </summary>
    /// <returns>Active watch sessions information</returns>
    [McpServerTool, Description("Lists all active Google Drive folder watch sessions")]
    public async Task<Result<string>> GetActiveWatchesAsync()
    {
        _logger.LogInformation("👁️ Getting active Google Drive watch sessions");

        try
        {
            var result = await _googleDriveService.GetActiveWatchesAsync();

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Retrieved active watch sessions");
            }
            else
            {
                _logger.LogError("❌ Failed to get active watch sessions: {Error}", result.Error);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting active watch sessions");
            return Result<string>.WithFailure($"Error getting active watches: {ex.Message}");
        }
    }

    /// <summary>
    /// Stops watching a specific folder
    /// </summary>
    /// <param name="watchId">The watch session identifier</param>
    /// <returns>Stop operation result</returns>
    [McpServerTool, Description("Stops watching a specific Google Drive folder")]
    public async Task<Result<string>> StopWatchingAsync(
        [Description("The watch session ID to stop")] string watchId)
    {
        _logger.LogInformation("🛑 Stopping Google Drive watch session {WatchId}", watchId);

        try
        {
            var result = await _googleDriveService.StopWatchingAsync(watchId);

            if (result.IsSuccess)
            {
                _logger.LogInformation("✅ Successfully stopped watch session {WatchId}", watchId);
            }
            else
            {
                _logger.LogError("❌ Failed to stop watch session {WatchId}: {Error}", watchId, result.Error);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error stopping watch session {WatchId}", watchId);
            return Result<string>.WithFailure($"Error stopping watch session: {ex.Message}");
        }
    }
}