using ExxerAI.Domain.DocumentProcessing;
using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Comprehensive tests for BusinessIntelligenceReport domain class following I-TDD principles.
/// Tests cover report generation, metrics calculation, insights management, and business rules.
/// </summary>
public class BusinessIntelligenceReportTests
{
    /// <summary>
    /// Contract Test: BusinessIntelligenceReport should initialize with defaults
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults_When_Created()
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
    /// Contract Test: BusinessIntelligenceReport properties should be settable
    /// </summary>
    [Fact]
    public void Properties_ShouldBeSettable_When_ValuesAssigned()
    {
        // Arrange
        var report = new BusinessIntelligenceReport();
        var reportPeriod = new DateRange 
        { 
            FromDate = DateTime.UtcNow.AddDays(-30), 
            ToDate = DateTime.UtcNow 
        };
        var groundingReport = new GroundingReport 
        { 
            TotalRecordsProcessed = 100, 
            SuccessRate = 0.95m 
        };
        var qualityMetrics = new DataQualityMetrics 
        { 
            OverallQualityScore = 0.88m 
        };
        var generatedAt = DateTime.UtcNow;
        var generatedBy = "ExxerAI-System";

        // Act
        report.ReportPeriod = reportPeriod;
        report.GroundingReport = groundingReport;
        report.QualityMetrics = qualityMetrics;
        report.GeneratedAt = generatedAt;
        report.GeneratedBy = generatedBy;

        // Assert
        report.ReportPeriod.ShouldBeSameAs(reportPeriod);
        report.GroundingReport.ShouldBeSameAs(groundingReport);
        report.QualityMetrics.ShouldBeSameAs(qualityMetrics);
        report.GeneratedAt.ShouldBe(generatedAt);
        report.GeneratedBy.ShouldBe(generatedBy);
    }

    /// <summary>
    /// Behavior Test: Insights collection should be modifiable
    /// </summary>
    [Fact]
    public void Insights_ShouldBeModifiable_When_InsightsAdded()
    {
        // Arrange
        var report = new BusinessIntelligenceReport();

        // Act
        report.Insights.Add("Invoice processing accuracy improved by 15%");
        report.Insights.Add("IMSS payment processing shows 99% success rate");
        report.Insights.Add("Tax document extraction confidence is above threshold");

        // Assert
        report.Insights.Count.ShouldBe(3);
        report.Insights.ShouldContain("Invoice processing accuracy improved by 15%");
        report.Insights.ShouldContain("IMSS payment processing shows 99% success rate");
        report.Insights.ShouldContain("Tax document extraction confidence is above threshold");
    }

    /// <summary>
    /// Behavior Test: RecommendedActions collection should be modifiable
    /// </summary>
    [Fact]
    public void RecommendedActions_ShouldBeModifiable_When_ActionsAdded()
    {
        // Arrange
        var report = new BusinessIntelligenceReport();

        // Act
        report.RecommendedActions.Add("Review failed extractions for pattern analysis");
        report.RecommendedActions.Add("Update validation rules for new document formats");
        report.RecommendedActions.Add("Increase confidence threshold for auto-processing");

        // Assert
        report.RecommendedActions.Count.ShouldBe(3);
        report.RecommendedActions.ShouldContain("Review failed extractions for pattern analysis");
        report.RecommendedActions.ShouldContain("Update validation rules for new document formats");
        report.RecommendedActions.ShouldContain("Increase confidence threshold for auto-processing");
    }

