namespace ExxerAI.Domain.Tests;

/// <summary>
/// Unit tests for ModelCapabilities value object
/// </summary>
public class ModelCapabilitiesTests
{
    [Fact]
    public void Should_InitializeDefaults_When_Created()
    {
        // Arrange & Act
        var capabilities = new ModelCapabilities();

        // Assert
        capabilities.SupportsTextGeneration.ShouldBeTrue();
        capabilities.SupportsCodeGeneration.ShouldBeFalse();
        capabilities.SupportsImageAnalysis.ShouldBeFalse();
        capabilities.SupportsFunctionCalling.ShouldBeFalse();
        capabilities.SupportsStreaming.ShouldBeTrue();
        capabilities.MaxOutputTokens.ShouldBe(2048);
        capabilities.SupportedFormats.ShouldNotBeNull();
        capabilities.SupportedFormats.ShouldContain("text");
    }

    [Fact]
    public void Should_AllowCapabilityConfiguration_When_Modified()
    {
        // Arrange
        var capabilities = new ModelCapabilities();

        // Act
        capabilities.SupportsCodeGeneration = true;
        capabilities.SupportsImageAnalysis = true;
        capabilities.MaxOutputTokens = 4096;

        // Assert
        capabilities.SupportsCodeGeneration.ShouldBeTrue();
        capabilities.SupportsImageAnalysis.ShouldBeTrue();
        capabilities.MaxOutputTokens.ShouldBe(4096);
    }
}