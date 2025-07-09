namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Unit tests for KeywordPattern extraction pattern
/// </summary>
public class KeywordPatternTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_KeywordPatternCreated()
    {
        // Act
        var pattern = new KeywordPattern();

        // Assert
        pattern.Id.ShouldNotBeNullOrEmpty();
        pattern.Name.ShouldBe(string.Empty);
        pattern.Confidence.ShouldBe(1.0f);
        pattern.IsActive.ShouldBeTrue();
        pattern.Keyword.ShouldBe(string.Empty);
        pattern.Strategy.ShouldBe(PositionStrategy.NextToken);
    }

    [Fact]
    public void Should_CreateKeywordPatternWithConstructor_When_ParametersProvided()
    {
        // Arrange
        var keyword = "TOTAL";
        var strategy = PositionStrategy.NextToken;
        var confidence = 0.85f;

        // Act
        var pattern = new KeywordPattern(keyword, strategy, confidence);

        // Assert
        pattern.Keyword.ShouldBe(keyword);
        pattern.Strategy.ShouldBe(strategy);
        pattern.Confidence.ShouldBe(confidence);
    }

    [Theory]
    [InlineData("TOTAL $15,000.00", "TOTAL", nameof(PositionStrategy.NextToken), "$15,000.00")]
    [InlineData("AMOUNT 25000", "AMOUNT", nameof(PositionStrategy.NextToken), "25000")]
    [InlineData("PERIODO 12-2023 SIGUIENTE", "PERIODO", nameof(PositionStrategy.NextToken), "12-2023")]
    public void Should_ExtractNextToken_When_NextTokenStrategyUsed(
        string text, string keyword, string strategyName, string expectedValue)
    {
        // Arrange
        var strategy = Enum.Parse<PositionStrategy>(strategyName);
        var pattern = new KeywordPattern(keyword, strategy, 0.9f);
        var context = new ExtractionContext();

        // Act
        var result = pattern.ExtractValue(text, context);

        // Assert
        result.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData("TOTAL: Payment amount is $15,000.00", "TOTAL", nameof(PositionStrategy.SameLine), ": Payment amount is $15,000.00")]
    [InlineData("AMOUNT $25,000 for December", "AMOUNT", nameof(PositionStrategy.SameLine), "$25,000 for December")]
    public void Should_ExtractSameLine_When_SameLineStrategyUsed(
        string text, string keyword, string strategyName, string expectedValue)
    {
        // Arrange
        var strategy = Enum.Parse<PositionStrategy>(strategyName);
        var pattern = new KeywordPattern(keyword, strategy, 0.9f);
        var context = new ExtractionContext();

        // Act
        var result = pattern.ExtractValue(text, context);

        // Assert
        result.ShouldBe(expectedValue);
    }

    [Fact]
    public void Should_ExtractNextLine_When_NextLineStrategyUsed()
    {
        // Arrange
        var text = "PAYMENT PERIOD:\n12-2023\nOther text";
        var pattern = new KeywordPattern("PAYMENT PERIOD", PositionStrategy.NextLine, 0.9f);
        var context = new ExtractionContext();

        // Act
        var result = pattern.ExtractValue(text, context);

        // Assert
        result.ShouldBe("12-2023");
    }

    [Fact]
    public void Should_ReturnNull_When_KeywordNotFound()
    {
        // Arrange
        var text = "This text does not contain the keyword";
        var pattern = new KeywordPattern("MISSING", PositionStrategy.NextToken, 0.9f);
        var context = new ExtractionContext();

        // Act
        var result = pattern.ExtractValue(text, context);

        // Assert
        result.ShouldBeNull();
    }
}