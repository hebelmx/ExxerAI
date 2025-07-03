using ExxerAI.Domain.DocumentProcessing;

namespace ExxerAI.Domain.Tests.DocumentProcessing;

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