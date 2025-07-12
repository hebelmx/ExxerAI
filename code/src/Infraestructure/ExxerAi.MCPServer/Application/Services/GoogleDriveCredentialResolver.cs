using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ExxerAI.Domain;
using ExxerAI.Domain.Operations;
using ExxerAi.MCPServer.Application.Interfaces;

namespace ExxerAi.MCPServer.Application.Services;

/// <summary>
/// Credential resolver for Google Drive API supporting multiple credential sources and formats
/// Priority: Environment Variables > User Secrets > appsettings.json > JSON credential file
/// </summary>
public class GoogleDriveCredentialResolver : IGoogleDriveCredentialResolver
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleDriveCredentialResolver> _logger;

    public GoogleDriveCredentialResolver(
        IConfiguration configuration,
        ILogger<GoogleDriveCredentialResolver> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Resolves Google Drive credentials from multiple sources
    /// </summary>
    /// <returns>Resolved credentials or failure result</returns>
    public async Task<Result<GoogleDriveCredentials>> ResolveCredentialsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔍 Resolving Google Drive credentials from multiple sources...");

        // Priority 1: Environment Variables (Production/CI/CD)
        var envCredentials = TryGetEnvironmentCredentials();
        if (envCredentials.IsSuccess)
        {
            _logger.LogInformation("✅ Credentials resolved from environment variables");
            return envCredentials;
        }

        // Priority 2: User Secrets (Development)
        var userSecretsCredentials = TryGetUserSecretsCredentials();
        if (userSecretsCredentials.IsSuccess)
        {
            _logger.LogInformation("✅ Credentials resolved from user secrets");
            return userSecretsCredentials;
        }

        // Priority 3: appsettings.json (Development)
        var appSettingsCredentials = TryGetAppSettingsCredentials();
        if (appSettingsCredentials.IsSuccess)
        {
            _logger.LogInformation("✅ Credentials resolved from appsettings.json");
            return appSettingsCredentials;
        }

        // Priority 4: JSON Credential File (Legacy/Desktop Apps)
        var jsonFileCredentials = await TryGetJsonFileCredentialsAsync(cancellationToken).ConfigureAwait(false);
        if (jsonFileCredentials.IsSuccess)
        {
            _logger.LogInformation("✅ Credentials resolved from JSON credential file");
            return jsonFileCredentials;
        }

        // All sources failed
        _logger.LogError("❌ Failed to resolve Google Drive credentials from any source");
        return Result<GoogleDriveCredentials>.WithFailure(
            "Google Drive credentials not found. Please configure credentials using one of the supported methods:\n" +
            "1. Environment Variables: GOOGLE_OAUTH_CLIENT_ID, GOOGLE_OAUTH_CLIENT_SECRET\n" +
            "2. User Secrets: GoogleDrive:ClientId, GoogleDrive:ClientSecret\n" +
            "3. appsettings.json: GoogleDrive section\n" +
            "4. JSON File: GoogleDrive:CredentialsPath pointing to OAuth credentials file");
    }

    /// <summary>
    /// Try to get credentials from environment variables
    /// </summary>
    private Result<GoogleDriveCredentials> TryGetEnvironmentCredentials()
    {
        var clientId = Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET");

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            _logger.LogDebug("🔍 Environment variables not found or incomplete");
            return Result<GoogleDriveCredentials>.WithFailure("Environment variables not configured");
        }

        return Result<GoogleDriveCredentials>.WithSuccess(new GoogleDriveCredentials
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            Source = "Environment Variables"
        });
    }

    /// <summary>
    /// Try to get credentials from user secrets
    /// </summary>
    private Result<GoogleDriveCredentials> TryGetUserSecretsCredentials()
    {
        var clientId = _configuration["GoogleDrive:ClientId"];
        var clientSecret = _configuration["GoogleDrive:ClientSecret"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            _logger.LogDebug("🔍 User secrets not found or incomplete");
            return Result<GoogleDriveCredentials>.WithFailure("User secrets not configured");
        }

        // Check if these are actually from user secrets (not appsettings.json)
        // This is a heuristic - user secrets typically have longer, more complex values
        if (clientId.Contains("your-client-id") || clientSecret.Contains("your-client-secret"))
        {
            _logger.LogDebug("🔍 User secrets contain placeholder values");
            return Result<GoogleDriveCredentials>.WithFailure("User secrets contain placeholder values");
        }

        return Result<GoogleDriveCredentials>.WithSuccess(new GoogleDriveCredentials
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            Source = "User Secrets"
        });
    }

    /// <summary>
    /// Try to get credentials from appsettings.json
    /// </summary>
    private Result<GoogleDriveCredentials> TryGetAppSettingsCredentials()
    {
        var clientId = _configuration["GoogleDrive:ClientId"];
        var clientSecret = _configuration["GoogleDrive:ClientSecret"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            _logger.LogDebug("🔍 appsettings.json not found or incomplete");
            return Result<GoogleDriveCredentials>.WithFailure("appsettings.json not configured");
        }

        // Skip placeholder values
        if (clientId.Contains("your-client-id") || clientSecret.Contains("your-client-secret"))
        {
            _logger.LogDebug("🔍 appsettings.json contains placeholder values");
            return Result<GoogleDriveCredentials>.WithFailure("appsettings.json contains placeholder values");
        }

        return Result<GoogleDriveCredentials>.WithSuccess(new GoogleDriveCredentials
        {
            ClientId = clientId,
            ClientSecret = clientSecret,
            Source = "appsettings.json"
        });
    }

    /// <summary>
    /// Try to get credentials from JSON credential file
    /// </summary>
    private async Task<Result<GoogleDriveCredentials>> TryGetJsonFileCredentialsAsync(CancellationToken cancellationToken = default)
    {
        var credentialsPath = _configuration["GoogleDrive:CredentialsPath"];
        if (string.IsNullOrEmpty(credentialsPath))
        {
            _logger.LogDebug("🔍 JSON credential file path not configured");
            return Result<GoogleDriveCredentials>.WithFailure("JSON credential file path not configured");
        }

        try
        {
            if (!File.Exists(credentialsPath))
            {
                _logger.LogDebug("🔍 JSON credential file not found: {Path}", credentialsPath);
                return Result<GoogleDriveCredentials>.WithFailure($"JSON credential file not found: {credentialsPath}");
            }

            var jsonContent = await File.ReadAllTextAsync(credentialsPath, cancellationToken).ConfigureAwait(false);
            
            // Try to parse as OAuth Desktop Application format (like your current file)
            var desktopCredentials = TryParseDesktopCredentials(jsonContent);
            if (desktopCredentials.IsSuccess)
            {
                return desktopCredentials;
            }

            // Try to parse as Service Account format
            var serviceAccountCredentials = TryParseServiceAccountCredentials(jsonContent);
            if (serviceAccountCredentials.IsSuccess)
            {
                return serviceAccountCredentials;
            }

            // Try to parse as simple OAuth format
            var simpleCredentials = TryParseSimpleCredentials(jsonContent);
            if (simpleCredentials.IsSuccess)
            {
                return simpleCredentials;
            }

            _logger.LogWarning("⚠️ JSON credential file format not recognized");
            return Result<GoogleDriveCredentials>.WithFailure("JSON credential file format not recognized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error reading JSON credential file: {Path}", credentialsPath);
            return Result<GoogleDriveCredentials>.WithFailure($"Error reading JSON credential file: {ex.Message}");
        }
    }

    /// <summary>
    /// Try to parse OAuth Desktop Application credentials (your current format)
    /// </summary>
    private Result<GoogleDriveCredentials> TryParseDesktopCredentials(string jsonContent)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            if (root.TryGetProperty("installed", out var installed))
            {
                if (installed.TryGetProperty("client_id", out var clientIdElement) &&
                    installed.TryGetProperty("client_secret", out var clientSecretElement))
                {
                    var clientId = clientIdElement.GetString();
                    var clientSecret = clientSecretElement.GetString();

                    if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
                    {
                        return Result<GoogleDriveCredentials>.WithSuccess(new GoogleDriveCredentials
                        {
                            ClientId = clientId,
                            ClientSecret = clientSecret,
                            Source = "JSON File (Desktop App)"
                        });
                    }
                }
            }

            return Result<GoogleDriveCredentials>.WithFailure("Desktop credentials format not valid");
        }
        catch (Exception ex)
        {
            _logger.LogDebug("🔍 Not a valid desktop credentials format: {Error}", ex.Message);
            return Result<GoogleDriveCredentials>.WithFailure($"Desktop credentials parsing failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Try to parse Service Account credentials
    /// </summary>
    private Result<GoogleDriveCredentials> TryParseServiceAccountCredentials(string jsonContent)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            if (root.TryGetProperty("type", out var type) && type.GetString() == "service_account")
            {
                // For service accounts, we need different handling
                // This is a different authentication flow
                _logger.LogInformation("🔍 Service Account credentials detected - requires different authentication flow");
                return Result<GoogleDriveCredentials>.WithFailure("Service Account credentials require different authentication flow");
            }

            return Result<GoogleDriveCredentials>.WithFailure("Not a service account credential");
        }
        catch (Exception ex)
        {
            _logger.LogDebug("🔍 Not a valid service account format: {Error}", ex.Message);
            return Result<GoogleDriveCredentials>.WithFailure($"Service account parsing failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Try to parse simple OAuth credentials
    /// </summary>
    private Result<GoogleDriveCredentials> TryParseSimpleCredentials(string jsonContent)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            if (root.TryGetProperty("client_id", out var clientIdElement) &&
                root.TryGetProperty("client_secret", out var clientSecretElement))
            {
                var clientId = clientIdElement.GetString();
                var clientSecret = clientSecretElement.GetString();

                if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
                {
                    return Result<GoogleDriveCredentials>.WithSuccess(new GoogleDriveCredentials
                    {
                        ClientId = clientId,
                        ClientSecret = clientSecret,
                        Source = "JSON File (Simple)"
                    });
                }
            }

            return Result<GoogleDriveCredentials>.WithFailure("Simple credentials format not valid");
        }
        catch (Exception ex)
        {
            _logger.LogDebug("🔍 Not a valid simple credentials format: {Error}", ex.Message);
            return Result<GoogleDriveCredentials>.WithFailure($"Simple credentials parsing failed: {ex.Message}");
        }
    }
}


/// <summary>
/// Represents resolved Google Drive credentials
/// </summary>
public class GoogleDriveCredentials
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string? RedirectUri { get; set; }
    public string? ProjectId { get; set; }
} 