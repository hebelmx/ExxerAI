using ExxerAI.Domain.DocumentProcessing;
using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Comprehensive unit tests for MCP types and classes
/// </summary>
public class MCPTypesTests
{
	public class MCPWatchOptionsTests
	{
		[Fact]
		public void Should_InitializeWithDefaults_When_Created()
		{
			// Act
			var options = new MCPWatchOptions();

			// Assert
			options.IncludeSubdirectories.ShouldBeTrue();
			options.AutoProcess.ShouldBeFalse();
			options.PollingIntervalSeconds.ShouldBe(30);
			options.FileTypes.ShouldNotBeNull();
			options.FileTypes.Count.ShouldBe(3);
			options.FileTypes.ShouldContain(".pdf");
			options.FileTypes.ShouldContain(".docx");
			options.FileTypes.ShouldContain(".xlsx");
			options.CustomParameters.ShouldNotBeNull();
			options.CustomParameters.ShouldBeEmpty();
		}

		[Fact]
		public void Should_AllowPropertyModification_When_ValuesSet()
		{
			// Arrange
			var options = new MCPWatchOptions();

			// Act
			options.IncludeSubdirectories = false;
			options.AutoProcess = true;
			options.PollingIntervalSeconds = 60;
			options.FileTypes.Add(".txt");
			options.CustomParameters["maxFiles"] = 100;

			// Assert
			options.IncludeSubdirectories.ShouldBeFalse();
			options.AutoProcess.ShouldBeTrue();
			options.PollingIntervalSeconds.ShouldBe(60);
			options.FileTypes.Count.ShouldBe(4);
			options.FileTypes.ShouldContain(".txt");
			options.CustomParameters["maxFiles"].ShouldBe(100);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(5)]
		[InlineData(120)]
		[InlineData(3600)]
		public void Should_AcceptValidPollingIntervals_When_Set(int intervalSeconds)
		{
			// Arrange
			var options = new MCPWatchOptions();

			// Act
			options.PollingIntervalSeconds = intervalSeconds;

			// Assert
			options.PollingIntervalSeconds.ShouldBe(intervalSeconds);
		}

		[Fact]
		public void Should_AllowFileTypeModification_When_Added()
		{
			// Arrange
			var options = new MCPWatchOptions();
			var initialCount = options.FileTypes.Count;

			// Act
			options.FileTypes.Add(".pptx");
			options.FileTypes.Add(".doc");

			// Assert
			options.FileTypes.Count.ShouldBe(initialCount + 2);
			options.FileTypes.ShouldContain(".pptx");
			options.FileTypes.ShouldContain(".doc");
		}

		[Fact]
		public void Should_AllowCustomParameters_When_Added()
		{
			// Arrange
			var options = new MCPWatchOptions();

			// Act
			options.CustomParameters["priority"] = "high";
			options.CustomParameters["retryCount"] = 3;
			options.CustomParameters["timeout"] = 30000;

			// Assert
			options.CustomParameters.Count.ShouldBe(3);
			options.CustomParameters["priority"].ShouldBe("high");
			options.CustomParameters["retryCount"].ShouldBe(3);
			options.CustomParameters["timeout"].ShouldBe(30000);
		}
	}

	public class MCPResponseTests
	{
		[Fact]
		public void Should_InitializeWithDefaults_When_Created()
		{
			// Act
			var response = new MCPResponse();

			// Assert
			response.Id.ShouldBe(string.Empty);
			response.IsSuccess.ShouldBeFalse();
			response.Message.ShouldBe(string.Empty);
			response.Timestamp.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
			response.Data.ShouldNotBeNull();
			response.Data.ShouldBeEmpty();
		}

