using ExxerAI.Domain.DocumentProcessing;
using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Unit tests for ProcessingHistory and related classes
/// </summary>
public class ProcessingHistoryTests
{
	private static ProcessingHistory CreateValidProcessingHistory()
	{
		return new ProcessingHistory(DocumentType.Invoice)
		{
			Id = "test-history-1",
			CreatedAt = DateTime.UtcNow.AddDays(-30),
			UpdatedAt = DateTime.UtcNow.AddDays(-1)
		};
	}

	private static DocumentProcessingResult CreateValidProcessingResult(bool isSuccessful = true, float confidence = 0.85f)
	{
		var result = new DocumentProcessingResult
		{
			DocumentId = Guid.NewGuid().ToString(),
			Confidence = confidence,
			LLMConfidence = confidence,
			GroundingConfidence = confidence,
			ProcessingTimeMs = Random.Shared.Next(1000, 5000)
		};
		
		if (!isSuccessful)
		{
			result.ErrorMessage = "Test processing error";
		}
		
		return result;
	}

	private static LearnedPattern CreateValidLearnedPattern(string fieldName = "Amount", int successCount = 10, int failureCount = 2)
	{
		return new LearnedPattern
		{
			Id = Guid.NewGuid().ToString(),
			FieldName = fieldName,
			Pattern = @"\$\d+\.\d{2}",
			PatternType = "Regex",
			SuccessCount = successCount,
			FailureCount = failureCount,
			Confidence = 0.8f
		};
	}

	private static SchemaEvolution CreateValidSchemaEvolution()
	{
		return new SchemaEvolution
		{
			Id = Guid.NewGuid().ToString(),
			SchemaId = "schema-v1",
			PreviousVersion = 1,
			NewVersion = 2,
			Changes = ["Added new field: TaxAmount", "Updated validation rules"],
			Reason = "Improved accuracy",
			AccuracyImprovement = 0.15f
		};
	}

	private static AdaptationRecommendation CreateValidRecommendation()
	{
		return new AdaptationRecommendation
		{
			Id = Guid.NewGuid().ToString(),
			Type = RecommendationType.SchemaImprovement,
			Priority = RecommendationPriority.High,
			Description = "Improve pattern accuracy",
			Confidence = 0.9f,
			ExpectedImpact = "15% improvement"
		};
	}

	public class ConstructorTests
	{
		[Fact]
		public void Should_CreateDefaultInstance_When_UsingParameterlessConstructor()
		{
			// Act
			var history = new ProcessingHistory();

			// Assert
			history.Id.ShouldNotBeNullOrEmpty();
			history.DocumentType.ShouldBe(DocumentType.Unknown);
			history.ProcessingResults.ShouldNotBeNull();
			history.ProcessingResults.ShouldBeEmpty();
			history.TimeRange.ShouldNotBeNull();
			history.LearnedPatterns.ShouldNotBeNull();
			history.LearnedPatterns.ShouldBeEmpty();
			history.Metrics.ShouldNotBeNull();
			history.SchemaEvolutions.ShouldNotBeNull();
			history.SchemaEvolutions.ShouldBeEmpty();
			history.Recommendations.ShouldNotBeNull();
			history.Recommendations.ShouldBeEmpty();
			history.Metadata.ShouldNotBeNull();
			history.Metadata.ShouldBeEmpty();
			(DateTime.UtcNow - history.CreatedAt).ShouldBeLessThan(TimeSpan.FromSeconds(5));
			(DateTime.UtcNow - history.UpdatedAt).ShouldBeLessThan(TimeSpan.FromSeconds(5));
		}

		[Theory]
		[InlineData(nameof(DocumentType.Invoice))]
		[InlineData(nameof(DocumentType.TaxDocument))]
		[InlineData(nameof(DocumentType.Contract))]
		public void Should_CreateInstanceWithDocumentType_When_UsingParameterizedConstructor(string documentTypeName)
		{
			// Arrange
			var documentType = Enum.Parse<DocumentType>(documentTypeName);

			// Act
			var history = new ProcessingHistory(documentType);

			// Assert
			history.DocumentType.ShouldBe(documentType);
			history.TimeRange.ShouldNotBeNull();
			var expectedFromDate = DateTime.UtcNow.AddMonths(-1);
			Math.Abs((history.TimeRange.FromDate - expectedFromDate).TotalHours).ShouldBeLessThan(1);
			(DateTime.UtcNow - history.TimeRange.ToDate).ShouldBeLessThan(TimeSpan.FromSeconds(5));
		}
	}

