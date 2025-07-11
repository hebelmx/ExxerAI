using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Unit tests for LearnedPattern entity
/// </summary>
public class LearnedPatternTests
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
		pattern.Confidence.ShouldBe(0.0f);
		(DateTime.UtcNow - pattern.LearnedAt).ShouldBeLessThan(TimeSpan.FromSeconds(5));
		(DateTime.UtcNow - pattern.LastUsedAt).ShouldBeLessThan(TimeSpan.FromSeconds(5));
	}

	[Theory]
	[InlineData(10, 2, 0.833f)]
	[InlineData(5, 5, 0.5f)]
	[InlineData(20, 0, 1.0f)]
	[InlineData(0, 10, 0.0f)]
	public void Should_CalculateCorrectSuccessRate_When_CountsProvided(int successCount, int failureCount, float expectedRate)
	{
		// Arrange
		var pattern = new LearnedPattern
		{
			SuccessCount = successCount,
			FailureCount = failureCount
		};

		// Act
		var successRate = pattern.SuccessRate;

		// Assert
		successRate.ShouldBe(expectedRate, 0.001);
	}

	[Fact]
	public void Should_ReturnZero_When_NoAttemptsRecorded()
	{
		// Arrange
		var pattern = new LearnedPattern
		{
			SuccessCount = 0,
			FailureCount = 0
		};

		// Act
		var successRate = pattern.SuccessRate;

		// Assert
		successRate.ShouldBe(0.0f);
	}
} 