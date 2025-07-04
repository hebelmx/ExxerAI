namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Unit tests for GroundingReport class
/// </summary>
public class GroundingReportTests
{
    /// <summary>
    /// Tests GroundingReport default constructor initialization
    /// </summary>
    [Fact]
    public void Should_Initialize_With_Default_Values_When_Using_Default_Constructor()
    {
        // Act
        var report = new GroundingReport();

        // Assert
        report.TotalRecordsProcessed.ShouldBe(0);
        report.SuccessRate.ShouldBe(0m);
        report.AverageConfidenceScore.ShouldBe(0m);
        report.ConflictsResolved.ShouldBe(0);
        report.RecordsRequiringReview.ShouldBe(0);
    }

    /// <summary>
    /// Tests property round-trip for TotalRecordsProcessed
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]
    public void Should_Set_And_Get_TotalRecordsProcessed_When_Valid_Value_Provided(int totalRecords)
    {
        // Arrange
        var report = new GroundingReport();

        // Act
        report.TotalRecordsProcessed = totalRecords;

        // Assert
        report.TotalRecordsProcessed.ShouldBe(totalRecords);
    }

    /// <summary>
    /// Tests property round-trip for SuccessRate
    /// </summary>
    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(0.85)]
    [InlineData(1.0)]
    public void Should_Set_And_Get_SuccessRate_When_Valid_Value_Provided(double successRate)
    {
        // Arrange
        var report = new GroundingReport();
        var successRateDecimal = (decimal)successRate;

        // Act
        report.SuccessRate = successRateDecimal;

        // Assert
        report.SuccessRate.ShouldBe(successRateDecimal);
    }

    /// <summary>
    /// Tests all properties can be set together
    /// </summary>
    [Fact]
    public void Should_Set_All_Properties_When_Creating_Complete_GroundingReport()
    {
        // Act
        var report = new GroundingReport
        {
            TotalRecordsProcessed = 1500,
            SuccessRate = 0.92m,
            AverageConfidenceScore = 0.87m,
            ConflictsResolved = 8,
            RecordsRequiringReview = 12
        };

        // Assert
        report.TotalRecordsProcessed.ShouldBe(1500);
        report.SuccessRate.ShouldBe(0.92m);
        report.AverageConfidenceScore.ShouldBe(0.87m);
        report.ConflictsResolved.ShouldBe(8);
        report.RecordsRequiringReview.ShouldBe(12);
    }
}