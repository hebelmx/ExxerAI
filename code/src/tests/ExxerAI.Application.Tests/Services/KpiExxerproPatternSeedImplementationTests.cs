using ExxerAI.Application.Data;
using ExxerAI.Application.Interfaces;
using Shouldly;
using System.Linq;
using Xunit;

namespace ExxerAI.Application.Tests.Services;

/// <summary>
/// Implementation tests for KpiExxerproPatternSeed - targeting 524 lines of static pattern logic
/// Tests all static methods and LINQ operations to achieve maximum mutation coverage
/// </summary>
public class KpiExxerproPatternSeedImplementationTests
{
    public class InitialPatternsTests
    {
        [Fact]
        public void InitialPatterns_Should_NotBeEmpty()
        {
            // Act
            var patterns = KpiExxerproPatternSeed.InitialPatterns;

            // Assert
            patterns.ShouldNotBeEmpty();
            patterns.Count.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void InitialPatterns_Should_HaveValidPatternEntities()
        {
            // Act
            var patterns = KpiExxerproPatternSeed.InitialPatterns;

            // Assert
            patterns.ShouldAllBe(p => !string.IsNullOrEmpty(p.FieldName));
            patterns.ShouldAllBe(p => !string.IsNullOrEmpty(p.DocumentType));
            patterns.ShouldAllBe(p => !string.IsNullOrEmpty(p.PatternType));
            patterns.ShouldAllBe(p => !string.IsNullOrEmpty(p.PatternExpression));
            patterns.ShouldAllBe(p => p.ConfidenceScore > 0f && p.ConfidenceScore <= 1f);
            patterns.ShouldAllBe(p => p.SuccessCount >= 0);
            patterns.ShouldAllBe(p => p.TotalAttempts > 0);
            patterns.ShouldAllBe(p => p.IsActive);
        }
    }

    public class GetPatternsForDocumentTypeTests
    {
        [Fact]
        public void GetPatternsForDocumentType_Should_ReturnFilteredPatterns_When_ValidDocumentType()
        {
            // Arrange
            var documentType = "IMSSPayment";

            // Act
            var result = KpiExxerproPatternSeed.GetPatternsForDocumentType(documentType);

            // Assert
            result.ShouldNotBeEmpty();
            result.ShouldAllBe(p => p.DocumentType == documentType);
        }

        [Fact]
        public void GetPatternsForDocumentType_Should_ReturnEmptyList_When_NonExistentDocumentType()
        {
            // Arrange
            var documentType = "NonExistentType";

            // Act
            var result = KpiExxerproPatternSeed.GetPatternsForDocumentType(documentType);

            // Assert
            result.ShouldBeEmpty();
        }
    }

    public class GetPatternsForFieldTests
    {
        [Fact]
        public void GetPatternsForField_Should_ReturnFilteredPatterns_When_ValidFieldName()
        {
            // Arrange
            var fieldName = "registro_patronal";

            // Act
            var result = KpiExxerproPatternSeed.GetPatternsForField(fieldName);

            // Assert
            result.ShouldNotBeEmpty();
            result.ShouldAllBe(p => p.FieldName == fieldName);
        }

        [Fact]
        public void GetPatternsForField_Should_ReturnOrderedByConfidence_When_ValidFieldName()
        {
            // Arrange
            var fieldName = "registro_patronal";

            // Act
            var result = KpiExxerproPatternSeed.GetPatternsForField(fieldName);

            // Assert
            result.ShouldNotBeEmpty();
            for (int i = 0; i < result.Count - 1; i++)
            {
                result[i].ConfidenceScore.ShouldBeGreaterThanOrEqualTo(result[i + 1].ConfidenceScore);
            }
        }
    }

    public class GetStatisticsTests
    {
        [Fact]
        public void GetStatistics_Should_ReturnValidStatistics()
        {
            // Act
            var result = KpiExxerproPatternSeed.GetStatistics();

            // Assert
            result.ShouldNotBeNull();
            result.TotalPatterns.ShouldBeGreaterThan(0);
            result.UniqueFields.ShouldBeGreaterThan(0);
            result.UniqueDocumentTypes.ShouldBeGreaterThan(0);
            result.AverageConfidence.ShouldBeGreaterThan(0f);
            result.AverageConfidence.ShouldBeLessThanOrEqualTo(1f);
            result.TotalSuccessfulExtractions.ShouldBeGreaterThan(0);
            result.TotalAttempts.ShouldBeGreaterThan(0);
            result.OverallSuccessRate.ShouldBeGreaterThan(0f);
            result.OverallSuccessRate.ShouldBeLessThanOrEqualTo(1f);
        }

        [Fact]
        public void GetStatistics_Should_CalculateCorrectTotalPatterns()
        {
            // Act
            var result = KpiExxerproPatternSeed.GetStatistics();
            var expectedTotal = KpiExxerproPatternSeed.InitialPatterns.Count;

            // Assert
            result.TotalPatterns.ShouldBe(expectedTotal);
        }

        [Fact]
        public void GetStatistics_Should_CalculateCorrectAverageConfidence()
        {
            // Act
            var result = KpiExxerproPatternSeed.GetStatistics();
            var expectedAverage = KpiExxerproPatternSeed.InitialPatterns.Average(p => p.ConfidenceScore);

            // Assert
            result.AverageConfidence.ShouldBe(expectedAverage, 0.001f);
        }
    }
}