	public class AddProcessingResultTests
	{
		[Fact]
		public void Should_AddResultAndUpdateMetrics_When_ValidResultProvided()
		{
			// Arrange
			var history = CreateValidProcessingHistory();
			var result = CreateValidProcessingResult();
			var originalUpdatedAt = history.UpdatedAt;

			// Act
			history.AddProcessingResult(result);

			// Assert
			history.ProcessingResults.ShouldContain(result);
			history.ProcessingResults.Count.ShouldBe(1);
			history.Metrics.TotalDocuments.ShouldBe(1);
			history.Metrics.SuccessfulDocuments.ShouldBe(1);
			history.Metrics.FailedDocuments.ShouldBe(0);
			history.Metrics.SuccessRate.ShouldBe(1.0f);
			history.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
		}

		[Fact]
		public void Should_NotAddResult_When_NullResultProvided()
		{
			// Arrange
			var history = CreateValidProcessingHistory();
			var originalCount = history.ProcessingResults.Count;
			var originalUpdatedAt = history.UpdatedAt;

			// Act
			history.AddProcessingResult(null!);

			// Assert
			history.ProcessingResults.Count.ShouldBe(originalCount);
			history.UpdatedAt.ShouldBe(originalUpdatedAt);
		}

		[Fact]
		public void Should_UpdateMetricsCorrectly_When_MultipleResultsAdded()
		{
			// Arrange
			var history = CreateValidProcessingHistory();
			var successfulResult1 = CreateValidProcessingResult(true, 0.9f);
			var successfulResult2 = CreateValidProcessingResult(true, 0.7f);
			var failedResult = CreateValidProcessingResult(false, 0.3f);

			// Act
			history.AddProcessingResult(successfulResult1);
			history.AddProcessingResult(successfulResult2);
			history.AddProcessingResult(failedResult);

			// Assert
			history.ProcessingResults.Count.ShouldBe(3);
			history.Metrics.TotalDocuments.ShouldBe(3);
			history.Metrics.SuccessfulDocuments.ShouldBe(2);
			history.Metrics.FailedDocuments.ShouldBe(1);
			history.Metrics.SuccessRate.ShouldBe(2.0f / 3.0f, 0.01);
			history.Metrics.AverageConfidence.ShouldBe((0.9f + 0.7f) / 2.0f, 0.01);
		}
	}

	public class AddLearnedPatternTests
	{
		[Fact]
		public void Should_AddPatternAndUpdateTime_When_ValidPatternProvided()
		{
			// Arrange
			var history = CreateValidProcessingHistory();
			var pattern = CreateValidLearnedPattern();
			var originalUpdatedAt = history.UpdatedAt;

			// Act
			history.AddLearnedPattern(pattern);

			// Assert
			history.LearnedPatterns.ShouldContain(pattern);
			history.LearnedPatterns.Count.ShouldBe(1);
			history.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
		}

		[Fact]
		public void Should_NotAddPattern_When_NullPatternProvided()
		{
			// Arrange
			var history = CreateValidProcessingHistory();
			var originalCount = history.LearnedPatterns.Count;
			var originalUpdatedAt = history.UpdatedAt;

			// Act
			history.AddLearnedPattern(null!);

			// Assert
			history.LearnedPatterns.Count.ShouldBe(originalCount);
			history.UpdatedAt.ShouldBe(originalUpdatedAt);
		}
	}

	public class AddSchemaEvolutionTests
	{
		[Fact]
		public void Should_AddEvolutionAndUpdateTime_When_ValidEvolutionProvided()
		{
			// Arrange
			var history = CreateValidProcessingHistory();
			var evolution = CreateValidSchemaEvolution();
			var originalUpdatedAt = history.UpdatedAt;

			// Act
			history.AddSchemaEvolution(evolution);

			// Assert
			history.SchemaEvolutions.ShouldContain(evolution);
			history.SchemaEvolutions.Count.ShouldBe(1);
			history.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
		}

		[Fact]
		public void Should_NotAddEvolution_When_NullEvolutionProvided()
		{
			// Arrange
			var history = CreateValidProcessingHistory();
			var originalCount = history.SchemaEvolutions.Count;
			var originalUpdatedAt = history.UpdatedAt;

			// Act
			history.AddSchemaEvolution(null!);

			// Assert
			history.SchemaEvolutions.Count.ShouldBe(originalCount);
			history.UpdatedAt.ShouldBe(originalUpdatedAt);
		}
	}

