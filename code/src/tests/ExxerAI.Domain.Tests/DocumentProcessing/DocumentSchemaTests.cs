using ExxerAI.Domain.DocumentProcessing;
using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Comprehensive unit tests for DocumentMetadata domain entity
/// Tests document classification, processing options, and metadata handling
/// </summary>
public class DocumentMetadataTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_DocumentMetadataCreated()
    {
        // Act
        var metadata = new DocumentMetadata();

        // Assert
        metadata.DocumentId.ShouldBe(string.Empty);
        metadata.FileName.ShouldBe(string.Empty);
        metadata.DocumentType.ShouldBe(DocumentType.Unknown);
        metadata.ExpectedSchema.ShouldBeNull();
        metadata.SourcePath.ShouldBe(string.Empty);
        metadata.ProcessingOptions.ShouldNotBeNull();
        metadata.CreatedDate.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        metadata.ModifiedDate.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        metadata.FileSize.ShouldBe(0);
        metadata.Properties.ShouldNotBeNull();
        metadata.Properties.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("01. SUA IMSS ENE2023.pdf", nameof(DocumentType.IMSSPayment))]
    [InlineData("factura_12345.pdf", nameof(DocumentType.Invoice))]
    [InlineData("invoice_2023.docx", nameof(DocumentType.Invoice))]
    [InlineData("tax_document_2023.pdf", nameof(DocumentType.TaxDocument))]
    [InlineData("random_file.txt", nameof(DocumentType.Unknown))]
    public void Should_SetDocumentProperties_When_DocumentMetadataInitializedWithValues(string fileName, string documentTypeName)
    {
        // Arrange
        var documentType = Enum.Parse<DocumentType>(documentTypeName);
        var createdDate = DateTime.UtcNow.AddDays(-30);
        var modifiedDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var metadata = new DocumentMetadata
        {
            DocumentId = "doc-123",
            FileName = fileName,
            DocumentType = documentType,
            SourcePath = $"/documents/{fileName}",
            CreatedDate = createdDate,
            ModifiedDate = modifiedDate,
            FileSize = 2048
        };

        // Assert
        metadata.DocumentId.ShouldBe("doc-123");
        metadata.FileName.ShouldBe(fileName);
        metadata.DocumentType.ShouldBe(documentType);
        metadata.SourcePath.ShouldBe($"/documents/{fileName}");
        metadata.CreatedDate.ShouldBe(createdDate);
        metadata.ModifiedDate.ShouldBe(modifiedDate);
        metadata.FileSize.ShouldBe(2048);
    }

    [Fact]
    public void Should_AllowPropertiesManipulation_When_DocumentMetadataCreated()
    {
        // Arrange
        var metadata = new DocumentMetadata();

        // Act
        metadata.Properties["Author"] = "John Doe";
        metadata.Properties["Department"] = "Finance";
        metadata.Properties["Classification"] = "Confidential";
        metadata.Properties["ProcessingPriority"] = 1;

        // Assert
        metadata.Properties.Count.ShouldBe(4);
        metadata.Properties["Author"].ShouldBe("John Doe");
        metadata.Properties["Department"].ShouldBe("Finance");
        metadata.Properties["Classification"].ShouldBe("Confidential");
        metadata.Properties["ProcessingPriority"].ShouldBe(1);
    }
}

/// <summary>
/// Unit tests for ProcessingOptions domain entity
/// </summary>
public class ProcessingOptionsTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_ProcessingOptionsCreated()
    {
        // Act
        var options = new ProcessingOptions();

        // Assert
        options.UseOCRFallback.ShouldBeTrue();
        options.UseLLMExtraction.ShouldBeTrue();
        options.EnableSchemaLearning.ShouldBeTrue();
        options.MinimumConfidenceThreshold.ShouldBe(0.7f);
        options.OCRLanguages.ShouldNotBeNull();
        options.OCRLanguages.Count.ShouldBe(2);
        options.OCRLanguages.ShouldContain("spa");
        options.OCRLanguages.ShouldContain("eng");
        options.CustomParameters.ShouldNotBeNull();
        options.CustomParameters.ShouldBeEmpty();
    }

    [Fact]
    public void Should_SetProcessingOptions_When_ProcessingOptionsInitializedWithValues()
    {
        // Act
        var options = new ProcessingOptions
        {
            UseOCRFallback = false,
            UseLLMExtraction = false,
            EnableSchemaLearning = false,
            MinimumConfidenceThreshold = 0.9f
        };
        
        options.OCRLanguages.Clear();
        options.OCRLanguages.Add("fra");
        options.OCRLanguages.Add("deu");
        
        options.CustomParameters["MaxProcessingTime"] = 5000;
        options.CustomParameters["EnableDebugMode"] = true;

        // Assert
        options.UseOCRFallback.ShouldBeFalse();
        options.UseLLMExtraction.ShouldBeFalse();
        options.EnableSchemaLearning.ShouldBeFalse();
        options.MinimumConfidenceThreshold.ShouldBe(0.9f);
        options.OCRLanguages.Count.ShouldBe(2);
        options.OCRLanguages.ShouldContain("fra");
        options.OCRLanguages.ShouldContain("deu");
        options.CustomParameters.Count.ShouldBe(2);
        options.CustomParameters["MaxProcessingTime"].ShouldBe(5000);
        options.CustomParameters["EnableDebugMode"].ShouldBe(true);
    }
}

