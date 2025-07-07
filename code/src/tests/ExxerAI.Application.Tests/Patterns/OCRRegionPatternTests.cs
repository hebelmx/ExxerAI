using ExxerAI.Application.DTOs;
using ExxerAI.Application.Enums;
using ExxerAI.Application.Patterns;
using ExxerAI.Domain.DocumentProcessing;
using Shouldly;

namespace ExxerAI.Application.Tests.Patterns;

/// <summary>
/// Unit tests for OCRRegionPattern class
/// </summary>
public class OCRRegionPatternTests
{
    /// <summary>
    /// Tests that OCRRegionPattern can be instantiated with default values
    /// </summary>
    [Fact]
    public void Should_InstantiateWithDefaultValues_When_Created()
    {
        // Act
        var pattern = new OCRRegionPattern();

        // Assert
        pattern.ShouldNotBeNull();
        pattern.ReferenceText.ShouldBe(string.Empty);
        pattern.SearchStrategy.ShouldBe(SearchStrategy.NextToken);
        pattern.RegionBounds.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests that OCRRegionPattern properties can be set correctly
    /// </summary>
    [Fact]
    public void Should_SetPropertiesCorrectly_When_Assigned()
    {
        // Arrange
        const string expectedReferenceText = "Invoice Number:";
        const SearchStrategy expectedStrategy = SearchStrategy.NextLineInRegion;
        var expectedBounds = new RegionBounds();

        // Act
        var pattern = new OCRRegionPattern
        {
            ReferenceText = expectedReferenceText,
            SearchStrategy = expectedStrategy,
            RegionBounds = expectedBounds
        };

        // Assert
        pattern.ReferenceText.ShouldBe(expectedReferenceText);
        pattern.SearchStrategy.ShouldBe(expectedStrategy);
        pattern.RegionBounds.ShouldBe(expectedBounds);
    }

    /// <summary>
    /// Tests that OCRRegionPattern inherits from ExtractionPattern correctly
    /// </summary>
    [Fact]
    public void Should_InheritFromExtractionPattern_When_Created()
    {
        // Act
        var pattern = new OCRRegionPattern();

        // Assert
        pattern.ShouldBeAssignableTo<ExtractionPattern>();
    }

    /// <summary>
    /// Tests NextToken strategy extraction
    /// </summary>
    [Fact]
    public void Should_ExtractNextToken_When_UsingNextTokenStrategy()
    {
        // Arrange
        var pattern = new OCRRegionPattern
        {
            ReferenceText = "Invoice Number:",
            SearchStrategy = SearchStrategy.NextToken
        };
        var context = new ExtractionContext();
        const string text = "Invoice Number: INV-2024-001\nOther data";

        // Act
        var result = pattern.ExtractValue(text, context);

        // Assert
        result.ShouldBe("INV-2024-001");
    }

    /// <summary>
    /// Tests NextLineInRegion strategy extraction
    /// </summary>
    [Fact]
    public void Should_ExtractFromNextLine_When_UsingNextLineInRegionStrategy()
    {
        // Arrange
        var pattern = new OCRRegionPattern
        {
            ReferenceText = "Total Amount:",
            SearchStrategy = SearchStrategy.NextLineInRegion
        };
        var context = new ExtractionContext();
        const string text = "Total Amount:\n1,250.50\nOther data";

        // Act
        var result = pattern.ExtractValue(text, context);

        // Assert
        result.ShouldBe("1,250.50");
    }

    /// <summary>
    /// Tests SameLineOrNext strategy extraction when found on same line
    /// </summary>
    [Fact]
    public void Should_ExtractFromSameLine_When_UsingSameLineOrNextStrategyAndFoundOnSameLine()
    {
        // Arrange
        var pattern = new OCRRegionPattern
        {
            ReferenceText = "Amount:",
            SearchStrategy = SearchStrategy.SameLineOrNext
        };
        var context = new ExtractionContext();
        const string text = "Amount: 500.75\nOther data";

        // Act
        var result = pattern.ExtractValue(text, context);

        // Assert
        result.ShouldBe("500.75");
    }

    /// <summary>
    /// Tests SameLineOrNext strategy extraction when not found on same line but found on next line
    /// </summary>
    [Fact]
    public void Should_ExtractFromNextLine_When_UsingSameLineOrNextStrategyAndNotFoundOnSameLine()
    {
        // Arrange
        var pattern = new OCRRegionPattern
        {
            ReferenceText = "Amount:",
            SearchStrategy = SearchStrategy.SameLineOrNext
        };
        var context = new ExtractionContext();
        const string text = "Amount:\n750.25\nOther data";

        // Act
        var result = pattern.ExtractValue(text, context);

        // Assert
        result.ShouldBe("750.25");
    }

    /// <summary>
    /// Tests that extraction returns null when reference text is not found
    /// </summary>
    [Fact]
    public void Should_ReturnNull_When_ReferenceTextNotFound()
    {
        // Arrange
        var pattern = new OCRRegionPattern
        {
            ReferenceText = "Not Found:",
            SearchStrategy = SearchStrategy.NextToken
        };
        var context = new ExtractionContext();
        const string text = "Invoice Number: INV-2024-001\nTotal: 500.00";

        // Act
        var result = pattern.ExtractValue(text, context);

        // Assert
        result.ShouldBeNull();
    }

    /// <summary>
    /// Tests case-insensitive reference text matching
    /// </summary>
    [Theory]
    [InlineData("INVOICE NUMBER:", "Invoice Number: INV-2024-001")]
    [InlineData("invoice number:", "Invoice Number: INV-2024-001")]
    [InlineData("Invoice Number:", "INVOICE NUMBER: INV-2024-001")]
    public void Should_MatchCaseInsensitive_When_ReferenceTextHasDifferentCase(string referenceText, string inputText)
    {
        // Arrange
        var pattern = new OCRRegionPattern
        {
            ReferenceText = referenceText,
            SearchStrategy = SearchStrategy.NextToken
        };
        var context = new ExtractionContext();

        // Act
        var result = pattern.ExtractValue(inputText, context);

        // Assert
        result.ShouldBe("INV-2024-001");
    }

    /// <summary>
    /// Tests extraction with multiple lines containing reference text
    /// </summary>
    [Fact]
    public void Should_ExtractFromFirstMatch_When_MultipleReferencesExist()
    {
        // Arrange
        var pattern = new OCRRegionPattern
        {
            ReferenceText = "Amount:",
            SearchStrategy = SearchStrategy.NextToken
        };
        var context = new ExtractionContext();
        const string text = "Amount: 100.00\nSubtotal Amount: 200.00\nTotal Amount: 300.00";

        // Act
        var result = pattern.ExtractValue(text, context);

        // Assert
        result.ShouldBe("100.00");
    }

    /// <summary>
    /// Tests NextToken strategy when no token follows reference text
    /// </summary>
    [Fact]
    public void Should_ReturnNull_When_NextTokenStrategyAndNoTokenFollows()
    {
        // Arrange
        var pattern = new OCRRegionPattern
        {
            ReferenceText = "Amount:",
            SearchStrategy = SearchStrategy.NextToken
        };
        var context = new ExtractionContext();
        const string text = "Amount:";

        // Act
        var result = pattern.ExtractValue(text, context);

        // Assert
        result.ShouldBeNull();
    }

    /// <summary>
    /// Tests NextLineInRegion strategy when no next line exists
    /// </summary>
    [Fact]
    public void Should_ReturnNull_When_NextLineInRegionStrategyAndNoNextLine()
    {
        // Arrange
        var pattern = new OCRRegionPattern
        {
            ReferenceText = "Amount:",
            SearchStrategy = SearchStrategy.NextLineInRegion
        };
        var context = new ExtractionContext();
        const string text = "Amount:";

        // Act
        var result = pattern.ExtractValue(text, context);

        // Assert
        result.ShouldBeNull();
    }

    /// <summary>
    /// Tests extraction with various numeric formats
    /// </summary>
    [Theory]
    [InlineData("Amount: 1,250.50", "1,250.50")]
    [InlineData("Amount: 500", "500")]
    [InlineData("Amount: 1250.75", "1250.75")]
    [InlineData("Amount: 1,000,000.99", "1,000,000.99")]
    [InlineData("Amount: 0.50", "0.50")]
    public void Should_ExtractNumericValues_When_UsingVariousFormats(string inputText, string expectedValue)
    {
        // Arrange
        var pattern = new OCRRegionPattern
        {
            ReferenceText = "Amount:",
            SearchStrategy = SearchStrategy.NextToken
        };
        var context = new ExtractionContext();

        // Act
        var result = pattern.ExtractValue(inputText, context);

        // Assert
        result.ShouldBe(expectedValue);
    }

    /// <summary>
    /// Tests extraction with empty or null input
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ReturnNull_When_InputIsNullOrEmpty(string? inputText)
    {
        // Arrange
        var pattern = new OCRRegionPattern
        {
            ReferenceText = "Amount:",
            SearchStrategy = SearchStrategy.NextToken
        };
        var context = new ExtractionContext();

        // Act
        var result = pattern.ExtractValue(inputText!, context);

        // Assert
        result.ShouldBeNull();
    }

    /// <summary>
    /// Tests extraction with null context
    /// </summary>
    [Fact]
    public void Should_HandleNullContext_When_ExtractValueCalled()
    {
        // Arrange
        var pattern = new OCRRegionPattern
        {
            ReferenceText = "Amount:",
            SearchStrategy = SearchStrategy.NextToken
        };
        const string text = "Amount: 500.00";

        // Act & Assert
        Should.NotThrow(() => pattern.ExtractValue(text, null!));
    }

    /// <summary>
    /// Tests extraction when line contains no numeric values
    /// </summary>
    [Fact]
    public void Should_ReturnNull_When_LineContainsNoNumericValues()
    {
        // Arrange
        var pattern = new OCRRegionPattern
        {
            ReferenceText = "Description:",
            SearchStrategy = SearchStrategy.NextLineInRegion
        };
        var context = new ExtractionContext();
        const string text = "Description:\nThis is a text description with no numbers";

        // Act
        var result = pattern.ExtractValue(text, context);

        // Assert
        result.ShouldBeNull();
    }

    /// <summary>
    /// Tests extraction with complex text formatting
    /// </summary>
    [Fact]
    public void Should_ExtractCorrectValue_When_TextHasComplexFormatting()
    {
        // Arrange
        var pattern = new OCRRegionPattern
        {
            ReferenceText = "Total:",
            SearchStrategy = SearchStrategy.NextLineInRegion
        };
        var context = new ExtractionContext();
        const string text = "Subtotal: 1,000.00\nTax: 100.00\nTotal:\n1,100.00 USD\nThank you";

        // Act
        var result = pattern.ExtractValue(text, context);

        // Assert
        result.ShouldBe("1,100.00");
    }
}