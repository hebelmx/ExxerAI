using System;
using System.Threading;
using System.Threading.Tasks;
using ExxerAI.Application.Services;
using ExxerAI.Application.Interfaces;
using ExxerAI.Domain;
using ExxerAI.Domain.DocumentProcessing;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ExxerAI.Application.Tests.Services;

public class EnhancedDocumentIntelligenceAgentTests
{
    private readonly IPolymorphicDocumentProcessor _documentProcessor = Substitute.For<IPolymorphicDocumentProcessor>();
    private readonly IPrimarySourceOfTruthSystem _truthSystem = Substitute.For<IPrimarySourceOfTruthSystem>();
    private readonly IMCPGoogleDriveService _mcpDriveService = Substitute.For<IMCPGoogleDriveService>();
    private readonly ILLMService _llmService = Substitute.For<ILLMService>();
    private readonly ILogger<EnhancedDocumentIntelligenceAgent> _logger = Substitute.For<ILogger<EnhancedDocumentIntelligenceAgent>>();
    private readonly EnhancedDocumentIntelligenceAgent _sut;

    public EnhancedDocumentIntelligenceAgentTests()
    {
        _sut = new EnhancedDocumentIntelligenceAgent(_documentProcessor, _truthSystem, _mcpDriveService, _llmService, _logger);
    }

    [Fact]
    public async Task ProcessBusinessDocumentAsync_Should_ReturnCancelled_When_CancellationRequested()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        // Act
        var result = await _sut.ProcessBusinessDocumentAsync("docId", new ProcessingOptions(), cts.Token);
        // Assert
        result.IsCancelled().ShouldBeTrue();
    }

    [Fact]
    public async Task StartBusinessDocumentMonitoringAsync_Should_ReturnCancelled_When_CancellationRequested()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        // Act
        var result = await _sut.StartBusinessDocumentMonitoringAsync("folderId", new MCPWatchOptions(), cts.Token);
        // Assert
        result.IsCancelled().ShouldBeTrue();
    }

    [Fact]
    public async Task ProcessBusinessDocumentAsync_Should_ReturnFailure_When_DownloadFails()
    {
        // Arrange
        _mcpDriveService.DownloadDocumentAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<byte[]>.WithFailure("Download error"));
        // Act
        var result = await _sut.ProcessBusinessDocumentAsync("docId", new ProcessingOptions());
        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Download error");
    }

    [Fact]
    public async Task ProcessBusinessDocumentAsync_Should_ReturnSuccess_When_AllStepsSucceed()
    {
        // Arrange
        var docBytes = new byte[] { 1, 2, 3 };
        var docResult = Result<byte[]>.Success(docBytes);
        var metaResult = Result<MCPDocumentMetadata>.Success(new MCPDocumentMetadata());
        var procResult = Result<DocumentProcessingResult>.Success(new DocumentProcessingResult());
        _mcpDriveService.DownloadDocumentAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(docResult);
        _mcpDriveService.GetDocumentMetadataAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(metaResult);
        _documentProcessor.ProcessDocumentAsync(Arg.Any<byte[]>(), Arg.Any<DocumentMetadata>(), Arg.Any<CancellationToken>()).Returns(procResult);
        // Act
        var result = await _sut.ProcessBusinessDocumentAsync("docId", new ProcessingOptions());
        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
    }
} 