namespace ExxerAI.Domain.Tests.PersonaFeatures;

/// <summary>
/// Unit tests for PromptTemplate domain entity
/// </summary>
public class PromptTemplateTests
{
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
        };

        // Act
        var rendered = template.RenderTemplate(context);

        // Assert
        rendered.ShouldBe("Process this Excel file with standard approach");
    }

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
        };

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => template.RenderTemplate(context));
    }

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
        Should.Throw<ArgumentNullException>(() => template.RenderTemplate(null!));
    }

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

    [Fact]
    public void Should_SetCorrectDefaults_When_TemplateInitialized()
    {
        // Act
        var template = new PromptTemplate();

        // Assert
        template.Id.ShouldNotBe(Guid.Empty);
        template.Name.ShouldBe(string.Empty);
        template.TemplateText.ShouldBe(string.Empty);
        template.ContextTag.ShouldBe(string.Empty);
        template.Description.ShouldBe(string.Empty);
        template.Version.ShouldBe(1);
        template.IsActive.ShouldBeTrue();
        template.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        template.UpdatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        template.ExpectedParameters.ShouldNotBeNull();
        template.DefaultParameters.ShouldNotBeNull();
        template.Metadata.ShouldNotBeNull();
        template.PersonaId.ShouldBeNull();
        template.Persona.ShouldBeNull();
    }
} 