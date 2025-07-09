namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Unit tests for LearningResult domain entity
/// </summary>
public class LearningResultTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_LearningResultCreated()
    {
        // Act
        var learning = new LearningResult();

        // Assert
        learning.PatternsLearned.ShouldBeFalse();
        learning.NewPatterns.ShouldNotBeNull();
        learning.NewPatterns.ShouldBeEmpty();
        learning.ConfidenceImprovement.ShouldBe(0.0f);
        learning.SchemaUpdates.ShouldNotBeNull();
        learning.SchemaUpdates.ShouldBeEmpty();
    }

    [Fact]
    public void Should_SetLearningResults_When_LearningResultInitializedWithValues()
    {
        // Act
        var learning = new LearningResult
        {
            PatternsLearned = true,
            ConfidenceImprovement = 0.15f
        };
        learning.NewPatterns.Add("Enhanced IMSS date pattern recognition");
        learning.NewPatterns.Add("Improved currency format detection");
        learning.SchemaUpdates.Add("Updated PaymentPeriod field pattern");
        learning.SchemaUpdates.Add("Enhanced Amount validation rules");

        // Assert
        learning.PatternsLearned.ShouldBeTrue();
        learning.ConfidenceImprovement.ShouldBe(0.15f);
        learning.NewPatterns.Count.ShouldBe(2);
        learning.NewPatterns.ShouldContain("Enhanced IMSS date pattern recognition");
        learning.NewPatterns.ShouldContain("Improved currency format detection");
        learning.SchemaUpdates.Count.ShouldBe(2);
        learning.SchemaUpdates.ShouldContain("Updated PaymentPeriod field pattern");
        learning.SchemaUpdates.ShouldContain("Enhanced Amount validation rules");
    }
}