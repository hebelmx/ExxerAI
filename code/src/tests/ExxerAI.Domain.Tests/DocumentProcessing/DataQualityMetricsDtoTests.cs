using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Unit tests for DataQualityMetrics class
/// </summary>
public class DataQualityMetricsDtoTests
{
    /// <summary>
    /// Tests DataQualityMetrics default constructor initialization
    /// </summary>
    [Fact]
    public void Should_Initialize_With_Default_Values_When_Using_Default_Constructor()
    {
        // Act
        var metrics = new DataQualityMetrics();

        // Assert
        metrics.OverallQualityScore.ShouldBe(0m);
        metrics.CompletenessScore.ShouldBe(0m);
        metrics.AccuracyScore.ShouldBe(0m);
        metrics.ConsistencyScore.ShouldBe(0m);
    }

    /// <summary>
    /// Tests property round-trip for OverallQualityScore
    /// </summary>
    [Theory]
    [InlineData(0.0)]
    [InlineData(0.75)]
    [InlineData(0.90)]
    [InlineData(1.0)]
    public void Should_Set_And_Get_OverallQualityScore_When_Valid_Value_Provided(double score)
    {
        // Arrange
        var metrics = new DataQualityMetrics();
        var scoreDecimal = (decimal)score;

        // Act
        metrics.OverallQualityScore = scoreDecimal;

        // Assert
        metrics.OverallQualityScore.ShouldBe(scoreDecimal);
    }

    /// <summary>
    /// Tests all properties can be set together
    /// </summary>
    [Fact]
    public void Should_Set_All_Properties_When_Creating_Complete_DataQualityMetrics()
    {
        // Act
        var metrics = new DataQualityMetrics
        {
            OverallQualityScore = 0.88m,
            CompletenessScore = 0.91m,
            AccuracyScore = 0.85m,
            ConsistencyScore = 0.89m
        };

        // Assert
        metrics.OverallQualityScore.ShouldBe(0.88m);
        metrics.CompletenessScore.ShouldBe(0.91m);
        metrics.AccuracyScore.ShouldBe(0.85m);
        metrics.ConsistencyScore.ShouldBe(0.89m);
    }

    /// <summary>
    /// Tests boundary values for all score properties
    /// </summary>
    [Theory]
    [InlineData(-1.0)]
    [InlineData(0.0)]
    [InlineData(1.0)]
    [InlineData(2.0)]
    public void Should_Accept_Any_Score_Values_When_No_Validation_Applied(double score)
    {
        // Arrange
        var metrics = new DataQualityMetrics();
        var scoreDecimal = (decimal)score;

        // Act
        metrics.OverallQualityScore = scoreDecimal;
        metrics.CompletenessScore = scoreDecimal;
        metrics.AccuracyScore = scoreDecimal;
        metrics.ConsistencyScore = scoreDecimal;

        // Assert
        metrics.OverallQualityScore.ShouldBe(scoreDecimal);
        metrics.CompletenessScore.ShouldBe(scoreDecimal);
        metrics.AccuracyScore.ShouldBe(scoreDecimal);
        metrics.ConsistencyScore.ShouldBe(scoreDecimal);
    }
}