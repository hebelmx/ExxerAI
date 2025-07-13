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
            "1. Environment Variables: GOOGLE_API_KEY or GOOGLE_OAUTH_CLIENT_ID + GOOGLE_OAUTH_CLIENT_SECRET\n" +
            "2. User Secrets: GoogleDrive:ApiKey or GoogleDrive:ClientId + GoogleDrive:ClientSecret\n" +
            "3. appsettings.json: GoogleDrive section with ApiKey or ClientId + ClientSecret\n" +
            "4. JSON File: GoogleDrive:CredentialsPath pointing to API key or OAuth credentials file");
    }

    /// <summary>
    /// Try to get credentials from environment variables
    /// </summary>
    private Result<GoogleDriveCredentials> TryGetEnvironmentCredentials()
    {
        // Try Service Account JSON first (most comprehensive)
        var serviceAccountJson = Environment.GetEnvironmentVariable("GOOGLE_SERVICE_ACCOUNT_JSON");
        if (!string.IsNullOrEmpty(serviceAccountJson))
        {
            var serviceAccountResult = TryParseServiceAccountCredentials(serviceAccountJson);
            if (serviceAccountResult.IsSuccess)
            {
                var creds = serviceAccountResult.Value;
                creds.Source = "Environment Variables";
                return Result<GoogleDriveCredentials>.WithSuccess(creds);
            }
        }

        // Try API Key (simpler)
        var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
        if (!string.IsNullOrEmpty(apiKey))
        {
            return Result<GoogleDriveCredentials>.WithSuccess(new GoogleDriveCredentials
            {
                ApiKey = apiKey,
                Type = CredentialType.ApiKey,
                Source = "Environment Variables"
            });
        }

        // Try OAuth credentials
        var clientId = Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET");

        if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
        {
            return Result<GoogleDriveCredentials>.WithSuccess(new GoogleDriveCredentials
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                Type = CredentialType.OAuth,
                Source = "Environment Variables"
            });
        }

        _logger.LogDebug("🔍 Environment variables not found or incomplete");
        return Result<GoogleDriveCredentials>.WithFailure("Environment variables not configured");
    }

    /// <summary>
    /// Try to get credentials from user secrets
    /// </summary>
    private Result<GoogleDriveCredentials> TryGetUserSecretsCredentials()
    {
        // Try API Key first
        var apiKey = _configuration["GoogleDrive:ApiKey"];
        if (!string.IsNullOrEmpty(apiKey) && !apiKey.Contains("your-api-key"))
        {
            return Result<GoogleDriveCredentials>.WithSuccess(new GoogleDriveCredentials
            {
                ApiKey = apiKey,
                Type = CredentialType.ApiKey,
                Source = "User Secrets"
            });
        }

        // Try OAuth credentials
        var clientId = _configuration["GoogleDrive:ClientId"];
        var clientSecret = _configuration["GoogleDrive:ClientSecret"];

        if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
        {
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
                Type = CredentialType.OAuth,
                Source = "User Secrets"
            });
        }

        _logger.LogDebug("🔍 User secrets not found or incomplete");
        return Result<GoogleDriveCredentials>.WithFailure("User secrets not configured");
    }

    /// <summary>
    /// Try to get credentials from appsettings.json
    /// </summary>
    private Result<GoogleDriveCredentials> TryGetAppSettingsCredentials()
    {
        // Try API Key first
        var apiKey = _configuration["GoogleDrive:ApiKey"];
        if (!string.IsNullOrEmpty(apiKey) && !apiKey.Contains("your-api-key"))
        {
            return Result<GoogleDriveCredentials>.WithSuccess(new GoogleDriveCredentials
            {
                ApiKey = apiKey,
                Type = CredentialType.ApiKey,
                Source = "appsettings.json"
            });
        }

        // Try OAuth credentials
        var clientId = _configuration["GoogleDrive:ClientId"];
        var clientSecret = _configuration["GoogleDrive:ClientSecret"];

        if (!string.IsNullOrEmpty(clientId) && !string.IsNullOrEmpty(clientSecret))
        {
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
                Type = CredentialType.OAuth,
                Source = "appsettings.json"
            });
        }

        _logger.LogDebug("🔍 appsettings.json not found or incomplete");
        return Result<GoogleDriveCredentials>.WithFailure("appsettings.json not configured");
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

            // Try to parse as ExxerAI API Key format (your custom format)
            var exxerAiCredentials = TryParseExxerAiApiCredentials(jsonContent);
            if (exxerAiCredentials.IsSuccess)
            {
                return exxerAiCredentials;
            }

            // Try to parse as OAuth Desktop Application format
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
    /// Try to parse ExxerAI API credentials format (GDrive.Api.json)
    /// </summary>
    private Result<GoogleDriveCredentials> TryParseExxerAiApiCredentials(string jsonContent)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            // Look for the ExxerAI format: { "GDrive": { "ApiKey": "..." }, "GDriveService": { ... } }
            if (root.TryGetProperty("GDrive", out var gdriveElement))
            {
                string? apiKey = null;

                // Try "ApiKey" first (correct spelling)
                if (gdriveElement.TryGetProperty("ApiKey", out var apiKeyElement))
                {
                    apiKey = apiKeyElement.GetString();
                }
                // Try "ApiKe" (typo in the user's file)
                else if (gdriveElement.TryGetProperty("ApiKe", out var apiKeElement))
                {
                    apiKey = apiKeElement.GetString();
                }

                if (!string.IsNullOrEmpty(apiKey))
                {
                    var credentials = new GoogleDriveCredentials
                    {
                        ApiKey = apiKey,
                        Type = CredentialType.ApiKey,
                        Source = "JSON File (ExxerAI API Key)"
                    };

                    // Also extract service account info if available
                    if (root.TryGetProperty("GDriveService", out var serviceElement))
                    {
                        if (serviceElement.TryGetProperty("name", out var nameElement))
                        {
                            credentials.ServiceAccountName = nameElement.GetString() ?? string.Empty;
                        }
                        if (serviceElement.TryGetProperty("Email", out var emailElement))
                        {
                            credentials.ServiceAccountEmail = emailElement.GetString() ?? string.Empty;
                        }
                        if (serviceElement.TryGetProperty("UniqueID", out var uniqueIdElement))
                        {
                            credentials.ServiceAccountUniqueId = uniqueIdElement.GetString() ?? string.Empty;
                        }
                    }

                    return Result<GoogleDriveCredentials>.WithSuccess(credentials);
                }
            }

            return Result<GoogleDriveCredentials>.WithFailure("ExxerAI API credentials format not valid");
        }
        catch (Exception ex)
        {
            _logger.LogDebug("🔍 Not a valid ExxerAI API credentials format: {Error}", ex.Message);
            return Result<GoogleDriveCredentials>.WithFailure($"ExxerAI API credentials parsing failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Try to parse OAuth Desktop Application credentials
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
                            Type = CredentialType.OAuth,
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
                // Extract service account details
                var clientEmail = root.TryGetProperty("client_email", out var emailElement) ? emailElement.GetString() : null;
                var projectId = root.TryGetProperty("project_id", out var projectElement) ? projectElement.GetString() : null;
                var clientId = root.TryGetProperty("client_id", out var clientIdElement) ? clientIdElement.GetString() : null;
                var privateKeyId = root.TryGetProperty("private_key_id", out var keyIdElement) ? keyIdElement.GetString() : null;
                var privateKey = root.TryGetProperty("private_key", out var keyElement) ? keyElement.GetString() : null;

                if (!string.IsNullOrEmpty(clientEmail) && !string.IsNullOrEmpty(privateKey))
                {
                    var credentials = new GoogleDriveCredentials
                    {
                        Type = CredentialType.ServiceAccount,
                        Source = "JSON File (Service Account)",
                        ServiceAccountEmail = clientEmail,
                        ProjectId = projectId ?? string.Empty,
                        ServiceAccountUniqueId = clientId ?? string.Empty,
                        // Store the entire JSON content for service account usage
                        ServiceAccountJson = jsonContent
                    };

                    _logger.LogInformation("✅ Service Account credentials parsed successfully: {Email}", clientEmail);
                    return Result<GoogleDriveCredentials>.WithSuccess(credentials);
                }
                else
                {
                    _logger.LogWarning("⚠️ Service Account missing required fields (client_email or private_key)");
                    return Result<GoogleDriveCredentials>.WithFailure("Service Account missing required fields");
                }
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
                        Type = CredentialType.OAuth,
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
/// Represents resolved Google Drive credentials with modern ADC support
/// </summary>
public class GoogleDriveCredentials
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ServiceAccountEmail { get; set; } = string.Empty;
    public string ServiceAccountName { get; set; } = string.Empty;
    public string ServiceAccountUniqueId { get; set; } = string.Empty;
    public string ServiceAccountJson { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string? RedirectUri { get; set; }
    public string? ProjectId { get; set; }
    public CredentialType Type { get; set; } = CredentialType.OAuth;

    // Modern ADC properties
    public Google.Apis.Auth.OAuth2.GoogleCredential? GoogleCredential { get; set; }

    public bool IsScoped { get; set; }
}

/// <summary>
/// Types of Google Drive credentials
/// </summary>
public enum CredentialType
{
    OAuth,
    ApiKey,
    ServiceAccount,
    ApplicationDefault  // Modern ADC approach
}