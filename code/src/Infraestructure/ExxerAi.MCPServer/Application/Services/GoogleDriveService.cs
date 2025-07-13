using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;
using ExxerAI.Application.Interfaces;
using ExxerAI.MCPServer.Application.Interfaces;
using ExxerAI.Domain.Operations;

namespace ExxerAI.MCPServer.Application.Services;

/// <summary>
/// Google Drive service implementation providing real API integration
/// Supports OAuth authentication, file operations, and real-time monitoring
/// Integrates with ExxerAI document processing pipeline
/// </summary>
public class GoogleDriveService : IGoogleDriveService
{
    private readonly ILogger<GoogleDriveService> _logger;
    private readonly IGoogleDriveCredentialResolver _credentialResolver;
    private readonly IHybridDocumentProcessor? _documentProcessor;
    private DriveService? _driveService;
    private readonly Dictionary<string, WatchSession> _activeSessions = [];

    /// <summary>
    /// Initializes a new instance of the GoogleDriveService
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="credentialResolver">Credential resolver for multiple sources</param>
    /// <param name="documentProcessor">Document processing pipeline (optional)</param>
    public GoogleDriveService(
        ILogger<GoogleDriveService> logger,
        IGoogleDriveCredentialResolver credentialResolver,
        IHybridDocumentProcessor? documentProcessor = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _credentialResolver = credentialResolver ?? throw new ArgumentNullException(nameof(credentialResolver));
        _documentProcessor = documentProcessor; // Optional for now
    }

    /// <summary>
    /// Initialize Google Drive service with OAuth authentication
    /// </summary>
    public async Task<Result<bool>> InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔑 Initializing Google Drive service...");

            // Early cancellation check
            if (cancellationToken.IsCancellationRequested)
                return ResultExtensions.Cancelled<bool>();

            // Check if we're in a test environment - skip OAuth to prevent browser opening
            var isTestEnvironment = IsTestEnvironment();
            if (isTestEnvironment)
            {
                _logger.LogInformation("🧪 Test environment detected - skipping OAuth initialization");
                return Result<bool>.WithFailure("OAuth initialization skipped in test environment to prevent browser opening");
            }

            // Resolve credentials
            var credentialsResult = await _credentialResolver.ResolveCredentialsAsync(cancellationToken).ConfigureAwait(false);
            if (credentialsResult.IsFailure)
            {
                _logger.LogWarning("⚠️ Failed to resolve credentials: {Error}", credentialsResult.Error);
                return credentialsResult.IsSuccess;
            }

            var credentials = credentialsResult.Value;
            _logger.LogInformation("🔑 Using credentials from: {Source}", credentials.Source);

            // Early cancellation check
            if (cancellationToken.IsCancellationRequested)
                return ResultExtensions.Cancelled<bool>();

            // Check for obsolete OAuth flows and skip
            if (credentials.Type == CredentialType.OAuth)
            {
                _logger.LogWarning("⚠️ OAuth credential type detected - this may trigger obsolete GeneralOAuthFlow");
                return Result<bool>.WithFailure("OAuth flow skipped to prevent obsolete GeneralOAuthFlow (Error 400: invalid_request)");
            }

