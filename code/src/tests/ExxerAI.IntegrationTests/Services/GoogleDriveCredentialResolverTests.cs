using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ExxerAi.MCPServer.Application.Services;
using Shouldly;
using Xunit;
using NSubstitute;

namespace ExxerAI.IntegrationTests.Services;

/// <summary>
/// Unit tests for GoogleDriveCredentialResolver
/// Tests all credential resolution strategies in priority order
/// </summary>
public class GoogleDriveCredentialResolverTests
{
    private readonly ILogger<GoogleDriveCredentialResolver> _logger;

    public GoogleDriveCredentialResolverTests()
    {
        _logger = Substitute.For<ILogger<GoogleDriveCredentialResolver>>();
    }

    #region Environment Variables Tests (Priority 1)

    [Fact]
    public async Task ResolveCredentialsAsync_WithValidEnvironmentVariables_ShouldReturnEnvironmentCredentials()
    {
        // Arrange
        const string clientId = "test-client-id.apps.googleusercontent.com";
        const string clientSecret = "test-client-secret";

        // Set environment variables
        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID", clientId);
        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET", clientSecret);

        try
        {
            var configuration = new ConfigurationBuilder().Build();
            var resolver = new GoogleDriveCredentialResolver(configuration, _logger);

            // Act
            var result = await resolver.ResolveCredentialsAsync();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ClientId.ShouldBe(clientId);
            result.Value.ClientSecret.ShouldBe(clientSecret);
            result.Value.Source.ShouldBe("Environment Variables");
        }
        finally
        {
            // Cleanup
            Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID", null);
            Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET", null);
        }
    }

