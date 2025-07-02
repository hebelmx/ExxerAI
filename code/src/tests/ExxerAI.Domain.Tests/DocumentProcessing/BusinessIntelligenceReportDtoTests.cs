using ExxerAI.Domain.DocumentProcessing;
using Shouldly;

namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Unit tests for BusinessIntelligenceReport and related DTO classes
/// </summary>
public class BusinessIntelligenceReportDtoTests
{
    /// <summary>
    /// Tests BusinessIntelligenceReport default constructor initialization
    /// </summary>
    [Fact]
    public void Should_Initialize_With_Default_Values_When_Using_Default_Constructor()
    {
        // Act
        var report = new BusinessIntelligenceReport();

        // Assert
        report.ReportPeriod.ShouldNotBeNull();
        report.GroundingReport.ShouldNotBeNull();
        report.QualityMetrics.ShouldNotBeNull();
        report.GeneratedAt.ShouldBe(default(DateTime));
        report.GeneratedBy.ShouldBe(string.Empty);
        report.Insights.ShouldNotBeNull();
        report.Insights.ShouldBeEmpty();
        report.RecommendedActions.ShouldNotBeNull();
        report.RecommendedActions.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests property round-trip for GeneratedBy
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("ExxerAI.Agent")]
    [InlineData("DocumentProcessor")]
    [InlineData("BusinessIntelligenceService")]
    public void Should_Set_And_Get_GeneratedBy_When_Valid_String_Provided(string generatedBy)
    {
        // Arrange
        var report = new BusinessIntelligenceReport();

        // Act
        report.GeneratedBy = generatedBy;

        // Assert
        report.GeneratedBy.ShouldBe(generatedBy);
    }

    /// <summary>
    /// Tests Insights collection manipulation
    /// </summary>
    [Fact]
    public void Should_Add_And_Retrieve_Insights_When_Adding_Multiple_Items()
    {
        // Arrange
        var report = new BusinessIntelligenceReport();

        // Act
        report.Insights.Add("Processing speed increased by 25%");
        report.Insights.Add("Error rate reduced to 2%");
        report.Insights.Add("User satisfaction improved");

        // Assert
        report.Insights.Count.ShouldBe(3);
        report.Insights.ShouldContain("Processing speed increased by 25%");
        report.Insights.ShouldContain("Error rate reduced to 2%");
        report.Insights.ShouldContain("User satisfaction improved");
    }

    /// <summary>
    /// Tests RecommendedActions collection manipulation
    /// </summary>
    [Fact]
    public void Should_Add_And_Retrieve_RecommendedActions_When_Adding_Multiple_Items()
    {
        // Arrange
        var report = new BusinessIntelligenceReport();

        // Act
        report.RecommendedActions.Add("Increase processing capacity");
        report.RecommendedActions.Add("Review validation rules");
        report.RecommendedActions.Add("Update training data");

        // Assert
        report.RecommendedActions.Count.ShouldBe(3);
        report.RecommendedActions.ShouldContain("Increase processing capacity");
        report.RecommendedActions.ShouldContain("Review validation rules");
        report.RecommendedActions.ShouldContain("Update training data");
    }
}

/// <summary>
/// Unit tests for DateRange class
/// </summary>
public class DateRangeDtoTests
{
    /// <summary>
    /// Tests DateRange default constructor initialization
    /// </summary>
    [Fact]
    public void Should_Initialize_With_Default_Values_When_Using_Default_Constructor()
    {
        // Act
        var dateRange = new DateRange();

        // Assert
        dateRange.FromDate.ShouldBe(default(DateTime));
        dateRange.ToDate.ShouldBe(default(DateTime));
    }

    /// <summary>
    /// Tests property round-trip for FromDate
    /// </summary>
    [Theory]
    [InlineData("2024-01-01")]
    [InlineData("2024-06-15")]
    [InlineData("2024-12-31")]
    public void Should_Set_And_Get_FromDate_When_Valid_DateTime_Provided(string dateString)
    {
        // Arrange
        var dateRange = new DateRange();
        var fromDate = DateTime.Parse(dateString);

        // Act
        dateRange.FromDate = fromDate;

        // Assert
        dateRange.FromDate.ShouldBe(fromDate);
    }

    /// <summary>
    /// Tests property round-trip for ToDate
    /// </summary>
    [Theory]
    [InlineData("2024-01-01")]
    [InlineData("2024-06-15")]
    [InlineData("2024-12-31")]
    public void Should_Set_And_Get_ToDate_When_Valid_DateTime_Provided(string dateString)
    {
        // Arrange
        var dateRange = new DateRange();
        var toDate = DateTime.Parse(dateString);

        // Act
        dateRange.ToDate = toDate;

        // Assert
        dateRange.ToDate.ShouldBe(toDate);
    }

    /// <summary>
    /// Tests both properties can be set together
    /// </summary>
    [Fact]
    public void Should_Set_Both_Dates_When_Creating_Complete_DateRange()
    {
        // Arrange
        var fromDate = new DateTime(2024, 1, 1);
        var toDate = new DateTime(2024, 12, 31);

        // Act
        var dateRange = new DateRange
        {
            FromDate = fromDate,
            ToDate = toDate
        };

        // Assert
        dateRange.FromDate.ShouldBe(fromDate);
        dateRange.ToDate.ShouldBe(toDate);
    }
}

/// <summary>
/// Unit tests for GroundingReport class
/// </summary>
public class GroundingReportDtoTests
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