            // Initialize OAuth flow - this will open browser tabs!
            _logger.LogWarning("⚠️ About to trigger OAuth flow - this will open browser tabs");
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = credentials.ClientId,
                    ClientSecret = credentials.ClientSecret
                },
                new[] { DriveService.Scope.DriveReadonly, DriveService.Scope.DriveFile },
                "user",
                cancellationToken).ConfigureAwait(false);

            // Create Drive service
            _driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "ExxerAI MCP Server"
            });

            _logger.LogInformation("✅ Google Drive service initialized successfully using {Source}", credentials.Source);
            return Result<bool>.WithSuccess(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to initialize Google Drive service");
            return Result<bool>.WithFailure($"Initialization failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Detects if we're running in a test environment
    /// </summary>
    private bool IsTestEnvironment()
    {
        // Check for common test indicators
        var testIndicators = new[]
        {
            "Microsoft.TestPlatform",
            "xunit",
            "nunit",
            "mstest",
            "testhost",
            "dotnet-test",
            "VSTest"
        };

        var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
        var processName = currentProcess.ProcessName.ToLowerInvariant();
        var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location.ToLowerInvariant();

        // Check process name and assembly location for test indicators
        return testIndicators.Any(indicator =>
            processName.Contains(indicator.ToLowerInvariant()) ||
            assemblyLocation.Contains(indicator.ToLowerInvariant()) ||
            assemblyLocation.Contains("test"));
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
        int pollingIntervalSeconds = 60,
        CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<string>();

        if (_driveService == null)
        {
            var initResult = await InitializeAsync(cancellationToken).ConfigureAwait(false);
            if (!initResult.IsSuccess)
                return Result<string>.WithFailure("Drive service not initialized");
        }

        try
        {
            _logger.LogInformation("Starting folder watch for {FolderId}", folderId);

            // Check cancellation before folder verification
            if (cancellationToken.IsCancellationRequested)
                return ResultExtensions.Cancelled<string>();

            // Verify folder exists
            var folder = await _driveService!.Files.Get(folderId).ExecuteAsync(cancellationToken).ConfigureAwait(false);
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

            // Start background monitoring - store task for proper lifecycle management
            session.MonitoringTask = Task.Run(async () => await MonitorFolderAsync(session, cancellationToken).ConfigureAwait(false), cancellationToken);

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
    public async Task<Result<byte[]>> DownloadDocumentAsync(string documentId, CancellationToken cancellationToken = default)
    {
        // Early cancellation check
        if (cancellationToken.IsCancellationRequested)
            return ResultExtensions.Cancelled<byte[]>();

        if (_driveService == null)
        {
            var initResult = await InitializeAsync(cancellationToken).ConfigureAwait(false);
            if (!initResult.IsSuccess)
                return Result<byte[]>.WithFailure("Drive service not initialized");
        }

        try
        {
            _logger.LogInformation("Downloading document {DocumentId}", documentId);

            // Get file metadata
            var file = await _driveService!.Files.Get(documentId).ExecuteAsync(cancellationToken).ConfigureAwait(false);
            if (file == null)
            {
                return Result<byte[]>.WithFailure($"File {documentId} not found");
            }

            // Download file content
            using var stream = new MemoryStream();
            var request = _driveService!.Files.Get(documentId);
            await request.DownloadAsync(stream, cancellationToken).ConfigureAwait(false);

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
    public async Task<Result<GoogleDriveFileMetadata>> GetDocumentMetadataAsync(string documentId, CancellationToken cancellationToken = default)
    {
        if (_driveService == null)
        {
            var initResult = await InitializeAsync(cancellationToken).ConfigureAwait(false);
            if (!initResult.IsSuccess)
                return Result<GoogleDriveFileMetadata>.WithFailure("Drive service not initialized");
        }

        try
        {
            _logger.LogInformation("Getting metadata for document {DocumentId}", documentId);

            var file = await _driveService!.Files.Get(documentId).ExecuteAsync(cancellationToken).ConfigureAwait(false);
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
                CreatedTime = file.CreatedTimeDateTimeOffset?.DateTime ?? DateTime.MinValue,
                ModifiedTime = file.ModifiedTimeDateTimeOffset?.DateTime ?? DateTime.MinValue,
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
    /// <param name="session">The watch session to monitor</param>
    /// <param name="cancellationToken">Token to cancel the monitoring operation</param>
    private async Task MonitorFolderAsync(WatchSession session, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔍 Starting background monitoring for watch {WatchId}", session.WatchId);

        while (session.IsActive && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                // List files in folder modified since last check
                var listRequest = _driveService!.Files.List();
                listRequest.Q = $"'{session.FolderId}' in parents and modifiedTime > '{session.LastCheck:yyyy-MM-ddTHH:mm:ss}'";
                listRequest.Fields = "files(id,name,mimeType,modifiedTime,size)";

                var files = await listRequest.ExecuteAsync(cancellationToken).ConfigureAwait(false);

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
                            // Store processing task for proper lifecycle management
                            var processingTask = Task.Run(async () => await ProcessDetectedDocumentAsync(file, session.WatchId, cancellationToken).ConfigureAwait(false), cancellationToken);
                            session.ProcessingTasks.Add(processingTask);
                        }
                    }
                }

                session.LastCheck = DateTime.UtcNow;
                await Task.Delay(session.PollingInterval, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in folder monitoring for watch {WatchId}", session.WatchId);
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken).ConfigureAwait(false); // Wait before retry
            }
        }

        _logger.LogInformation("🛑 Stopped monitoring watch {WatchId}", session.WatchId);
    }

    /// <summary>
    /// Processes a detected document through the ExxerAI pipeline
    /// </summary>
    /// <param name="file">The detected file to process</param>
    /// <param name="watchId">The watch session ID</param>
    /// <param name="cancellationToken">Token to cancel the processing operation</param>
    private async Task ProcessDetectedDocumentAsync(Google.Apis.Drive.v3.Data.File file, string watchId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔄 Auto-processing detected file: {FileName} (ID: {FileId})", file.Name, file.Id);

            // Download document
            var downloadResult = await DownloadDocumentAsync(file.Id, cancellationToken).ConfigureAwait(false);
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
                var processingResult = await _documentProcessor.ProcessDocumentAsync(downloadResult.Value!, metadata, cancellationToken).ConfigureAwait(false);

                if (processingResult.IsSuccess && processingResult.Value is not null)
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
    public Task<Result<string>> GetActiveWatchesAsync(CancellationToken cancellationToken = default)
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
    public async Task<Result<string>> StopWatchingAsync(string watchId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_activeSessions.TryGetValue(watchId, out var session))
            {
                return Result<string>.WithFailure($"Watch session {watchId} not found");
            }

            session.IsActive = false;

            // Wait for monitoring task to complete gracefully
            if (session.MonitoringTask != null && !session.MonitoringTask.IsCompleted)
            {
                try
                {
                    await session.MonitoringTask.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error while stopping monitoring task for watch {WatchId}", watchId);
                }
            }

            // Wait for all processing tasks to complete
            if (session.ProcessingTasks.Count > 0)
            {
                try
                {
                    await Task.WhenAll(session.ProcessingTasks).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error while stopping processing tasks for watch {WatchId}", watchId);
                }
            }

            var duration = DateTime.UtcNow - session.StartTime;

            var result = $"✅ Successfully stopped watching session {watchId}\n" +
                        $"🛑 Watch AgentStatus: Stopped\n" +
                        $"🕐 Stopped At: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC\n" +
                        $"📊 Session Duration: {duration.TotalHours:F1}h\n" +
                        $"📄 Documents Processed: {session.DetectedChanges.Count}\n" +
                        $"💾 Resources Released: Yes";

            _logger.LogInformation("✅ Successfully stopped watch session {WatchId}", watchId);
            return Result<string>.WithSuccess(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping watch session {WatchId}", watchId);
            return Result<string>.WithFailure($"Error stopping watch session: {ex.Message}");
        }
    }
}