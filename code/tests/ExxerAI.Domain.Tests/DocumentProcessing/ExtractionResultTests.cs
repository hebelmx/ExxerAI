using ExxerAI.Domain.DocumentProcessing;
using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Comprehensive tests for ExtractionResult domain class following I-TDD principles.
/// Tests cover field extraction, confidence scoring, validation, and business rules.
/// </summary>
public class ExtractionResultTests
{
    /// <summary>
    /// Contract Test: ExtractionResult should initialize with defaults
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults_When_Created()
    {
        // Act
        var result = new ExtractionResult();

        // Assert
        result.ExtractedFields.ShouldNotBeNull();
        result.ExtractedFields.ShouldBeEmpty();
        result.Confidence.ShouldBe(0.0f);
        result.Method.ShouldBe(ExtractionMethod.DirectText);
        result.Warnings.ShouldNotBeNull();
        result.Warnings.ShouldBeEmpty();
        result.SchemaId.ShouldBeNull();
        result.ProcessingTimeMs.ShouldBe(0);
        result.ExtractedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    /// <summary>
    /// Contract Test: ExtractionResult properties should be settable
    /// </summary>
    [Fact]
    public void Properties_ShouldBeSettable_When_ValuesAssigned()
    {
        // Arrange
        var result = new ExtractionResult();
        var extractedFields = new Dictionary<string, FieldValue>
        {
            ["invoice_number"] = new FieldValue { Value = "INV-001", Confidence = 0.95f }
        };
        var extractedAt = DateTime.UtcNow.AddMinutes(-5);

        // Act
        result.ExtractedFields = extractedFields;
        result.Confidence = 0.87f;
        result.Method = ExtractionMethod.OCR;
        result.SchemaId = "invoice-schema-v1";
        result.ProcessingTimeMs = 1500;
        result.ExtractedAt = extractedAt;

        // Assert
        result.ExtractedFields.ShouldBeSameAs(extractedFields);
        result.Confidence.ShouldBe(0.87f);
        result.Method.ShouldBe(ExtractionMethod.OCR);
        result.SchemaId.ShouldBe("invoice-schema-v1");
        result.ProcessingTimeMs.ShouldBe(1500);
        result.ExtractedAt.ShouldBe(extractedAt);
    }

    /// <summary>
    /// Property Test: IsSuccessful should return true when fields exist with confidence
    /// </summary>
    [Theory]
    [InlineData(0.1f, true)]
    [InlineData(0.5f, true)]
    [InlineData(0.9f, true)]
    public void IsSuccessful_ShouldReturnTrue_When_FieldsExistWithConfidence(float confidence, bool expected)
    {
        // Arrange
        var result = new ExtractionResult();
        result.AddField("test_field", "test_value", confidence);
        result.Confidence = confidence;

        // Act
        var isSuccessful = result.IsSuccessful;

        // Assert
        isSuccessful.ShouldBe(expected);
    }

    /// <summary>
    /// Property Test: IsSuccessful should return false when no fields or zero confidence
    /// </summary>
    [Theory]
    [InlineData(0, 0.0f, false)]   // No fields, no confidence
    [InlineData(1, 0.0f, false)]   // Has fields but no confidence
    [InlineData(0, 0.5f, false)]   // Has confidence but no fields
    public void IsSuccessful_ShouldReturnFalse_When_RequirementsNotMet(int fieldCount, float confidence, bool expected)
    {
        // Arrange
        var result = new ExtractionResult();
        if (fieldCount > 0)
        {
            result.AddField("test_field", "test_value", 0.5f);
        }
        result.Confidence = confidence;

        // Act
        var isSuccessful = result.IsSuccessful;

        // Assert
        isSuccessful.ShouldBe(expected);
    }

    /// <summary>
    /// Property Test: FieldCount should return correct number of fields
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void FieldCount_ShouldReturnCorrectCount_When_FieldsAdded(int expectedCount)
    {
        // Arrange
        var result = new ExtractionResult();

        // Act
        for (int i = 0; i < expectedCount; i++)
        {
            result.AddField($"field_{i}", $"value_{i}", 0.8f);
        }

        // Assert
        result.FieldCount.ShouldBe(expectedCount);
    }

    /// <summary>
    /// Property Test: AverageFieldConfidence should calculate correctly
    /// </summary>
    [Fact]
    public void AverageFieldConfidence_ShouldCalculateCorrectly_When_MultipleFields()
    {
        // Arrange
        var result = new ExtractionResult();
        result.AddField("field1", "value1", 0.9f);
        result.AddField("field2", "value2", 0.8f);
        result.AddField("field3", "value3", 0.7f);

        // Act
        var averageConfidence = result.AverageFieldConfidence;

        // Assert
        averageConfidence.ShouldBe(0.8f, 0.001f); // (0.9 + 0.8 + 0.7) / 3 = 0.8
    }

    /// <summary>
    /// Property Test: AverageFieldConfidence should return zero when no fields
    /// </summary>
    [Fact]
    public void AverageFieldConfidence_ShouldReturnZero_When_NoFields()
    {
        // Arrange
        var result = new ExtractionResult();

        // Act
        var averageConfidence = result.AverageFieldConfidence;

        // Assert
        averageConfidence.ShouldBe(0.0f);
    }

    /// <summary>
    /// Behavior Test: AddField should add field correctly
    /// </summary>
    [Fact]
    public void AddField_ShouldAddFieldCorrectly_When_Called()
    {
        // Arrange
        var result = new ExtractionResult();

        // Act
        result.AddField("invoice_number", "INV-12345", 0.95f, "String");

        // Assert
        result.ExtractedFields.ShouldContainKey("invoice_number");
        var field = result.ExtractedFields["invoice_number"];
        field.Value.ShouldBe("INV-12345");
        field.Confidence.ShouldBe(0.95f);
        field.DataType.ShouldBe("String");
    }

    /// <summary>
    /// Behavior Test: AddField should infer data type when not provided
    /// </summary>
    [Theory]
    [InlineData("test string", "String")]
    [InlineData(12345, "Int32")]
    [InlineData(123.45, "Double")]
    [InlineData(true, "Boolean")]
    public void AddField_ShouldInferDataType_When_DataTypeNotProvided(object value, string expectedType)
    {
        // Arrange
        var result = new ExtractionResult();

        // Act
        result.AddField("test_field", value, 0.8f);

        // Assert
        result.ExtractedFields["test_field"].DataType.ShouldBe(expectedType);
    }

    /// <summary>
    /// Behavior Test: AddField should handle null values
    /// </summary>
    [Fact]
    public void AddField_ShouldHandleNullValues_When_ValueIsNull()
    {
        // Arrange
        var result = new ExtractionResult();

        // Act
        result.AddField("null_field", null!, 0.5f);

        // Assert
        result.ExtractedFields["null_field"].Value.ShouldBeNull();
        result.ExtractedFields["null_field"].DataType.ShouldBe("Unknown");
    }

    /// <summary>
    /// Behavior Test: AddWarning should add warning correctly
    /// </summary>
    [Fact]
    public void AddWarning_ShouldAddWarningCorrectly_When_Called()
    {
        // Arrange
        var result = new ExtractionResult();

        // Act
        result.AddWarning("Low confidence extraction detected");
        result.AddWarning("Field validation failed");

        // Assert
        result.Warnings.Count.ShouldBe(2);
        result.Warnings.ShouldContain("Low confidence extraction detected");
        result.Warnings.ShouldContain("Field validation failed");
    }

    /// <summary>
    /// Behavior Test: GetField should return field when exists
    /// </summary>
    [Fact]
    public void GetField_ShouldReturnField_When_FieldExists()
    {
        // Arrange
        var result = new ExtractionResult();
        result.AddField("existing_field", "test_value", 0.8f);

        // Act
        var field = result.GetField("existing_field");

        // Assert
        field.ShouldNotBeNull();
        field.Value.ShouldBe("test_value");
        field.Confidence.ShouldBe(0.8f);
    }

    /// <summary>
    /// Behavior Test: GetField should return null when field doesn't exist
    /// </summary>
    [Fact]
    public void GetField_ShouldReturnNull_When_FieldDoesNotExist()
    {
        // Arrange
        var result = new ExtractionResult();

        // Act
        var field = result.GetField("non_existing_field");

        // Assert
        field.ShouldBeNull();
    }

    /// <summary>
    /// Behavior Test: HasField should return true when field exists
    /// </summary>
    [Fact]
    public void HasField_ShouldReturnTrue_When_FieldExists()
    {
        // Arrange
        var result = new ExtractionResult();
        result.AddField("existing_field", "test_value", 0.8f);

        // Act
        var hasField = result.HasField("existing_field");

        // Assert
        hasField.ShouldBeTrue();
    }

    /// <summary>
    /// Behavior Test: HasField should return false when field doesn't exist
    /// </summary>
    [Fact]
    public void HasField_ShouldReturnFalse_When_FieldDoesNotExist()
    {
        // Arrange
        var result = new ExtractionResult();

        // Act
        var hasField = result.HasField("non_existing_field");

        // Assert
        hasField.ShouldBeFalse();
    }

    /// <summary>
    /// Integration Test: Complete extraction result should work correctly
    /// </summary>
    [Fact]
    public void CompleteExtractionResult_ShouldWorkCorrectly_When_FullyPopulated()
    {
        // Arrange & Act
        var result = new ExtractionResult
        {
            Confidence = 0.89f,
            Method = ExtractionMethod.OCR,
            SchemaId = "invoice-schema-v2",
            ProcessingTimeMs = 2500,
            ExtractedAt = DateTime.UtcNow
        };

        result.AddField("invoice_number", "INV-2023-001", 0.95f, "String");
        result.AddField("total_amount", 1250.50m, 0.92f, "Decimal");
        result.AddField("invoice_date", DateTime.Parse("2023-12-01"), 0.88f, "DateTime");
        result.AddField("vendor_name", "Acme Corporation", 0.85f, "String");
        
        result.AddWarning("Date format required manual validation");

        // Assert
        result.IsSuccessful.ShouldBeTrue();
        result.FieldCount.ShouldBe(4);
        result.HasField("invoice_number").ShouldBeTrue();
        result.HasField("total_amount").ShouldBeTrue();
        result.GetField("invoice_number")!.Value.ShouldBe("INV-2023-001");
        result.GetField("total_amount")!.Value.ShouldBe(1250.50m);
        result.AverageFieldConfidence.ShouldBe(0.9f, 0.01f); // (0.95+0.92+0.88+0.85)/4
        result.Warnings.Count.ShouldBe(1);
        result.Method.ShouldBe(ExtractionMethod.OCR);
        result.ProcessingTimeMs.ShouldBe(2500);
    }
}

/// <summary>
/// Comprehensive tests for FieldValue class
/// </summary>
public class FieldValueTests
{
    /// <summary>
    /// Contract Test: FieldValue should initialize with defaults
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults_When_Created()
    {
        // Act
        var fieldValue = new FieldValue();

        // Assert
        fieldValue.Value.ShouldNotBeNull();
        fieldValue.Confidence.ShouldBe(0.0f);
        fieldValue.DataType.ShouldBe(string.Empty);
        fieldValue.ExtractedBy.ShouldBeNull();
        fieldValue.SourceLocation.ShouldBeNull();
        fieldValue.OriginalText.ShouldBeNull();
        fieldValue.IsValidated.ShouldBeFalse();
    }

