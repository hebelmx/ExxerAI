namespace ExxerAI.Domain.Tests;

/// <summary>
/// Unit tests for PromptTemplate domain entity
/// </summary>
public class PromptTemplateTests
{
    /// <summary>
    /// Tests prompt template creation with valid data
    /// </summary>
    [Fact]
    public void Should_CreatePromptTemplate_When_ValidDataProvided()
    {
        // Arrange & Act
        var template = new PromptTemplate
        {
            Name = "Code Review Template",
            TemplateText = "Please review this {{language}} code: {{code}}",
            ContextTag = "code_review",
            Description = "Template for reviewing code submissions"
        };

        // Assert
        template.Id.ShouldNotBe(Guid.Empty);
        template.Name.ShouldBe("Code Review Template");
        template.TemplateText.ShouldBe("Please review this {{language}} code: {{code}}");
        template.ContextTag.ShouldBe("code_review");
        template.Description.ShouldBe("Template for reviewing code submissions");
        template.Version.ShouldBe(1);
        template.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        template.UpdatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        template.IsActive.ShouldBeTrue();
        template.ExpectedParameters.ShouldNotBeNull();
        template.ExpectedParameters.ShouldBeEmpty();
        template.DefaultParameters.ShouldNotBeNull();
        template.DefaultParameters.ShouldBeEmpty();
        template.Metadata.ShouldNotBeNull();
        template.Metadata.ShouldBeEmpty();
        template.PersonaId.ShouldBeNull();
        template.Persona.ShouldBeNull();
    }

    /// <summary>
    /// Tests rendering template with simple token substitution
    /// </summary>
    [Fact]
    public void Should_RenderTemplate_When_ValidContextProvided()
    {
        // Arrange
        var template = new PromptTemplate
        {
            Name = "Greeting Template",
            TemplateText = "Hello {{name}}, welcome to {{system}}!",
            ContextTag = "greeting"
        };

        var context = new Dictionary<string, object>
        {
            { "name", "John" },
            { "system", "ExxerAI" }
        };

        // Act
        var rendered = template.RenderTemplate(context);

        // Assert
        rendered.ShouldBe("Hello John, welcome to ExxerAI!");
    }

    /// <summary>
    /// Tests rendering template with expected parameters
    /// </summary>
    [Fact]
    public void Should_RenderTemplate_When_ExpectedParametersProvided()
    {
        // Arrange
        var template = new PromptTemplate
        {
            Name = "Analysis Template",
            TemplateText = "Analyze this {{document_type}} document: {{content}}",
            ContextTag = "analysis"
        };

        template.AddExpectedParameter("document_type", "Type of document to analyze");
        template.AddExpectedParameter("content", "Document content to analyze");

        var context = new Dictionary<string, object>
        {
            { "document_type", "financial report" },
            { "content", "Q4 earnings data..." }
        };

        // Act
        var rendered = template.RenderTemplate(context);

        // Assert
        rendered.ShouldBe("Analyze this financial report document: Q4 earnings data...");
    }

    /// <summary>
    /// Tests rendering template with default parameters
    /// </summary>
    [Fact]
    public void Should_UseDefaultValues_When_ParametersNotProvided()
    {
        // Arrange
        var template = new PromptTemplate
        {
            Name = "Default Template",
            TemplateText = "Process this {{format}} file with {{method}} approach",
            ContextTag = "processing"
        };

        template.AddExpectedParameter("format", "File format", "PDF");
        template.AddExpectedParameter("method", "Processing method", "standard");

        var context = new Dictionary<string, object>
        {
            { "format", "Excel" }
            // method not provided, should use default
        };

        // Act
        var rendered = template.RenderTemplate(context);

        // Assert
        rendered.ShouldBe("Process this Excel file with standard approach");
    }

