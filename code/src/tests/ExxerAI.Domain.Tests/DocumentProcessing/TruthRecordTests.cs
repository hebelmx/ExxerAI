using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Comprehensive unit tests for TruthRecord domain entity
/// Tests audit trail, agentStatus transitions, and truth system business rules
/// </summary>
public class TruthRecordTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_TruthRecordCreated()
    {
        // Act
        var truthRecord = new TruthRecord();

        // Assert
        truthRecord.Id.ShouldNotBeNullOrEmpty();
        truthRecord.Data.ShouldNotBeNull();
        truthRecord.Source.ShouldNotBeNull();
        truthRecord.ValidationResultsDocument.ShouldNotBeNull();
        truthRecord.Timestamp.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        truthRecord.LineageId.ShouldBe(string.Empty);
        truthRecord.DataHash.ShouldBe(string.Empty);
        truthRecord.Status.ShouldBe(TruthRecordStatus.Active);
        truthRecord.Version.ShouldBe(1);
        truthRecord.ConfidenceScore.ShouldBe(1.0f);
        truthRecord.ConflictResolution.ShouldBeNull();
        truthRecord.Metadata.ShouldNotBeNull();
        truthRecord.Metadata.ShouldBeEmpty();
        truthRecord.CreatedBy.ShouldBe("System");
        truthRecord.LastModified.ShouldBeNull();
        truthRecord.ModifiedBy.ShouldBeNull();
        truthRecord.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Should_MarkAsSuperseded_When_MarkAsSupersededCalled()
    {
        // Arrange
        var truthRecord = new TruthRecord();
        var newVersionId = "truth-v2-001";
        var modifiedBy = "TestUser";
        var originalTimestamp = truthRecord.Timestamp;

        // Act
        truthRecord.MarkAsSuperseded(newVersionId, modifiedBy);

        // Assert
        truthRecord.Status.ShouldBe(TruthRecordStatus.Superseded);
        truthRecord.LastModified.ShouldNotBeNull();
        truthRecord.LastModified!.Value.ShouldBeGreaterThan(originalTimestamp);
        truthRecord.ModifiedBy.ShouldBe(modifiedBy);
        truthRecord.Metadata.ShouldContainKey("SupersededBy");
        truthRecord.Metadata["SupersededBy"].ShouldBe(newVersionId);
        truthRecord.IsActive.ShouldBeFalse();
    }

    [Fact]
    public void Should_RequireHumanReview_When_RequireHumanReviewCalled()
    {
        // Arrange
        var truthRecord = new TruthRecord();
        var reviewReason = "Low confidence score detected";
        var originalTimestamp = truthRecord.Timestamp;

        // Act
        truthRecord.RequireHumanReview(reviewReason);

        // Assert
        truthRecord.Status.ShouldBe(TruthRecordStatus.RequiresHumanReview);
        truthRecord.LastModified.ShouldNotBeNull();
        truthRecord.LastModified!.Value.ShouldBeGreaterThan(originalTimestamp);
        truthRecord.Metadata.ShouldContainKey("ReviewReason");
        truthRecord.Metadata["ReviewReason"].ShouldBe(reviewReason);
        truthRecord.Metadata.ShouldContainKey("ReviewRequested");
        truthRecord.Metadata["ReviewRequested"].ShouldBeOfType<DateTime>();
        truthRecord.IsActive.ShouldBeFalse();
    }

    [Theory]
    [InlineData(nameof(TruthRecordStatus.Active), true)]
    [InlineData(nameof(TruthRecordStatus.Superseded), false)]
    [InlineData(nameof(TruthRecordStatus.UnderReview), false)]
    [InlineData(nameof(TruthRecordStatus.RequiresHumanReview), false)]
    [InlineData(nameof(TruthRecordStatus.Archived), false)]
    [InlineData(nameof(TruthRecordStatus.Invalid), false)]
    public void Should_ReturnCorrectIsActive_When_StatusSet(string statusName, bool expectedIsActive)
    {
        // Arrange
        var truthRecord = new TruthRecord();
        var status = Enum.Parse<TruthRecordStatus>(statusName);

        // Act
        truthRecord.Status = status;

        // Assert
        truthRecord.IsActive.ShouldBe(expectedIsActive);
    }

    [Fact]
    public void Should_SetComplexTruthRecord_When_InitializedWithBusinessData()
    {
        // Arrange
        var extractedData = new ExtractedData();
        extractedData.Fields["PaymentPeriod"] = "12-2023";
        extractedData.Fields["Amount"] = "15000.00";
        extractedData.FieldConfidences["PaymentPeriod"] = 0.95f;
        extractedData.FieldConfidences["Amount"] = 0.90f;

        var dataSource = new DataSource
        {
            Type = "GoogleDrive",
            Id = "doc-123",
            Path = "/business/imss/payment_12_2023.pdf",
            ProcessedBy = "DocumentIntelligenceAgent",
            ProcessedAt = DateTime.UtcNow
        };

        var validationResults = new ValidationResultDocument
        {
            IsValid = true,
            Confidence = 0.93f
        };

        // Act
        var truthRecord = new TruthRecord
        {
            Data = extractedData,
            Source = dataSource,
            ValidationResultsDocument = validationResults,
            LineageId = "lineage-001",
            DataHash = "sha256hash123",
            ConfidenceScore = 0.92f,
            CreatedBy = "EnhancedDocumentAgent"
        };

        // Assert
        truthRecord.Data.ShouldBe(extractedData);
        truthRecord.Source.ShouldBe(dataSource);
        truthRecord.ValidationResultsDocument.ShouldBe(validationResults);
        truthRecord.LineageId.ShouldBe("lineage-001");
        truthRecord.DataHash.ShouldBe("sha256hash123");
        truthRecord.ConfidenceScore.ShouldBe(0.92f);
        truthRecord.CreatedBy.ShouldBe("EnhancedDocumentAgent");
        truthRecord.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Should_HandleConflictResolution_When_ConflictResolutionSet()
    {
        // Arrange
        var truthRecord = new TruthRecord();
        var conflictResolution = new ConflictResolution
        {
            Strategy = ConflictResolutionStrategy.MostConfident,
            RequiresHumanIntervention = false,
            ResolutionConfidence = 0.88f,
            ResolvedBy = "AutoResolutionEngine",
            Notes = "Resolved using highest confidence source"
        };

        // Act
        truthRecord.ConflictResolution = conflictResolution;

        // Assert
        truthRecord.ConflictResolution.ShouldNotBeNull();
        truthRecord.ConflictResolution.Strategy.ShouldBe(ConflictResolutionStrategy.MostConfident);
        truthRecord.ConflictResolution.RequiresHumanIntervention.ShouldBeFalse();
        truthRecord.ConflictResolution.ResolutionConfidence.ShouldBe(0.88f);
        truthRecord.ConflictResolution.ResolvedBy.ShouldBe("AutoResolutionEngine");
        truthRecord.ConflictResolution.Notes.ShouldBe("Resolved using highest confidence source");
    }

    [Fact]
    public void Should_PreserveMetadata_When_MetadataManipulated()
    {
        // Arrange
        var truthRecord = new TruthRecord();

        // Act
        truthRecord.Metadata["ProcessingAgent"] = "DocumentIntelligenceAgent";
        truthRecord.Metadata["DocumentType"] = "IMSS Payment";
        truthRecord.Metadata["OriginalFilename"] = "payment_12_2023.pdf";
        truthRecord.Metadata["ProcessingDuration"] = 1500;

        // Assert
        truthRecord.Metadata.Count.ShouldBe(4);
        truthRecord.Metadata["ProcessingAgent"].ShouldBe("DocumentIntelligenceAgent");
        truthRecord.Metadata["DocumentType"].ShouldBe("IMSS Payment");
        truthRecord.Metadata["OriginalFilename"].ShouldBe("payment_12_2023.pdf");
        truthRecord.Metadata["ProcessingDuration"].ShouldBe(1500);
    }
}