    /// <summary>
    /// Contract Test: FieldValue properties should be settable
    /// </summary>
    [Fact]
    public void Properties_ShouldBeSettable_When_ValuesAssigned()
    {
        // Arrange
        var fieldValue = new FieldValue();

        // Act
        fieldValue.Value = "Test Value";
        fieldValue.Confidence = 0.85f;
        fieldValue.DataType = "String";
        fieldValue.ExtractedBy = "regex-pattern-v1";
        fieldValue.SourceLocation = "page:1,x:100,y:200";
        fieldValue.OriginalText = "Test Value (original)";
        fieldValue.IsValidated = true;

        // Assert
        fieldValue.Value.ShouldBe("Test Value");
        fieldValue.Confidence.ShouldBe(0.85f);
        fieldValue.DataType.ShouldBe("String");
        fieldValue.ExtractedBy.ShouldBe("regex-pattern-v1");
        fieldValue.SourceLocation.ShouldBe("page:1,x:100,y:200");
        fieldValue.OriginalText.ShouldBe("Test Value (original)");
        fieldValue.IsValidated.ShouldBeTrue();
    }

    /// <summary>
    /// Property Test: StringValue should return string representation
    /// </summary>
    [Theory]
    [InlineData("string value", "string value")]
    [InlineData(12345, "12345")]
    [InlineData(123.45, "123.45")]
    [InlineData(true, "True")]
    public void StringValue_ShouldReturnStringRepresentation_When_ValueSet(object value, string expected)
    {
        // Arrange
        var fieldValue = new FieldValue { Value = value };

        // Act
        var stringValue = fieldValue.StringValue;

        // Assert
        stringValue.ShouldBe(expected);
    }

