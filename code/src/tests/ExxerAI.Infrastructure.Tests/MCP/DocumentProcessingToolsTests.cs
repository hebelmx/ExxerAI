using ExxerAi.MCPServer.Application.Tools;
using ExxerAI.Domain.Operations;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ExxerAI.Infrastructure.Tests.MCP;

/// <summary>
/// Infrastructure tests for DocumentProcessingTools MCP implementation.
/// Tests the MCP protocol tools layer for document processing operations.
/// Validates tool registration, parameter handling, and basic functionality.
/// </summary>
public class DocumentProcessingToolsTests
{
    private readonly ILogger<DocumentProcessingTools> _logger;
    private readonly DocumentProcessingTools _tools;

    public DocumentProcessingToolsTests()
    {
        _logger = Substitute.For<ILogger<DocumentProcessingTools>>();
        _tools = new DocumentProcessingTools(_logger);
    }

/// <summary>
/// Begin Tests Constructor Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Act & Assert
        _tools.ShouldNotBeNull();
        _tools.ShouldBeOfType<DocumentProcessingTools>();
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() => 
            new DocumentProcessingTools(null!))
            .ParamName.ShouldBe("logger");
    }

/// <summary>
/// End Tests Constructor Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Process Document Tool Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task ProcessDocumentAsync_WithValidPath_ShouldReturnSuccessResult()
    {
        // Arrange
        const string documentPath = "/test/sample.pdf";
        const string documentType = "invoice";

        // Act
        var result = await _tools.ProcessDocumentAsync(documentPath, documentType);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNullOrEmpty();
        result.Value.ShouldContain("Document Processing Complete");
        result.Value.ShouldContain(documentType);
    }

    [Fact]
    public async Task ProcessDocumentAsync_WithDefaultParameters_ShouldUseDefaults()
    {
        // Arrange
        const string documentPath = "/test/default.pdf";

        // Act
        var result = await _tools.ProcessDocumentAsync(documentPath);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldContain("Type: auto");
        result.Value.ShouldContain("Level: detailed");
    }

    [Fact]
    public async Task ProcessDocumentAsync_WithNullPath_ShouldHandleGracefully()
    {
        // Act
        var result = await _tools.ProcessDocumentAsync(null!);

        // Assert
        result.ShouldNotBeNull();
        // The actual behavior depends on the implementation
        // This test ensures it doesn't throw unhandled exceptions
    }

/// <summary>
/// End Tests Process Document Tool Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Extract Text Tool Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task ExtractTextAsync_WithValidDocument_ShouldReturnText()
    {
        // Arrange
        const string documentPath = "/test/text-extract.pdf";

        // Act
        var result = await _tools.ExtractTextAsync(documentPath);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNullOrEmpty();
        result.Value.ShouldContain("Text Extraction Complete");
    }

/// <summary>
/// End Tests Extract Text Tool Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Validate Extraction Tool Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task ValidateExtractionAsync_WithValidData_ShouldReturnValidation()
    {
        // Arrange
        const string extractionData = "Sample extracted data";
        const string originalDocument = "/test/validation.pdf";

        // Act
        var result = await _tools.ValidateExtractionAsync(extractionData, originalDocument);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNullOrEmpty();
        result.Value.ShouldContain("Validation Complete");
    }

/// <summary>
/// End Tests Validate Extraction Tool Tests
/// </summary>
/// <returns></returns>

/// <summary>
/// Begin Tests Performance Tests
/// </summary>
/// <returns></returns>

    [Fact]
    public async Task ProcessDocumentAsync_WithMultipleRequests_ShouldHandleConcurrency()
    {
        // Arrange
        const int concurrentRequests = 5;
        var tasks = new List<Task<Result<string>>>();

        // Act
        for (int i = 0; i < concurrentRequests; i++)
        {
            tasks.Add(_tools.ProcessDocumentAsync($"/test/concurrent-{i}.pdf", "report"));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        results.ShouldAllBe(result => result is not null);
        results.ShouldAllBe(result => result.IsSuccess);
        results.Length.ShouldBe(concurrentRequests);
    }

/// <summary>
/// End Tests Performance Tests
/// </summary>
/// <returns></returns>
}