using ExxerAI.Orchestration.Configuration;

namespace ExxerAI.Orchestration.Tests;

/// <summary>
/// Unit tests for the ExxerAI Orchestration project
/// </summary>
/// <remarks>
/// Contains tests for workflow orchestration, agent scheduling, and multi-agent coordination.
/// This is a placeholder test class that will be expanded with comprehensive orchestration tests.
/// </remarks>
public class UnitTest1
{
    /// <summary>
    /// Placeholder test method for orchestration functionality
    /// </summary>
    /// <remarks>
    /// This test method is a placeholder that will be replaced with actual orchestration tests
    /// covering workflow execution, agent coordination, and task distribution.
    /// </remarks>

    public class SharedConfigTests
    {
        [Fact]
        public void SharedConfig_Binds_Correctly_From_Defaults()
        {
            var inMemorySettings = new Dictionary<string, string>
            {
                {"SharedConfig:TelemetryEnabled", "true"},
                {"SharedConfig:DefaultRedisPort", "6380"},
                {"SharedConfig:SqlConnection", "DataSource=test;"},
                {"SharedConfig:DefaultOllamaModel", "gemma-7b"},
            };

            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            var services = new ServiceCollection();
            services.Configure<SharedConfig>(config.GetSection("SharedConfig"));

            var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<SharedConfig>>().Value;

            options.DatabaseConfig.DatabaseName.ShouldBe("localai");
        }
    }
}