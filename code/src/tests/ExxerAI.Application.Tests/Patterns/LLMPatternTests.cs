using ExxerAI.Application.Patterns;
using ExxerAI.Domain.DocumentProcessing;
using Shouldly;

namespace ExxerAI.Application.Tests.Patterns;

/// <summary>
/// Unit tests for LLMPattern class
/// </summary>
public class LLMPatternTests
{
    /// <summary>
    /// Tests that LLMPattern can be instantiated with default values
    /// </summary>
    [Fact]
    public void Should_InstantiateWithDefaultValues_When_Created()
    {
        // Act
        var pattern = new LLMPattern();

        // Assert
        pattern.ShouldNotBeNull();
        pattern.Prompt.ShouldBe(string.Empty);
        pattern.ExpectedFormat.ShouldBe(string.Empty);
    }

    /// <summary>
    /// Tests that LLMPattern properties can be set correctly
    /// </summary>
    [Fact]
    public void Should_SetPropertiesCorrectly_When_Assigned()
    {
        // Arrange
        const string expectedPrompt = "Extract the invoice number from the following text";
        const string expectedFormat = "JSON";

        // Act
        var pattern = new LLMPattern
        {
            Prompt = expectedPrompt,
            ExpectedFormat = expectedFormat
        };

        // Assert
        pattern.Prompt.ShouldBe(expectedPrompt);
        pattern.ExpectedFormat.ShouldBe(expectedFormat);
    }

    /// <summary>
    /// Tests that ExtractValue returns null as expected (placeholder implementation)
    /// </summary>
    [Fact]
    public void Should_ReturnNull_When_ExtractValueCalled()
    {
        // Arrange
        var pattern = new LLMPattern
        {
            Prompt = "Extract invoice number",
            ExpectedFormat = "JSON"
        };
        var context = new ExtractionContext();
        const string testText = "Invoice Number: INV-2024-001";

        // Act
        var result = pattern.ExtractValue(testText, context);

        // Assert
        result.ShouldBeNull();
    }

    /// <summary>
    /// Tests that LLMPattern inherits from ExtractionPattern correctly
    /// </summary>
    [Fact]
    public void Should_InheritFromExtractionPattern_When_Created()
    {
        // Act
        var pattern = new LLMPattern();

        // Assert
        pattern.ShouldBeAssignableTo<ExtractionPattern>();
    }

    /// <summary>
    /// Tests LLMPattern with various prompt configurations
    /// </summary>
    [Theory]
    [InlineData("", "")]
    [InlineData("Simple prompt", "")]
    [InlineData("", "JSON format")]
    [InlineData("Extract data from invoice", "JSON")]
    [InlineData("Find customer information in the document", "XML")]
    public void Should_HandleVariousPromptConfigurations_When_SetupWithDifferentValues(string prompt, string format)
    {
        // Act
        var pattern = new LLMPattern
        {
            Prompt = prompt,
            ExpectedFormat = format
        };

        // Assert
        pattern.Prompt.ShouldBe(prompt);
        pattern.ExpectedFormat.ShouldBe(format);
    }

    /// <summary>
    /// Tests that LLMPattern handles null context gracefully
    /// </summary>
    [Fact]
    public void Should_HandleNullContext_When_ExtractValueCalled()
    {
        // Arrange
        var pattern = new LLMPattern();
        const string testText = "Sample text";

        // Act & Assert
        Should.NotThrow(() => pattern.ExtractValue(testText, null!));
    }

    /// <summary>
    /// Tests that LLMPattern handles null or empty text gracefully
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_HandleNullOrEmptyText_When_ExtractValueCalled(string? text)
    {
        // Arrange
        var pattern = new LLMPattern();
        var context = new ExtractionContext();

        // Act & Assert
        Should.NotThrow(() => pattern.ExtractValue(text!, context));
    }
} 