/// <summary>
/// Unit tests for SchemaDefinition domain entity
/// </summary>
public class SchemaDefinitionTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_SchemaDefinitionCreated()
    {
        // Act
        var schema = new SchemaDefinition();

        // Assert
        schema.Id.ShouldNotBeNullOrEmpty();
        schema.Name.ShouldBe(string.Empty);
        schema.DocumentType.ShouldBe(DocumentType.Unknown);
        schema.Version.ShouldBe(1);
        schema.Fields.ShouldNotBeNull();
        schema.Fields.ShouldBeEmpty();
        schema.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        schema.UpdatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        schema.AccuracyScore.ShouldBe(1.0f);
        schema.IsActive.ShouldBeTrue();
        schema.Metadata.ShouldNotBeNull();
        schema.Metadata.ShouldBeEmpty();
    }

    [Fact]
    public void Should_SetSchemaProperties_When_SchemaDefinitionInitializedWithValues()
    {
        // Arrange
        var createdDate = DateTime.UtcNow.AddDays(-10);
        var updatedDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var schema = new SchemaDefinition
        {
            Name = "IMSS_Payment_Schema_V2",
            DocumentType = DocumentType.IMSSPayment,
            Version = 2,
            CreatedAt = createdDate,
            UpdatedAt = updatedDate,
            AccuracyScore = 0.92f,
            IsActive = true
        };

        // Assert
        schema.Name.ShouldBe("IMSS_Payment_Schema_V2");
        schema.DocumentType.ShouldBe(DocumentType.IMSSPayment);
        schema.Version.ShouldBe(2);
        schema.CreatedAt.ShouldBe(createdDate);
        schema.UpdatedAt.ShouldBe(updatedDate);
        schema.AccuracyScore.ShouldBe(0.92f);
        schema.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Should_HandleFieldDefinitions_When_FieldsAdded()
    {
        // Arrange
        var schema = new SchemaDefinition();
        
        var field1 = new FieldDefinition("PaymentPeriod", FieldType.Date_MMYYYY, true, @"(?:PERIODO|PERIOD)[:\s]*(\d{2}-\d{4})");
        var field2 = new FieldDefinition("Amount", FieldType.Currency, true, @"(?:IMPORTE|TOTAL)[:\s]*\$?([0-9,]+\.?\d*)");
        var field3 = new FieldDefinition("EmployerNumber", FieldType.AlphaNumeric, false, @"(?:REGISTRO PATRONAL)[:\s]*([A-Z0-9\-]+)");

        // Act
        schema.Fields.Add(field1);
        schema.Fields.Add(field2);
        schema.Fields.Add(field3);

        // Assert
        schema.Fields.Count.ShouldBe(3);
        schema.RequiredFields.Count().ShouldBe(2); // field1 and field2
        schema.OptionalFields.Count().ShouldBe(1); // field3
        
        schema.RequiredFields.ShouldContain(field1);
        schema.RequiredFields.ShouldContain(field2);
        schema.OptionalFields.ShouldContain(field3);
    }

    [Fact]
    public void Should_FilterRequiredAndOptionalFields_When_FieldsWithDifferentRequirementAdded()
    {
        // Arrange
        var schema = new SchemaDefinition();
        var requiredField1 = new FieldDefinition("RequiredField1", FieldType.Text, true, "pattern1");
        var requiredField2 = new FieldDefinition("RequiredField2", FieldType.Integer, true, "pattern2");
        var optionalField1 = new FieldDefinition("OptionalField1", FieldType.Decimal, false, "pattern3");
        var optionalField2 = new FieldDefinition("OptionalField2", FieldType.Date, false, "pattern4");

        // Act
        schema.Fields.Add(requiredField1);
        schema.Fields.Add(optionalField1);
        schema.Fields.Add(requiredField2);
        schema.Fields.Add(optionalField2);

        // Assert
        schema.Fields.Count.ShouldBe(4);
        schema.RequiredFields.Count().ShouldBe(2);
        schema.OptionalFields.Count().ShouldBe(2);
        
        schema.RequiredFields.ShouldContain(requiredField1);
        schema.RequiredFields.ShouldContain(requiredField2);
        schema.OptionalFields.ShouldContain(optionalField1);
        schema.OptionalFields.ShouldContain(optionalField2);
    }
}

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