    /// <summary>
    /// Integration Test: Complete business intelligence report should work correctly
    /// </summary>
    [Fact]
    public void CompleteReport_ShouldWorkCorrectly_When_FullyPopulated()
    {
        // Arrange & Act
        var report = new BusinessIntelligenceReport
        {
            ReportPeriod = new DateRange
            {
                FromDate = DateTime.UtcNow.AddDays(-30),
                ToDate = DateTime.UtcNow
            },
            GroundingReport = new GroundingReport
            {
                TotalRecordsProcessed = 1500,
                SuccessRate = 0.96m,
                AverageConfidenceScore = 0.87m,
                ConflictsResolved = 25,
                RecordsRequiringReview = 15
            },
            QualityMetrics = new DataQualityMetrics
            {
                OverallQualityScore = 0.89m,
                CompletenessScore = 0.92m,
                AccuracyScore = 0.94m,
                ConsistencyScore = 0.88m
            },
            GeneratedAt = DateTime.UtcNow,
            GeneratedBy = "ExxerAI-ReportingEngine",
            Insights = { "High success rate maintained", "Quality metrics improved" },
            RecommendedActions = { "Continue monitoring", "Review edge cases" }
        };

        // Assert
        report.ReportPeriod.FromDate.ShouldBeLessThan(report.ReportPeriod.ToDate);
        report.GroundingReport.TotalRecordsProcessed.ShouldBe(1500);
        report.GroundingReport.SuccessRate.ShouldBe(0.96m);
        report.QualityMetrics.OverallQualityScore.ShouldBe(0.89m);
        report.GeneratedBy.ShouldBe("ExxerAI-ReportingEngine");
        report.Insights.Count.ShouldBe(2);
        report.RecommendedActions.Count.ShouldBe(2);
    }
}

/// <summary>
/// Comprehensive tests for DateRange class
/// </summary>
public class DateRangeTests
{
    /// <summary>
    /// Contract Test: DateRange should initialize with default dates
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults_When_Created()
    {
        // Act
        var dateRange = new DateRange();

        // Assert
        dateRange.FromDate.ShouldBe(default(DateTime));
        dateRange.ToDate.ShouldBe(default(DateTime));
    }

    /// <summary>
    /// Contract Test: DateRange properties should be settable
    /// </summary>
    [Fact]
    public void Properties_ShouldBeSettable_When_ValuesAssigned()
    {
        // Arrange
        var dateRange = new DateRange();
        var fromDate = DateTime.UtcNow.AddDays(-30);
        var toDate = DateTime.UtcNow;

        // Act
        dateRange.FromDate = fromDate;
        dateRange.ToDate = toDate;

        // Assert
        dateRange.FromDate.ShouldBe(fromDate);
        dateRange.ToDate.ShouldBe(toDate);
    }

    /// <summary>
    /// Business Rule Test: DateRange should support various business periods
    /// </summary>
    [Theory]
    [InlineData(1, "Daily")]
    [InlineData(7, "Weekly")]
    [InlineData(30, "Monthly")]
    [InlineData(90, "Quarterly")]
    [InlineData(365, "Yearly")]
    public void DateRange_ShouldSupportBusinessPeriods_When_VariousPeriodsSet(int days, string periodType)
    {
        // Arrange
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-days);

        // Act
        var dateRange = new DateRange
        {
            FromDate = startDate,
            ToDate = endDate
        };

        // Assert
        dateRange.FromDate.ShouldBe(startDate);
        dateRange.ToDate.ShouldBe(endDate);
        (dateRange.ToDate - dateRange.FromDate).Days.ShouldBe(days);
    }

    /// <summary>
    /// Edge Case Test: DateRange should handle same date for from and to
    /// </summary>
    [Fact]
    public void DateRange_ShouldHandleSameDate_When_FromAndToAreEqual()
    {
        // Arrange
        var sameDate = DateTime.UtcNow.Date;

        // Act
        var dateRange = new DateRange
        {
            FromDate = sameDate,
            ToDate = sameDate
        };

        // Assert
        dateRange.FromDate.ShouldBe(sameDate);
        dateRange.ToDate.ShouldBe(sameDate);
        (dateRange.ToDate - dateRange.FromDate).TotalDays.ShouldBe(0);
    }

    /// <summary>
    /// Edge Case Test: DateRange should allow reverse dates (validation elsewhere)
    /// </summary>
    [Fact]
    public void DateRange_ShouldAllowReverseDates_When_ToDateBeforeFromDate()
    {
        // Arrange
        var fromDate = DateTime.UtcNow;
        var toDate = DateTime.UtcNow.AddDays(-10);

        // Act
        var dateRange = new DateRange
        {
            FromDate = fromDate,
            ToDate = toDate
        };

        // Assert - Domain model allows this, validation would be elsewhere
        dateRange.FromDate.ShouldBe(fromDate);
        dateRange.ToDate.ShouldBe(toDate);
        dateRange.FromDate.ShouldBeGreaterThan(dateRange.ToDate);
    }
}