		[Fact]
		public void Should_AllowPropertyAssignment_When_ValuesSet()
		{
			// Arrange
			var response = new MCPResponse();
			var testTime = DateTime.UtcNow.AddHours(-1);

			// Act
			response.Id = "test-response-123";
			response.IsSuccess = true;
			response.Message = "Operation completed successfully";
			response.Timestamp = testTime;
			response.Data["result"] = "success";

			// Assert
			response.Id.ShouldBe("test-response-123");
			response.IsSuccess.ShouldBeTrue();
			response.Message.ShouldBe("Operation completed successfully");
			response.Timestamp.ShouldBe(testTime);
			response.Data["result"].ShouldBe("success");
		}

		[Theory]
		[InlineData(true, "Success")]
		[InlineData(false, "Failure")]
		public void Should_HandleSuccessAndFailure_When_StatusSet(bool isSuccess, string message)
		{
			// Arrange
			var response = new MCPResponse();

			// Act
			response.IsSuccess = isSuccess;
			response.Message = message;

			// Assert
			response.IsSuccess.ShouldBe(isSuccess);
			response.Message.ShouldBe(message);
		}

		[Fact]
		public void Should_AllowDataManipulation_When_Modified()
		{
			// Arrange
			var response = new MCPResponse();

			// Act
			response.Data["files"] = new List<string> { "file1.pdf", "file2.docx" };
			response.Data["count"] = 2;
			response.Data["metadata"] = new { processed = true };

			// Assert
			response.Data.Count.ShouldBe(3);
			response.Data["files"].ShouldNotBeNull();
			response.Data["count"].ShouldBe(2);
			response.Data["metadata"].ShouldNotBeNull();
		}
	}

	public class MCPDocumentMetadataTests
	{
		[Fact]
		public void Should_InitializeWithDefaults_When_Created()
		{
			// Act
			var metadata = new MCPDocumentMetadata();

			// Assert
			metadata.Id.ShouldBe(string.Empty);
			metadata.Name.ShouldBe(string.Empty);
			metadata.MimeType.ShouldBe(string.Empty);
			metadata.Size.ShouldBe(0);
			metadata.CreatedTime.ShouldBe(default(DateTime));
			metadata.ModifiedTime.ShouldBe(default(DateTime));
			metadata.DriveFilePath.ShouldBe(string.Empty);
			metadata.Properties.ShouldNotBeNull();
			metadata.Properties.ShouldBeEmpty();
		}

		[Fact]
		public void Should_AllowFullMetadataAssignment_When_ValuesSet()
		{
			// Arrange
			var metadata = new MCPDocumentMetadata();
			var createTime = DateTime.UtcNow.AddDays(-7);
			var modifyTime = DateTime.UtcNow.AddDays(-1);

			// Act
			metadata.Id = "1A2B3C4D5E6F";
			metadata.Name = "test-document.pdf";
			metadata.MimeType = "application/pdf";
			metadata.Size = 1024768;
			metadata.CreatedTime = createTime;
			metadata.ModifiedTime = modifyTime;
			metadata.DriveFilePath = "/My Drive/Documents/test-document.pdf";
			metadata.Properties["owner"] = "test@example.com";

			// Assert
			metadata.Id.ShouldBe("1A2B3C4D5E6F");
			metadata.Name.ShouldBe("test-document.pdf");
			metadata.MimeType.ShouldBe("application/pdf");
			metadata.Size.ShouldBe(1024768);
			metadata.CreatedTime.ShouldBe(createTime);
			metadata.ModifiedTime.ShouldBe(modifyTime);
			metadata.DriveFilePath.ShouldBe("/My Drive/Documents/test-document.pdf");
			metadata.Properties["owner"].ShouldBe("test@example.com");
		}

