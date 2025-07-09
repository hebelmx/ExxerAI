namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Unit tests for ConflictResolution domain entity
/// </summary>
public class ConflictResolutionTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_ConflictResolutionCreated()
    {
        // Act
        var resolution = new ConflictResolution();

        // Assert
        resolution.Id.ShouldNotBeNullOrEmpty();
        resolution.ConflictingSources.ShouldNotBeNull();
        resolution.ConflictingSources.ShouldBeEmpty();
        resolution.Strategy.ShouldBe(ConflictResolutionStrategy.MostConfident);
        resolution.ResolvedData.ShouldNotBeNull();
        resolution.RequiresHumanIntervention.ShouldBeFalse();
        resolution.ResolutionConfidence.ShouldBe(1.0f);
        resolution.ResolvedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        resolution.ResolvedBy.ShouldBe("System");
        resolution.Notes.ShouldBe(string.Empty);
    }

    [Theory]
    [InlineData(nameof(ConflictResolutionStrategy.MostConfident), "Use highest confidence source")]
    [InlineData(nameof(ConflictResolutionStrategy.MostRecent), "Use most recent data")]
    [InlineData(nameof(ConflictResolutionStrategy.MostTrusted), "Use most trusted source")]
    [InlineData(nameof(ConflictResolutionStrategy.Merge), "Merge multiple sources")]
    [InlineData(nameof(ConflictResolutionStrategy.HumanReview), "Require human review")]
    [InlineData(nameof(ConflictResolutionStrategy.Custom), "Use custom algorithm")]
    public void Should_SetResolutionStrategy_When_ConflictResolutionInitializedWithStrategy(
        string strategyName, string expectedNotes)
    {
        // Arrange
        var strategy = Enum.Parse<ConflictResolutionStrategy>(strategyName);

        // Act
        var resolution = new ConflictResolution
        {
            Strategy = strategy,
            Notes = expectedNotes,
            RequiresHumanIntervention = strategy == ConflictResolutionStrategy.HumanReview
        };

        // Assert
        resolution.Strategy.ShouldBe(strategy);
        resolution.Notes.ShouldBe(expectedNotes);
        resolution.RequiresHumanIntervention.ShouldBe(strategy == ConflictResolutionStrategy.HumanReview);
    }

    [Fact]
    public void Should_HandleMultipleConflictingSources_When_ConflictingSourcesAdded()
    {
        // Arrange
        var resolution = new ConflictResolution();
        var source1 = new DataSource { Type = "GoogleDrive", Id = "doc-1", ProcessedBy = "Agent1" };
        var source2 = new DataSource { Type = "EmailAttachment", Id = "doc-2", ProcessedBy = "Agent2" };
        var source3 = new DataSource { Type = "FileUpload", Id = "doc-3", ProcessedBy = "Agent3" };

        // Act
        resolution.ConflictingSources.Add(source1);
        resolution.ConflictingSources.Add(source2);
        resolution.ConflictingSources.Add(source3);

        // Assert
        resolution.ConflictingSources.Count.ShouldBe(3);
        resolution.ConflictingSources.ShouldContain(source1);
        resolution.ConflictingSources.ShouldContain(source2);
        resolution.ConflictingSources.ShouldContain(source3);
    }

    [Fact]
    public void Should_SetResolvedData_When_ConflictResolved()
    {
        // Arrange
        var resolution = new ConflictResolution();
        var resolvedData = new ExtractedData();
        resolvedData.Fields["ResolvedField"] = "Resolved Value";
        resolvedData.FieldConfidences["ResolvedField"] = 0.95f;

        // Act
        resolution.ResolvedData = resolvedData;
        resolution.ResolutionConfidence = 0.93f;
        resolution.ResolvedBy = "ConflictResolutionEngine";

        // Assert
        resolution.ResolvedData.ShouldBe(resolvedData);
        resolution.ResolvedData.Fields.ShouldContainKey("ResolvedField");
        resolution.ResolvedData.Fields["ResolvedField"].ShouldBe("Resolved Value");
        resolution.ResolutionConfidence.ShouldBe(0.93f);
        resolution.ResolvedBy.ShouldBe("ConflictResolutionEngine");
    }
}