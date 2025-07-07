using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.DocumentProcessing;
using ExxerAI.Domain.Operations;
using ExxerAI.Domain;
using NSubstitute;
using Shouldly;

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
        _cancellationToken = CancellationToken.None;
    }

    public class StoreExtractedDataAsyncTests : PrimarySourceOfTruthSystemTests
    {
        [Fact]
        public async Task Should_ReturnSuccessResult_When_ValidDataProvided()
        {
            // Arrange
            var extractedData = CreateValidExtractedData();
            var dataSource = CreateValidDataSource();
            var expectedTruthRecord = CreateValidTruthRecord();
            
            _truthSystem.StoreExtractedDataAsync(extractedData, dataSource, _cancellationToken)
                .Returns(Result<TruthRecord>.Success(expectedTruthRecord));

            // Act
            var result = await _truthSystem.StoreExtractedDataAsync(extractedData, dataSource, _cancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.Id.ShouldBe(expectedTruthRecord.Id);
        }

        [Fact]
        public async Task Should_ReturnFailureResult_When_NullDataProvided()
        {
            // Arrange
            ExtractedData nullData = null!;
            var dataSource = CreateValidDataSource();
            
            _truthSystem.StoreExtractedDataAsync(nullData, dataSource, _cancellationToken)
                .Returns(Result<TruthRecord>.WithFailure("Data cannot be null"));

            // Act
            var result = await _truthSystem.StoreExtractedDataAsync(nullData, dataSource, _cancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Data cannot be null");
        }

        [Fact]
        public async Task Should_ReturnFailureResult_When_NullDataSourceProvided()
        {
            // Arrange
            var extractedData = CreateValidExtractedData();
            DataSource nullSource = null!;
            
            _truthSystem.StoreExtractedDataAsync(extractedData, nullSource, _cancellationToken)
                .Returns(Result<TruthRecord>.WithFailure("Data source cannot be null"));

            // Act
            var result = await _truthSystem.StoreExtractedDataAsync(extractedData, nullSource, _cancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Data source cannot be null");
        }

        [Fact]
        public async Task Should_HandleCancellation_When_CancellationRequested()
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
        public async Task Should_ReturnValidationResult_When_ValidDataProvided()
        {
            // Arrange
            var extractedData = CreateValidExtractedData();
            var expectedValidationResult = CreateValidValidationResult();
            
            _truthSystem.ValidateAgainstTruthAsync(extractedData, _cancellationToken)
                .Returns(Result<DocumentProcessing.ValidationResult>.Success(expectedValidationResult));

            // Act
            var result = await _truthSystem.ValidateAgainstTruthAsync(extractedData, _cancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.IsValid.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_ReturnInvalidResult_When_DataConflictsWithTruth()
        {
            // Arrange
            var conflictingData = CreateConflictingExtractedData();
            var validationResult = CreateInvalidValidationResult();
            
            _truthSystem.ValidateAgainstTruthAsync(conflictingData, _cancellationToken)
                .Returns(Result<DocumentProcessing.ValidationResult>.Success(validationResult));

            // Act
            var result = await _truthSystem.ValidateAgainstTruthAsync(conflictingData, _cancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.IsValid.ShouldBeFalse();
            result.Value.Errors.ShouldNotBeEmpty();
        }
    }

    public class GetAuthoritativeRecordAsyncTests : PrimarySourceOfTruthSystemTests
    {
        [Fact]
        public async Task Should_ReturnTruthRecord_When_ValidRecordIdProvided()
        {
            // Arrange
            var recordId = "valid-record-id";
            var expectedRecord = CreateValidTruthRecord();
            
            _truthSystem.GetAuthoritativeRecordAsync(recordId, _cancellationToken)
                .Returns(Result<TruthRecord>.Success(expectedRecord));

            // Act
            var result = await _truthSystem.GetAuthoritativeRecordAsync(recordId, _cancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.Id.ShouldBe(expectedRecord.Id);
        }

        [Fact]
        public async Task Should_ReturnNotFound_When_RecordDoesNotExist()
        {
            // Arrange
            var nonExistentId = "non-existent-id";
            
            _truthSystem.GetAuthoritativeRecordAsync(nonExistentId, _cancellationToken)
                .Returns(Result<TruthRecord>.WithFailure("Record not found"));

            // Act
            var result = await _truthSystem.GetAuthoritativeRecordAsync(nonExistentId, _cancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Record not found");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task Should_ReturnFailure_When_InvalidRecordIdProvided(string invalidId)
        {
            // Arrange
            _truthSystem.GetAuthoritativeRecordAsync(invalidId, _cancellationToken)
                .Returns(Result<TruthRecord>.WithFailure("Record ID cannot be empty"));

            // Act
            var result = await _truthSystem.GetAuthoritativeRecordAsync(invalidId, _cancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Record ID cannot be empty");
        }
    }

    public class ResolveDataConflictAsyncTests : PrimarySourceOfTruthSystemTests
    {
        [Fact]
        public async Task Should_ReturnConflictResolution_When_ConflictingDataProvided()
        {
            // Arrange
            var conflictingData = CreateConflictingDataSet();
            var expectedResolution = CreateValidConflictResolution();
            
            _truthSystem.ResolveDataConflictAsync(conflictingData, _cancellationToken)
                .Returns(Result<ConflictResolution>.Success(expectedResolution));

            // Act
            var result = await _truthSystem.ResolveDataConflictAsync(conflictingData, _cancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.ResolvedData.ShouldNotBeNull();
        }

        [Fact]
        public async Task Should_ReturnFailure_When_EmptyConflictingDataProvided()
        {
            // Arrange
            var emptyData = Array.Empty<ExtractedData>();
            
            _truthSystem.ResolveDataConflictAsync(emptyData, _cancellationToken)
                .Returns(Result<ConflictResolution>.WithFailure("No conflicting data provided"));

            // Act
            var result = await _truthSystem.ResolveDataConflictAsync(emptyData, _cancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("No conflicting data provided");
        }
    }

    public class GetDataLineageAsyncTests : PrimarySourceOfTruthSystemTests
    {
        [Fact]
        public async Task Should_ReturnDataLineage_When_ValidRecordIdProvided()
        {
            // Arrange
            var recordId = "valid-record-id";
            var expectedLineage = CreateValidDataLineage();
            
            _truthSystem.GetDataLineageAsync(recordId, _cancellationToken)
                .Returns(Result<DataLineage>.Success(expectedLineage));

            // Act
            var result = await _truthSystem.GetDataLineageAsync(recordId, _cancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.RecordId.ShouldBe(recordId);
        }
    }

    public class GenerateGroundingReportAsyncTests : PrimarySourceOfTruthSystemTests
    {
        [Fact]
        public async Task Should_ReturnGroundingReport_When_ValidDateRangeProvided()
        {
            // Arrange
            var fromDate = DateTime.UtcNow.AddDays(-30);
            var toDate = DateTime.UtcNow;
            var expectedReport = CreateValidGroundingReport();
            
            _truthSystem.GenerateGroundingReportAsync(fromDate, toDate, _cancellationToken)
                .Returns(Result<GroundingReport>.Success(expectedReport));

            // Act
            var result = await _truthSystem.GenerateGroundingReportAsync(fromDate, toDate, _cancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.FromDate.ShouldBe(fromDate);
            result.Value.ToDate.ShouldBe(toDate);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_InvalidDateRangeProvided()
        {
            // Arrange
            var fromDate = DateTime.UtcNow;
            var toDate = DateTime.UtcNow.AddDays(-30); // Invalid: to date before from date
            
            _truthSystem.GenerateGroundingReportAsync(fromDate, toDate, _cancellationToken)
                .Returns(Result<GroundingReport>.WithFailure("Invalid date range"));

            // Act
            var result = await _truthSystem.GenerateGroundingReportAsync(fromDate, toDate, _cancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Invalid date range");
        }
    }

    public class FindSimilarRecordsAsyncTests : PrimarySourceOfTruthSystemTests
    {
        [Fact]
        public async Task Should_ReturnSimilarRecords_When_ValidDataProvided()
        {
            // Arrange
            var extractedData = CreateValidExtractedData();
            var similarityThreshold = 0.85f;
            var expectedRecords = CreateSimilarTruthRecords();
            
            _truthSystem.FindSimilarRecordsAsync(extractedData, similarityThreshold, _cancellationToken)
                .Returns(Result<IEnumerable<TruthRecord>>.Success(expectedRecords));

            // Act
            var result = await _truthSystem.FindSimilarRecordsAsync(extractedData, similarityThreshold, _cancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.Count().ShouldBeGreaterThan(0);
        }

        [Theory]
        [InlineData(-0.1f)]
        [InlineData(1.1f)]
        [InlineData(float.NaN)]
        public async Task Should_ReturnFailure_When_InvalidSimilarityThresholdProvided(float invalidThreshold)
        {
            // Arrange
            var extractedData = CreateValidExtractedData();
            
            _truthSystem.FindSimilarRecordsAsync(extractedData, invalidThreshold, _cancellationToken)
                .Returns(Result<IEnumerable<TruthRecord>>.WithFailure("Invalid similarity threshold"));

            // Act
            var result = await _truthSystem.FindSimilarRecordsAsync(extractedData, invalidThreshold, _cancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Invalid similarity threshold");
        }
    }

    public class UpdateTruthRecordAsyncTests : PrimarySourceOfTruthSystemTests
    {
        [Fact]
        public async Task Should_ReturnUpdatedRecord_When_ValidUpdateProvided()
        {
            // Arrange
            var recordId = "valid-record-id";
            var updatedData = CreateValidExtractedData();
            var updatedBy = "test-user";
            var expectedRecord = CreateValidTruthRecord();
            
            _truthSystem.UpdateTruthRecordAsync(recordId, updatedData, updatedBy, _cancellationToken)
                .Returns(Result<TruthRecord>.Success(expectedRecord));

            // Act
            var result = await _truthSystem.UpdateTruthRecordAsync(recordId, updatedData, updatedBy, _cancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.Id.ShouldBe(recordId);
        }
    }

    public class RequireHumanReviewAsyncTests : PrimarySourceOfTruthSystemTests
    {
        [Fact]
        public async Task Should_ReturnSuccess_When_ValidReviewRequestProvided()
        {
            // Arrange
            var recordId = "valid-record-id";
            var reason = "Data quality concerns";
            
            _truthSystem.RequireHumanReviewAsync(recordId, reason, _cancellationToken)
                .Returns(Result<bool>.Success(true));

            // Act
            var result = await _truthSystem.RequireHumanReviewAsync(recordId, reason, _cancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldBeTrue();
        }
    }

    public class GetRecordsRequiringReviewAsyncTests : PrimarySourceOfTruthSystemTests
    {
        [Fact]
        public async Task Should_ReturnRecordsRequiringReview_When_Called()
        {
            // Arrange
            var expectedRecords = CreateRecordsRequiringReview();
            
            _truthSystem.GetRecordsRequiringReviewAsync(_cancellationToken)
                .Returns(Result<IEnumerable<TruthRecord>>.Success(expectedRecords));

            // Act
            var result = await _truthSystem.GetRecordsRequiringReviewAsync(_cancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
        }
    }

    public class GetDataQualityMetricsAsyncTests : PrimarySourceOfTruthSystemTests
    {
        [Fact]
        public async Task Should_ReturnQualityMetrics_When_ValidDateRangeProvided()
        {
            // Arrange
            var fromDate = DateTime.UtcNow.AddDays(-30);
            var toDate = DateTime.UtcNow;
            var expectedMetrics = CreateValidDataQualityMetrics();
            
            _truthSystem.GetDataQualityMetricsAsync(fromDate, toDate, _cancellationToken)
                .Returns(Result<DataQualityMetrics>.Success(expectedMetrics));

            // Act
            var result = await _truthSystem.GetDataQualityMetricsAsync(fromDate, toDate, _cancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.FromDate.ShouldBe(fromDate);
            result.Value.ToDate.ShouldBe(toDate);
        }
    }

    // Test Data Factory Methods
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

    private static TruthRecord CreateValidTruthRecord()
    {
        return new TruthRecord
        {
            Id = "truth-record-123",
            Data = CreateValidExtractedData(),
            Source = CreateValidDataSource(),
            CreatedAt = DateTime.UtcNow,
            Version = 1,
            Status = "Active"
        };
    }

    private static DocumentProcessing.ValidationResult CreateValidValidationResult()
    {
        return new DocumentProcessing.ValidationResult
        {
            IsValid = true,
            Errors = Array.Empty<string>(),
            Warnings = Array.Empty<string>(),
            ValidatedAt = DateTime.UtcNow
        };
    }

    private static DocumentProcessing.ValidationResult CreateInvalidValidationResult()
    {
        return new ValidationResult
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
            ResolutionStrategy = "HighestConfidence",
            ResolvedAt = DateTime.UtcNow,
            ConflictingSources = CreateConflictingDataSet().ToList()
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

    private static GroundingReport CreateValidGroundingReport()
    {
        return new GroundingReport
        {
            FromDate = DateTime.UtcNow.AddDays(-30),
            ToDate = DateTime.UtcNow,
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

    private static DataQualityMetrics CreateValidDataQualityMetrics()
    {
        return new DataQualityMetrics
        {
            FromDate = DateTime.UtcNow.AddDays(-30),
            ToDate = DateTime.UtcNow,
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