		[Theory]
		[InlineData("application/pdf", ".pdf")]
		[InlineData("application/vnd.openxmlformats-officedocument.wordprocessingml.document", ".docx")]
		[InlineData("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx")]
		[InlineData("text/plain", ".txt")]
		public void Should_HandleDifferentMimeTypes_When_Set(string mimeType, string expectedExtension)
		{
			// Arrange
			var metadata = new MCPDocumentMetadata();

			// Act
			metadata.MimeType = mimeType;
			metadata.Name = $"test{expectedExtension}";

			// Assert
			metadata.MimeType.ShouldBe(mimeType);
			metadata.Name.ShouldEndWith(expectedExtension);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1024)]
		[InlineData(1048576)] // 1MB
		[InlineData(10485760)] // 10MB
		public void Should_HandleDifferentFileSizes_When_Set(long size)
		{
			// Arrange
			var metadata = new MCPDocumentMetadata();

			// Act
			metadata.Size = size;

			// Assert
			metadata.Size.ShouldBe(size);
		}

		[Fact]
		public void Should_AllowPropertiesManipulation_When_Modified()
		{
			// Arrange
			var metadata = new MCPDocumentMetadata();

			// Act
			metadata.Properties["version"] = "1.0";
			metadata.Properties["tags"] = new List<string> { "important", "contract" };
			metadata.Properties["isShared"] = true;

			// Assert
			metadata.Properties.Count.ShouldBe(3);
			metadata.Properties["version"].ShouldBe("1.0");
			metadata.Properties["tags"].ShouldNotBeNull();
			metadata.Properties["isShared"].ShouldBe(true);
		}
	}

	public class MCPUploadResultTests
	{
		[Fact]
		public void Should_InitializeWithDefaults_When_Created()
		{
			// Act
			var result = new MCPUploadResult();

			// Assert
			result.FileId.ShouldBe(string.Empty);
			result.UploadUrl.ShouldBe(string.Empty);
			result.IsSuccess.ShouldBeFalse();
			result.UploadedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
			result.Metadata.ShouldNotBeNull();
			result.Metadata.ShouldBeEmpty();
		}

		[Fact]
		public void Should_AllowFullResultAssignment_When_ValuesSet()
		{
			// Arrange
			var result = new MCPUploadResult();
			var uploadTime = DateTime.UtcNow.AddMinutes(-5);

			// Act
			result.FileId = "upload-123-456";
			result.UploadUrl = "https://drive.google.com/file/d/upload-123-456";
			result.IsSuccess = true;
			result.UploadedAt = uploadTime;
			result.Metadata["size"] = 2048576;

			// Assert
			result.FileId.ShouldBe("upload-123-456");
			result.UploadUrl.ShouldBe("https://drive.google.com/file/d/upload-123-456");
			result.IsSuccess.ShouldBeTrue();
			result.UploadedAt.ShouldBe(uploadTime);
			result.Metadata["size"].ShouldBe(2048576);
		}

		[Theory]
		[InlineData(true, "Upload successful")]
		[InlineData(false, "Upload failed")]
		public void Should_HandleSuccessAndFailure_When_StatusSet(bool isSuccess, string description)
		{
			// Arrange
			var result = new MCPUploadResult();

			// Act
			result.IsSuccess = isSuccess;
			result.Metadata["description"] = description;

			// Assert
			result.IsSuccess.ShouldBe(isSuccess);
			result.Metadata["description"].ShouldBe(description);
		}

		[Fact]
		public void Should_AllowMetadataManipulation_When_Modified()
		{
			// Arrange
			var result = new MCPUploadResult();

			// Act
			result.Metadata["uploadSpeed"] = "1.5 MB/s";
			result.Metadata["retryCount"] = 0;
			result.Metadata["compression"] = true;

			// Assert
			result.Metadata.Count.ShouldBe(3);
			result.Metadata["uploadSpeed"].ShouldBe("1.5 MB/s");
			result.Metadata["retryCount"].ShouldBe(0);
			result.Metadata["compression"].ShouldBe(true);
		}
	}

	public class MCPHealthStatusTests
	{
		[Fact]
		public void Should_InitializeWithDefaults_When_Created()
		{
			// Act
			var status = new MCPHealthStatus();

			// Assert
			status.IsHealthy.ShouldBeFalse();
			status.Version.ShouldBe(string.Empty);
			status.LastChecked.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
			status.ResponseTimeMs.ShouldBe(0);
			status.ErrorMessage.ShouldBeNull();
			status.Metrics.ShouldNotBeNull();
			status.Metrics.ShouldBeEmpty();
		}

		[Fact]
		public void Should_AllowFullStatusAssignment_When_ValuesSet()
		{
			// Arrange
			var status = new MCPHealthStatus();
			var checkTime = DateTime.UtcNow.AddMinutes(-2);

			// Act
			status.IsHealthy = true;
			status.Version = "1.2.3";
			status.LastChecked = checkTime;
			status.ResponseTimeMs = 150;
			status.ErrorMessage = null;
			status.Metrics["cpuUsage"] = 25.5;

			// Assert
			status.IsHealthy.ShouldBeTrue();
			status.Version.ShouldBe("1.2.3");
			status.LastChecked.ShouldBe(checkTime);
			status.ResponseTimeMs.ShouldBe(150);
			status.ErrorMessage.ShouldBeNull();
			status.Metrics["cpuUsage"].ShouldBe(25.5);
		}

		[Theory]
		[InlineData(true, 50, null)]
		[InlineData(false, 5000, "Connection timeout")]
		[InlineData(false, 0, "Server unreachable")]
		public void Should_HandleHealthyAndUnhealthyStates_When_StatusSet(bool isHealthy, long responseTime, string? errorMessage)
		{
			// Arrange
			var status = new MCPHealthStatus();

			// Act
			status.IsHealthy = isHealthy;
			status.ResponseTimeMs = responseTime;
			status.ErrorMessage = errorMessage;

			// Assert
			status.IsHealthy.ShouldBe(isHealthy);
			status.ResponseTimeMs.ShouldBe(responseTime);
			status.ErrorMessage.ShouldBe(errorMessage);
		}

		[Fact]
		public void Should_AllowMetricsManipulation_When_Modified()
		{
			// Arrange
			var status = new MCPHealthStatus();

			// Act
			status.Metrics["memoryUsage"] = 512;
			status.Metrics["diskSpace"] = "95% free";
			status.Metrics["activeConnections"] = 42;

			// Assert
			status.Metrics.Count.ShouldBe(3);
			status.Metrics["memoryUsage"].ShouldBe(512);
			status.Metrics["diskSpace"].ShouldBe("95% free");
			status.Metrics["activeConnections"].ShouldBe(42);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(50)]
		[InlineData(1000)]
		[InlineData(5000)]
		public void Should_HandleDifferentResponseTimes_When_Set(long responseTimeMs)
		{
			// Arrange
			var status = new MCPHealthStatus();

			// Act
			status.ResponseTimeMs = responseTimeMs;

			// Assert
			status.ResponseTimeMs.ShouldBe(responseTimeMs);
		}
	}

	public class DocumentChangeTests
	{
		[Fact]
		public void Should_InitializeWithDefaults_When_Created()
		{
			// Act
			var change = new DocumentChange();

			// Assert
			change.ChangeId.ShouldBe(string.Empty);
			change.DocumentId.ShouldBe(string.Empty);
			change.ChangeType.ShouldBe(DocumentChangeType.Created);
			change.ChangedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
			change.DocumentMetadata.ShouldBeNull();
		}

		[Fact]
		public void Should_AllowFullChangeAssignment_When_ValuesSet()
		{
			// Arrange
			var change = new DocumentChange();
			var changeTime = DateTime.UtcNow.AddMinutes(-10);
			var metadata = new MCPDocumentMetadata { Id = "doc-123", Name = "test.pdf" };

			// Act
			change.ChangeId = "change-789";
			change.DocumentId = "doc-123";
			change.ChangeType = DocumentChangeType.Modified;
			change.ChangedAt = changeTime;
			change.DocumentMetadata = metadata;

			// Assert
			change.ChangeId.ShouldBe("change-789");
			change.DocumentId.ShouldBe("doc-123");
			change.ChangeType.ShouldBe(DocumentChangeType.Modified);
			change.ChangedAt.ShouldBe(changeTime);
			change.DocumentMetadata.ShouldNotBeNull();
			change.DocumentMetadata.Id.ShouldBe("doc-123");
		}

		[Theory]
		[InlineData(nameof(DocumentChangeType.Created))]
		[InlineData(nameof(DocumentChangeType.Modified))]
		[InlineData(nameof(DocumentChangeType.Deleted))]
		[InlineData(nameof(DocumentChangeType.Moved))]
		[InlineData(nameof(DocumentChangeType.Renamed))]
		[InlineData(nameof(DocumentChangeType.Restored))]
		public void Should_HandleAllChangeTypes_When_Set(string changeTypeName)
		{
			// Arrange
			var change = new DocumentChange();
			var changeType = Enum.Parse<DocumentChangeType>(changeTypeName);

			// Act
			change.ChangeType = changeType;

			// Assert
			change.ChangeType.ShouldBe(changeType);
		}

		[Fact]
		public void Should_AllowMetadataAssignment_When_Set()
		{
			// Arrange
			var change = new DocumentChange();
			var metadata = new MCPDocumentMetadata
			{
				Id = "metadata-test",
				Name = "document.docx",
				Size = 2048
			};

			// Act
			change.DocumentMetadata = metadata;

			// Assert
			change.DocumentMetadata.ShouldNotBeNull();
			change.DocumentMetadata.Id.ShouldBe("metadata-test");
			change.DocumentMetadata.Name.ShouldBe("document.docx");
			change.DocumentMetadata.Size.ShouldBe(2048);
		}
	}

	public class ProcessedDocumentTests
	{
		[Fact]
		public void Should_InitializeWithDefaults_When_Created()
		{
			// Act
			var document = new ProcessedDocument();

			// Assert
			document.OriginalDocumentId.ShouldBe(string.Empty);
			document.Content.ShouldBeEquivalentTo(Array.Empty<byte>());
			document.FileName.ShouldBe(string.Empty);
			document.MimeType.ShouldBe(string.Empty);
			document.ProcessingResults.ShouldBeNull();
			document.Metadata.ShouldNotBeNull();
			document.Metadata.ShouldBeEmpty();
		}

		[Fact]
		public void Should_AllowFullDocumentAssignment_When_ValuesSet()
		{
			// Arrange
			var document = new ProcessedDocument();
			var content = new byte[] { 0x50, 0x44, 0x46, 0x2D }; // PDF header
			var processingResult = new DocumentProcessingResult { DocumentId = "test-doc" };

			// Act
			document.OriginalDocumentId = "original-123";
			document.Content = content;
			document.FileName = "processed-document.pdf";
			document.MimeType = "application/pdf";
			document.ProcessingResults = processingResult;
			document.Metadata["processor"] = "ExxerAI";

			// Assert
			document.OriginalDocumentId.ShouldBe("original-123");
			document.Content.ShouldBeEquivalentTo(content);
			document.FileName.ShouldBe("processed-document.pdf");
			document.MimeType.ShouldBe("application/pdf");
			document.ProcessingResults.ShouldNotBeNull();
			document.ProcessingResults.DocumentId.ShouldBe("test-doc");
			document.Metadata["processor"].ShouldBe("ExxerAI");
		}

		[Theory]
		[InlineData("document.pdf", "application/pdf")]
		[InlineData("document.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
		[InlineData("document.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
		[InlineData("document.txt", "text/plain")]
		public void Should_HandleDifferentFileTypes_When_Set(string fileName, string mimeType)
		{
			// Arrange
			var document = new ProcessedDocument();

			// Act
			document.FileName = fileName;
			document.MimeType = mimeType;

			// Assert
			document.FileName.ShouldBe(fileName);
			document.MimeType.ShouldBe(mimeType);
		}

		[Fact]
		public void Should_AllowContentManipulation_When_Modified()
		{
			// Arrange
			var document = new ProcessedDocument();
			var smallContent = new byte[] { 0x01, 0x02, 0x03 };
			var largeContent = new byte[1024];

			// Act & Assert - Small content
			document.Content = smallContent;
			document.Content.Length.ShouldBe(3);
			document.Content.ShouldBeEquivalentTo(smallContent);

			// Act & Assert - Large content
			document.Content = largeContent;
			document.Content.Length.ShouldBe(1024);
		}

		[Fact]
		public void Should_AllowMetadataManipulation_When_Modified()
		{
			// Arrange
			var document = new ProcessedDocument();

			// Act
			document.Metadata["extractedText"] = "Sample document content";
			document.Metadata["pages"] = 5;
			document.Metadata["confidence"] = 0.95;

			// Assert
			document.Metadata.Count.ShouldBe(3);
			document.Metadata["extractedText"].ShouldBe("Sample document content");
			document.Metadata["pages"].ShouldBe(5);
			document.Metadata["confidence"].ShouldBe(0.95);
		}
	}

	public class MCPWatchSessionTests
	{
		[Fact]
		public void Should_InitializeWithDefaults_When_Created()
		{
			// Act
			var session = new MCPWatchSession();

			// Assert
			session.WatchId.ShouldBe(string.Empty);
			session.FolderId.ShouldBe(string.Empty);
			session.StartedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
			session.Options.ShouldNotBeNull();
			session.IsActive.ShouldBeTrue();
		}

		[Fact]
		public void Should_AllowFullSessionAssignment_When_ValuesSet()
		{
			// Arrange
			var session = new MCPWatchSession();
			var startTime = DateTime.UtcNow.AddHours(-1);
			var options = new MCPWatchOptions { PollingIntervalSeconds = 60 };

			// Act
			session.WatchId = "watch-session-456";
			session.FolderId = "folder-789";
			session.StartedAt = startTime;
			session.Options = options;
			session.IsActive = false;

			// Assert
			session.WatchId.ShouldBe("watch-session-456");
			session.FolderId.ShouldBe("folder-789");
			session.StartedAt.ShouldBe(startTime);
			session.Options.ShouldNotBeNull();
			session.Options.PollingIntervalSeconds.ShouldBe(60);
			session.IsActive.ShouldBeFalse();
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void Should_HandleActiveState_When_Set(bool isActive)
		{
			// Arrange
			var session = new MCPWatchSession();

			// Act
			session.IsActive = isActive;

			// Assert
			session.IsActive.ShouldBe(isActive);
		}

		[Fact]
		public void Should_AllowOptionsModification_When_Changed()
		{
			// Arrange
			var session = new MCPWatchSession();

			// Act
			session.Options.PollingIntervalSeconds = 120;
			session.Options.AutoProcess = true;
			session.Options.FileTypes.Add(".pptx");

			// Assert
			session.Options.PollingIntervalSeconds.ShouldBe(120);
			session.Options.AutoProcess.ShouldBeTrue();
			session.Options.FileTypes.ShouldContain(".pptx");
		}
	}

	public class MCPDocumentRequestTests
	{
		[Fact]
		public void Should_InitializeWithDefaults_When_Created()
		{
			// Act
			var request = new MCPDocumentRequest();

			// Assert
			request.DocumentId.ShouldBe(string.Empty);
			request.ProcessingOptions.ShouldNotBeNull();
			request.SessionId.ShouldBe(string.Empty);
			request.Parameters.ShouldNotBeNull();
			request.Parameters.ShouldBeEmpty();
		}

		[Fact]
		public void Should_AllowFullRequestAssignment_When_ValuesSet()
		{
			// Arrange
			var request = new MCPDocumentRequest();
			var processingOptions = new ProcessingOptions();

			// Act
			request.DocumentId = "document-999";
			request.ProcessingOptions = processingOptions;
			request.SessionId = "session-888";

			// Assert
			request.DocumentId.ShouldBe("document-999");
			request.ProcessingOptions.ShouldNotBeNull();
			request.SessionId.ShouldBe("session-888");
		}

		[Fact]
		public void Should_AllowParametersManipulation_When_Modified()
		{
			// Arrange
			var request = new MCPDocumentRequest();

			// Act
			// Note: Parameters has init-only setter, so we can only add to it
			request.Parameters.Add("priority", "high");
			request.Parameters.Add("timeout", 30000);
			request.Parameters.Add("retryOnFailure", true);

			// Assert
			request.Parameters.Count.ShouldBe(3);
			request.Parameters["priority"].ShouldBe("high");
			request.Parameters["timeout"].ShouldBe(30000);
			request.Parameters["retryOnFailure"].ShouldBe(true);
		}

		[Fact]
		public void Should_MaintainParametersReference_When_Accessed()
		{
			// Arrange
			var request = new MCPDocumentRequest();

			// Act
			var parametersRef = request.Parameters;
			parametersRef["test"] = "value";

			// Assert
			request.Parameters["test"].ShouldBe("value");
			request.Parameters.ShouldBeSameAs(parametersRef);
		}
	}

	public class DocumentChangeTypeTests
	{
		[Fact]
		public void Should_HaveAllExpectedValues_When_Enumerated()
		{
			// Act
			var values = Enum.GetValues<DocumentChangeType>();

			// Assert
			values.Length.ShouldBe(6);
			values.ShouldContain(DocumentChangeType.Created);
			values.ShouldContain(DocumentChangeType.Modified);
			values.ShouldContain(DocumentChangeType.Deleted);
			values.ShouldContain(DocumentChangeType.Moved);
			values.ShouldContain(DocumentChangeType.Renamed);
			values.ShouldContain(DocumentChangeType.Restored);
		}

		[Theory]
		[InlineData(DocumentChangeType.Created, 0)]
		[InlineData(DocumentChangeType.Modified, 1)]
		[InlineData(DocumentChangeType.Deleted, 2)]
		[InlineData(DocumentChangeType.Moved, 3)]
		[InlineData(DocumentChangeType.Renamed, 4)]
		[InlineData(DocumentChangeType.Restored, 5)]
		public void Should_HaveCorrectIntegerValues_When_CastToInt(DocumentChangeType changeType, int expectedValue)
		{
			// Act
			var intValue = (int)changeType;

			// Assert
			intValue.ShouldBe(expectedValue);
		}

		[Theory]
		[InlineData(nameof(DocumentChangeType.Created))]
		[InlineData(nameof(DocumentChangeType.Modified))]
		[InlineData(nameof(DocumentChangeType.Deleted))]
		[InlineData(nameof(DocumentChangeType.Moved))]
		[InlineData(nameof(DocumentChangeType.Renamed))]
		[InlineData(nameof(DocumentChangeType.Restored))]
		public void Should_ParseFromString_When_ValidName(string changeTypeName)
		{
			// Act
			var parsed = Enum.Parse<DocumentChangeType>(changeTypeName);
			var parseSuccess = Enum.TryParse<DocumentChangeType>(changeTypeName, out var tryParsed);

			// Assert
			parseSuccess.ShouldBeTrue();
			parsed.ShouldBe(tryParsed);
			parsed.ToString().ShouldBe(changeTypeName);
		}

		[Fact]
		public void Should_HandleDefaultValue_When_NotSet()
		{
			// Act
			var defaultChange = new DocumentChange();

			// Assert
			defaultChange.ChangeType.ShouldBe(DocumentChangeType.Created);
		}
	}
} 