using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Unit tests for recommendation-related enums
/// </summary>
public class RecommendationEnumTests
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
			type.ShouldBeOneOf(
				RecommendationType.SchemaImprovement,
				RecommendationType.ConfidenceImprovement,
				RecommendationType.PerformanceOptimization,
				RecommendationType.ValidationEnhancement,
				RecommendationType.ErrorHandlingImprovement
			);
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
			priority.ShouldBeOneOf(
				RecommendationPriority.Low,
				RecommendationPriority.Medium,
				RecommendationPriority.High,
				RecommendationPriority.Critical
			);
		}

		[Fact]
		public void Should_HaveCorrectOrderingValues_When_ComparingPriorities()
		{
			// Assert
			((int)RecommendationPriority.Low).ShouldBeLessThan((int)RecommendationPriority.Medium);
			((int)RecommendationPriority.Medium).ShouldBeLessThan((int)RecommendationPriority.High);
			((int)RecommendationPriority.High).ShouldBeLessThan((int)RecommendationPriority.Critical);
		}
	}
} 