/// <summary>
/// Comprehensive tests for GroundingReport class
/// </summary>
public class GroundingReportTests
{
    /// <summary>
    /// Contract Test: GroundingReport should initialize with defaults
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults_When_Created()
    {
        // Act
        var report = new GroundingReport();

        // Assert
        report.TotalRecordsProcessed.ShouldBe(0);
        report.SuccessRate.ShouldBe(0);
        report.AverageConfidenceScore.ShouldBe(0);
        report.ConflictsResolved.ShouldBe(0);
        report.RecordsRequiringReview.ShouldBe(0);
    }

    /// <summary>
    /// Contract Test: GroundingReport properties should be settable
    /// </summary>
    [Fact]
    public void Properties_ShouldBeSettable_When_ValuesAssigned()
    {
        // Arrange
        var report = new GroundingReport();

        // Act
        report.TotalRecordsProcessed = 1000;
        report.SuccessRate = 0.95m;
        report.AverageConfidenceScore = 0.87m;
        report.ConflictsResolved = 50;
        report.RecordsRequiringReview = 25;

        // Assert
        report.TotalRecordsProcessed.ShouldBe(1000);
        report.SuccessRate.ShouldBe(0.95m);
        report.AverageConfidenceScore.ShouldBe(0.87m);
        report.ConflictsResolved.ShouldBe(50);
        report.RecordsRequiringReview.ShouldBe(25);
    }

    /// <summary>
    /// Business Rule Test: GroundingReport should handle realistic processing scenarios
    /// </summary>
    [Theory]
    [InlineData(100, 95, 0.95, 5, 3)]
    [InlineData(1000, 980, 0.98, 15, 5)]
    [InlineData(5000, 4850, 0.97, 100, 50)]
    public void GroundingReport_ShouldHandleRealisticScenarios_When_ProcessingMetricsSet(
        int totalRecords, int successfulRecords, decimal successRate, int conflicts, int reviewRequired)
    {
        // Arrange & Act
        var report = new GroundingReport
        {
            TotalRecordsProcessed = totalRecords,
            SuccessRate = successRate,
            AverageConfidenceScore = 0.85m,
            ConflictsResolved = conflicts,
            RecordsRequiringReview = reviewRequired
        };

        // Assert - Business rules validation
        report.TotalRecordsProcessed.ShouldBe(totalRecords);
        report.SuccessRate.ShouldBe(successRate);
        report.ConflictsResolved.ShouldBeLessThanOrEqualTo(totalRecords);
        report.RecordsRequiringReview.ShouldBeLessThanOrEqualTo(totalRecords);
    }

    /// <summary>
    /// Edge Case Test: GroundingReport should handle zero processing
    /// </summary>
    [Fact]
    public void GroundingReport_ShouldHandleZeroProcessing_When_NoRecordsProcessed()
    {
        // Arrange & Act
        var report = new GroundingReport
        {
            TotalRecordsProcessed = 0,
            SuccessRate = 0m,
            AverageConfidenceScore = 0m,
            ConflictsResolved = 0,
            RecordsRequiringReview = 0
        };

        // Assert
        report.TotalRecordsProcessed.ShouldBe(0);
        report.SuccessRate.ShouldBe(0m);
        report.AverageConfidenceScore.ShouldBe(0m);
    }

    /// <summary>
    /// Business Rule Test: GroundingReport should support high-volume scenarios
    /// </summary>
    [Fact]
    public void GroundingReport_ShouldSupportHighVolume_When_LargeNumbersProcessed()
    {
        // Arrange & Act
        var report = new GroundingReport
        {
            TotalRecordsProcessed = 1_000_000,
            SuccessRate = 0.999m,
            AverageConfidenceScore = 0.92m,
            ConflictsResolved = 5000,
            RecordsRequiringReview = 1000
        };

        // Assert
        report.TotalRecordsProcessed.ShouldBe(1_000_000);
        report.SuccessRate.ShouldBe(0.999m);
        report.AverageConfidenceScore.ShouldBe(0.92m);
        report.ConflictsResolved.ShouldBe(5000);
        report.RecordsRequiringReview.ShouldBe(1000);
    }
}

/// <summary>
/// Comprehensive tests for DataQualityMetrics class
/// </summary>
public class DataQualityMetricsTests
{
    /// <summary>
    /// Contract Test: DataQualityMetrics should initialize with defaults
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults_When_Created()
    {
        // Act
        var metrics = new DataQualityMetrics();

        // Assert
        metrics.OverallQualityScore.ShouldBe(0);
        metrics.CompletenessScore.ShouldBe(0);
        metrics.AccuracyScore.ShouldBe(0);
        metrics.ConsistencyScore.ShouldBe(0);
    }