	public class FilteringTests
	{
		[Fact]
		public void Should_ReturnOnlySuccessfulResults_When_QueryingSuccessfulResults()
		{
			// Arrange
			var history = CreateValidProcessingHistory();
			var successfulResult1 = CreateValidProcessingResult(true);
			var successfulResult2 = CreateValidProcessingResult(true);
			var failedResult = CreateValidProcessingResult(false);
			
			history.AddProcessingResult(successfulResult1);
			history.AddProcessingResult(successfulResult2);
			history.AddProcessingResult(failedResult);

			// Act
			var successfulResults = history.SuccessfulResults.ToList();

			// Assert
			successfulResults.Count.ShouldBe(2);
			successfulResults.ShouldContain(successfulResult1);
			successfulResults.ShouldContain(successfulResult2);
			successfulResults.ShouldNotContain(failedResult);
		}

		[Fact]
		public void Should_ReturnOnlyFailedResults_When_QueryingFailedResults()
		{
			// Arrange
			var history = CreateValidProcessingHistory();
			var successfulResult = CreateValidProcessingResult(true);
			var failedResult1 = CreateValidProcessingResult(false);
			var failedResult2 = CreateValidProcessingResult(false);
			
			history.AddProcessingResult(successfulResult);
			history.AddProcessingResult(failedResult1);
			history.AddProcessingResult(failedResult2);

			// Act
			var failedResults = history.FailedResults.ToList();

			// Assert
			failedResults.Count.ShouldBe(2);
			failedResults.ShouldContain(failedResult1);
			failedResults.ShouldContain(failedResult2);
			failedResults.ShouldNotContain(successfulResult);
		}

		[Theory]
		[InlineData(0.5f, 0.8f, 2)]
		[InlineData(0.9f, 1.0f, 1)]
		[InlineData(0.0f, 0.4f, 1)]
		public void Should_ReturnFilteredResults_When_QueryingByConfidenceRange(float minConfidence, float maxConfidence, int expectedCount)
		{
			// Arrange
			var history = CreateValidProcessingHistory();
			history.AddProcessingResult(CreateValidProcessingResult(true, 0.3f));
			history.AddProcessingResult(CreateValidProcessingResult(true, 0.6f));
			history.AddProcessingResult(CreateValidProcessingResult(true, 0.7f));
			history.AddProcessingResult(CreateValidProcessingResult(true, 0.95f));

			// Act
			var filteredResults = history.GetResultsByConfidence(minConfidence, maxConfidence).ToList();

			// Assert
			filteredResults.Count.ShouldBe(expectedCount);
			filteredResults.ShouldAllBe(r => r.OverallConfidence >= minConfidence && r.OverallConfidence <= maxConfidence);
		}

		[Fact]
		public void Should_ReturnTopPatterns_When_QueryingCommonPatternsForField()
		{
			// Arrange
			var history = CreateValidProcessingHistory();
			var pattern1 = CreateValidLearnedPattern("Amount", 20, 1);
			var pattern2 = CreateValidLearnedPattern("Amount", 15, 2);
			var pattern3 = CreateValidLearnedPattern("Amount", 25, 0);
			var pattern4 = CreateValidLearnedPattern("Date", 10, 1); // Different field
			
			history.AddLearnedPattern(pattern1);
			history.AddLearnedPattern(pattern2);
			history.AddLearnedPattern(pattern3);
			history.AddLearnedPattern(pattern4);

			// Act
			var commonPatterns = history.GetCommonPatternsForField("Amount").ToList();

			// Assert
			commonPatterns.Count.ShouldBe(3);
			commonPatterns[0].ShouldBe(pattern3.Pattern); // Highest success count
			commonPatterns[1].ShouldBe(pattern1.Pattern); // Second highest
			commonPatterns[2].ShouldBe(pattern2.Pattern); // Third highest
		}

		[Fact]
		public void Should_ReturnEmptyCollection_When_QueryingNonExistentField()
		{
			// Arrange
			var history = CreateValidProcessingHistory();
			history.AddLearnedPattern(CreateValidLearnedPattern("Amount"));

			// Act
			var patterns = history.GetCommonPatternsForField("NonExistentField").ToList();

			// Assert
			patterns.ShouldBeEmpty();
		}

