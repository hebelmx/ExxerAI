namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Unit tests for DataLineage domain entity
/// </summary>
public class DataLineageTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_DataLineageCreated()
    {
        // Act
        var lineage = new DataLineage();

        // Assert
        lineage.Id.ShouldNotBeNullOrEmpty();
        lineage.OriginalSource.ShouldNotBeNull();
        lineage.ProcessingSteps.ShouldNotBeNull();
        lineage.ProcessingSteps.ShouldBeEmpty();
        lineage.FinalTruthRecordId.ShouldBe(string.Empty);
        lineage.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        lineage.Metadata.ShouldNotBeNull();
        lineage.Metadata.ShouldBeEmpty();
    }

    [Fact]
    public void Should_TrackProcessingSteps_When_ProcessingStepsAdded()
    {
        // Arrange
        var lineage = new DataLineage();
        
        var step1 = new ProcessingStep
        {
            StepName = "Document Download",
            ProcessedBy = "MCPGoogleDriveService",
            Duration = TimeSpan.FromMilliseconds(500),
            IsSuccessful = true
        };
        
        var step2 = new ProcessingStep
        {
            StepName = "Text Extraction",
            ProcessedBy = "PolymorphicDocumentProcessor",
            Duration = TimeSpan.FromMilliseconds(1200),
            IsSuccessful = true
        };
        
        var step3 = new ProcessingStep
        {
            StepName = "Field Validation",
            ProcessedBy = "ValidationEngine",
            Duration = TimeSpan.FromMilliseconds(300),
            IsSuccessful = true
        };

        // Act
        lineage.ProcessingSteps.Add(step1);
        lineage.ProcessingSteps.Add(step2);
        lineage.ProcessingSteps.Add(step3);
        lineage.FinalTruthRecordId = "truth-record-001";

        // Assert
        lineage.ProcessingSteps.Count.ShouldBe(3);
        lineage.ProcessingSteps[0].StepName.ShouldBe("Document Download");
        lineage.ProcessingSteps[1].StepName.ShouldBe("Text Extraction");
        lineage.ProcessingSteps[2].StepName.ShouldBe("Field Validation");
        lineage.FinalTruthRecordId.ShouldBe("truth-record-001");

        // Verify all steps are successful
        lineage.ProcessingSteps.All(step => step.IsSuccessful).ShouldBeTrue();
    }

    [Fact]
    public void Should_SetOriginalSource_When_DataLineageInitializedWithSource()
    {
        // Arrange
        var originalSource = new DataSource
        {
            Type = "GoogleDrive",
            Id = "original-doc-123",
            Path = "/source/documents/original.pdf",
            ProcessedBy = "InitialProcessor"
        };

        // Act
        var lineage = new DataLineage
        {
            OriginalSource = originalSource
        };

        // Assert
        lineage.OriginalSource.ShouldBe(originalSource);
        lineage.OriginalSource.Type.ShouldBe("GoogleDrive");
        lineage.OriginalSource.Id.ShouldBe("original-doc-123");
        lineage.OriginalSource.Path.ShouldBe("/source/documents/original.pdf");
        lineage.OriginalSource.ProcessedBy.ShouldBe("InitialProcessor");
    }
} 