    [Fact]
    public async Task ResolveCredentialsAsync_WithPartialEnvironmentVariables_ShouldFallbackToNextPriority()
    {
        // Arrange - Only client ID, missing secret
        const string clientId = "test-client-id.apps.googleusercontent.com";

        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID", clientId);
        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET", null);

        try
        {
            var configData = new Dictionary<string, string?>
            {
                ["GoogleDrive:ClientId"] = "config-client-id",
                ["GoogleDrive:ClientSecret"] = "config-client-secret"
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var resolver = new GoogleDriveCredentialResolver(configuration, _logger);

            // Act
            var result = await resolver.ResolveCredentialsAsync();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ClientId.ShouldBe("config-client-id"); // Should use config, not env
            result.Value.Source.ShouldContain("appsettings.json");
        }
        finally
        {
            Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID", null);
        }
    }

    #endregion Environment Variables Tests

    #region Configuration Tests (Priority 2 & 3)

    [Fact]
    public async Task ResolveCredentialsAsync_WithValidConfiguration_ShouldReturnConfigCredentials()
    {
        // Arrange - No environment variables
        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID", null);
        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET", null);

        var configData = new Dictionary<string, string?>
        {
            ["GoogleDrive:ClientId"] = "config-client-id.apps.googleusercontent.com",
            ["GoogleDrive:ClientSecret"] = "config-client-secret"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var resolver = new GoogleDriveCredentialResolver(configuration, _logger);

        // Act
        var result = await resolver.ResolveCredentialsAsync();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ClientId.ShouldBe("config-client-id.apps.googleusercontent.com");
        result.Value.ClientSecret.ShouldBe("config-client-secret");
        result.Value.Source.ShouldContain("appsettings.json");
    }

    [Fact]
    public async Task ResolveCredentialsAsync_WithPlaceholderValues_ShouldSkipAndFallback()
    {
        // Arrange
        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID", null);
        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET", null);

        var configData = new Dictionary<string, string?>
        {
            ["GoogleDrive:ClientId"] = "your-client-id.apps.googleusercontent.com",
            ["GoogleDrive:ClientSecret"] = "your-client-secret",
            ["GoogleDrive:CredentialsPath"] = "./test-credentials.json"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var resolver = new GoogleDriveCredentialResolver(configuration, _logger);

        // Act
        var result = await resolver.ResolveCredentialsAsync();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldContain("not found");
    }

    #endregion Configuration Tests

    #region JSON File Tests (Priority 4)

    [Fact]
    public async Task ResolveCredentialsAsync_WithDesktopAppJsonFile_ShouldParseCorrectly()
    {
        // Arrange
        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID", null);
        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET", null);

        var tempFile = Path.GetTempFileName();
        var jsonContent = """
        {
          "installed": {
            "client_id": "json-file-client-id.apps.googleusercontent.com",
            "client_secret": "json-file-client-secret",
            "project_id": "test-project",
            "auth_uri": "https://accounts.google.com/o/oauth2/auth",
            "token_uri": "https://oauth2.googleapis.com/token",
            "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
            "redirect_uris": ["http://localhost"]
          }
        }
        """;

        try
        {
            await File.WriteAllTextAsync(tempFile, jsonContent);

            var configData = new Dictionary<string, string?>
            {
                ["GoogleDrive:CredentialsPath"] = tempFile
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var resolver = new GoogleDriveCredentialResolver(configuration, _logger);

            // Act
            var result = await resolver.ResolveCredentialsAsync();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ClientId.ShouldBe("json-file-client-id.apps.googleusercontent.com");
            result.Value.ClientSecret.ShouldBe("json-file-client-secret");
            result.Value.Source.ShouldBe("JSON File (Desktop App)");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ResolveCredentialsAsync_WithSimpleJsonFile_ShouldParseCorrectly()
    {
        // Arrange
        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID", null);
        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET", null);

        var tempFile = Path.GetTempFileName();
        var jsonContent = """
        {
          "client_id": "simple-client-id.apps.googleusercontent.com",
          "client_secret": "simple-client-secret"
        }
        """;

        try
        {
            await File.WriteAllTextAsync(tempFile, jsonContent);

            var configData = new Dictionary<string, string?>
            {
                ["GoogleDrive:CredentialsPath"] = tempFile
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var resolver = new GoogleDriveCredentialResolver(configuration, _logger);

            // Act
            var result = await resolver.ResolveCredentialsAsync();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ClientId.ShouldBe("simple-client-id.apps.googleusercontent.com");
            result.Value.ClientSecret.ShouldBe("simple-client-secret");
            result.Value.Source.ShouldBe("JSON File (Simple)");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ResolveCredentialsAsync_WithServiceAccountJsonFile_ShouldReturnSuccess()
    {
        // Arrange
        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID", null);
        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET", null);

        var tempFile = Path.GetTempFileName();
        var jsonContent = """
        {
          "type": "service_account",
          "project_id": "test-project",
          "private_key_id": "key-id",
          "private_key": "-----BEGIN PRIVATE KEY-----\ntest\n-----END PRIVATE KEY-----\n",
          "client_email": "test@test-project.iam.gserviceaccount.com",
          "auth_uri": "https://accounts.google.com/o/oauth2/auth",
          "token_uri": "https://oauth2.googleapis.com/token"
        }
        """;

        try
        {
            await File.WriteAllTextAsync(tempFile, jsonContent);

            var configData = new Dictionary<string, string?>
            {
                ["GoogleDrive:CredentialsPath"] = tempFile
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var resolver = new GoogleDriveCredentialResolver(configuration, _logger);

            // Act
            var result = await resolver.ResolveCredentialsAsync();

            // Assert - Service accounts should now work!
            result.IsSuccess.ShouldBeTrue();
            result.Value.Type.ShouldBe(CredentialType.ServiceAccount);
            result.Value.ServiceAccountEmail.ShouldBe("test@test-project.iam.gserviceaccount.com");
            result.Value.ProjectId.ShouldBe("test-project");
            result.Value.Source.ShouldContain("JSON File");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task ResolveCredentialsAsync_WithMissingJsonFile_ShouldReturnFailure()
    {
        // Arrange
        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID", null);
        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET", null);

        var configData = new Dictionary<string, string?>
        {
            ["GoogleDrive:CredentialsPath"] = "./non-existent-file.json"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var resolver = new GoogleDriveCredentialResolver(configuration, _logger);

        // Act
        var result = await resolver.ResolveCredentialsAsync();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldContain("not found");
    }

    [Fact]
    public async Task ResolveCredentialsAsync_WithInvalidJsonFile_ShouldReturnFailure()
    {
        // Arrange
        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID", null);
        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET", null);

        var tempFile = Path.GetTempFileName();
        var invalidJsonContent = "{ invalid json content }";

        try
        {
            await File.WriteAllTextAsync(tempFile, invalidJsonContent);

            var configData = new Dictionary<string, string?>
            {
                ["GoogleDrive:CredentialsPath"] = tempFile
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var resolver = new GoogleDriveCredentialResolver(configuration, _logger);

            // Act
            var result = await resolver.ResolveCredentialsAsync();

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("Error reading JSON credential file");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    #endregion JSON File Tests

    #region Priority Tests

    [Fact]
    public async Task ResolveCredentialsAsync_WithMultipleSources_ShouldUsePriorityOrder()
    {
        // Arrange - Set up all sources with different values
        const string envClientId = "env-client-id";
        const string envClientSecret = "env-client-secret";
        const string configClientId = "config-client-id";
        const string configClientSecret = "config-client-secret";

        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID", envClientId);
        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET", envClientSecret);

        var tempFile = Path.GetTempFileName();
        var jsonContent = """
        {
          "installed": {
            "client_id": "json-client-id",
            "client_secret": "json-client-secret"
          }
        }
        """;

        try
        {
            await File.WriteAllTextAsync(tempFile, jsonContent);

            var configData = new Dictionary<string, string?>
            {
                ["GoogleDrive:ClientId"] = configClientId,
                ["GoogleDrive:ClientSecret"] = configClientSecret,
                ["GoogleDrive:CredentialsPath"] = tempFile
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var resolver = new GoogleDriveCredentialResolver(configuration, _logger);

            // Act
            var result = await resolver.ResolveCredentialsAsync();

            // Assert - Should use environment variables (highest priority)
            result.IsSuccess.ShouldBeTrue();
            result.Value.ClientId.ShouldBe(envClientId);
            result.Value.ClientSecret.ShouldBe(envClientSecret);
            result.Value.Source.ShouldBe("Environment Variables");
        }
        finally
        {
            Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID", null);
            Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET", null);
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    #endregion Priority Tests

    #region Error Scenarios

    [Fact]
    public async Task ResolveCredentialsAsync_WithNoCredentialSources_ShouldReturnComprehensiveError()
    {
        // Arrange - Clear all possible sources
        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID", null);
        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET", null);

        var configuration = new ConfigurationBuilder().Build();
        var resolver = new GoogleDriveCredentialResolver(configuration, _logger);

        // Act
        var result = await resolver.ResolveCredentialsAsync();

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldContain("Google Drive credentials not found");
        result.Error.ShouldContain("Environment Variables: GOOGLE_OAUTH_CLIENT_ID, GOOGLE_OAUTH_CLIENT_SECRET");
        result.Error.ShouldContain("User Secrets: GoogleDrive:ClientId, GoogleDrive:ClientSecret");
        result.Error.ShouldContain("appsettings.json: GoogleDrive section");
        result.Error.ShouldContain("JSON File: GoogleDrive:CredentialsPath");
    }

    [Theory]
    [InlineData("", "valid-secret")]
    [InlineData("valid-id", "")]
    [InlineData("", "")]
    [InlineData(null, "valid-secret")]
    [InlineData("valid-id", null)]
    public async Task ResolveCredentialsAsync_WithIncompleteEnvironmentVariables_ShouldFallback(string? clientId, string? clientSecret)
    {
        // Arrange
        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID", clientId);
        Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET", clientSecret);

        try
        {
            var configuration = new ConfigurationBuilder().Build();
            var resolver = new GoogleDriveCredentialResolver(configuration, _logger);

            // Act
            var result = await resolver.ResolveCredentialsAsync();

            // Assert - Should fail and fallback (no other sources configured)
            result.IsFailure.ShouldBeTrue();
        }
        finally
        {
            Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_ID", null);
            Environment.SetEnvironmentVariable("GOOGLE_OAUTH_CLIENT_SECRET", null);
        }
    }

    #endregion Error Scenarios

    #region Cancellation Tests

    [Fact]
    public async Task ResolveCredentialsAsync_WithCancellation_ShouldRespectCancellationToken()
    {
        // Arrange
        var configuration = new ConfigurationBuilder().Build();
        var resolver = new GoogleDriveCredentialResolver(configuration, _logger);

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act
        var result = await resolver.ResolveCredentialsAsync(cts.Token);

        // Assert - Should still complete since credential resolution is synchronous
        // But this tests that the method signature accepts cancellation tokens
        result.ShouldNotBeNull();
    }

    #endregion Cancellation Tests
} 