		[Fact]
		public void Should_BeCaseInsensitive_When_QueryingPatternsByFieldName()
		{
			// Arrange
			var history = CreateValidProcessingHistory();
			var pattern = CreateValidLearnedPattern("Amount");
			history.AddLearnedPattern(pattern);

			// Act
			var patternsLower = history.GetCommonPatternsForField("amount").ToList();
			var patternsUpper = history.GetCommonPatternsForField("AMOUNT").ToList();

			// Assert
			patternsLower.Count.ShouldBe(1);
			patternsUpper.Count.ShouldBe(1);
			patternsLower[0].ShouldBe(pattern.Pattern);
			patternsUpper[0].ShouldBe(pattern.Pattern);
		}
	}

	public class AnalysisAndRecommendationTests
	{
		[Fact]
		public void Should_GenerateSchemaImprovementRecommendation_When_SuccessRateIsLow()
		{
			// Arrange
			var history = CreateValidProcessingHistory();
			// Add mostly failed results (success rate < 80%)
			history.AddProcessingResult(CreateValidProcessingResult(true));
			history.AddProcessingResult(CreateValidProcessingResult(false));
			history.AddProcessingResult(CreateValidProcessingResult(false));
			history.AddProcessingResult(CreateValidProcessingResult(false));
			history.AddProcessingResult(CreateValidProcessingResult(false));

			// Act
			history.AnalyzeAndGenerateRecommendations();

			// Assert
			var schemaRecommendations = history.Recommendations
				.Where(r => r.Type == RecommendationType.SchemaImprovement)
				.ToList();
			
			schemaRecommendations.ShouldNotBeEmpty();
			schemaRecommendations[0].Priority.ShouldBe(RecommendationPriority.High);
			schemaRecommendations[0].Description.ShouldContain("Success rate is low");
			schemaRecommendations[0].Confidence.ShouldBe(0.9f);
		}

		[Fact]
		public void Should_GenerateConfidenceImprovementRecommendation_When_AverageConfidenceIsLow()
		{
			// Arrange
			var history = CreateValidProcessingHistory();
			// Add successful but low confidence results
			history.AddProcessingResult(CreateValidProcessingResult(true, 0.5f));
			history.AddProcessingResult(CreateValidProcessingResult(true, 0.6f));
			history.AddProcessingResult(CreateValidProcessingResult(true, 0.65f));

			// Act
			history.AnalyzeAndGenerateRecommendations();

			// Assert
			var confidenceRecommendations = history.Recommendations
				.Where(r => r.Type == RecommendationType.ConfidenceImprovement)
				.ToList();
			
			confidenceRecommendations.ShouldNotBeEmpty();
			confidenceRecommendations[0].Priority.ShouldBe(RecommendationPriority.Medium);
			confidenceRecommendations[0].Description.ShouldContain("Average confidence is low");
			confidenceRecommendations[0].Confidence.ShouldBe(0.8f);
		}

		[Fact]
		public void Should_GeneratePerformanceOptimizationRecommendation_When_ProcessingTimeIsHigh()
		{
			// Arrange
			var history = CreateValidProcessingHistory();
			var slowResult1 = CreateValidProcessingResult(true, 0.8f);
			slowResult1.ProcessingTimeMs = 12000; // > 10 seconds
			var slowResult2 = CreateValidProcessingResult(true, 0.8f);
			slowResult2.ProcessingTimeMs = 15000; // > 10 seconds
			
			history.AddProcessingResult(slowResult1);
			history.AddProcessingResult(slowResult2);

			// Act
			history.AnalyzeAndGenerateRecommendations();

			// Assert
			var performanceRecommendations = history.Recommendations
				.Where(r => r.Type == RecommendationType.PerformanceOptimization)
				.ToList();
			
			performanceRecommendations.ShouldNotBeEmpty();
			performanceRecommendations[0].Priority.ShouldBe(RecommendationPriority.Low);
			performanceRecommendations[0].Description.ShouldContain("Average processing time is high");
			performanceRecommendations[0].Confidence.ShouldBe(0.7f);
		}

		[Fact]
		public void Should_ClearPreviousRecommendations_When_AnalyzingAgain()
		{
			// Arrange
			var history = CreateValidProcessingHistory();
			history.Recommendations.Add(CreateValidRecommendation());
			var originalRecommendation = history.Recommendations[0];

			// Act
			history.AnalyzeAndGenerateRecommendations();

			// Assert
			history.Recommendations.ShouldNotContain(originalRecommendation);
		}

		[Fact]
		public void Should_UpdateTimestamp_When_AnalyzingRecommendations()
		{
			// Arrange
			var history = CreateValidProcessingHistory();
			var originalUpdatedAt = history.UpdatedAt;

			// Act
			history.AnalyzeAndGenerateRecommendations();

			// Assert
			history.UpdatedAt.ShouldBeGreaterThan(originalUpdatedAt);
		}

