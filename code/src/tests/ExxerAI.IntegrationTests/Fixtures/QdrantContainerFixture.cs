using Testcontainers.Qdrant;
using Microsoft.Extensions.Logging;

namespace ExxerAI.IntegrationTests.Fixtures;

/// <summary>
/// Test fixture for Qdrant vector database container.
/// Provides a real Qdrant instance for integration testing.
/// </summary>
public class QdrantContainerFixture : IAsyncLifetime
{
    private QdrantContainer? _qdrantContainer;
    private readonly ILogger<QdrantContainerFixture> _logger;

    public string QdrantUrl { get; private set; } = string.Empty;
    public int QdrantPort { get; private set; }
    public bool IsAvailable { get; private set; }

    public QdrantContainerFixture()
    {
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<QdrantContainerFixture>();
    }

    public async ValueTask InitializeAsync()
    {
        try
        {
            _logger.LogInformation("🐳 Starting Qdrant container for integration tests...");

            // Build Qdrant container using the specific Qdrant testcontainer
            _qdrantContainer = new QdrantBuilder()
                .WithImage("qdrant/qdrant:v1.13.4")
                .Build();

            // Start the container
            await _qdrantContainer.StartAsync();

            // Get connection details
            QdrantPort = _qdrantContainer.GetMappedPublicPort(6333);
            QdrantUrl = $"http://localhost:{QdrantPort}";
            
            _logger.LogInformation("✅ Qdrant container started at {QdrantUrl}", QdrantUrl);

            // Verify container is healthy
            await VerifyQdrantHealthAsync();
            
            IsAvailable = true;
            _logger.LogInformation("🎯 Qdrant container fixture ready for testing");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to start Qdrant container");
            IsAvailable = false;
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_qdrantContainer != null)
        {
            _logger.LogInformation("🧹 Cleaning up Qdrant container...");
            
            try
            {
                await _qdrantContainer.StopAsync();
                await _qdrantContainer.DisposeAsync();
                _logger.LogInformation("✅ Qdrant container cleaned up successfully");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error during Qdrant container cleanup");
            }
        }
    }

    /// <summary>
    /// Verify that Qdrant is responding to health checks
    /// </summary>
    private async Task VerifyQdrantHealthAsync()
    {
        using var httpClient = new HttpClient();
        var maxRetries = 10;
        var retryDelay = TimeSpan.FromSeconds(2);

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var response = await httpClient.GetAsync($"{QdrantUrl}/");
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("✅ Qdrant health check passed");
                    return;
                }
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                _logger.LogDebug("🔄 Qdrant health check attempt {Attempt}/{MaxRetries} failed: {Error}", 
                    i + 1, maxRetries, ex.Message);
                await Task.Delay(retryDelay);
            }
        }

        throw new InvalidOperationException($"Qdrant container failed health check after {maxRetries} attempts");
    }

    /// <summary>
    /// Ensures the container is available for testing
    /// </summary>
    public void EnsureAvailable()
    {
        if (!IsAvailable)
        {
            throw new InvalidOperationException(
                "Qdrant container is not available. " +
                "This test requires Docker to be running and accessible. " +
                "Please ensure Docker is installed and running, then try again.");
        }
    }

    /// <summary>
    /// Get connection configuration for Qdrant client
    /// </summary>
    public QdrantConnectionConfig GetConnectionConfig()
    {
        EnsureAvailable();
        return new QdrantConnectionConfig
        {
            Host = "localhost",
            Port = QdrantPort,
            HttpUrl = QdrantUrl,
            IsSecure = false
        };
    }
}

/// <summary>
/// Configuration for connecting to Qdrant test instance
/// </summary>
public class QdrantConnectionConfig
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string HttpUrl { get; set; } = string.Empty;
    public bool IsSecure { get; set; }
}