    /// <summary>
    /// Contract Test: DataQualityMetrics properties should be settable
    /// </summary>
    [Fact]
    public void Properties_ShouldBeSettable_When_ValuesAssigned()
    {
        // Arrange
        var metrics = new DataQualityMetrics();

        // Act
        metrics.OverallQualityScore = 0.89m;
        metrics.CompletenessScore = 0.92m;
        metrics.AccuracyScore = 0.94m;
        metrics.ConsistencyScore = 0.88m;

        // Assert
        metrics.OverallQualityScore.ShouldBe(0.89m);
        metrics.CompletenessScore.ShouldBe(0.92m);
        metrics.AccuracyScore.ShouldBe(0.94m);
        metrics.ConsistencyScore.ShouldBe(0.88m);
    }

    /// <summary>
    /// Business Rule Test: DataQualityMetrics should support quality assessment ranges
    /// </summary>
    [Theory]
    [InlineData(0.95, "Excellent")]
    [InlineData(0.85, "Good")]
    [InlineData(0.75, "Fair")]
    [InlineData(0.65, "Poor")]
    [InlineData(0.50, "Unacceptable")]
    public void DataQualityMetrics_ShouldSupportQualityRanges_When_DifferentScoresSet(decimal score, string qualityLevel)
    {
        // Arrange & Act
        var metrics = new DataQualityMetrics
        {
            OverallQualityScore = score,
            CompletenessScore = score,
            AccuracyScore = score,
            ConsistencyScore = score
        };

        // Assert
        metrics.OverallQualityScore.ShouldBe(score);
        metrics.CompletenessScore.ShouldBe(score);
        metrics.AccuracyScore.ShouldBe(score);
        metrics.ConsistencyScore.ShouldBe(score);
        
        // Business rule validation (would typically be in a service)
        score.ShouldBeInRange(0m, 1m);
    }

    /// <summary>
    /// Edge Case Test: DataQualityMetrics should handle perfect scores
    /// </summary>
    [Fact]
    public void DataQualityMetrics_ShouldHandlePerfectScores_When_AllScoresAreOne()
    {
        // Arrange & Act
        var metrics = new DataQualityMetrics
        {
            OverallQualityScore = 1.0m,
            CompletenessScore = 1.0m,
            AccuracyScore = 1.0m,
            ConsistencyScore = 1.0m
        };

        // Assert
        metrics.OverallQualityScore.ShouldBe(1.0m);
        metrics.CompletenessScore.ShouldBe(1.0m);
        metrics.AccuracyScore.ShouldBe(1.0m);
        metrics.ConsistencyScore.ShouldBe(1.0m);
    }

    /// <summary>
    /// Edge Case Test: DataQualityMetrics should handle zero scores
    /// </summary>
    [Fact]
    public void DataQualityMetrics_ShouldHandleZeroScores_When_AllScoresAreZero()
    {
        // Arrange & Act
        var metrics = new DataQualityMetrics
        {
            OverallQualityScore = 0.0m,
            CompletenessScore = 0.0m,
            AccuracyScore = 0.0m,
            ConsistencyScore = 0.0m
        };

        // Assert
        metrics.OverallQualityScore.ShouldBe(0.0m);
        metrics.CompletenessScore.ShouldBe(0.0m);
        metrics.AccuracyScore.ShouldBe(0.0m);
        metrics.ConsistencyScore.ShouldBe(0.0m);
    }

    /// <summary>
    /// Business Rule Test: DataQualityMetrics should support varied metric combinations
    /// </summary>
    [Fact]
    public void DataQualityMetrics_ShouldSupportVariedCombinations_When_DifferentMetricsSet()
    {
        // Arrange & Act
        var metrics = new DataQualityMetrics
        {
            OverallQualityScore = 0.87m,  // Good overall
            CompletenessScore = 0.95m,    // Excellent completeness
            AccuracyScore = 0.82m,        // Good accuracy 
            ConsistencyScore = 0.78m      // Fair consistency
        };

        // Assert
        metrics.CompletenessScore.ShouldBeGreaterThan(metrics.OverallQualityScore);
        metrics.AccuracyScore.ShouldBeLessThan(metrics.CompletenessScore);
        metrics.ConsistencyScore.ShouldBeLessThan(metrics.AccuracyScore);
        
        // All scores should be in valid range
        metrics.OverallQualityScore.ShouldBeInRange(0m, 1m);
        metrics.CompletenessScore.ShouldBeInRange(0m, 1m);
        metrics.AccuracyScore.ShouldBeInRange(0m, 1m);
        metrics.ConsistencyScore.ShouldBeInRange(0m, 1m);
    }
}