		[Fact]
		public void Should_HandleEmptyResults_When_AnalyzingRecommendations()
		{
			// Arrange
			var history = CreateValidProcessingHistory();

			// Act
			history.AnalyzeAndGenerateRecommendations();

			// Assert
			history.Recommendations.ShouldBeEmpty();
		}
	}

	public class MetricsCalculationTests
	{
		[Fact]
		public void Should_CalculateCorrectMetrics_When_NoResults()
		{
			// Arrange
			var history = CreateValidProcessingHistory();

			// Act & Assert
			history.Metrics.TotalDocuments.ShouldBe(0);
			history.Metrics.SuccessfulDocuments.ShouldBe(0);
			history.Metrics.FailedDocuments.ShouldBe(0);
			history.Metrics.SuccessRate.ShouldBe(0f);
			history.Metrics.AverageConfidence.ShouldBe(0f);
			history.Metrics.AverageProcessingTime.ShouldBe(0);
		}

		[Fact]
		public void Should_CalculateConfidenceDistribution_When_ResultsExist()
		{
			// Arrange
			var history = CreateValidProcessingHistory();
			history.AddProcessingResult(CreateValidProcessingResult(true, 0.1f));  // 0.0-0.2
			history.AddProcessingResult(CreateValidProcessingResult(true, 0.3f));  // 0.2-0.4
			history.AddProcessingResult(CreateValidProcessingResult(true, 0.5f));  // 0.4-0.6
			history.AddProcessingResult(CreateValidProcessingResult(true, 0.7f));  // 0.6-0.8
			history.AddProcessingResult(CreateValidProcessingResult(true, 0.9f));  // 0.8-1.0
			history.AddProcessingResult(CreateValidProcessingResult(true, 0.9f));  // 0.8-1.0

			// Act
			var distribution = history.Metrics.ConfidenceDistribution;

			// Assert
			distribution["0.0-0.2"].ShouldBe(1);
			distribution["0.2-0.4"].ShouldBe(1);
			distribution["0.4-0.6"].ShouldBe(1);
			distribution["0.6-0.8"].ShouldBe(1);
			distribution["0.8-1.0"].ShouldBe(2);
		}

		[Fact]
		public void Should_HandleBoundaryConfidenceValues_When_CalculatingDistribution()
		{
			// Arrange
			var history = CreateValidProcessingHistory();
			history.AddProcessingResult(CreateValidProcessingResult(true, 0.0f));   // 0.0-0.2
			history.AddProcessingResult(CreateValidProcessingResult(true, 0.2f));   // 0.2-0.4
			history.AddProcessingResult(CreateValidProcessingResult(true, 0.4f));   // 0.4-0.6
			history.AddProcessingResult(CreateValidProcessingResult(true, 0.6f));   // 0.6-0.8
			history.AddProcessingResult(CreateValidProcessingResult(true, 0.8f));   // 0.8-1.0
			history.AddProcessingResult(CreateValidProcessingResult(true, 1.0f));   // 0.8-1.0

			// Act
			var distribution = history.Metrics.ConfidenceDistribution;

			// Assert
			distribution["0.0-0.2"].ShouldBe(1);
			distribution["0.2-0.4"].ShouldBe(1);
			distribution["0.4-0.6"].ShouldBe(1);
			distribution["0.6-0.8"].ShouldBe(1);
			distribution["0.8-1.0"].ShouldBe(2);
		}
	}

	public class SummaryTests
	{
		[Fact]
		public void Should_ReturnCorrectSummary_When_HistoryHasData()
		{
			// Arrange
			var history = CreateValidProcessingHistory();
			history.AddProcessingResult(CreateValidProcessingResult(true));
			history.AddProcessingResult(CreateValidProcessingResult(true));
			history.AddProcessingResult(CreateValidProcessingResult(false));

			// Act
			var summary = history.GetSummary();

			// Assert
			summary.ShouldContain("Invoice");
			summary.ShouldContain("3 documents");
			summary.ShouldContain("67%"); // 2/3 success rate
		}

		[Fact]
		public void Should_ReturnCorrectSummary_When_HistoryIsEmpty()
		{
			// Arrange
			var history = CreateValidProcessingHistory();

			// Act
			var summary = history.GetSummary();

			// Assert
			summary.ShouldContain("Invoice");
			summary.ShouldContain("0 documents");
			summary.ShouldContain("0%");
		}
	}
}

