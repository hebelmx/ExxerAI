using ExxerAI.Domain.DocumentProcessing;
using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Unit tests for ProcessingMetrics entity
/// </summary>
public class ProcessingMetricsTests
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
		metrics.SuccessRate.ShouldBe(0.0f);
		metrics.AverageConfidence.ShouldBe(0.0f);
		metrics.AverageProcessingTime.ShouldBe(0.0);
		metrics.ConfidenceDistribution.ShouldNotBeNull();
		metrics.ConfidenceDistribution.ShouldBeEmpty();
		metrics.AdditionalMetrics.ShouldNotBeNull();
		metrics.AdditionalMetrics.ShouldBeEmpty();
	}

	[Fact]
	public void Should_AllowSettingProperties_When_ValidValuesProvided()
	{
		// Arrange
		var metrics = new ProcessingMetrics();

		// Act
		metrics.TotalDocuments = 100;
		metrics.SuccessfulDocuments = 85;
		metrics.FailedDocuments = 15;
		metrics.SuccessRate = 0.85f;
		metrics.AverageConfidence = 0.75f;
		metrics.AverageProcessingTime = 2500.0;

		// Assert
		metrics.TotalDocuments.ShouldBe(100);
		metrics.SuccessfulDocuments.ShouldBe(85);
		metrics.FailedDocuments.ShouldBe(15);
		metrics.SuccessRate.ShouldBe(0.85f);
		metrics.AverageConfidence.ShouldBe(0.75f);
		metrics.AverageProcessingTime.ShouldBe(2500.0);
	}
} 