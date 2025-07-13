using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ExxerAI.Domain;
using ExxerAI.Domain.Operations;
using ExxerAi.MCPServer.Application.Interfaces;
using System.Text.Json;

namespace ExxerAi.MCPServer.Application.Services;

/// <summary>
/// Hybrid Google Drive credential resolver that supports both modern ADC and legacy methods
/// Priority: ADC (production) > Environment Variables > Configuration > JSON Files
/// Designed for gradual migration from legacy to modern authentication
/// </summary>
public class HybridGoogleDriveCredentialResolver : IGoogleDriveCredentialResolver
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<HybridGoogleDriveCredentialResolver> _logger;

    public HybridGoogleDriveCredentialResolver(
        IConfiguration configuration,
        ILogger<HybridGoogleDriveCredentialResolver> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Resolves Google Drive credentials using a fallback chain
    /// </summary>
    public async Task<Result<GoogleDriveCredentials>> ResolveCredentialsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔍 Resolving Google Drive credentials (hybrid approach)...");

        // Priority 1: Try Application Default Credentials (modern/production)
        var adcResult = await TryApplicationDefaultCredentials(cancellationToken);
        if (adcResult.IsSuccess)
        {
            _logger.LogInformation("✅ Using Application Default Credentials");
            return adcResult;
        }

        _logger.LogDebug("⚠️ ADC not available, falling back to legacy methods: {Error}", adcResult.Error);

        // Priority 2: Environment Variables
        var envResult = await TryEnvironmentVariables();
        if (envResult.IsSuccess)
        {
            _logger.LogInformation("✅ Using Environment Variables");
            return envResult;
        }

        // Priority 3: Configuration (User Secrets, appsettings.json)
        var configResult = await TryConfiguration();
        if (configResult.IsSuccess)
        {
            _logger.LogInformation("✅ Using Configuration");
            return configResult;
        }

        // Priority 4: JSON File
        var jsonResult = await TryJsonFile();
        if (jsonResult.IsSuccess)
        {
            _logger.LogInformation("✅ Using JSON File");
            return jsonResult;
        }

        // All methods failed
        var comprehensiveError = BuildComprehensiveErrorMessage(adcResult.Error, envResult.Error, configResult.Error, jsonResult.Error);
        _logger.LogError("❌ All credential resolution methods failed");
        return Result<GoogleDriveCredentials>.WithFailure(comprehensiveError);
    }

    /// <summary>
    /// Try Application Default Credentials (modern approach)
    /// </summary>
    private async Task<Result<GoogleDriveCredentials>> TryApplicationDefaultCredentials(CancellationToken cancellationToken)
    {
        try
        {
            var credential = await GoogleCredential.GetApplicationDefaultAsync(cancellationToken);

            if (credential != null)
            {
                // Ensure the credential has the required scopes
                if (credential.IsCreateScopedRequired)
                {
                    credential = credential.CreateScoped(DriveService.Scope.Drive);
                }

                var credentialSource = DetermineCredentialSource(credential);

                var modernCredentials = new GoogleDriveCredentials
                {
                    Type = CredentialType.ApplicationDefault,
                    Source = credentialSource,
                    GoogleCredential = credential,
                    IsScoped = !credential.IsCreateScopedRequired
                };

                return Result<GoogleDriveCredentials>.WithSuccess(modernCredentials);
            }

            return Result<GoogleDriveCredentials>.WithFailure("No Application Default Credentials found");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Could not load") || ex.Message.Contains("not found"))
        {
            return Result<GoogleDriveCredentials>.WithFailure($"ADC not configured: {ex.Message}");
        }
        catch (Exception ex)
        {
            return Result<GoogleDriveCredentials>.WithFailure($"ADC error: {ex.Message}");
        }
    }

    /// <summary>
    /// Try environment variables
    /// </summary>
    private async Task<Result<GoogleDriveCredentials>> TryEnvironmentVariables()
    {
        var clientId = Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET");

        if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
        {
            var credentials = new GoogleDriveCredentials
            {
                Type = CredentialType.OAuth,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Source = "Environment Variables"
            };

            return Result<GoogleDriveCredentials>.WithSuccess(credentials);
        }

        return Result<GoogleDriveCredentials>.WithFailure("Environment variables not found or incomplete");
    }

    /// <summary>
    /// Try configuration (User Secrets, appsettings.json)
    /// </summary>
    private async Task<Result<GoogleDriveCredentials>> TryConfiguration()
    {
        var clientId = _configuration["GoogleDrive:ClientId"];
        var clientSecret = _configuration["GoogleDrive:ClientSecret"];

        if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret) &&
            !IsPlaceholder(clientId) && !IsPlaceholder(clientSecret))
        {
            var credentials = new GoogleDriveCredentials
            {
                Type = CredentialType.OAuth,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Source = "Configuration"
            };

            return Result<GoogleDriveCredentials>.WithSuccess(credentials);
        }

        return Result<GoogleDriveCredentials>.WithFailure("Configuration not found or contains placeholder values");
    }

    /// <summary>
    /// Try JSON file
    /// </summary>
    private async Task<Result<GoogleDriveCredentials>> TryJsonFile()
    {
        var credentialsPath = _configuration["GoogleDrive:CredentialsPath"];

        if (string.IsNullOrEmpty(credentialsPath))
        {
            return Result<GoogleDriveCredentials>.WithFailure("JSON credential file path not configured");
        }

        if (!File.Exists(credentialsPath))
        {
            return Result<GoogleDriveCredentials>.WithFailure($"JSON credential file not found: {credentialsPath}");
        }

        try
        {
            var jsonContent = await File.ReadAllTextAsync(credentialsPath);
            var jsonDoc = JsonDocument.Parse(jsonContent);
            var root = jsonDoc.RootElement;

            // Try different JSON formats
            if (root.TryGetProperty("type", out var typeElement) &&
                typeElement.GetString() == "service_account")
            {
                // Service Account JSON
                var clientEmail = root.GetProperty("client_email").GetString();
                var projectId = root.GetProperty("project_id").GetString();

                var credentials = new GoogleDriveCredentials
                {
                    Type = CredentialType.ServiceAccount,
                    ServiceAccountEmail = clientEmail,
                    ProjectId = projectId,
                    Source = "JSON File (Service Account)"
                };

                return Result<GoogleDriveCredentials>.WithSuccess(credentials);
            }
            else if (root.TryGetProperty("installed", out var installedElement))
            {
                // Desktop Application JSON
                var clientId = installedElement.GetProperty("client_id").GetString();
                var clientSecret = installedElement.GetProperty("client_secret").GetString();

                var credentials = new GoogleDriveCredentials
                {
                    Type = CredentialType.OAuth,
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    Source = "JSON File (Desktop App)"
                };

                return Result<GoogleDriveCredentials>.WithSuccess(credentials);
            }
            else if (root.TryGetProperty("client_id", out var clientIdElement))
            {
                // Simple JSON format
                var clientId = clientIdElement.GetString();
                var clientSecret = root.GetProperty("client_secret").GetString();

                var credentials = new GoogleDriveCredentials
                {
                    Type = CredentialType.OAuth,
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    Source = "JSON File (Simple)"
                };

                return Result<GoogleDriveCredentials>.WithSuccess(credentials);
            }

            return Result<GoogleDriveCredentials>.WithFailure("Unsupported JSON credential format");
        }
        catch (Exception ex)
        {
            return Result<GoogleDriveCredentials>.WithFailure($"Failed to parse JSON credential file: {ex.Message}");
        }
    }

    /// <summary>
    /// Check if a value is a placeholder
    /// </summary>
    private static bool IsPlaceholder(string value)
    {
        return string.IsNullOrWhiteSpace(value) ||
               value.StartsWith("your-", StringComparison.OrdinalIgnoreCase) ||
               value.StartsWith("replace-", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("example") ||
               value.Contains("placeholder");
    }

    /// <summary>
    /// Determine credential source for ADC
    /// </summary>
    private string DetermineCredentialSource(GoogleCredential credential)
    {
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
    /// Build comprehensive error message
    /// </summary>
    private static string BuildComprehensiveErrorMessage(params string[] errors)
    {
        var errorMessage = "Google Drive credentials not found. Tried all resolution methods:\n";
        errorMessage += "1. Application Default Credentials (ADC)\n";
        errorMessage += "2. Environment Variables: GOOGLE_OAUTH_CLIENT_ID, GOOGLE_OAUTH_CLIENT_SECRET\n";
        errorMessage += "3. Configuration: GoogleDrive:ClientId, GoogleDrive:ClientSecret\n";
        errorMessage += "4. JSON File: GoogleDrive:CredentialsPath\n\n";
        errorMessage += "Errors encountered:\n";

        for (int i = 0; i < errors.Length; i++)
        {
            errorMessage += $"{i + 1}. {errors[i]}\n";
        }

        errorMessage += "\nFor production: Run 'gcloud auth application-default login' or set up Workload Identity Federation\n";
        errorMessage += "For development: Set environment variables or add credentials to appsettings.json";

        return errorMessage;
    }
}