/// <summary>
/// Unit tests for LearnedPattern class
/// </summary>
public class LearnedPatternTests
{
	private static LearnedPattern CreateValidLearnedPattern()
	{
		return new LearnedPattern
		{
			FieldName = "Amount",
			Pattern = @"\$\d+\.\d{2}",
			PatternType = "Regex",
			SuccessCount = 15,
			FailureCount = 3,
			Confidence = 0.85f
		};
	}

	public class ConstructorTests
	{
		[Fact]
		public void Should_InitializeWithDefaults_When_UsingDefaultConstructor()
		{
			// Act
			var pattern = new LearnedPattern();

			// Assert
			pattern.Id.ShouldNotBeNullOrEmpty();
			pattern.FieldName.ShouldBe(string.Empty);
			pattern.Pattern.ShouldBe(string.Empty);
			pattern.PatternType.ShouldBe(string.Empty);
			pattern.SuccessCount.ShouldBe(0);
			pattern.FailureCount.ShouldBe(0);
			pattern.Confidence.ShouldBe(0f);
			(DateTime.UtcNow - pattern.LearnedAt).ShouldBeLessThan(TimeSpan.FromSeconds(5));
			(DateTime.UtcNow - pattern.LastUsedAt).ShouldBeLessThan(TimeSpan.FromSeconds(5));
		}
	}

	public class SuccessRateTests
	{
		[Theory]
		[InlineData(10, 2, 0.833f)]
		[InlineData(5, 5, 0.5f)]
		[InlineData(20, 0, 1.0f)]
		[InlineData(0, 10, 0.0f)]
		public void Should_CalculateCorrectSuccessRate_When_CountsProvided(int successCount, int failureCount, float expectedRate)
		{
			// Arrange
			var pattern = CreateValidLearnedPattern();
			pattern.SuccessCount = successCount;
			pattern.FailureCount = failureCount;

			// Act
			var successRate = pattern.SuccessRate;

			// Assert
			successRate.ShouldBe(expectedRate, 0.001f);
		}

		[Fact]
		public void Should_ReturnZero_When_NoAttemptsRecorded()
		{
			// Arrange
			var pattern = CreateValidLearnedPattern();
			pattern.SuccessCount = 0;
			pattern.FailureCount = 0;

			// Act
			var successRate = pattern.SuccessRate;

			// Assert
			successRate.ShouldBe(0f);
		}
	}

	public class PropertyTests
	{
		[Fact]
		public void Should_AllowSettingProperties_When_ValidValuesProvided()
		{
			// Arrange
			var pattern = new LearnedPattern();
			var testDate = DateTime.UtcNow.AddDays(-5);

			// Act
			pattern.FieldName = "TestField";
			pattern.Pattern = "TestPattern";
			pattern.PatternType = "TestType";
			pattern.SuccessCount = 42;
			pattern.FailureCount = 8;
			pattern.Confidence = 0.75f;
			pattern.LearnedAt = testDate;
			pattern.LastUsedAt = testDate;

			// Assert
			pattern.FieldName.ShouldBe("TestField");
			pattern.Pattern.ShouldBe("TestPattern");
			pattern.PatternType.ShouldBe("TestType");
			pattern.SuccessCount.ShouldBe(42);
			pattern.FailureCount.ShouldBe(8);
			pattern.Confidence.ShouldBe(0.75f);
			pattern.LearnedAt.ShouldBe(testDate);
			pattern.LastUsedAt.ShouldBe(testDate);
		}
	}
}

/// <summary>
/// Unit tests for ProcessingMetrics class
/// </summary>
public class ProcessingMetricsTests
{
	public class ConstructorTests
	{
		[Fact]
		public void Should_InitializeWithDefaults_When_UsingDefaultConstructor()
		{
			// Act
			var metrics = new ProcessingMetrics();

			// Assert
			metrics.TotalDocuments.ShouldBe(0);
			metrics.SuccessfulDocuments.ShouldBe(0);
			metrics.FailedDocuments.ShouldBe(0);
			metrics.SuccessRate.ShouldBe(0f);
			metrics.AverageConfidence.ShouldBe(0f);
			metrics.AverageProcessingTime.ShouldBe(0);
			metrics.ConfidenceDistribution.ShouldNotBeNull();
			metrics.ConfidenceDistribution.ShouldBeEmpty();
			metrics.AdditionalMetrics.ShouldNotBeNull();
			metrics.AdditionalMetrics.ShouldBeEmpty();
		}
	}