/// <summary>
/// Unit tests for ValidationRule domain entity
/// </summary>
public class ValidationRuleTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_ValidationRuleCreated()
    {
        // Act
        var rule = new ValidationRule();

        // Assert
        rule.Id.ShouldNotBeNullOrEmpty();
        rule.Name.ShouldBe(string.Empty);
        rule.Type.ShouldBe(ValidationType.Required);
        rule.Pattern.ShouldBe(string.Empty);
        rule.ErrorMessage.ShouldBe(string.Empty);
        rule.IsActive.ShouldBeTrue();
    }

    [Theory]
    [InlineData(nameof(ValidationType.Required), "", "Field is required")]
    [InlineData(nameof(ValidationType.RegexPattern), @"\d{2}-\d{4}", "Invalid date format")]
    [InlineData(nameof(ValidationType.NumericRange), "1-100", "Value must be between 1 and 100")]
    [InlineData(nameof(ValidationType.DateFormat), "dd/MM/yyyy", "Invalid date format")]
    [InlineData(nameof(ValidationType.LengthLimit), "5-50", "Length must be between 5 and 50 characters")]
    [InlineData(nameof(ValidationType.Custom), "custom_validation", "Custom validation failed")]
    public void Should_CreateValidationRules_When_ValidationRuleInitializedWithDifferentTypes(
        string validationTypeName, string pattern, string errorMessage)
    {
        // Arrange
        var validationType = Enum.Parse<ValidationType>(validationTypeName);

        // Act
        var rule = new ValidationRule
        {
            Name = $"{validationTypeName} Rule",
            Type = validationType,
            Pattern = pattern,
            ErrorMessage = errorMessage,
            IsActive = true
        };

        // Assert
        rule.Name.ShouldBe($"{validationTypeName} Rule");
        rule.Type.ShouldBe(validationType);
        rule.Pattern.ShouldBe(pattern);
        rule.ErrorMessage.ShouldBe(errorMessage);
        rule.IsActive.ShouldBeTrue();
    }
}

