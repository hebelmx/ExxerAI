namespace ExxerAI.Domain.Tests;

/// <summary>
/// Unit tests for LanguageModel domain entity
/// </summary>
public class LanguageModelTests
{
    [Fact]
    public void Should_CreateLanguageModel_When_ValidDataProvided()
    {
        // Arrange & Act
        var model = new LanguageModel
        {
            Name = "gpt-4",
            Provider = "OpenAI",
            Version = "2023-07-01",
            ContextWindowSize = 8192,
            IsAvailable = true
        };

        // Assert
        model.Id.ShouldNotBe(Guid.Empty);
        model.Name.ShouldBe("gpt-4");
        model.Provider.ShouldBe("OpenAI");
        model.Version.ShouldBe("2023-07-01");
        model.ContextWindowSize.ShouldBe(8192);
        model.IsAvailable.ShouldBeTrue();
        model.Capabilities.ShouldNotBeNull();
        model.Configuration.ShouldNotBeNull();
    }

    [Fact]
    public void Should_InitializeDefaultCapabilities_When_ModelCreated()
    {
        // Arrange & Act
        var model = new LanguageModel();

        // Assert
        model.Capabilities.ShouldNotBeNull();
        model.Capabilities.SupportsTextGeneration.ShouldBeTrue();
        model.Capabilities.MaxOutputTokens.ShouldBe(2048);
        model.Capabilities.SupportedFormats.ShouldContain("text");
    }

    [Fact]
    public void Should_InitializeDefaultConfiguration_When_ModelCreated()
    {
        // Arrange & Act
        var model = new LanguageModel();

        // Assert
        model.Configuration.ShouldNotBeNull();
        model.Configuration.DefaultTemperature.ShouldBe(0.7);
        model.Configuration.MaxTokensPerRequest.ShouldBe(1000);
        model.Configuration.RequestTimeoutSeconds.ShouldBe(30);
        model.Configuration.RateLimitPerMinute.ShouldBe(60);
    }
}