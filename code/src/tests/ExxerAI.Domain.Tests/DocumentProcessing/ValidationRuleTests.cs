namespace ExxerAI.Domain.Tests.DocumentProcessing;

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