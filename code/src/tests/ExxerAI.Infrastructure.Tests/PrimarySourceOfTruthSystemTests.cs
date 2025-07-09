using ExxerAI.Application.Interfaces;

namespace ExxerAI.Infrastructure.Tests;

/// <summary>
/// Comprehensive ITDD test bed for IPrimarySourceOfTruthSystem
/// Tests the interface contract and expected behaviors without implementation details
/// </summary>
public class PrimarySourceOfTruthSystemTests
{
    private readonly IPrimarySourceOfTruthSystem _truthSystem;
    private readonly CancellationToken _cancellationToken;

    public PrimarySourceOfTruthSystemTests()
    {
        _truthSystem = Substitute.For<IPrimarySourceOfTruthSystem>();
        _cancellationToken = TestContext.Current.CancellationToken;
    }

    public class StoreExtractedDataAsyncTests : PrimarySourceOfTruthSystemTests
    {
        [Fact]
        public async Task Should_ReturnSuccessResult_When_ValidDataProvidedAsync()
        {
            // Arrange
            var extractedData = CreateValidExtractedData();
            var dataSource = CreateValidDataSource();
            var expectedTruthRecord = CreateValidTruthRecord();

            _truthSystem.StoreExtractedDataAsync(extractedData, dataSource, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<TruthRecord>.Success(expectedTruthRecord));

            // Act
            var result = await _truthSystem.StoreExtractedDataAsync(extractedData, dataSource, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldNotBeNull();
            result.Value!.Id.ShouldBe(expectedTruthRecord.Id);
        }

        [Fact]
        public async Task Should_ReturnFailureResult_When_NullDataProvidedAsync()
        {
            // Arrange
            ExtractedData nullData = null!;
            var dataSource = CreateValidDataSource();

            _truthSystem.StoreExtractedDataAsync(nullData, dataSource, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<TruthRecord>.WithFailure("Value cannot be null"));

            // Act
            var result = await _truthSystem.StoreExtractedDataAsync(nullData, dataSource, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Value cannot be null");
        }

        [Fact]
        public async Task Should_ReturnFailureResult_When_NullDataSourceProvidedAsync()
        {
            // Arrange
            var extractedData = CreateValidExtractedData();
            DataSource nullSource = null!;

            _truthSystem.StoreExtractedDataAsync(extractedData, nullSource, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<TruthRecord>.WithFailure("Value source cannot be null"));

            // Act
            var result = await _truthSystem.StoreExtractedDataAsync(extractedData, nullSource, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Value source cannot be null");
        }

        [Fact]
        public async Task Should_HandleCancellation_When_CancellationRequestedAsync()
        {
            // Arrange
            var extractedData = CreateValidExtractedData();
            var dataSource = CreateValidDataSource();
            var cancellationToken = new CancellationToken(true);

            _truthSystem.StoreExtractedDataAsync(extractedData, dataSource, cancellationToken)
                .Returns(Result<TruthRecord>.WithFailure("Operation was cancelled"));

            // Act
            var result = await _truthSystem.StoreExtractedDataAsync(extractedData, dataSource, cancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Operation was cancelled");
        }
    }

    public class ValidateAgainstTruthAsyncTests : PrimarySourceOfTruthSystemTests
    {
        [Fact]
        public async Task Should_ReturnValidationResult_When_ValidDataProvidedAsync()
        {
            // Arrange
            var extractedData = CreateValidExtractedData();
            var expectedValidationResult = CreateValidValidationResult();

            _truthSystem.ValidateAgainstTruthAsync(extractedData, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<ExxerAI.Domain.DocumentProcessing.ValidationResult>.Success(expectedValidationResult));

            // Act
            var result = await _truthSystem.ValidateAgainstTruthAsync(extractedData, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldNotBeNull();
            result.Value!.IsValid.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_ReturnInvalidResult_When_DataConflictsWithTruthAsync()
        {
            // Arrange
            var conflictingData = CreateConflictingExtractedData();
            var validationResult = CreateInvalidValidationResult();

            _truthSystem.ValidateAgainstTruthAsync(conflictingData, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<ExxerAI.Domain.DocumentProcessing.ValidationResult>.Success(validationResult));

            // Act
            var result = await _truthSystem.ValidateAgainstTruthAsync(conflictingData, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.IsValid.ShouldBeFalse();
            result.Value!.Errors.ShouldNotBeEmpty();
        }
    }

    public class GetAuthoritativeRecordAsyncTests : PrimarySourceOfTruthSystemTests
    {
        [Fact]
        public async Task Should_ReturnTruthRecord_When_ValidRecordIdProvidedAsync()
        {
            // Arrange
            var recordId = "valid-record-id";
            var expectedRecord = CreateValidTruthRecord();

            _truthSystem.GetAuthoritativeRecordAsync(recordId, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<TruthRecord>.Success(expectedRecord));

            // Act
            var result = await _truthSystem.GetAuthoritativeRecordAsync(recordId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldNotBeNull();
            result.Value!.Id.ShouldBe(expectedRecord.Id);
        }

        [Fact]
        public async Task Should_ReturnNotFound_When_RecordDoesNotExistAsync()
        {
            // Arrange
            var nonExistentId = "non-existent-id";

            _truthSystem.GetAuthoritativeRecordAsync(nonExistentId, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<TruthRecord>.WithFailure("Record not found"));

            // Act
            var result = await _truthSystem.GetAuthoritativeRecordAsync(nonExistentId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Record not found");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public async Task Should_ReturnFailure_When_InvalidRecordIdProvidedAsync(string? invalidId)
        {
            // Arrange
            _truthSystem.GetAuthoritativeRecordAsync(invalidId!, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<TruthRecord>.WithFailure("Record ID cannot be empty"));

            // Act
            var result = await _truthSystem.GetAuthoritativeRecordAsync(invalidId!, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Record ID cannot be empty");
        }
    }

    public class ResolveDataConflictAsyncTests : PrimarySourceOfTruthSystemTests
    {
        [Fact]
        public async Task Should_ReturnConflictResolution_When_ConflictingDataProvidedAsync()
        {
            // Arrange
            var conflictingData = CreateConflictingDataSet();
            var expectedResolution = CreateValidConflictResolution();

            _truthSystem.ResolveDataConflictAsync(conflictingData, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<ConflictResolution>.Success(expectedResolution));

            // Act
            var result = await _truthSystem.ResolveDataConflictAsync(conflictingData, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldNotBeNull();
            result.Value!.ResolvedData.ShouldNotBeNull();
        }

        [Fact]
        public async Task Should_ReturnFailure_When_EmptyConflictingDataProvidedAsync()
        {
            // Arrange
            var emptyData = Array.Empty<ExtractedData>();

            _truthSystem.ResolveDataConflictAsync(emptyData, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<ConflictResolution>.WithFailure("No conflicting data provided"));

            // Act
            var result = await _truthSystem.ResolveDataConflictAsync(emptyData, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("No conflicting data provided");
        }
    }

    public class GetDataLineageAsyncTests : PrimarySourceOfTruthSystemTests
    {
        [Fact]
        public async Task Should_ReturnDataLineage_When_ValidRecordIdProvidedAsync()
        {
            // Arrange
            var recordId = "valid-record-id";
            var expectedLineage = CreateValidDataLineage();

            _truthSystem.GetDataLineageAsync(recordId, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<DataLineage>.Success(expectedLineage));

            // Act
            var result = await _truthSystem.GetDataLineageAsync(recordId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldNotBeNull();
            result.Value!.RecordId.ShouldBe(recordId);
        }
    }

    public class GenerateGroundingReportAsyncTests : PrimarySourceOfTruthSystemTests
    {
        [Fact]
        public async Task Should_ReturnGroundingReport_When_ValidDateRangeProvidedAsync()
        {
            // Arrange
            var fromDate = DateTime.UtcNow.AddDays(-30);
            var toDate = DateTime.UtcNow;
            var expectedReport = CreateValidGroundingReport(fromDate, toDate);

            _truthSystem.GenerateGroundingReportAsync(fromDate, toDate, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<ExxerAI.Domain.DocumentProcessing.GroundingReport>.Success(expectedReport));

            // Act
            var result = await _truthSystem.GenerateGroundingReportAsync(fromDate, toDate, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldNotBeNull();
            result.Value!.FromDate.ShouldBe(fromDate);
            result.Value!.ToDate.ShouldBe(toDate);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_InvalidDateRangeProvidedAsync()
        {
            // Arrange
            var fromDate = DateTime.UtcNow;
            var toDate = DateTime.UtcNow.AddDays(-30); // Invalid: to date before from date

            _truthSystem.GenerateGroundingReportAsync(fromDate, toDate, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<ExxerAI.Domain.DocumentProcessing.GroundingReport>.WithFailure("Invalid date range"));

            // Act
            var result = await _truthSystem.GenerateGroundingReportAsync(fromDate, toDate, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Invalid date range");
        }
    }

    public class FindSimilarRecordsAsyncTests : PrimarySourceOfTruthSystemTests
    {
        [Fact]
        public async Task Should_ReturnSimilarRecords_When_ValidDataProvidedAsync()
        {
            // Arrange
            var extractedData = CreateValidExtractedData();
            var similarityThreshold = 0.85f;
            var expectedRecords = CreateSimilarTruthRecords();

            _truthSystem.FindSimilarRecordsAsync(extractedData, similarityThreshold, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<TruthRecord>>.Success(expectedRecords));

            // Act
            var result = await _truthSystem.FindSimilarRecordsAsync(extractedData, similarityThreshold, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldNotBeNull();
            result.Value!.Count().ShouldBeGreaterThan(0);
        }

        [Theory]
        [InlineData(-0.1f)]
        [InlineData(1.1f)]
        [InlineData(float.NaN)]
        public async Task Should_ReturnFailure_When_InvalidSimilarityThresholdProvidedAsync(float invalidThreshold)
        {
            // Arrange
            var extractedData = CreateValidExtractedData();

            _truthSystem.FindSimilarRecordsAsync(extractedData, invalidThreshold, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<TruthRecord>>.WithFailure("Invalid similarity threshold"));

            // Act
            var result = await _truthSystem.FindSimilarRecordsAsync(extractedData, invalidThreshold, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Invalid similarity threshold");
        }
    }

    public class UpdateTruthRecordAsyncTests : PrimarySourceOfTruthSystemTests
    {
        [Fact]
        public async Task Should_ReturnUpdatedRecord_When_ValidUpdateProvidedAsync()
        {
            // Arrange
            var recordId = "valid-record-id";
            var updatedData = CreateValidExtractedData();
            var updatedBy = "test-user";
            var expectedRecord = CreateValidTruthRecord(recordId);

            _truthSystem.UpdateTruthRecordAsync(recordId, updatedData, updatedBy, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<TruthRecord>.Success(expectedRecord));

            // Act
            var result = await _truthSystem.UpdateTruthRecordAsync(recordId, updatedData, updatedBy, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldNotBeNull();
            result.Value!.Id.ShouldBe(recordId);
        }
    }

    public class RequireHumanReviewAsyncTests : PrimarySourceOfTruthSystemTests
    {
        [Fact]
        public async Task Should_ReturnSuccess_When_ValidReviewRequestProvidedAsync()
        {
            // Arrange
            var recordId = "valid-record-id";
            var reason = "Value quality concerns";

            _truthSystem.RequireHumanReviewAsync(recordId, reason, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<bool>.Success(true));

            // Act
            var result = await _truthSystem.RequireHumanReviewAsync(recordId, reason, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldBeTrue();
        }
    }

    public class GetRecordsRequiringReviewAsyncTests : PrimarySourceOfTruthSystemTests
    {
        [Fact]
        public async Task Should_ReturnRecordsRequiringReview_When_CalledAsync()
        {
            // Arrange
            var expectedRecords = CreateRecordsRequiringReview();

            _truthSystem.GetRecordsRequiringReviewAsync(cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<IEnumerable<TruthRecord>>.Success(expectedRecords));

            // Act
            var result = await _truthSystem.GetRecordsRequiringReviewAsync(cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldNotBeNull();
        }
    }

    public class GetDataQualityMetricsAsyncTests : PrimarySourceOfTruthSystemTests
    {
        [Fact]
        public async Task Should_ReturnQualityMetrics_When_ValidDateRangeProvidedAsync()
        {
            // Arrange
            var fromDate = DateTime.UtcNow.AddDays(-30);
            var toDate = DateTime.UtcNow;
            var expectedMetrics = CreateValidDataQualityMetrics(fromDate, toDate);

            _truthSystem.GetDataQualityMetricsAsync(fromDate, toDate, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<ExxerAI.Domain.DocumentProcessing.DataQualityMetrics>.Success(expectedMetrics));

            // Act
            var result = await _truthSystem.GetDataQualityMetricsAsync(fromDate, toDate, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldNotBeNull();
            result.Value!.FromDate.ShouldBe(fromDate);
            result.Value!.ToDate.ShouldBe(toDate);
        }
    }

    // Test Value Factory Methods
    private static ExtractedData CreateValidExtractedData()
    {
        return new ExtractedData
        {
            DocumentId = "doc-123",
            ExtractedFields = new Dictionary<string, object>
            {
                { "InvoiceNumber", "INV-2024-001" },
                { "Amount", 1250.50m },
                { "Date", DateTime.UtcNow }
            },
            ConfidenceScore = 0.95f,
            ProcessedAt = DateTime.UtcNow
        };
    }

    private static DataSource CreateValidDataSource()
    {
        return new DataSource
        {
            Type = "GoogleDrive",
            Id = "drive-doc-123",
            Path = "/invoices/2024/invoice-001.pdf",
            ProcessedBy = "PolymorphicDocumentProcessor"
        };
    }

    private static TruthRecord CreateValidTruthRecord(string? id = null)
    {
        return new TruthRecord
        {
            Id = id ?? "truth-record-123",
            Data = CreateValidExtractedData(),
            Source = CreateValidDataSource(),
            CreatedAt = DateTime.UtcNow,
            Version = 1,
            Status = "Active"
        };
    }

    private static ExxerAI.Domain.DocumentProcessing.ValidationResult CreateValidValidationResult()
    {
        return new ExxerAI.Domain.DocumentProcessing.ValidationResult
        {
            IsValid = true,
            Errors = Array.Empty<string>(),
            Warnings = Array.Empty<string>(),
            ValidatedAt = DateTime.UtcNow
        };
    }

    private static ExxerAI.Domain.DocumentProcessing.ValidationResult CreateInvalidValidationResult()
    {
        return new ExxerAI.Domain.DocumentProcessing.ValidationResult
        {
            IsValid = false,
            Errors = new[] { "Amount mismatch with existing records" },
            Warnings = new[] { "Date format inconsistency" },
            ValidatedAt = DateTime.UtcNow
        };
    }

    private static ExtractedData CreateConflictingExtractedData()
    {
        return new ExtractedData
        {
            DocumentId = "doc-123",
            ExtractedFields = new Dictionary<string, object>
            {
                { "InvoiceNumber", "INV-2024-001" },
                { "Amount", 999.99m }, // Conflicting amount
                { "Date", DateTime.UtcNow }
            },
            ConfidenceScore = 0.75f,
            ProcessedAt = DateTime.UtcNow
        };
    }

    private static IEnumerable<ExtractedData> CreateConflictingDataSet()
    {
        return new[]
        {
            CreateValidExtractedData(),
            CreateConflictingExtractedData()
        };
    }

    private static ConflictResolution CreateValidConflictResolution()
    {
        return new ConflictResolution
        {
            ResolvedData = CreateValidExtractedData(),
            Strategy = ConflictResolutionStrategy.MostConfident,
            ResolvedAt = DateTime.UtcNow,
            ConflictingSourcesData = CreateConflictingDataSet().ToList()
        };
    }

    private static DataLineage CreateValidDataLineage()
    {
        return new DataLineage
        {
            RecordId = "valid-record-id",
            SourceDocuments = new[] { "doc-123", "doc-456" },
            ProcessingHistory = new[] { "OCR", "Validation", "Grounding" },
            CreatedAt = DateTime.UtcNow
        };
    }

    private static ExxerAI.Domain.DocumentProcessing.GroundingReport CreateValidGroundingReport(DateTime? fromDate = null, DateTime? toDate = null)
    {
        return new ExxerAI.Domain.DocumentProcessing.GroundingReport
        {
            FromDate = fromDate ?? DateTime.UtcNow.AddDays(-30),
            ToDate = toDate ?? DateTime.UtcNow,
            TotalRecords = 1000,
            ValidRecords = 950,
            InvalidRecords = 50,
            AverageConfidence = 0.92f,
            GeneratedAt = DateTime.UtcNow
        };
    }

    private static IEnumerable<TruthRecord> CreateSimilarTruthRecords()
    {
        return new[]
        {
            CreateValidTruthRecord(),
            new TruthRecord
            {
                Id = "similar-record-456",
                Data = CreateValidExtractedData(),
                Source = CreateValidDataSource(),
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Version = 1,
                Status = "Active"
            }
        };
    }

    private static IEnumerable<TruthRecord> CreateRecordsRequiringReview()
    {
        return new[]
        {
            new TruthRecord
            {
                Id = "review-record-789",
                Data = CreateValidExtractedData(),
                Source = CreateValidDataSource(),
                CreatedAt = DateTime.UtcNow,
                Version = 1,
                Status = "RequiresReview"
            }
        };
    }

    private static ExxerAI.Domain.DocumentProcessing.DataQualityMetrics CreateValidDataQualityMetrics(DateTime? fromDate = null, DateTime? toDate = null)
    {
        return new ExxerAI.Domain.DocumentProcessing.DataQualityMetrics
        {
            FromDate = fromDate ?? DateTime.UtcNow.AddDays(-30),
            ToDate = toDate ?? DateTime.UtcNow,
            TotalRecords = 1000,
            HighQualityRecords = 900,
            MediumQualityRecords = 80,
            LowQualityRecords = 20,
            AverageConfidence = 0.92f,
            DataCompleteness = 0.98f,
            DataAccuracy = 0.95f
        };
    }
}