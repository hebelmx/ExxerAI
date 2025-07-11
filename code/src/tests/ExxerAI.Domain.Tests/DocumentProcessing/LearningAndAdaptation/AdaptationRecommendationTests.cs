using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Unit tests for AdaptationRecommendation entity
/// </summary>
public class AdaptationRecommendationTests
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
		recommendation.Confidence.ShouldBe(0.0f);
		recommendation.ExpectedImpact.ShouldBe(string.Empty);
		recommendation.IsImplemented.ShouldBeFalse();
		(DateTime.UtcNow - recommendation.GeneratedAt).ShouldBeLessThan(TimeSpan.FromSeconds(5));
		recommendation.IsImplemented.ShouldBe(false);
		recommendation.ImplementedAt.ShouldBeNull();
	}

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
		var recommendationType = Enum.Parse<RecommendationType>(recommendationTypeName);

		// Act
		recommendation.Type = recommendationType;

		// Assert
		recommendation.Type.ShouldBe(recommendationType);
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

		// Act
		recommendation.IsImplemented = true;
		recommendation.ImplementedAt = DateTime.UtcNow;

		// Assert
		recommendation.IsImplemented.ShouldBeTrue();
		recommendation.ImplementedAt.ShouldNotBeNull();
	}

	[Fact]
	public void Should_AllowSettingAllProperties_When_ValidValuesProvided()
	{
		// Arrange
		var recommendation = new AdaptationRecommendation();

		// Act
		recommendation.Type = RecommendationType.PerformanceOptimization;
		recommendation.Priority = RecommendationPriority.High;
		recommendation.Description = "Optimize document processing pipeline";
		recommendation.Confidence = 0.85f;
		recommendation.ExpectedImpact = "30% faster processing";
		recommendation.IsImplemented = false;

		// Assert
		recommendation.Type.ShouldBe(RecommendationType.PerformanceOptimization);
		recommendation.Priority.ShouldBe(RecommendationPriority.High);
		recommendation.Description.ShouldBe("Optimize document processing pipeline");
		recommendation.Confidence.ShouldBe(0.85f);
		recommendation.ExpectedImpact.ShouldBe("30% faster processing");
		recommendation.IsImplemented.ShouldBeFalse();
	}
} 