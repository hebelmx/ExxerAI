namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Unit tests for RegexPattern extraction pattern
/// </summary>
public class RegexPatternTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_RegexPatternCreated()
    {
        // Act
        var pattern = new RegexPattern();

        // Assert
        pattern.Id.ShouldNotBeNullOrEmpty();
        pattern.Name.ShouldBe(string.Empty);
        pattern.Confidence.ShouldBe(1.0f);
        pattern.IsActive.ShouldBeTrue();
        pattern.Metadata.ShouldNotBeNull();
        pattern.Metadata.ShouldBeEmpty();
        pattern.Pattern.ShouldBe(string.Empty);
        pattern.Options.ShouldBe(System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    [Fact]
    public void Should_CreateRegexPatternWithConstructor_When_ParametersProvided()
    {
        // Arrange
        var regexPattern = @"(?:PERIODO|PERIOD)[:\s]*(\d{2}-\d{4})";
        var confidence = 0.9f;

        // Act
        var pattern = new RegexPattern(regexPattern, confidence);

        // Assert
        pattern.Pattern.ShouldBe(regexPattern);
        pattern.Confidence.ShouldBe(confidence);
    }

    [Theory]
    [InlineData("PERIODO: 12-2023", @"(?:PERIODO|PERIOD)[:\s]*(\d{2}-\d{4})", "12-2023")]
    [InlineData("PERIOD: 01-2024", @"(?:PERIODO|PERIOD)[:\s]*(\d{2}-\d{4})", "01-2024")]
    [InlineData("TOTAL: $15,000.00", @"(?:TOTAL|IMPORTE)[:\s]*\$?([0-9,]+\.?\d*)", "15,000.00")]
    [InlineData("IMPORTE $25,500", @"(?:TOTAL|IMPORTE)[:\s]*\$?([0-9,]+\.?\d*)", "25,500")]
    public void Should_ExtractValue_When_RegexPatternMatches(string text, string regexPattern, string expectedValue)
    {
        // Arrange
        var pattern = new RegexPattern(regexPattern, 0.9f);
        var context = new ExtractionContext();

        // Act
        var result = pattern.ExtractValue(text, context);

        // Assert
        result.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData("NO MATCH TEXT", @"(?:PERIODO|PERIOD)[:\s]*(\d{2}-\d{4})")]
    [InlineData("DIFFERENT PATTERN", @"(?:TOTAL|IMPORTE)[:\s]*\$?([0-9,]+\.?\d*)")]
    public void Should_ReturnNull_When_RegexPatternDoesNotMatch(string text, string regexPattern)
    {
        // Arrange
        var pattern = new RegexPattern(regexPattern, 0.9f);
        var context = new ExtractionContext();

        // Act
        var result = pattern.ExtractValue(text, context);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Should_HandleInvalidRegex_When_RegexPatternIsInvalid()
    {
        // Arrange
        var invalidPattern = @"[unclosed bracket";
        var pattern = new RegexPattern(invalidPattern, 0.9f);
        var context = new ExtractionContext();

        // Act
        var result = pattern.ExtractValue("test text", context);

        // Assert
        result.ShouldBeNull(); // Should handle regex errors gracefully
    }
}