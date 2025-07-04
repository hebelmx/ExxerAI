using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Unit tests for ProcessingHistory entity
/// </summary>
public class ProcessingHistoryEntityTests
{
	// Test helper methods and test classes would go here
	// This is a placeholder for the focused ProcessingHistory tests
	[Fact]
	public void Should_CreateDefaultInstance_When_UsingParameterlessConstructor()
	{
		// Act
		var history = new ProcessingHistory();

		// Assert
		history.Id.ShouldNotBeNullOrEmpty();
		history.DocumentType.ShouldBe(DocumentType.Unknown);
		history.ProcessingResults.ShouldNotBeNull();
		history.ProcessingResults.ShouldBeEmpty();
	}
}