    /// <summary>
    /// Property Test: StringValue should return empty string when value is null
    /// </summary>
    [Fact]
    public void StringValue_ShouldReturnEmpty_When_ValueIsNull()
    {
        // Arrange
        var fieldValue = new FieldValue { Value = null! };

        // Act
        var stringValue = fieldValue.StringValue;

        // Assert
        stringValue.ShouldBe(string.Empty);
    }

    /// <summary>
    /// Property Test: HasValue should reflect value presence
    /// </summary>
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("value", true)]
    [InlineData(123, true)]
    [InlineData(0, true)] // Zero is still a valid value
    public void HasValue_ShouldReflectValuePresence_When_ValueSet(object? value, bool expected)
    {
        // Arrange
        var fieldValue = new FieldValue { Value = value! };

        // Act
        var hasValue = fieldValue.HasValue;

        // Assert
        hasValue.ShouldBe(expected);
    }

    /// <summary>
    /// Property Test: IsHighConfidence should return true for confidence >= 0.8
    /// </summary>
    [Theory]
    [InlineData(0.79f, false)]
    [InlineData(0.8f, true)]
    [InlineData(0.85f, true)]
    [InlineData(0.9f, true)]
    [InlineData(1.0f, true)]
    public void IsHighConfidence_ShouldReturnCorrectValue_When_ConfidenceSet(float confidence, bool expected)
    {
        // Arrange
        var fieldValue = new FieldValue { Confidence = confidence };

        // Act
        var isHighConfidence = fieldValue.IsHighConfidence;

        // Assert
        isHighConfidence.ShouldBe(expected);
    }

