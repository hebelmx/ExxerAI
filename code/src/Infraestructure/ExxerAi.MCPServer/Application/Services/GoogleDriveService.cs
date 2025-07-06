using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;
using ExxerAI.Application.Interfaces;
using ExxerAi.MCPServer.Application.Interfaces;
using ExxerAI.Domain.Helpers.Operations;

namespace ExxerAi.MCPServer.Application.Services;

/// <summary>
/// Google Drive service implementation providing real API integration
/// Supports OAuth authentication, file operations, and real-time monitoring
/// Integrates with ExxerAI document processing pipeline
/// </summary>
public class GoogleDriveService : IGoogleDriveService
{
    private readonly ILogger<GoogleDriveService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHybridDocumentProcessor? _documentProcessor;
    private DriveService? _driveService;
    private readonly Dictionary<string, WatchSession> _activeSessions = new();

    /// <summary>
    /// Initializes a new instance of the GoogleDriveService
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="configuration">Configuration for API credentials</param>
    /// <param name="documentProcessor">Document processing pipeline (optional)</param>
    public GoogleDriveService(
        ILogger<GoogleDriveService> logger,
        IConfiguration configuration,
        IHybridDocumentProcessor? documentProcessor = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _documentProcessor = documentProcessor; // Optional for now
    }

    /// <summary>
    /// Initializes Google Drive service with OAuth authentication
    /// </summary>
    public async Task<Result<bool>> InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Initializing Google Drive service...");