	public class PropertyTests
	{
		[Fact]
		public void Should_AllowSettingProperties_When_ValidValuesProvided()
		{
			// Arrange
			var metrics = new ProcessingMetrics();
			var distribution = new Dictionary<string, int> { ["high"] = 10, ["low"] = 5 };
			var additionalMetrics = new Dictionary<string, object> { ["custom"] = "value" };

			// Act
			metrics.TotalDocuments = 100;
			metrics.SuccessfulDocuments = 85;
			metrics.FailedDocuments = 15;
			metrics.SuccessRate = 0.85f;
			metrics.AverageConfidence = 0.75f;
			metrics.AverageProcessingTime = 2500.5;
			metrics.ConfidenceDistribution = distribution;
			metrics.AdditionalMetrics = additionalMetrics;

			// Assert
			metrics.TotalDocuments.ShouldBe(100);
			metrics.SuccessfulDocuments.ShouldBe(85);
			metrics.FailedDocuments.ShouldBe(15);
			metrics.SuccessRate.ShouldBe(0.85f);
			metrics.AverageConfidence.ShouldBe(0.75f);
			metrics.AverageProcessingTime.ShouldBe(2500.5);
			metrics.ConfidenceDistribution.ShouldBe(distribution);
			metrics.AdditionalMetrics.ShouldBe(additionalMetrics);
		}
	}
}

/// <summary>
/// Unit tests for SchemaEvolution class
/// </summary>
public class SchemaEvolutionTests
{
	public class ConstructorTests
	{
		[Fact]
		public void Should_InitializeWithDefaults_When_UsingDefaultConstructor()
		{
			// Act
			var evolution = new SchemaEvolution();

			// Assert
			evolution.Id.ShouldNotBeNullOrEmpty();
			evolution.SchemaId.ShouldBe(string.Empty);
			evolution.PreviousVersion.ShouldBe(1);
			evolution.NewVersion.ShouldBe(2);
			evolution.Changes.ShouldNotBeNull();
			evolution.Changes.ShouldBeEmpty();
			evolution.Reason.ShouldBe(string.Empty);
			(DateTime.UtcNow - evolution.EvolvedAt).ShouldBeLessThan(TimeSpan.FromSeconds(5));
			evolution.AccuracyImprovement.ShouldBe(0f);
		}
	}

	public class PropertyTests
	{
		[Fact]
		public void Should_AllowSettingProperties_When_ValidValuesProvided()
		{
			// Arrange
			var evolution = new SchemaEvolution();
			var changes = new List<string> { "Change 1", "Change 2" };
			var testDate = DateTime.UtcNow.AddDays(-2);

			// Act
			evolution.SchemaId = "test-schema";
			evolution.PreviousVersion = 5;
			evolution.NewVersion = 6;
			evolution.Changes = changes;
			evolution.Reason = "Performance improvement";
			evolution.EvolvedAt = testDate;
			evolution.AccuracyImprovement = 0.25f;

			// Assert
			evolution.SchemaId.ShouldBe("test-schema");
			evolution.PreviousVersion.ShouldBe(5);
			evolution.NewVersion.ShouldBe(6);
			evolution.Changes.ShouldBe(changes);
			evolution.Reason.ShouldBe("Performance improvement");
			evolution.EvolvedAt.ShouldBe(testDate);
			evolution.AccuracyImprovement.ShouldBe(0.25f);
		}
	}
}

/// <summary>
/// Unit tests for AdaptationRecommendation class
/// </summary>
public class AdaptationRecommendationTests
{
	public class ConstructorTests
	{
		[Fact]
		public void Should_InitializeWithDefaults_When_UsingDefaultConstructor()
		{
			// Act
			var recommendation = new AdaptationRecommendation();

			// Assert
			recommendation.Id.ShouldNotBeNullOrEmpty();
			recommendation.Type.ShouldBe(RecommendationType.SchemaImprovement);
			recommendation.Priority.ShouldBe(RecommendationPriority.Medium);
			recommendation.Description.ShouldBe(string.Empty);
			recommendation.Confidence.ShouldBe(0f);
			recommendation.ExpectedImpact.ShouldBe(string.Empty);
			(DateTime.UtcNow - recommendation.GeneratedAt).ShouldBeLessThan(TimeSpan.FromSeconds(5));
			recommendation.IsImplemented.ShouldBeFalse();
			recommendation.ImplementedAt.ShouldBeNull();
		}
	}

