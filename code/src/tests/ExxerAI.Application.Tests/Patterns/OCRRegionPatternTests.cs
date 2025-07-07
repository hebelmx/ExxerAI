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

    /// <summary>
    /// Tests word boundary matching to prevent false positives
    /// </summary>
    [Theory]
    [InlineData("Subtotal: 1,000.00\nTotal: 500.00", "Total:", "500.00")]
    [InlineData("Grand Total Amount: 750.00", "Total:", null)]
    [InlineData("Total Amount: 1,250.50", "Total:", null)]
    [InlineData("Total: 999.99", "Total:", "999.99")]
    public void Should_RespectWordBoundaries_When_MatchingReferenceText(string inputText, string referenceText, string? expectedValue)
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
        result.ShouldBe(expectedValue);
    }

    /// <summary>
    /// Tests extraction with various currency symbols and formats
    /// </summary>
    [Theory]
    [InlineData("Price: $1,500.00", "1,500.00")]
    [InlineData("Cost: €2,750.50", "2,750.50")]
    [InlineData("Amount: £999.99", "999.99")]
    [InlineData("Total: ¥10,000", "10,000")]
    [InlineData("Value: 1,234.56 USD", "1,234.56")]
    [InlineData("Sum: 5,678.90 EUR", "5,678.90")]
    public void Should_ExtractCurrencyValues_When_CurrencySymbolsPresent(string inputText, string expectedValue)
    {
        // Arrange
        var pattern = new OCRRegionPattern
        {
            ReferenceText = inputText.Split(':')[0] + ":",
            SearchStrategy = SearchStrategy.NextToken
        };
        var context = new ExtractionContext();

        // Act
        var result = pattern.ExtractValue(inputText, context);

        // Assert
        result.ShouldBe(expectedValue);
    }

    /// <summary>
    /// Tests extraction with irregular spacing and formatting
    /// </summary>
    [Theory]
    [InlineData("Amount:   1,500.00", "1,500.00")]
    [InlineData("Total:\t\t2,750.50", "2,750.50")]
    [InlineData("Price:          999.99", "999.99")]
    [InlineData("Cost:1,234.56", "1,234.56")]
    public void Should_HandleIrregularSpacing_When_ExtractingValues(string inputText, string expectedValue)
    {
        // Arrange
        var pattern = new OCRRegionPattern
        {
            ReferenceText = inputText.Split(':')[0] + ":",
            SearchStrategy = SearchStrategy.NextToken
        };
        var context = new ExtractionContext();

        // Act
        var result = pattern.ExtractValue(inputText, context);

        // Assert
        result.ShouldBe(expectedValue);
    }

    /// <summary>
    /// Tests extraction with very large numbers
    /// </summary>
    [Theory]
    [InlineData("Budget: 1,000,000.00", "1,000,000.00")]
    [InlineData("Revenue: 25,500,750.99", "25,500,750.99")]
    [InlineData("Assets: 100,000,000", "100,000,000")]
    public void Should_ExtractLargeNumbers_When_Present(string inputText, string expectedValue)
    {
        // Arrange
        var pattern = new OCRRegionPattern
        {
            ReferenceText = inputText.Split(':')[0] + ":",
            SearchStrategy = SearchStrategy.NextToken
        };
        var context = new ExtractionContext();

        // Act
        var result = pattern.ExtractValue(inputText, context);

        // Assert
        result.ShouldBe(expectedValue);
    }

    /// <summary>
    /// Tests extraction with very small decimal numbers
    /// </summary>
    [Theory]
    [InlineData("Fee: 0.01", "0.01")]
    [InlineData("Tax: 0.99", "0.99")]
    [InlineData("Interest: 0.05", "0.05")]
    public void Should_ExtractSmallDecimals_When_Present(string inputText, string expectedValue)
    {
        // Arrange
        var pattern = new OCRRegionPattern
        {
            ReferenceText = inputText.Split(':')[0] + ":",
            SearchStrategy = SearchStrategy.NextToken
        };
        var context = new ExtractionContext();

        // Act
        var result = pattern.ExtractValue(inputText, context);

        // Assert
        result.ShouldBe(expectedValue);
    }

    /// <summary>
    /// Tests extraction with negative numbers
    /// </summary>
    [Theory]
    [InlineData("Balance: -500.00", "500.00")]
    [InlineData("Loss: -1,250.75", "1,250.75")]
    [InlineData("Deficit: -10,000", "10,000")]
    public void Should_ExtractAbsoluteValue_When_NegativeNumbersPresent(string inputText, string expectedValue)
    {
        // Arrange
        var pattern = new OCRRegionPattern
        {
            ReferenceText = inputText.Split(':')[0] + ":",
            SearchStrategy = SearchStrategy.NextToken
        };
        var context = new ExtractionContext();

        // Act
        var result = pattern.ExtractValue(inputText, context);

        // Assert
        result.ShouldBe(expectedValue);
    }

    /// <summary>
    /// Tests extraction with percentage values
    /// </summary>
    [Theory]
    [InlineData("Rate: 5.25%", "5.25")]
    [InlineData("Interest: 10%", "10")]
    [InlineData("Discount: 15.5%", "15.5")]
    public void Should_ExtractPercentageValues_When_Present(string inputText, string expectedValue)
    {
        // Arrange
        var pattern = new OCRRegionPattern
        {
            ReferenceText = inputText.Split(':')[0] + ":",
            SearchStrategy = SearchStrategy.NextToken
        };
        var context = new ExtractionContext();

        // Act
        var result = pattern.ExtractValue(inputText, context);

        // Assert
        result.ShouldBe(expectedValue);
    }

    /// <summary>
    /// Tests extraction with multiple numbers on the same line
    /// </summary>
    [Fact]
    public void Should_ExtractFirstNumber_When_MultipleNumbersOnSameLine()
    {
        // Arrange
        var pattern = new OCRRegionPattern
        {
            ReferenceText = "Values:",
            SearchStrategy = SearchStrategy.NextToken
        };
        var context = new ExtractionContext();
        const string text = "Values: 100.00 200.00 300.00";

        // Act
        var result = pattern.ExtractValue(text, context);

        // Assert
        result.ShouldBe("100.00");
    }

    /// <summary>
    /// Tests extraction with mixed alphanumeric content
    /// </summary>
    [Theory]
    [InlineData("Invoice: ABC123 Amount: 1,500.00", "Amount:", "1,500.00")]
    [InlineData("Order: XYZ789 Total: 2,750.50", "Total:", "2,750.50")]
    [InlineData("Reference: DEF456 Cost: 999.99", "Cost:", "999.99")]
    public void Should_ExtractNumbers_When_MixedAlphanumericContent(string inputText, string referenceText, string expectedValue)
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
        result.ShouldBe(expectedValue);
    }

    /// <summary>
    /// Tests extraction with special characters and punctuation
    /// </summary>
    [Theory]
    [InlineData("Total (incl. tax): 1,500.00", "Total (incl. tax):", "1,500.00")]
    [InlineData("Amount [USD]: 2,750.50", "Amount [USD]:", "2,750.50")]
    [InlineData("Cost - Final: 999.99", "Cost - Final:", "999.99")]
    public void Should_ExtractValues_When_SpecialCharactersInReference(string inputText, string referenceText, string expectedValue)
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
        result.ShouldBe(expectedValue);
    }

    /// <summary>
    /// Tests performance with very long text input
    /// </summary>
    [Fact]
    public void Should_PerformEfficiently_When_ProcessingLongText()
    {
        // Arrange
        var pattern = new OCRRegionPattern
        {
            ReferenceText = "Target:",
            SearchStrategy = SearchStrategy.NextToken
        };
        var context = new ExtractionContext();
        
        // Create a long text with the target at the end
        var longText = string.Join("\n", Enumerable.Range(1, 1000).Select(i => $"Line {i}: Some content here"))
                      + "\nTarget: 12,345.67\nMore content";

        // Act
        var result = pattern.ExtractValue(longText, context);

        // Assert
        result.ShouldBe("12,345.67");
    }

    /// <summary>
    /// Tests extraction with Unicode and international characters
    /// </summary>
    [Theory]
    [InlineData("Montant: 1,500.00", "Montant:", "1,500.00")]
    [InlineData("Betrag: 2,750.50", "Betrag:", "2,750.50")]
    [InlineData("Cantidad: 999.99", "Cantidad:", "999.99")]
    public void Should_ExtractValues_When_InternationalCharacters(string inputText, string referenceText, string expectedValue)
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
        result.ShouldBe(expectedValue);
    }
}