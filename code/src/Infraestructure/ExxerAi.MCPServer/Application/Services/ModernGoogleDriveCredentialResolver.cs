using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Microsoft.Extensions.Logging;
using ExxerAI.Domain;
using ExxerAI.Domain.Operations;
using ExxerAI.MCPServer.Application.Interfaces;

namespace ExxerAI.MCPServer.Application.Services;

/// <summary>
/// Modern Google Drive credential resolver using Application Default Credentials (ADC)
/// This follows Google's current security best practices and eliminates JSON key files
/// </summary>
public class ModernGoogleDriveCredentialResolver : IGoogleDriveCredentialResolver
{
    private readonly ILogger<ModernGoogleDriveCredentialResolver> _logger;

    public ModernGoogleDriveCredentialResolver(ILogger<ModernGoogleDriveCredentialResolver> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Resolves Google Drive credentials using Application Default Credentials (ADC)
    /// Priority: ADC (gcloud auth) > Workload Identity Federation > Environment Variables > Service Account Files
    /// </summary>
    public async Task<Result<GoogleDriveCredentials>> ResolveCredentialsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔍 Resolving Google Drive credentials using modern ADC approach...");
        if (cancellationToken.IsCancellationRequested)
            return Result<GoogleDriveCredentials>.WithFailure("Task Canceled");
        try
        {
            // Try Application Default Credentials (recommended approach)
            var credential = await GoogleCredential.GetApplicationDefaultAsync(cancellationToken);

            if (credential != null)
            {
                // Ensure the credential has the required scopes
                if (credential.IsCreateScopedRequired)
                {
                    credential = credential.CreateScoped(DriveService.Scope.Drive);
                    _logger.LogDebug("🔧 Added Drive scope to credential");
                }

                // Determine the credential source for logging
                var credentialSource = DetermineCredentialSource(credential);

                _logger.LogInformation("✅ Successfully resolved credentials from: {Source}", credentialSource);

                // Create our standard credential response
                var modernCredentials = new GoogleDriveCredentials
                {
                    Type = CredentialType.ApplicationDefault,
                    Source = credentialSource,
                    GoogleCredential = credential,
                    IsScoped = !credential.IsCreateScopedRequired
                };

                return Result<GoogleDriveCredentials>.WithSuccess(modernCredentials);
            }

            _logger.LogWarning("⚠️ No Application Default Credentials found");
            return Result<GoogleDriveCredentials>.WithFailure("No Application Default Credentials available");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Could not load"))
        {
            _logger.LogWarning("⚠️ ADC not configured: {Message}", ex.Message);
            return Result<GoogleDriveCredentials>.WithFailure(
                "Application Default Credentials not configured. Please run 'gcloud auth application-default login' " +
                "or set up Workload Identity Federation for production environments.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to resolve Google Drive credentials");
            return Result<GoogleDriveCredentials>.WithFailure($"Credential resolution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Determines the source of the credential for logging and debugging
    /// </summary>
    private string DetermineCredentialSource(GoogleCredential credential)
    {
        // Check the credential type to determine source
        var credentialType = credential.GetType().Name;

        return credentialType switch
        {
            "UserCredential" => "Application Default Credentials (gcloud auth)",
            "ServiceAccountCredential" => "Service Account (ADC)",
            "ExternalAccountCredential" => "Workload Identity Federation",
            "ComputeCredential" => "Google Compute Engine Metadata",
            _ => $"Application Default Credentials ({credentialType})"
        };
    }

    /// <summary>
    /// Creates a properly configured DriveService instance
    /// </summary>
    public async Task<Result<DriveService>> CreateDriveServiceAsync(CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
            return Result<DriveService>.WithFailure("Task Canceled");
        var credentialResult = await ResolveCredentialsAsync(cancellationToken);

        if (!credentialResult.IsSuccess)
        {
            throw new InvalidOperationException($"Failed to resolve credentials: {string.Join(", ", credentialResult.Errors)}");
        }

        var credential = credentialResult.Value.GoogleCredential;

        var service = new DriveService(new Google.Apis.Services.BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "ExxerAI Drive Integration",
        });

        _logger.LogInformation("🚀 Drive service created successfully");
        return service;
    }

    /// <summary>
    /// Tests the credential by making a simple API call
    /// </summary>
    public async Task<Result<bool>> TestCredentialAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var serviceResult = await CreateDriveServiceAsync(cancellationToken);
            var service = serviceResult.Value;

            // Make a simple API call to test the credential
            var aboutRequest = service.About.Get();
            aboutRequest.Fields = "user/emailAddress";
            var about = await aboutRequest.ExecuteAsync(cancellationToken);

            _logger.LogInformation("✅ Credential test successful - authenticated as: {Email}",
                about.User?.EmailAddress ?? "Unknown");

            return Result<bool>.WithSuccess(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Credential test failed");
            return Result<bool>.WithFailure($"Credential test failed: {ex.Message}");
        }
    }
}