	public class PropertyTests
	{
		[Theory]
		[InlineData(nameof(RecommendationType.SchemaImprovement))]
		[InlineData(nameof(RecommendationType.ConfidenceImprovement))]
		[InlineData(nameof(RecommendationType.PerformanceOptimization))]
		[InlineData(nameof(RecommendationType.ValidationEnhancement))]
		[InlineData(nameof(RecommendationType.ErrorHandlingImprovement))]
		public void Should_AllowSettingRecommendationType_When_ValidTypeProvided(string recommendationTypeName)
		{
			// Arrange
			var recommendation = new AdaptationRecommendation();
			var type = Enum.Parse<RecommendationType>(recommendationTypeName);

			// Act
			recommendation.Type = type;

			// Assert
			recommendation.Type.ShouldBe(type);
		}

		[Theory]
		[InlineData(nameof(RecommendationPriority.Low))]
		[InlineData(nameof(RecommendationPriority.Medium))]
		[InlineData(nameof(RecommendationPriority.High))]
		[InlineData(nameof(RecommendationPriority.Critical))]
		public void Should_AllowSettingRecommendationPriority_When_ValidPriorityProvided(string priorityName)
		{
			// Arrange
			var recommendation = new AdaptationRecommendation();
			var priority = Enum.Parse<RecommendationPriority>(priorityName);

			// Act
			recommendation.Priority = priority;

			// Assert
			recommendation.Priority.ShouldBe(priority);
		}

		[Fact]
		public void Should_AllowSettingImplementationStatus_When_ValidValuesProvided()
		{
			// Arrange
			var recommendation = new AdaptationRecommendation();
			var implementedDate = DateTime.UtcNow.AddDays(-1);

			// Act
			recommendation.IsImplemented = true;
			recommendation.ImplementedAt = implementedDate;

			// Assert
			recommendation.IsImplemented.ShouldBeTrue();
			recommendation.ImplementedAt.ShouldBe(implementedDate);
		}

		[Fact]
		public void Should_AllowSettingAllProperties_When_ValidValuesProvided()
		{
			// Arrange
			var recommendation = new AdaptationRecommendation();
			var generatedDate = DateTime.UtcNow.AddHours(-2);

			// Act
			recommendation.Description = "Test description";
			recommendation.Confidence = 0.95f;
			recommendation.ExpectedImpact = "Significant improvement";
			recommendation.GeneratedAt = generatedDate;

			// Assert
			recommendation.Description.ShouldBe("Test description");
			recommendation.Confidence.ShouldBe(0.95f);
			recommendation.ExpectedImpact.ShouldBe("Significant improvement");
			recommendation.GeneratedAt.ShouldBe(generatedDate);
		}
	}
}

/// <summary>
/// Unit tests for enum values
/// </summary>
public class EnumTests
{
	public class RecommendationTypeTests
	{
		[Theory]
		[InlineData(RecommendationType.SchemaImprovement)]
		[InlineData(RecommendationType.ConfidenceImprovement)]
		[InlineData(RecommendationType.PerformanceOptimization)]
		[InlineData(RecommendationType.ValidationEnhancement)]
		[InlineData(RecommendationType.ErrorHandlingImprovement)]
		public void Should_HaveValidEnumValues_When_AccessingRecommendationType(RecommendationType type)
		{
			// Act & Assert
			Enum.IsDefined(typeof(RecommendationType), type).ShouldBeTrue();
			type.ToString().ShouldNotBeNullOrEmpty();
		}
	}

	public class RecommendationPriorityTests
	{
		[Theory]
		[InlineData(RecommendationPriority.Low)]
		[InlineData(RecommendationPriority.Medium)]
		[InlineData(RecommendationPriority.High)]
		[InlineData(RecommendationPriority.Critical)]
		public void Should_HaveValidEnumValues_When_AccessingRecommendationPriority(RecommendationPriority priority)
		{
			// Act & Assert
			Enum.IsDefined(typeof(RecommendationPriority), priority).ShouldBeTrue();
			priority.ToString().ShouldNotBeNullOrEmpty();
		}

		[Fact]
		public void Should_HaveCorrectOrderingValues_When_ComparingPriorities()
		{
			// Act & Assert
			((int)RecommendationPriority.Low).ShouldBeLessThan((int)RecommendationPriority.Medium);
			((int)RecommendationPriority.Medium).ShouldBeLessThan((int)RecommendationPriority.High);
			((int)RecommendationPriority.High).ShouldBeLessThan((int)RecommendationPriority.Critical);
		}
	}
} 