using Shouldly;
using Xunit;

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