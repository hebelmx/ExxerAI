using ExxerAI.Domain.DocumentProcessing;
using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Comprehensive unit tests for TruthRecord domain entity
/// Tests audit trail, status transitions, and truth system business rules
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
        truthRecord.ValidationResults.ShouldNotBeNull();
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
            ProcessedBy = "DocumentIntelligenceAgent"
        };

        var validationResults = new ValidationResult
        {
            IsValid = true,
            Confidence = 0.93f
        };

        // Act
        var truthRecord = new TruthRecord
        {
            Data = extractedData,
            Source = dataSource,
            ValidationResults = validationResults,
            LineageId = "lineage-001",
            DataHash = "sha256hash123",
            ConfidenceScore = 0.92f,
            CreatedBy = "EnhancedDocumentAgent"
        };

        // Assert
        truthRecord.Data.ShouldBe(extractedData);
        truthRecord.Source.ShouldBe(dataSource);
        truthRecord.ValidationResults.ShouldBe(validationResults);
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

/// <summary>
/// Unit tests for DataSource domain entity
/// </summary>
public class DataSourceTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_DataSourceCreated()
    {
        // Act
        var dataSource = new DataSource();

        // Assert
        dataSource.Type.ShouldBe(string.Empty);
        dataSource.Id.ShouldBe(string.Empty);
        dataSource.Path.ShouldBe(string.Empty);
        dataSource.ProcessedBy.ShouldBe(string.Empty);
        dataSource.MCPSessionId.ShouldBeNull();
        dataSource.Metadata.ShouldNotBeNull();
        dataSource.Metadata.ShouldBeEmpty();
        dataSource.AccessedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    [Fact]
    public void Should_SetProperties_When_DataSourceInitializedWithValues()
    {
        // Arrange
        var accessTime = DateTime.UtcNow.AddHours(-2);

        // Act
        var dataSource = new DataSource
        {
            Type = "GoogleDrive",
            Id = "gdrive-doc-123",
            Path = "/business/finance/imss_payments/payment_12_2023.pdf",
            ProcessedBy = "EnhancedDocumentIntelligenceAgent",
            MCPSessionId = "mcp-session-456",
            AccessedAt = accessTime
        };

        dataSource.Metadata["FileSize"] = 2048;
        dataSource.Metadata["MimeType"] = "application/pdf";

        // Assert
        dataSource.Type.ShouldBe("GoogleDrive");
        dataSource.Id.ShouldBe("gdrive-doc-123");
        dataSource.Path.ShouldBe("/business/finance/imss_payments/payment_12_2023.pdf");
        dataSource.ProcessedBy.ShouldBe("EnhancedDocumentIntelligenceAgent");
        dataSource.MCPSessionId.ShouldBe("mcp-session-456");
        dataSource.AccessedAt.ShouldBe(accessTime);
        dataSource.Metadata["FileSize"].ShouldBe(2048);
        dataSource.Metadata["MimeType"].ShouldBe("application/pdf");
    }
}

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

/// <summary>
/// Unit tests for ProcessingStep domain entity
/// </summary>
public class ProcessingStepTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_ProcessingStepCreated()
    {
        // Act
        var step = new ProcessingStep();

        // Assert
        step.StepId.ShouldNotBeNullOrEmpty();
        step.StepName.ShouldBe(string.Empty);
        step.ExecutedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        step.ProcessedBy.ShouldBe(string.Empty);
        step.Input.ShouldNotBeNull();
        step.Input.ShouldBeEmpty();
        step.Output.ShouldNotBeNull();
        step.Output.ShouldBeEmpty();
        step.Duration.ShouldBe(TimeSpan.Zero);
        step.IsSuccessful.ShouldBeTrue();
        step.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public void Should_SetProcessingStepProperties_When_ProcessingStepInitializedWithValues()
    {
        // Arrange
        var executionTime = DateTime.UtcNow.AddMinutes(-5);
        var duration = TimeSpan.FromMilliseconds(1500);

        // Act
        var step = new ProcessingStep
        {
            StepName = "Document OCR Processing",
            ExecutedAt = executionTime,
            ProcessedBy = "TesseractOCREngine",
            Duration = duration,
            IsSuccessful = false,
            ErrorMessage = "OCR processing failed - low image quality"
        };

        step.Input["ImageData"] = "base64imagedata...";
        step.Input["Language"] = "spa,eng";
        step.Output["ErrorCode"] = "OCR_001";
        step.Output["PartialText"] = "PARTIAL EXTRACTED TEXT";

        // Assert
        step.StepName.ShouldBe("Document OCR Processing");
        step.ExecutedAt.ShouldBe(executionTime);
        step.ProcessedBy.ShouldBe("TesseractOCREngine");
        step.Duration.ShouldBe(duration);
        step.IsSuccessful.ShouldBeFalse();
        step.ErrorMessage.ShouldBe("OCR processing failed - low image quality");
        
        step.Input.Count.ShouldBe(2);
        step.Input["ImageData"].ShouldBe("base64imagedata...");
        step.Input["Language"].ShouldBe("spa,eng");
        
        step.Output.Count.ShouldBe(2);
        step.Output["ErrorCode"].ShouldBe("OCR_001");
        step.Output["PartialText"].ShouldBe("PARTIAL EXTRACTED TEXT");
    }

    [Fact]
    public void Should_HandleSuccessfulProcessingStep_When_ProcessingStepCompleted()
    {
        // Arrange
        var step = new ProcessingStep();

        // Act
        step.StepName = "Field Extraction";
        step.ProcessedBy = "FieldExtractionEngine";
        step.Duration = TimeSpan.FromMilliseconds(800);
        step.IsSuccessful = true;
        
        step.Input["ExtractedText"] = "PAYMENT PERIOD: 12-2023\nAMOUNT: $15,000.00";
        step.Input["SchemaDefinition"] = "IMSS_Payment_Schema";
        
        step.Output["PaymentPeriod"] = "12-2023";
        step.Output["Amount"] = "15000.00";
        step.Output["ConfidenceScore"] = 0.95f;

        // Assert
        step.IsSuccessful.ShouldBeTrue();
        step.ErrorMessage.ShouldBeNull();
        step.Duration.TotalMilliseconds.ShouldBe(800);
        
        step.Input.ShouldContainKey("ExtractedText");
        step.Input.ShouldContainKey("SchemaDefinition");
        
        step.Output.ShouldContainKey("PaymentPeriod");
        step.Output.ShouldContainKey("Amount");
        step.Output.ShouldContainKey("ConfidenceScore");
        step.Output["ConfidenceScore"].ShouldBe(0.95f);
    }
} 