namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Unit tests for FieldDefinition domain entity
/// </summary>
public class FieldDefinitionTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_FieldDefinitionCreated()
    {
        // Act
        var field = new FieldDefinition();

        // Assert
        field.Name.ShouldBe(string.Empty);
        field.Type.ShouldBe(FieldType.Text);
        field.IsRequired.ShouldBeFalse();
        field.PrimaryPattern.ShouldBe(string.Empty);
        field.AlternativePatterns.ShouldNotBeNull();
        field.AlternativePatterns.ShouldBeEmpty();
        field.ValidationRules.ShouldNotBeNull();
        field.ValidationRules.ShouldBeEmpty();
        field.Description.ShouldBe(string.Empty);
        field.ExampleValues.ShouldNotBeNull();
        field.ExampleValues.ShouldBeEmpty();
        field.ConfidenceThreshold.ShouldBe(0.8f);
    }

    [Theory]
    [InlineData("PaymentPeriod", nameof(FieldType.Date_MMYYYY), true)]
    [InlineData("Amount", nameof(FieldType.Currency), true)]
    [InlineData("EmployerNumber", nameof(FieldType.AlphaNumeric), false)]
    [InlineData("Notes", nameof(FieldType.Text), false)]
    public void Should_CreateFieldDefinitionWithConstructor_When_ParametersProvided(
        string name, string fieldTypeName, bool isRequired)
    {
        // Arrange
        var fieldType = Enum.Parse<FieldType>(fieldTypeName);
        var primaryPattern = @"[A-Z0-9\-]+";

        // Act
        var field = new FieldDefinition(name, fieldType, isRequired, primaryPattern);

        // Assert
        field.Name.ShouldBe(name);
        field.Type.ShouldBe(fieldType);
        field.IsRequired.ShouldBe(isRequired);
        field.PrimaryPattern.ShouldBe(primaryPattern);
    }

    [Fact]
    public void Should_SetFieldProperties_When_FieldDefinitionInitializedWithValues()
    {
        // Act
        var field = new FieldDefinition
        {
            Name = "PaymentAmount",
            Type = FieldType.Currency,
            IsRequired = true,
            PrimaryPattern = @"(?:TOTAL|IMPORTE)[:\s]*\$?([0-9,]+\.?\d*)",
            Description = "Payment amount in Mexican Pesos",
            ConfidenceThreshold = 0.95f
        };
        
        field.ExampleValues.Add("$15,000.00");
        field.ExampleValues.Add("$25,500.50");
        field.ExampleValues.Add("$8,750.25");

        // Assert
        field.Name.ShouldBe("PaymentAmount");
        field.Type.ShouldBe(FieldType.Currency);
        field.IsRequired.ShouldBeTrue();
        field.PrimaryPattern.ShouldBe(@"(?:TOTAL|IMPORTE)[:\s]*\$?([0-9,]+\.?\d*)");
        field.Description.ShouldBe("Payment amount in Mexican Pesos");
        field.ConfidenceThreshold.ShouldBe(0.95f);
        field.ExampleValues.Count.ShouldBe(3);
        field.ExampleValues.ShouldContain("$15,000.00");
        field.ExampleValues.ShouldContain("$25,500.50");
        field.ExampleValues.ShouldContain("$8,750.25");
    }

    [Fact]
    public void Should_HandleAlternativePatterns_When_AlternativePatternsAdded()
    {
        // Arrange
        var field = new FieldDefinition();
        var pattern1 = new RegexPattern(@"PERIODO[:\s]*(\d{2}-\d{4})", 0.9f);
        var pattern2 = new RegexPattern(@"PERIOD[:\s]*(\d{2}-\d{4})", 0.85f);

        // Act
        field.AlternativePatterns.Add(pattern1);
        field.AlternativePatterns.Add(pattern2);

        // Assert
        field.AlternativePatterns.Count.ShouldBe(2);
        field.AlternativePatterns.ShouldContain(pattern1);
        field.AlternativePatterns.ShouldContain(pattern2);
        field.AlternativePatterns[0].Confidence.ShouldBe(0.9f);
        field.AlternativePatterns[1].Confidence.ShouldBe(0.85f);
    }

    [Fact]
    public void Should_HandleValidationRules_When_ValidationRulesAdded()
    {
        // Arrange
        var field = new FieldDefinition();
        var rule1 = new ValidationRule
        {
            Name = "Required Check",
            Type = ValidationType.Required,
            ErrorMessage = "Field is required"
        };
        var rule2 = new ValidationRule
        {
            Name = "Format Check",
            Type = ValidationType.RegexPattern,
            Pattern = @"\d{2}-\d{4}",
            ErrorMessage = "Invalid date format"
        };

        // Act
        field.ValidationRules.Add(rule1);
        field.ValidationRules.Add(rule2);

        // Assert
        field.ValidationRules.Count.ShouldBe(2);
        field.ValidationRules.ShouldContain(rule1);
        field.ValidationRules.ShouldContain(rule2);
        field.ValidationRules[0].Type.ShouldBe(ValidationType.Required);
        field.ValidationRules[1].Type.ShouldBe(ValidationType.RegexPattern);
        field.ValidationRules[1].Pattern.ShouldBe(@"\d{2}-\d{4}");
    }
} 