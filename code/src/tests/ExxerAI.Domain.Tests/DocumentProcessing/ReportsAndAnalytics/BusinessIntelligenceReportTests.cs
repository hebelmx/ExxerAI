namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Unit tests for BusinessIntelligenceReport and related classes
/// </summary>
public class BusinessIntelligenceReportTests
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
    /// Tests property round-trip for ReportPeriod
    /// </summary>
    [Fact]
    public void Should_Set_And_Get_ReportPeriod_When_Valid_DateRange_Provided()
    {
        // Arrange
        var report = new BusinessIntelligenceReport();
        var dateRange = new DateRange 
        { 
            FromDate = new DateTime(2024, 1, 1), 
            ToDate = new DateTime(2024, 12, 31) 
        };

        // Act
        report.ReportPeriod = dateRange;

        // Assert
        report.ReportPeriod.ShouldBe(dateRange);
        report.ReportPeriod.FromDate.ShouldBe(new DateTime(2024, 1, 1));
        report.ReportPeriod.ToDate.ShouldBe(new DateTime(2024, 12, 31));
    }

    /// <summary>
    /// Tests property round-trip for GroundingReport
    /// </summary>
    [Fact]
    public void Should_Set_And_Get_GroundingReport_When_Valid_Report_Provided()
    {
        // Arrange
        var report = new BusinessIntelligenceReport();
        var groundingReport = new GroundingReport
        {
            TotalRecordsProcessed = 1000,
            SuccessRate = 0.95m,
            AverageConfidenceScore = 0.85m,
            ConflictsResolved = 5,
            RecordsRequiringReview = 10
        };

        // Act
        report.GroundingReport = groundingReport;

        // Assert
        report.GroundingReport.ShouldBe(groundingReport);
        report.GroundingReport.TotalRecordsProcessed.ShouldBe(1000);
        report.GroundingReport.SuccessRate.ShouldBe(0.95m);
    }

    /// <summary>
    /// Tests property round-trip for QualityMetrics
    /// </summary>
    [Fact]
    public void Should_Set_And_Get_QualityMetrics_When_Valid_Metrics_Provided()
    {
        // Arrange
        var report = new BusinessIntelligenceReport();
        var qualityMetrics = new DataQualityMetrics
        {
            OverallQualityScore = 0.90m,
            CompletenessScore = 0.88m,
            AccuracyScore = 0.92m,
            ConsistencyScore = 0.87m
        };

        // Act
        report.QualityMetrics = qualityMetrics;

        // Assert
        report.QualityMetrics.ShouldBe(qualityMetrics);
        report.QualityMetrics.OverallQualityScore.ShouldBe(0.90m);
        report.QualityMetrics.AccuracyScore.ShouldBe(0.92m);
    }

    /// <summary>
    /// Tests property round-trip for GeneratedAt
    /// </summary>
    [Fact]
    public void Should_Set_And_Get_GeneratedAt_When_Valid_DateTime_Provided()
    {
        // Arrange
        var report = new BusinessIntelligenceReport();
        var generatedAt = new DateTime(2024, 7, 1, 15, 30, 45);

        // Act
        report.GeneratedAt = generatedAt;

        // Assert
        report.GeneratedAt.ShouldBe(generatedAt);
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

    /// <summary>
    /// Tests complete report creation with all properties
    /// </summary>
    [Fact]
    public void Should_Set_All_Properties_When_Creating_Complete_Report()
    {
        // Arrange
        var dateRange = new DateRange { FromDate = DateTime.Today.AddDays(-30), ToDate = DateTime.Today };
        var groundingReport = new GroundingReport { TotalRecordsProcessed = 500 };
        var qualityMetrics = new DataQualityMetrics { OverallQualityScore = 0.80m };
        var insights = new List<string> { "Insight 1", "Insight 2" };
        var actions = new List<string> { "Action 1", "Action 2" };

        // Act
        var report = new BusinessIntelligenceReport
        {
            ReportPeriod = dateRange,
            GroundingReport = groundingReport,
            QualityMetrics = qualityMetrics,
            GeneratedAt = DateTime.UtcNow,
            GeneratedBy = "TestSystem",
            Insights = insights,
            RecommendedActions = actions
        };

        // Assert
        report.ReportPeriod.ShouldBe(dateRange);
        report.GroundingReport.ShouldBe(groundingReport);
        report.QualityMetrics.ShouldBe(qualityMetrics);
        report.GeneratedBy.ShouldBe("TestSystem");
        report.Insights.ShouldBe(insights);
        report.RecommendedActions.ShouldBe(actions);
    }
} 