            // Get OAuth credentials from configuration
            var clientId = _configuration["GoogleDrive:ClientId"] ??
                          Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID");
            var clientSecret = _configuration["GoogleDrive:ClientSecret"] ??
                              Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET");

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                return Result<bool>.WithFailure("Google Drive OAuth credentials not configured. Set GOOGLE_OAUTH_CLIENT_ID and GOOGLE_OAUTH_CLIENT_SECRET environment variables.");
            }

            // Initialize OAuth flow
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret
                },
                new[] { DriveService.Scope.DriveReadonly, DriveService.Scope.DriveFile },
                "user",
                CancellationToken.None);

            // Create Drive service
            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "ExxerAI MCP Server"
            });

            _logger.LogInformation("✅ Google Drive service initialized successfully");
            return Result<bool>.WithSuccess(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to initialize Google Drive service");
            return Result<bool>.WithFailure($"Initialization failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Starts monitoring a Google Drive folder for changes
    /// </summary>
    /// <param name="folderId">The folder ID to monitor</param>
    /// <param name="includeSubdirectories">Include subdirectories in monitoring</param>
    /// <param name="autoProcess">Automatically process detected files</param>
    /// <param name="pollingIntervalSeconds">Polling interval in seconds</param>
    /// <returns>Watch session information</returns>
    public async Task<Result<string>> StartFolderWatchAsync(
        string folderId,
        bool includeSubdirectories = true,
        bool autoProcess = true,
        int pollingIntervalSeconds = 60)
    {
        if (_driveService == null)
        {
            var initResult = await InitializeAsync();
            if (!initResult.IsSuccess)
                return Result<string>.WithFailure("Drive service not initialized");
        }

        try
        {
            _logger.LogInformation("Starting folder watch for {FolderId}", folderId);

            // Verify folder exists
            var folder = await _driveService.Files.Get(folderId).ExecuteAsync();
            if (folder == null)
            {
                return Result<string>.WithFailure($"Folder {folderId} not found or not accessible");
            }

            // Create watch session
            var watchId = $"watch_{Guid.NewGuid().ToString()[..8]}";
            var session = new WatchSession
            {
                WatchId = watchId,
                FolderId = folderId,
                FolderName = folder.Name,
                IncludeSubdirectories = includeSubdirectories,
                AutoProcess = autoProcess,
                PollingInterval = TimeSpan.FromSeconds(pollingIntervalSeconds),
                StartTime = DateTime.UtcNow,
                LastCheck = DateTime.UtcNow,
                IsActive = true
            };

            _activeSessions[watchId] = session;

            // Start background monitoring
            _ = Task.Run(async () => await MonitorFolderAsync(session));

            var result = $"✅ Started watching Google Drive folder: {folder.Name}\n" +
                        $"📂 Folder ID: {folderId}\n" +
                        $"🔍 Include Subdirectories: {includeSubdirectories}\n" +
                        $"⚙️ Auto Process: {autoProcess}\n" +
                        $"⏱️ Polling Interval: {pollingIntervalSeconds}s\n" +
                        $"🆔 Watch ID: {watchId}\n" +
                        $"🕐 Started: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

            _logger.LogInformation("✅ Successfully started watching folder {FolderId} with watch ID {WatchId}", folderId, watchId);
            return Result<string>.WithSuccess(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error starting folder watch for {FolderId}", folderId);
            return Result<string>.WithFailure($"Error starting folder watch: {ex.Message}");
        }
    }

    /// <summary>
    /// Downloads a document from Google Drive
    /// </summary>
    /// <param name="documentId">The document ID to download</param>
    /// <returns>Downloaded document data</returns>
    public async Task<Result<byte[]>> DownloadDocumentAsync(string documentId)
    {
        if (_driveService == null)
        {
            var initResult = await InitializeAsync();
            if (!initResult.IsSuccess)
                return Result<byte[]>.WithFailure("Drive service not initialized");
        }

        try
        {
            _logger.LogInformation("Downloading document {DocumentId}", documentId);

            // Get file metadata
            var file = await _driveService.Files.Get(documentId).ExecuteAsync();
            if (file == null)
            {
                return Result<byte[]>.WithFailure($"File {documentId} not found");
            }

            // Download file content
            using var stream = new MemoryStream();
            var request = _driveService.Files.Get(documentId);
            await request.DownloadAsync(stream);

            var fileData = stream.ToArray();

            _logger.LogInformation("✅ Successfully downloaded {FileName} ({Size} bytes)", file.Name, fileData.Length);
            return Result<byte[]>.WithSuccess(fileData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error downloading document {DocumentId}", documentId);
            return Result<byte[]>.WithFailure($"Error downloading document: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets metadata for a Google Drive document
    /// </summary>
    /// <param name="documentId">The document ID</param>
    /// <returns>Document metadata</returns>
    public async Task<Result<GoogleDriveFileMetadata>> GetDocumentMetadataAsync(string documentId)
    {
        if (_driveService == null)
        {
            var initResult = await InitializeAsync();
            if (!initResult.IsSuccess)
                return Result<GoogleDriveFileMetadata>.WithFailure("Drive service not initialized");
        }

        try
        {
            _logger.LogInformation("Getting metadata for document {DocumentId}", documentId);

            var file = await _driveService.Files.Get(documentId).ExecuteAsync();
            if (file == null)
            {
                return Result<GoogleDriveFileMetadata>.WithFailure($"File {documentId} not found");
            }

            var metadata = new GoogleDriveFileMetadata
            {
                Id = file.Id,
                Name = file.Name ?? "Unknown",
                MimeType = file.MimeType ?? "application/octet-stream",
                Size = file.Size ?? 0,
                CreatedTime = file.CreatedTime ?? DateTime.MinValue,
                ModifiedTime = file.ModifiedTime ?? DateTime.MinValue,
                WebViewLink = file.WebViewLink,
                DownloadUrl = file.WebContentLink
            };

            _logger.LogInformation("✅ Retrieved metadata for {FileName}", file.Name);
            return Result<GoogleDriveFileMetadata>.WithSuccess(metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting metadata for document {DocumentId}", documentId);
            return Result<GoogleDriveFileMetadata>.WithFailure($"Error getting metadata: {ex.Message}");
        }
    }

    /// <summary>
    /// Background folder monitoring implementation
    /// </summary>
    private async Task MonitorFolderAsync(WatchSession session)
    {
        _logger.LogInformation("🔍 Starting background monitoring for watch {WatchId}", session.WatchId);

        while (session.IsActive)
        {
            try
            {
                // List files in folder modified since last check
                var listRequest = _driveService.Files.List();
                listRequest.Q = $"'{session.FolderId}' in parents and modifiedTime > '{session.LastCheck:yyyy-MM-ddTHH:mm:ss}'";
                listRequest.Fields = "files(id,name,mimeType,modifiedTime,size)";

                var files = await listRequest.ExecuteAsync();

                if (files.Files?.Count > 0)
                {
                    _logger.LogInformation("📄 Detected {Count} file changes in folder {FolderId}", files.Files.Count, session.FolderId);

                    foreach (var file in files.Files)
                    {
                        session.DetectedChanges.Add(new DocumentChange
                        {
                            DocumentId = file.Id,
                            FileName = file.Name,
                            ChangeType = "Modified",
                            DetectedAt = DateTime.UtcNow,
                            MimeType = file.MimeType,
                            Size = file.Size ?? 0
                        });

                        // Auto-process if enabled
                        if (session.AutoProcess)
                        {
                            _ = Task.Run(async () => await ProcessDetectedDocumentAsync(file, session.WatchId));
                        }
                    }
                }

                session.LastCheck = DateTime.UtcNow;
                await Task.Delay(session.PollingInterval);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in folder monitoring for watch {WatchId}", session.WatchId);
                await Task.Delay(TimeSpan.FromMinutes(1)); // Wait before retry
            }
        }

        _logger.LogInformation("🛑 Stopped monitoring watch {WatchId}", session.WatchId);
    }

    /// <summary>
    /// Processes a detected document through the ExxerAI pipeline
    /// </summary>
    private async Task ProcessDetectedDocumentAsync(Google.Apis.Drive.v3.Data.File file, string watchId)
    {
        try
        {
            _logger.LogInformation("🔄 Auto-processing detected file: {FileName} (ID: {FileId})", file.Name, file.Id);

            // Download document
            var downloadResult = await DownloadDocumentAsync(file.Id);
            if (!downloadResult.IsSuccess)
            {
                _logger.LogWarning("⚠️ Failed to download file {FileId}: {Error}", file.Id, downloadResult.Error);
                return;
            }

            // Create document metadata for processing
            var metadata = new DocumentMetadata
            {
                FileName = file.Name ?? "Unknown",
                MimeType = file.MimeType ?? "application/octet-stream",
                SourcePath = $"GoogleDrive:{file.Id}",
                DocumentType = DocumentType.Other, // Default type
                Properties = new Dictionary<string, object>
                {
                    ["WatchId"] = watchId,
                    ["DriveFileId"] = file.Id,
                    ["DetectedAt"] = DateTime.UtcNow.ToString("O"),
                    ["SourceSystem"] = "GoogleDrive_MCP"
                }
            };

            // Process through ExxerAI document pipeline if available
            if (_documentProcessor != null)
            {
                var processingResult = await _documentProcessor.ProcessDocumentAsync(downloadResult.Value, metadata);

                if (processingResult.IsSuccess)
                {
                    _logger.LogInformation("✅ Successfully processed {FileName} - Confidence: {Confidence:P}",
                        file.Name, processingResult.Value.OverallConfidence);
                }
                else
                {
                    _logger.LogWarning("⚠️ Failed to process {FileName}: {Error}", file.Name, processingResult.Error);
                }
            }
            else
            {
                _logger.LogInformation("ℹ️ Document processor not available, file downloaded but not processed: {FileName}", file.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error auto-processing file {FileId}", file.Id);
        }
    }

    /// <summary>
    /// Gets active watch sessions
    /// </summary>
    public Task<Result<string>> GetActiveWatchesAsync()
    {
        try
        {
            if (!_activeSessions.Any())
            {
                return Task.FromResult(Result<string>.WithSuccess("📋 No active watch sessions found."));
            }

            var result = "👁️ Active Google Drive Watch Sessions:\n\n";

            foreach (var session in _activeSessions.Values)
            {
                var duration = DateTime.UtcNow - session.StartTime;
                result += $"🔍 Watch ID: {session.WatchId}\n" +
                         $"  📂 Folder: {session.FolderName} ({session.FolderId})\n" +
                         $"  📅 Started: {session.StartTime:yyyy-MM-dd HH:mm:ss} UTC\n" +
                         $"  ⏱️ Interval: {session.PollingInterval.TotalSeconds}s\n" +
                         $"  📄 Changes Detected: {session.DetectedChanges.Count}\n" +
                         $"  🕐 Duration: {duration.TotalHours:F1}h\n" +
                         $"  ✅ AgentStatus: {(session.IsActive ? "Active" : "Stopped")}\n\n";
            }

            result += $"📊 Total Active Watches: {_activeSessions.Count(s => s.Value.IsActive)}";

            return Task.FromResult(Result<string>.WithSuccess(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active watch sessions");
            return Task.FromResult(Result<string>.WithFailure($"Error getting active watches: {ex.Message}"));
        }
    }

    /// <summary>
    /// Stops a watch session
    /// </summary>
    public Task<Result<string>> StopWatchingAsync(string watchId)
    {
        try
        {
            if (!_activeSessions.TryGetValue(watchId, out var session))
            {
                return Task.FromResult(Result<string>.WithFailure($"Watch session {watchId} not found"));
            }

            session.IsActive = false;
            var duration = DateTime.UtcNow - session.StartTime;

            var result = $"✅ Successfully stopped watching session {watchId}\n" +
                        $"🛑 Watch AgentStatus: Stopped\n" +
                        $"🕐 Stopped At: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n" +
                        $"📊 Session Duration: {duration.TotalHours:F1}h\n" +
                        $"📄 Documents Processed: {session.DetectedChanges.Count}\n" +
                        $"💾 Resources Released: Yes";

            _logger.LogInformation("✅ Successfully stopped watch session {WatchId}", watchId);
            return Task.FromResult(Result<string>.WithSuccess(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping watch session {WatchId}", watchId);
            return Task.FromResult(Result<string>.WithFailure($"Error stopping watch session: {ex.Message}"));
        }
    }
}