    /// <summary>
    /// Business Rule Test: FieldValue should support different data types
    /// </summary>
    [Theory]
    [InlineData("text", "String")]
    [InlineData(42, "Int32")]
    [InlineData(3.14159, "Double")]
    [InlineData(true, "Boolean")]
    public void FieldValue_ShouldSupportDifferentDataTypes_When_ValuesSet(object value, string dataType)
    {
        // Arrange & Act
        var fieldValue = new FieldValue
        {
            Value = value,
            DataType = dataType,
            Confidence = 0.9f
        };

        // Assert
        fieldValue.Value.ShouldBe(value);
        fieldValue.DataType.ShouldBe(dataType);
        fieldValue.HasValue.ShouldBeTrue();
        fieldValue.IsHighConfidence.ShouldBeTrue();
    }

    /// <summary>
    /// Edge Case Test: FieldValue should handle edge cases gracefully
    /// </summary>
    [Fact]
    public void FieldValue_ShouldHandleEdgeCases_When_EdgeValuesSet()
    {
        // Arrange & Act
        var fieldValue = new FieldValue
        {
            Value = new object(), // Empty object
            Confidence = 0.0f,
            DataType = string.Empty,
            OriginalText = ""
        };

        // Assert
        fieldValue.Value.ShouldNotBeNull();
        fieldValue.HasValue.ShouldBeFalse(); // Empty string representation
        fieldValue.IsHighConfidence.ShouldBeFalse();
        fieldValue.StringValue.ShouldNotBeNull();
    }

    /// <summary>
    /// Business Rule Test: FieldValue should support extraction metadata
    /// </summary>
    [Fact]
    public void FieldValue_ShouldSupportExtractionMetadata_When_MetadataSet()
    {
        // Arrange & Act
        var fieldValue = new FieldValue
        {
            Value = "ACME-12345",
            Confidence = 0.92f,
            DataType = "String",
            ExtractedBy = "invoice-number-regex",
            SourceLocation = "page:1,line:5,col:15-25",
            OriginalText = "Invoice Number: ACME-12345",
            IsValidated = true
        };

        // Assert
        fieldValue.Value.ShouldBe("ACME-12345");
        fieldValue.ExtractedBy.ShouldBe("invoice-number-regex");
        fieldValue.SourceLocation.ShouldBe("page:1,line:5,col:15-25");
        fieldValue.OriginalText.ShouldBe("Invoice Number: ACME-12345");
        fieldValue.IsValidated.ShouldBeTrue();
        fieldValue.HasValue.ShouldBeTrue();
        fieldValue.IsHighConfidence.ShouldBeTrue();
    }
}