    /// <summary>
    /// Tests rendering throws exception when required parameters missing
    /// </summary>
    [Fact]
    public void Should_ThrowException_When_RequiredParametersMissing()
    {
        // Arrange
        var template = new PromptTemplate
        {
            Name = "Required Template",
            TemplateText = "Process {{required_param}} with care",
            ContextTag = "required"
        };

        template.AddExpectedParameter("required_param", "This parameter is required");

        var context = new Dictionary<string, object>
        {
            { "other_param", "value" }
            // required_param not provided
        };

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => template.RenderTemplate(context));
    }

    /// <summary>
    /// Tests rendering with null context throws exception
    /// </summary>
    [Fact]
    public void Should_ThrowException_When_ContextIsNull()
    {
        // Arrange
        var template = new PromptTemplate
        {
            Name = "Test Template",
            TemplateText = "Hello {{name}}",
            ContextTag = "test"
        };

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => template.RenderTemplate(null));
    }

    /// <summary>
    /// Tests rendering handles additional context parameters
    /// </summary>
    [Fact]
    public void Should_RenderAdditionalParameters_When_NotInExpectedList()
    {
        // Arrange
        var template = new PromptTemplate
        {
            Name = "Flexible Template",
            TemplateText = "Process {{type}} with {{method}} and {{extra}}",
            ContextTag = "flexible"
        };

        template.AddExpectedParameter("type", "Document type");
        template.AddExpectedParameter("method", "Processing method");
        // extra is not in expected parameters

        var context = new Dictionary<string, object>
        {
            { "type", "PDF" },
            { "method", "OCR" },
            { "extra", "special handling" }
        };

        // Act
        var rendered = template.RenderTemplate(context);

        // Assert
        rendered.ShouldBe("Process PDF with OCR and special handling");
    }

    /// <summary>
    /// Tests creating new version of template
    /// </summary>
    [Fact]
    public void Should_CreateNewVersion_When_UpdatedTextProvided()
    {
        // Arrange
        var originalTemplate = new PromptTemplate
        {
            Name = "Original Template",
            TemplateText = "Original text with {{param}}",
            ContextTag = "test",
            Version = 1
        };

        originalTemplate.AddExpectedParameter("param", "Test parameter");

        // Act
        var newVersion = originalTemplate.CreateNewVersion(
            "Updated text with {{param}} and improvements", 
            "Added improvements");

        // Assert
        newVersion.ShouldNotBe(originalTemplate);
        newVersion.Id.ShouldNotBe(originalTemplate.Id);
        newVersion.Name.ShouldBe(originalTemplate.Name);
        newVersion.TemplateText.ShouldBe("Updated text with {{param}} and improvements");
        newVersion.ContextTag.ShouldBe(originalTemplate.ContextTag);
        newVersion.Version.ShouldBe(2);
        newVersion.Description.ShouldBe("Added improvements");
        newVersion.ExpectedParameters.Count.ShouldBe(1);
        newVersion.ExpectedParameters.ShouldContainKey("param");
        newVersion.PersonaId.ShouldBe(originalTemplate.PersonaId);
    }

    /// <summary>
    /// Tests creating new version with null or empty text throws exception
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ThrowException_When_CreatingVersionWithInvalidText(string? invalidText)
    {
        // Arrange
        var template = new PromptTemplate
        {
            Name = "Test Template",
            TemplateText = "Original text",
            ContextTag = "test"
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => template.CreateNewVersion(invalidText));
    }

    /// <summary>
    /// Tests template validation with valid template
    /// </summary>
    [Fact]
    public void Should_ReturnValidResult_When_TemplateIsValid()
    {
        // Arrange
        var template = new PromptTemplate
        {
            Name = "Valid Template",
            TemplateText = "Process {{document}} with {{method}}",
            ContextTag = "processing"
        };

        template.AddExpectedParameter("document", "Document to process");
        template.AddExpectedParameter("method", "Processing method");

        // Act
        var validation = template.Validate();

        // Assert
        validation.IsValid.ShouldBeTrue();
        validation.Errors.ShouldBeEmpty();
        validation.TokensFound.Count.ShouldBe(2);
        validation.TokensFound.ShouldContain("document");
        validation.TokensFound.ShouldContain("method");
    }

    /// <summary>
    /// Tests template validation with missing required fields
    /// </summary>
    [Fact]
    public void Should_ReturnInvalidResult_When_RequiredFieldsMissing()
    {
        // Arrange
        var template = new PromptTemplate
        {
            Name = "",
            TemplateText = "",
            ContextTag = ""
        };

        // Act
        var validation = template.Validate();

        // Assert
        validation.IsValid.ShouldBeFalse();
        validation.Errors.Count.ShouldBe(2);
        validation.Errors.ShouldContain("Template text is required");
        validation.Errors.ShouldContain("Context tag is required");
    }

    /// <summary>
    /// Tests template validation with undefined parameters
    /// </summary>
    [Fact]
    public void Should_ReturnWarnings_When_UndefinedParametersFound()
    {
        // Arrange
        var template = new PromptTemplate
        {
            Name = "Template with undefined params",
            TemplateText = "Process {{document}} with {{undefined_param}}",
            ContextTag = "processing"
        };

        template.AddExpectedParameter("document", "Document to process");
        // undefined_param is not in expected parameters

        // Act
        var validation = template.Validate();

        // Assert
        validation.IsValid.ShouldBeTrue();
        validation.Warnings.Count.ShouldBe(1);
        validation.Warnings[0].ShouldContain("undefined parameters: undefined_param");
    }

    /// <summary>
    /// Tests template validation with unused expected parameters
    /// </summary>
    [Fact]
    public void Should_ReturnWarnings_When_UnusedParametersFound()
    {
        // Arrange
        var template = new PromptTemplate
        {
            Name = "Template with unused params",
            TemplateText = "Process {{document}}",
            ContextTag = "processing"
        };

        template.AddExpectedParameter("document", "Document to process");
        template.AddExpectedParameter("unused_param", "This parameter is not used");

        // Act
        var validation = template.Validate();

        // Assert
        validation.IsValid.ShouldBeTrue();
        validation.Warnings.Count.ShouldBe(1);
        validation.Warnings[0].ShouldContain("Expected parameters not used in template: unused_param");
    }

    /// <summary>
    /// Tests adding expected parameter
    /// </summary>
    [Fact]
    public void Should_AddExpectedParameter_When_ValidParameterProvided()
    {
        // Arrange
        var template = new PromptTemplate
        {
            Name = "Test Template",
            TemplateText = "Test {{param}}",
            ContextTag = "test"
        };
        var originalUpdateTime = template.UpdatedAt;

        // Act
        template.AddExpectedParameter("param", "Test parameter description", "default_value");

        // Assert
        template.ExpectedParameters.ShouldContainKey("param");
        template.ExpectedParameters["param"].ShouldBe("Test parameter description");
        template.DefaultParameters.ShouldContainKey("param");
        template.DefaultParameters["param"].ShouldBe("default_value");
        template.UpdatedAt.ShouldBeGreaterThan(originalUpdateTime);
    }

    /// <summary>
    /// Tests adding expected parameter with null name throws exception
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ThrowException_When_AddingParameterWithInvalidName(string? invalidName)
    {
        // Arrange
        var template = new PromptTemplate
        {
            Name = "Test Template",
            TemplateText = "Test text",
            ContextTag = "test"
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => template.AddExpectedParameter(invalidName, "description"));
    }

    /// <summary>
    /// Tests removing expected parameter
    /// </summary>
    [Fact]
    public void Should_RemoveExpectedParameter_When_ParameterExists()
    {
        // Arrange
        var template = new PromptTemplate
        {
            Name = "Test Template",
            TemplateText = "Test {{param}}",
            ContextTag = "test"
        };

        template.AddExpectedParameter("param", "Test parameter", "default");
        var originalUpdateTime = template.UpdatedAt;

        // Act
        var removed = template.RemoveExpectedParameter("param");

        // Assert
        removed.ShouldBeTrue();
        template.ExpectedParameters.ShouldNotContainKey("param");
        template.DefaultParameters.ShouldNotContainKey("param");
        template.UpdatedAt.ShouldBeGreaterThan(originalUpdateTime);
    }

    /// <summary>
    /// Tests removing non-existent parameter returns false
    /// </summary>
    [Fact]
    public void Should_ReturnFalse_When_RemovingNonExistentParameter()
    {
        // Arrange
        var template = new PromptTemplate
        {
            Name = "Test Template",
            TemplateText = "Test text",
            ContextTag = "test"
        };

        // Act
        var removed = template.RemoveExpectedParameter("non_existent");

        // Assert
        removed.ShouldBeFalse();
    }

    /// <summary>
    /// Tests deactivating template
    /// </summary>
    [Fact]
    public void Should_DeactivateTemplate_When_DeactivateCalled()
    {
        // Arrange
        var template = new PromptTemplate
        {
            Name = "Test Template",
            TemplateText = "Test text",
            ContextTag = "test"
        };
        var originalUpdateTime = template.UpdatedAt;

        // Act
        template.Deactivate();

        // Assert
        template.IsActive.ShouldBeFalse();
        template.UpdatedAt.ShouldBeGreaterThan(originalUpdateTime);
    }

    /// <summary>
    /// Tests activating template
    /// </summary>
    [Fact]
    public void Should_ActivateTemplate_When_ActivateCalled()
    {
        // Arrange
        var template = new PromptTemplate
        {
            Name = "Test Template",
            TemplateText = "Test text",
            ContextTag = "test",
            IsActive = false
        };
        var originalUpdateTime = template.UpdatedAt;

        // Act
        template.Activate();

        // Assert
        template.IsActive.ShouldBeTrue();
        template.UpdatedAt.ShouldBeGreaterThan(originalUpdateTime);
    }

    /// <summary>
    /// Tests template handles complex token patterns
    /// </summary>
    [Theory]
    [InlineData("{{simple}}", "simple")]
    [InlineData("{{with_underscore}}", "with_underscore")]
    [InlineData("{{number123}}", "number123")]
    [InlineData("{{camelCase}}", "camelCase")]
    public void Should_HandleVariousTokenPatterns_When_ValidTokensProvided(string tokenText, string expectedToken)
    {
        // Arrange
        var template = new PromptTemplate
        {
            Name = "Token Test Template",
            TemplateText = $"Process {tokenText} correctly",
            ContextTag = "token_test"
        };

        template.AddExpectedParameter(expectedToken, "Test parameter");

        // Act
        var validation = template.Validate();

        // Assert
        validation.IsValid.ShouldBeTrue();
        validation.TokensFound.ShouldContain(expectedToken);
    }

    /// <summary>
    /// Tests template with multiple occurrences of same token
    /// </summary>
    [Fact]
    public void Should_HandleMultipleOccurrences_When_SameTokenRepeated()
    {
        // Arrange
        var template = new PromptTemplate
        {
            Name = "Repeated Token Template",
            TemplateText = "Start with {{param}}, process {{param}}, end with {{param}}",
            ContextTag = "repeated"
        };

        template.AddExpectedParameter("param", "Repeated parameter");

        var context = new Dictionary<string, object>
        {
            { "param", "VALUE" }
        };

        // Act
        var rendered = template.RenderTemplate(context);

        // Assert
        rendered.ShouldBe("Start with VALUE, process VALUE, end with VALUE");
    }

    /// <summary>
    /// Tests template initialization sets correct default values
    /// </summary>
    [Fact]
    public void Should_SetCorrectDefaults_When_TemplateInitialized()
    {
        // Arrange & Act
        var template = new PromptTemplate();

        // Assert
        template.Id.ShouldNotBe(Guid.Empty);
        template.Name.ShouldBe(string.Empty);
        template.TemplateText.ShouldBe(string.Empty);
        template.ContextTag.ShouldBe(string.Empty);
        template.Version.ShouldBe(1);
        template.Description.ShouldBe(string.Empty);
        template.IsActive.ShouldBeTrue();
        template.ExpectedParameters.ShouldNotBeNull();
        template.ExpectedParameters.ShouldBeEmpty();
        template.DefaultParameters.ShouldNotBeNull();
        template.DefaultParameters.ShouldBeEmpty();
        template.Metadata.ShouldNotBeNull();
        template.Metadata.ShouldBeEmpty();
        template.PersonaId.ShouldBeNull();
        template.Persona.ShouldBeNull();
    }

    /// <summary>
    /// Tests case sensitivity in token replacement
    /// </summary>
    [Fact]
    public void Should_BeCaseInsensitive_When_ReplacingTokens()
    {
        // Arrange
        var template = new PromptTemplate
        {
            Name = "Case Test Template",
            TemplateText = "Process {{Parameter}} and {{PARAM}}",
            ContextTag = "case_test"
        };

        var context = new Dictionary<string, object>
        {
            { "parameter", "value1" },
            { "param", "value2" }
        };

        // Act
        var rendered = template.RenderTemplate(context);

        // Assert
        rendered.ShouldBe("Process value1 and value2");
    }
}