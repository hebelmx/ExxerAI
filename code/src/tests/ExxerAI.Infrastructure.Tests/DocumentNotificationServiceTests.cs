using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.DocumentProcessing;
using ExxerAI.Domain.Operations;
using ExxerAI.Domain;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ExxerAI.Infrastructure.Tests;

/// <summary>
/// Comprehensive ITDD test bed for IDocumentNotificationService
/// Tests the interface contract for document event notifications
/// </summary>
public class DocumentNotificationServiceTests
{
    private readonly IDocumentNotificationService _notificationService;
    private readonly CancellationToken _cancellationToken;

    public DocumentNotificationServiceTests()
    {
        _notificationService = Substitute.For<IDocumentNotificationService>();
        _cancellationToken = TestContext.Current.CancellationToken;
    }

    public class NotifyDocumentAddedAsyncTests : DocumentNotificationServiceTests
    {
        [Fact]
        public async Task Should_ReturnSuccess_When_ValidDocumentProvided()
        {
            // Arrange
            var document = CreateValidDocumentAsset();

            _notificationService.NotifyDocumentAddedAsync(document, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<bool>.Success(true));

            // Act
            var result = await _notificationService.NotifyDocumentAddedAsync(document, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_ReturnFailure_When_NullDocumentProvided()
        {
            // Arrange
            DocumentAsset nullDocument = null!;

            _notificationService.NotifyDocumentAddedAsync(nullDocument, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Document cannot be null"));

            // Act
            var result = await _notificationService.NotifyDocumentAddedAsync(nullDocument, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Document cannot be null");
        }

        [Fact]
        public async Task Should_HandleCancellation_When_CancellationRequested()
        {
            // Arrange
            var document = CreateValidDocumentAsset();
            var cancellationToken = new CancellationToken(true);

            _notificationService.NotifyDocumentAddedAsync(document, cancellationToken)
                .Returns(Result<bool>.WithFailure("Operation was cancelled"));

            // Act
            var result = await _notificationService.NotifyDocumentAddedAsync(document, cancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Operation was cancelled");
        }
    }

    public class NotifyDocumentModifiedAsyncTests : DocumentNotificationServiceTests
    {
        [Fact]
        public async Task Should_ReturnSuccess_When_ValidDocumentAndVersionProvided()
        {
            // Arrange
            var document = CreateValidDocumentAsset();
            var previousVersion = "v1.0";

            _notificationService.NotifyDocumentModifiedAsync(document, previousVersion, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<bool>.Success(true));

            // Act
            var result = await _notificationService.NotifyDocumentModifiedAsync(document, previousVersion, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_ReturnFailure_When_NullDocumentProvided()
        {
            // Arrange
            DocumentAsset nullDocument = null!;
            var previousVersion = "v1.0";

            _notificationService.NotifyDocumentModifiedAsync(nullDocument, previousVersion, _cancellationToken)
                .Returns(Result<bool>.WithFailure("Document cannot be null"));

            // Act
            var result = await _notificationService.NotifyDocumentModifiedAsync(nullDocument, previousVersion, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Document cannot be null");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task Should_ReturnFailure_When_InvalidPreviousVersionProvided(string? invalidVersion)
        {
            // Arrange
            var document = CreateValidDocumentAsset();

            _notificationService.NotifyDocumentModifiedAsync(document, invalidVersion!, _cancellationToken)
                .Returns(Result<bool>.WithFailure("Previous version cannot be empty"));

            // Act
            var result = await _notificationService.NotifyDocumentModifiedAsync(document, invalidVersion!, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Previous version cannot be empty");
        }
    }

    public class NotifyDocumentRemovedAsyncTests : DocumentNotificationServiceTests
    {
        [Fact]
        public async Task Should_ReturnSuccess_When_ValidDocumentInfoProvided()
        {
            // Arrange
            var documentId = "doc-123";
            var documentName = "test-document.pdf";

            _notificationService.NotifyDocumentRemovedAsync(documentId, documentName, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<bool>.Success(true));

            // Act
            var result = await _notificationService.NotifyDocumentRemovedAsync(documentId, documentName, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldBeTrue();
        }

        [Theory]
        [InlineData("", "test-document.pdf")]
        [InlineData("   ", "test-document.pdf")]
        [InlineData(null!, "test-document.pdf")]
        public async Task Should_ReturnFailure_When_InvalidDocumentIdProvided(string? invalidId, string documentName)
        {
            // Arrange
            _notificationService.NotifyDocumentRemovedAsync(invalidId!, documentName, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Document ID cannot be empty"));

            // Act
            var result = await _notificationService.NotifyDocumentRemovedAsync(invalidId!, documentName, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Document ID cannot be empty");
        }

        [Theory]
        [InlineData("doc-123", "")]
        [InlineData("doc-123", "   ")]
        [InlineData("doc-123", null)]
        public async Task Should_ReturnFailure_When_InvalidDocumentNameProvided(string documentId, string? invalidName)
        {
            // Arrange
            _notificationService.NotifyDocumentRemovedAsync(documentId, invalidName!, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Document name cannot be empty"));

            // Act
            var result = await _notificationService.NotifyDocumentRemovedAsync(documentId, invalidName!, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Document name cannot be empty");
        }
    }

    public class NotifyProcessingFailedAsyncTests : DocumentNotificationServiceTests
    {
        [Fact]
        public async Task Should_ReturnSuccess_When_ValidDocumentAndErrorProvided()
        {
            // Arrange
            var document = CreateValidDocumentAsset();
            var error = new InvalidOperationException("Processing failed");

            _notificationService.NotifyProcessingFailedAsync(document, error, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<bool>.Success(true));

            // Act
            var result = await _notificationService.NotifyProcessingFailedAsync(document, error, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_ReturnFailure_When_NullDocumentProvided()
        {
            // Arrange
            DocumentAsset nullDocument = null!;
            var error = new InvalidOperationException("Processing failed");

            _notificationService.NotifyProcessingFailedAsync(nullDocument, error, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Document cannot be null"));

            // Act
            var result = await _notificationService.NotifyProcessingFailedAsync(nullDocument, error, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Document cannot be null");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_NullErrorProvided()
        {
            // Arrange
            var document = CreateValidDocumentAsset();
            Exception nullError = null!;

            _notificationService.NotifyProcessingFailedAsync(document, nullError, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Error cannot be null"));

            // Act
            var result = await _notificationService.NotifyProcessingFailedAsync(document, nullError, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Error cannot be null");
        }
    }

    public class NotifyProcessingCompletedAsyncTests : DocumentNotificationServiceTests
    {
        [Fact]
        public async Task Should_ReturnSuccess_When_ValidDocumentAndResultProvided()
        {
            // Arrange
            var document = CreateValidDocumentAsset();
            var processingResult = CreateValidDocumentProcessingResult();

            _notificationService.NotifyProcessingCompletedAsync(document, processingResult, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<bool>.Success(true));

            // Act
            var result = await _notificationService.NotifyProcessingCompletedAsync(document, processingResult, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_ReturnFailure_When_NullDocumentProvided()
        {
            // Arrange
            DocumentAsset nullDocument = null!;
            var processingResult = CreateValidDocumentProcessingResult();

            _notificationService.NotifyProcessingCompletedAsync(nullDocument, processingResult, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Document cannot be null"));

            // Act
            var result = await _notificationService.NotifyProcessingCompletedAsync(nullDocument, processingResult, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Document cannot be null");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_NullProcessingResultProvided()
        {
            // Arrange
            var document = CreateValidDocumentAsset();
            DocumentProcessingResult nullResult = null!;

            _notificationService.NotifyProcessingCompletedAsync(document, nullResult, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Processing result cannot be null"));

            // Act
            var result = await _notificationService.NotifyProcessingCompletedAsync(document, nullResult, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Processing result cannot be null");
        }
    }

    public class RegisterSubscriberAsyncTests : DocumentNotificationServiceTests
    {
        [Fact]
        public async Task Should_ReturnSuccess_When_ValidSubscriberProvided()
        {
            // Arrange
            var subscriberId = "subscriber-123";
            var callback = CreateValidNotificationCallback();

            _notificationService.RegisterSubscriberAsync(subscriberId, callback, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<bool>.Success(true));

            // Act
            var result = await _notificationService.RegisterSubscriberAsync(subscriberId, callback, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldBeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public async Task Should_ReturnFailure_When_InvalidSubscriberIdProvided(string? invalidId)
        {
            // Arrange
            var callback = CreateValidNotificationCallback();

            _notificationService.RegisterSubscriberAsync(invalidId!, callback, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Subscriber ID cannot be empty"));

            // Act
            var result = await _notificationService.RegisterSubscriberAsync(invalidId!, callback, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Subscriber ID cannot be empty");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_NullCallbackProvided()
        {
            // Arrange
            var subscriberId = "subscriber-123";
            Func<DocumentNotification, Task> nullCallback = null!;

            _notificationService.RegisterSubscriberAsync(subscriberId, nullCallback, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Callback cannot be null"));

            // Act
            var result = await _notificationService.RegisterSubscriberAsync(subscriberId, nullCallback, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Callback cannot be null");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_SubscriberAlreadyExists()
        {
            // Arrange
            var subscriberId = "existing-subscriber";
            var callback = CreateValidNotificationCallback();

            _notificationService.RegisterSubscriberAsync(subscriberId, callback, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Subscriber already exists"));

            // Act
            var result = await _notificationService.RegisterSubscriberAsync(subscriberId, callback, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Subscriber already exists");
        }
    }

    public class UnregisterSubscriberAsyncTests : DocumentNotificationServiceTests
    {
        [Fact]
        public async Task Should_ReturnSuccess_When_ValidSubscriberIdProvided()
        {
            // Arrange
            var subscriberId = "subscriber-123";

            _notificationService.UnregisterSubscriberAsync(subscriberId, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<bool>.Success(true));

            // Act
            var result = await _notificationService.UnregisterSubscriberAsync(subscriberId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value!.ShouldBeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public async Task Should_ReturnFailure_When_InvalidSubscriberIdProvided(string? invalidId)
        {
            // Arrange
            _notificationService.UnregisterSubscriberAsync(invalidId!, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Subscriber ID cannot be empty"));

            // Act
            var result = await _notificationService.UnregisterSubscriberAsync(invalidId!, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Subscriber ID cannot be empty");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_SubscriberNotFound()
        {
            // Arrange
            var nonExistentId = "non-existent-subscriber";

            _notificationService.UnregisterSubscriberAsync(nonExistentId, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Subscriber not found"));

            // Act
            var result = await _notificationService.UnregisterSubscriberAsync(nonExistentId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Subscriber not found");
        }
    }

    public class NotificationIntegrationTests : DocumentNotificationServiceTests
    {
        [Fact]
        public async Task Should_HandleMultipleNotifications_When_MultipleEventsOccur()
        {
            // Arrange
            var document = CreateValidDocumentAsset();
            var processingResult = CreateValidDocumentProcessingResult();

            _notificationService.NotifyDocumentAddedAsync(document, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<bool>.Success(true));
            _notificationService.NotifyProcessingCompletedAsync(document, processingResult, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<bool>.Success(true));

            // Act
            var addResult = await _notificationService.NotifyDocumentAddedAsync(document, cancellationToken: TestContext.Current.CancellationToken);
            var completeResult = await _notificationService.NotifyProcessingCompletedAsync(document, processingResult, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            addResult.IsSuccess.ShouldBeTrue();
            completeResult.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_HandleNotificationFailure_When_SubscriberThrowsException()
        {
            // Arrange
            var subscriberId = "faulty-subscriber";
            var faultyCallback = CreateFaultyNotificationCallback();

            _notificationService.RegisterSubscriberAsync(subscriberId, faultyCallback, cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Result<bool>.WithFailure("Subscriber callback failed"));

            // Act
            var result = await _notificationService.RegisterSubscriberAsync(subscriberId, faultyCallback, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Errors.ShouldContain("Subscriber callback failed");
        }
    }

    // Test Value Factory Methods
    private static DocumentAsset CreateValidDocumentAsset()
    {
        var content = System.Text.Encoding.UTF8.GetBytes("Test document content");
        var document = new DocumentAsset("test-document.pdf", content, "/documents/test-document.pdf")
        {
            MimeType = "application/pdf"
        };

        document.SetContentHash("abc123def456");
        document.AddMetadata("DocumentType", "Invoice");
        document.AddMetadata("Language", "en-US");
        document.MarkAsActive();

        return document;
    }

    private static DocumentProcessingResult CreateValidDocumentProcessingResult()
    {
        return new DocumentProcessingResult
        {
            DocumentId = "doc-123",
            ExtractedText = "Test extracted text",
            GroundedData = new ExtractedData
            {
                DocumentId = "doc-123",
                ExtractedFields = new Dictionary<string, object>
                {
                    { "InvoiceNumber", "INV-2024-001" },
                    { "Amount", 1250.50m }
                },
                ConfidenceScore = 0.95f,
                ProcessedAt = DateTime.UtcNow
            },
            Confidence = 0.95f,
            LLMConfidence = 0.95f,
            GroundingConfidence = 0.95f,
            ProcessingTimeMs = 5000,
            ErrorMessage = null
        };
    }

    private static Func<DocumentNotification, Task> CreateValidNotificationCallback()
    {
        return async notification =>
        {
            // Simulate processing notification
            await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken: TestContext.Current.CancellationToken);
            Console.WriteLine($"Notification received: {notification.Type}");
        };
    }

    private static Func<DocumentNotification, Task> CreateFaultyNotificationCallback()
    {
        return async notification =>
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken: TestContext.Current.CancellationToken);
            throw new InvalidOperationException("Callback failed");
        };
    }
}