/// <summary>
/// Unit tests for RegexPattern extraction pattern
/// </summary>
public class RegexPatternTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_RegexPatternCreated()
    {
        // Act
        var pattern = new RegexPattern();

        // Assert
        pattern.Id.ShouldNotBeNullOrEmpty();
        pattern.Name.ShouldBe(string.Empty);
        pattern.Confidence.ShouldBe(1.0f);
        pattern.IsActive.ShouldBeTrue();
        pattern.Metadata.ShouldNotBeNull();
        pattern.Metadata.ShouldBeEmpty();
        pattern.Pattern.ShouldBe(string.Empty);
        pattern.Options.ShouldBe(System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }

    [Fact]
    public void Should_CreateRegexPatternWithConstructor_When_ParametersProvided()
    {
        // Arrange
        var regexPattern = @"(?:PERIODO|PERIOD)[:\s]*(\d{2}-\d{4})";
        var confidence = 0.9f;

        // Act
        var pattern = new RegexPattern(regexPattern, confidence);

        // Assert
        pattern.Pattern.ShouldBe(regexPattern);
        pattern.Confidence.ShouldBe(confidence);
    }

    [Theory]
    [InlineData("PERIODO: 12-2023", @"(?:PERIODO|PERIOD)[:\s]*(\d{2}-\d{4})", "12-2023")]
    [InlineData("PERIOD: 01-2024", @"(?:PERIODO|PERIOD)[:\s]*(\d{2}-\d{4})", "01-2024")]
    [InlineData("TOTAL: $15,000.00", @"(?:TOTAL|IMPORTE)[:\s]*\$?([0-9,]+\.?\d*)", "15,000.00")]
    [InlineData("IMPORTE $25,500", @"(?:TOTAL|IMPORTE)[:\s]*\$?([0-9,]+\.?\d*)", "25,500")]
    public void Should_ExtractValue_When_RegexPatternMatches(string text, string regexPattern, string expectedValue)
    {
        // Arrange
        var pattern = new RegexPattern(regexPattern, 0.9f);
        var context = new ExtractionContext();

        // Act
        var result = pattern.ExtractValue(text, context);

        // Assert
        result.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData("NO MATCH TEXT", @"(?:PERIODO|PERIOD)[:\s]*(\d{2}-\d{4})")]
    [InlineData("DIFFERENT PATTERN", @"(?:TOTAL|IMPORTE)[:\s]*\$?([0-9,]+\.?\d*)")]
    public void Should_ReturnNull_When_RegexPatternDoesNotMatch(string text, string regexPattern)
    {
        // Arrange
        var pattern = new RegexPattern(regexPattern, 0.9f);
        var context = new ExtractionContext();

        // Act
        var result = pattern.ExtractValue(text, context);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public void Should_HandleInvalidRegex_When_RegexPatternIsInvalid()
    {
        // Arrange
        var invalidPattern = @"[unclosed bracket";
        var pattern = new RegexPattern(invalidPattern, 0.9f);
        var context = new ExtractionContext();

        // Act
        var result = pattern.ExtractValue("test text", context);

        // Assert
        result.ShouldBeNull(); // Should handle regex errors gracefully
    }
}

/// <summary>
/// Unit tests for KeywordPattern extraction pattern
/// </summary>
public class KeywordPatternTests
{
    [Fact]
    public void Should_InitializeWithDefaultValues_When_KeywordPatternCreated()
    {
        // Act
        var pattern = new KeywordPattern();

        // Assert
        pattern.Id.ShouldNotBeNullOrEmpty();
        pattern.Name.ShouldBe(string.Empty);
        pattern.Confidence.ShouldBe(1.0f);
        pattern.IsActive.ShouldBeTrue();
        pattern.Keyword.ShouldBe(string.Empty);
        pattern.Strategy.ShouldBe(PositionStrategy.NextToken);
    }

    [Fact]
    public void Should_CreateKeywordPatternWithConstructor_When_ParametersProvided()
    {
        // Arrange
        var keyword = "TOTAL";
        var strategy = PositionStrategy.NextToken;
        var confidence = 0.85f;

        // Act
        var pattern = new KeywordPattern(keyword, strategy, confidence);

        // Assert
        pattern.Keyword.ShouldBe(keyword);
        pattern.Strategy.ShouldBe(strategy);
        pattern.Confidence.ShouldBe(confidence);
    }

    [Theory]
    [InlineData("TOTAL $15,000.00", "TOTAL", nameof(PositionStrategy.NextToken), "$15,000.00")]
    [InlineData("AMOUNT: 25000", "AMOUNT", nameof(PositionStrategy.NextToken), "25000")]
    [InlineData("PERIODO 12-2023 SIGUIENTE", "PERIODO", nameof(PositionStrategy.NextToken), "12-2023")]
    public void Should_ExtractNextToken_When_NextTokenStrategyUsed(
        string text, string keyword, string strategyName, string expectedValue)
    {
        // Arrange
        var strategy = Enum.Parse<PositionStrategy>(strategyName);
        var pattern = new KeywordPattern(keyword, strategy, 0.9f);
        var context = new ExtractionContext();

        // Act
        var result = pattern.ExtractValue(text, context);

        // Assert
        result.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData("TOTAL: Payment amount is $15,000.00", "TOTAL", nameof(PositionStrategy.SameLine), ": Payment amount is $15,000.00")]
    [InlineData("AMOUNT $25,000 for December", "AMOUNT", nameof(PositionStrategy.SameLine), "$25,000 for December")]
    public void Should_ExtractSameLine_When_SameLineStrategyUsed(
        string text, string keyword, string strategyName, string expectedValue)
    {
        // Arrange
        var strategy = Enum.Parse<PositionStrategy>(strategyName);
        var pattern = new KeywordPattern(keyword, strategy, 0.9f);
        var context = new ExtractionContext();

        // Act
        var result = pattern.ExtractValue(text, context);

        // Assert
        result.ShouldBe(expectedValue);
    }

    [Fact]
    public void Should_ExtractNextLine_When_NextLineStrategyUsed()
    {
        // Arrange
        var text = "PAYMENT PERIOD:\n12-2023\nOther text";
        var pattern = new KeywordPattern("PAYMENT PERIOD", PositionStrategy.NextLine, 0.9f);
        var context = new ExtractionContext();

        // Act
        var result = pattern.ExtractValue(text, context);

        // Assert
        result.ShouldBe("12-2023");
    }

    [Fact]
    public void Should_ReturnNull_When_KeywordNotFound()
    {
        // Arrange
        var text = "This text does not contain the keyword";
        var pattern = new KeywordPattern("MISSING", PositionStrategy.NextToken, 0.9f);
        var context = new ExtractionContext();

        // Act
        var result = pattern.ExtractValue(text, context);

        // Assert
        result.ShouldBeNull();
    }
} 