/// <summary>
/// Integration tests for business intelligence reporting workflow
/// </summary>
public class BusinessIntelligenceReportingWorkflowTests
{
    /// <summary>
    /// Workflow Test: Complete BI report generation workflow
    /// </summary>
    [Fact]
    public void BIReportWorkflow_ShouldCompleteSuccessfully_When_FullReportGenerated()
    {
        // Arrange - Simulate BI report generation process
        var reportPeriod = new DateRange
        {
            FromDate = DateTime.UtcNow.AddDays(-30),
            ToDate = DateTime.UtcNow
        };

        // Act - Generate comprehensive BI report
        var report = new BusinessIntelligenceReport
        {
            ReportPeriod = reportPeriod,
            GroundingReport = new GroundingReport
            {
                TotalRecordsProcessed = 2500,
                SuccessRate = 0.97m,
                AverageConfidenceScore = 0.89m,
                ConflictsResolved = 75,
                RecordsRequiringReview = 25
            },
            QualityMetrics = new DataQualityMetrics
            {
                OverallQualityScore = 0.91m,
                CompletenessScore = 0.94m,
                AccuracyScore = 0.96m,
                ConsistencyScore = 0.89m
            },
            GeneratedAt = DateTime.UtcNow,
            GeneratedBy = "ExxerAI-BI-Engine"
        };

        // Add insights based on metrics
        if (report.GroundingReport.SuccessRate >= 0.95m)
            report.Insights.Add("Excellent processing success rate maintained");
        
        if (report.QualityMetrics.AccuracyScore >= 0.95m)
            report.Insights.Add("High accuracy standards exceeded");

        // Add recommended actions
        if (report.GroundingReport.RecordsRequiringReview > 20)
            report.RecommendedActions.Add("Review manual intervention threshold");

        // Assert - Verify complete BI report
        report.ReportPeriod.FromDate.ShouldBeLessThan(report.ReportPeriod.ToDate);
        report.GroundingReport.TotalRecordsProcessed.ShouldBe(2500);
        report.GroundingReport.SuccessRate.ShouldBe(0.97m);
        report.QualityMetrics.OverallQualityScore.ShouldBe(0.91m);
        report.GeneratedBy.ShouldBe("ExxerAI-BI-Engine");
        report.Insights.Count.ShouldBe(2);
        report.RecommendedActions.Count.ShouldBe(1);
        report.GeneratedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    /// <summary>
    /// Performance Test: BI report operations should be efficient
    /// </summary>
    [Fact]
    public void BIReportOperations_ShouldBeEfficient_When_MultipleReportsGenerated()
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var reports = new List<BusinessIntelligenceReport>();

        // Act - Generate multiple BI reports
        for (int i = 0; i < 50; i++)
        {
            var report = new BusinessIntelligenceReport
            {
                ReportPeriod = new DateRange
                {
                    FromDate = DateTime.UtcNow.AddDays(-30 - i),
                    ToDate = DateTime.UtcNow.AddDays(-i)
                },
                GroundingReport = new GroundingReport
                {
                    TotalRecordsProcessed = 1000 + i * 10,
                    SuccessRate = 0.95m + (i % 5) * 0.01m
                },
                QualityMetrics = new DataQualityMetrics
                {
                    OverallQualityScore = 0.85m + (i % 10) * 0.01m
                },
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = $"System-{i}"
            };
            
            reports.Add(report);
        }

        var duration = DateTime.UtcNow - startTime;

        // Assert
        reports.Count.ShouldBe(50);
        reports.All(r => r.ReportPeriod != null).ShouldBeTrue();
        reports.All(r => r.GroundingReport != null).ShouldBeTrue();
        reports.All(r => r.QualityMetrics != null).ShouldBeTrue();
        duration.ShouldBeLessThan(TimeSpan.FromSeconds(1)); // Should be very fast
    }
} 