/// <summary>
/// Integration tests for extraction result workflow
/// </summary>
public class ExtractionResultWorkflowTests
{
    /// <summary>
    /// Workflow Test: Complete field extraction workflow
    /// </summary>
    [Fact]
    public void ExtractionWorkflow_ShouldCompleteSuccessfully_When_FieldsExtracted()
    {
        // Arrange - Simulate document field extraction
        var startTime = DateTime.UtcNow;

        // Act - Perform extraction workflow
        var result = new ExtractionResult
        {
            Confidence = 0.91f,
            Method = ExtractionMethod.OCR,
            SchemaId = "invoice-schema-v3",
            ExtractedAt = DateTime.UtcNow
        };

        // Extract invoice fields
        result.AddField("invoice_number", "INV-2023-12345", 0.98f, "String");
        result.AddField("vendor_name", "Global Supplies Inc.", 0.95f, "String");
        result.AddField("total_amount", 2847.50m, 0.93f, "Decimal");
        result.AddField("tax_amount", 284.75m, 0.90f, "Decimal");
        result.AddField("invoice_date", DateTime.Parse("2023-12-15"), 0.89f, "DateTime");
        result.AddField("due_date", DateTime.Parse("2024-01-15"), 0.87f, "DateTime");

        // Add processing metadata
        result.ProcessingTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

        // Add warnings for low confidence fields
        if (result.GetField("due_date")!.Confidence < 0.9f)
        {
            result.AddWarning("Due date extraction has lower confidence - verify manually");
        }

        // Assert - Verify extraction workflow
        result.IsSuccessful.ShouldBeTrue();
        result.FieldCount.ShouldBe(6);
        result.AverageFieldConfidence.ShouldBeGreaterThan(0.9f);
        result.HasField("invoice_number").ShouldBeTrue();
        result.HasField("vendor_name").ShouldBeTrue();
        result.HasField("total_amount").ShouldBeTrue();
        
        // Verify high-value fields have high confidence
        result.GetField("invoice_number")!.IsHighConfidence.ShouldBeTrue();
        result.GetField("vendor_name")!.IsHighConfidence.ShouldBeTrue();
        result.GetField("total_amount")!.IsHighConfidence.ShouldBeTrue();
        
        result.Warnings.Count.ShouldBe(1);
        result.ProcessingTimeMs.ShouldBeGreaterThan(0);
    }

    /// <summary>
    /// Error Handling Test: Extraction with low confidence should handle gracefully
    /// </summary>
    [Fact]
    public void ExtractionWorkflow_ShouldHandleLowConfidence_When_ExtractionUncertain()
    {
        // Arrange & Act - Simulate low-confidence extraction
        var result = new ExtractionResult
        {
            Confidence = 0.45f,
            Method = ExtractionMethod.OCR,
            ProcessingTimeMs = 3500
        };

        result.AddField("unclear_field", "maybe_value?", 0.3f, "String");
        result.AddField("partial_field", "incomplete...", 0.6f, "String");
        
        result.AddWarning("Document quality is poor - OCR confidence low");
        result.AddWarning("Multiple fields could not be extracted reliably");

        // Assert - Verify low-confidence handling
        result.IsSuccessful.ShouldBeTrue(); // Has fields with some confidence
        result.FieldCount.ShouldBe(2);
        result.AverageFieldConfidence.ShouldBe(0.45f, 0.01f);
        result.GetField("unclear_field")!.IsHighConfidence.ShouldBeFalse();
        result.GetField("partial_field")!.IsHighConfidence.ShouldBeFalse();
        result.Warnings.Count.ShouldBe(2);
    }

    /// <summary>
    /// Performance Test: Extraction operations should be efficient
    /// </summary>
    [Fact]
    public void ExtractionOperations_ShouldBeEfficient_When_ManyFieldsProcessed()
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var results = new List<ExtractionResult>();

        // Act - Process multiple extraction results
        for (int i = 0; i < 20; i++)
        {
            var result = new ExtractionResult
            {
                Confidence = 0.85f + (i % 10) * 0.01f,
                Method = ExtractionMethod.DirectText,
                SchemaId = $"schema-{i % 3}"
            };

            // Add multiple fields per result
            for (int j = 0; j < 5; j++)
            {
                result.AddField($"field_{j}", $"value_{i}_{j}", 0.8f + j * 0.02f);
            }

            results.Add(result);
        }

        var duration = DateTime.UtcNow - startTime;

        // Assert
        results.Count.ShouldBe(20);
        results.All(r => r.IsSuccessful).ShouldBeTrue();
        results.All(r => r.FieldCount == 5).ShouldBeTrue();
        results.Sum(r => r.FieldCount).ShouldBe(100);
        duration.ShouldBeLessThan(TimeSpan.FromSeconds(1)); // Should be very fast
    }
} 