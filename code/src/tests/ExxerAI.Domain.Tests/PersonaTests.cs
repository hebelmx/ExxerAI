namespace ExxerAI.Domain.Tests;

/// <summary>
/// Unit tests for Persona domain entity
/// </summary>
public class PersonaTests
{
    /// <summary>
    /// Tests persona creation with valid data
    /// </summary>
    [Fact]
    public void Should_CreatePersona_When_ValidDataProvided()
    {
        // Arrange & Act
        var persona = new Persona
        {
            Name = "Technical Expert",
            Role = "Software Architect",
            Description = "Expert in software design and architecture patterns",
            SystemPrompt = "You are a technical expert specializing in software architecture."
        };

        // Assert
        persona.Id.ShouldNotBe(Guid.Empty);
        persona.Name.ShouldBe("Technical Expert");
        persona.Role.ShouldBe("Software Architect");
        persona.Description.ShouldBe("Expert in software design and architecture patterns");
        persona.SystemPrompt.ShouldBe("You are a technical expert specializing in software architecture.");
        persona.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        persona.UpdatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        persona.IsActive.ShouldBeTrue();
        persona.Traits.ShouldNotBeNull();
        persona.Traits.ShouldBeEmpty();
        persona.KnowledgeDomains.ShouldNotBeNull();
        persona.KnowledgeDomains.ShouldBeEmpty();
        persona.Templates.ShouldNotBeNull();
        persona.Templates.ShouldBeEmpty();
        persona.Metadata.ShouldNotBeNull();
        persona.Metadata.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests persona creation with traits and knowledge domains
    /// </summary>
    [Fact]
    public void Should_CreatePersonaWithTraitsAndDomains_When_InitializedWithData()
    {
        // Arrange
        var traits = new Dictionary<string, string>
        {
            { "tone", "professional" },
            { "expertise_level", "expert" },
            { "communication_style", "concise" }
        };

        var domains = new List<string>
        {
            "software_architecture",
            "cloud_computing",
            "microservices"
        };

        // Act
        var persona = new Persona
        {
            Name = "Cloud Architect",
            Role = "Solution Architect",
            Description = "Expert in cloud-native architectures",
            SystemPrompt = "You are a cloud architecture expert.",
            Traits = traits,
            KnowledgeDomains = domains
        };

        // Assert
        persona.Traits.Count.ShouldBe(3);
        persona.Traits["tone"].ShouldBe("professional");
        persona.Traits["expertise_level"].ShouldBe("expert");
        persona.Traits["communication_style"].ShouldBe("concise");
        
        persona.KnowledgeDomains.Count.ShouldBe(3);
        persona.KnowledgeDomains.ShouldContain("software_architecture");
        persona.KnowledgeDomains.ShouldContain("cloud_computing");
        persona.KnowledgeDomains.ShouldContain("microservices");
    }

    /// <summary>
    /// Tests adding traits to persona
    /// </summary>
    [Theory]
    [InlineData("tone", "friendly")]
    [InlineData("expertise_level", "beginner")]
    [InlineData("communication_style", "detailed")]
    public void Should_AddTrait_When_ValidKeyValueProvided(string key, string value)
    {
        // Arrange
        var persona = new Persona
        {
            Name = "Test Persona",
            Role = "Tester",
            Description = "Test description",
            SystemPrompt = "Test prompt"
        };
        var originalUpdateTime = persona.UpdatedAt;

        // Act
        persona.AddTrait(key, value);

        // Assert
        persona.Traits.ShouldContainKey(key);
        persona.Traits[key].ShouldBe(value);
        persona.UpdatedAt.ShouldBeGreaterThan(originalUpdateTime);
    }

    /// <summary>
    /// Tests adding trait with null or empty key throws exception
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ThrowException_When_AddingTraitWithInvalidKey(string invalidKey)
    {
        // Arrange
        var persona = new Persona
        {
            Name = "Test Persona",
            Role = "Tester",
            Description = "Test description",
            SystemPrompt = "Test prompt"
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => persona.AddTrait(invalidKey, "value"));
    }

    /// <summary>
    /// Tests retrieving trait value
    /// </summary>
    [Fact]
    public void Should_ReturnTraitValue_When_TraitExists()
    {
        // Arrange
        var persona = new Persona
        {
            Name = "Test Persona",
            Role = "Tester",
            Description = "Test description",
            SystemPrompt = "Test prompt"
        };
        persona.AddTrait("tone", "professional");

        // Act
        var value = persona.GetTrait("tone");

        // Assert
        value.ShouldBe("professional");
    }

    /// <summary>
    /// Tests retrieving non-existent trait returns empty string
    /// </summary>
    [Fact]
    public void Should_ReturnEmptyString_When_TraitDoesNotExist()
    {
        // Arrange
        var persona = new Persona
        {
            Name = "Test Persona",
            Role = "Tester",
            Description = "Test description",
            SystemPrompt = "Test prompt"
        };

        // Act
        var value = persona.GetTrait("non_existent");

        // Assert
        value.ShouldBe(string.Empty);
    }

    /// <summary>
    /// Tests adding knowledge domain
    /// </summary>
    [Theory]
    [InlineData("artificial_intelligence")]
    [InlineData("machine_learning")]
    [InlineData("data_science")]
    public void Should_AddKnowledgeDomain_When_ValidDomainProvided(string domain)
    {
        // Arrange
        var persona = new Persona
        {
            Name = "AI Expert",
            Role = "Data Scientist",
            Description = "Expert in AI and ML",
            SystemPrompt = "You are an AI expert."
        };
        var originalUpdateTime = persona.UpdatedAt;

        // Act
        persona.AddKnowledgeDomain(domain);

        // Assert
        persona.KnowledgeDomains.ShouldContain(domain);
        persona.UpdatedAt.ShouldBeGreaterThan(originalUpdateTime);
    }

    /// <summary>
    /// Tests adding duplicate knowledge domain does not duplicate
    /// </summary>
    [Fact]
    public void Should_NotDuplicateDomain_When_AddingSameDomainTwice()
    {
        // Arrange
        var persona = new Persona
        {
            Name = "AI Expert",
            Role = "Data Scientist",
            Description = "Expert in AI and ML",
            SystemPrompt = "You are an AI expert."
        };

        // Act
        persona.AddKnowledgeDomain("machine_learning");
        persona.AddKnowledgeDomain("machine_learning");

        // Assert
        persona.KnowledgeDomains.Count.ShouldBe(1);
        persona.KnowledgeDomains.ShouldContain("machine_learning");
    }

    /// <summary>
    /// Tests adding knowledge domain with null or empty value throws exception
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ThrowException_When_AddingInvalidKnowledgeDomain(string invalidDomain)
    {
        // Arrange
        var persona = new Persona
        {
            Name = "Test Persona",
            Role = "Tester",
            Description = "Test description",
            SystemPrompt = "Test prompt"
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => persona.AddKnowledgeDomain(invalidDomain));
    }

    /// <summary>
    /// Tests removing knowledge domain
    /// </summary>
    [Fact]
    public void Should_RemoveKnowledgeDomain_When_DomainExists()
    {
        // Arrange
        var persona = new Persona
        {
            Name = "AI Expert",
            Role = "Data Scientist",
            Description = "Expert in AI and ML",
            SystemPrompt = "You are an AI expert."
        };
        persona.AddKnowledgeDomain("machine_learning");
        persona.AddKnowledgeDomain("data_science");
        var originalUpdateTime = persona.UpdatedAt;

        // Act
        var removed = persona.RemoveKnowledgeDomain("machine_learning");

        // Assert
        removed.ShouldBeTrue();
        persona.KnowledgeDomains.ShouldNotContain("machine_learning");
        persona.KnowledgeDomains.ShouldContain("data_science");
        persona.UpdatedAt.ShouldBeGreaterThan(originalUpdateTime);
    }

    /// <summary>
    /// Tests removing non-existent knowledge domain returns false
    /// </summary>
    [Fact]
    public void Should_ReturnFalse_When_RemovingNonExistentDomain()
    {
        // Arrange
        var persona = new Persona
        {
            Name = "AI Expert",
            Role = "Data Scientist",
            Description = "Expert in AI and ML",
            SystemPrompt = "You are an AI expert."
        };

        // Act
        var removed = persona.RemoveKnowledgeDomain("non_existent");

        // Assert
        removed.ShouldBeFalse();
    }

    /// <summary>
    /// Tests checking if persona has knowledge domain
    /// </summary>
    [Fact]
    public void Should_ReturnTrue_When_PersonaHasKnowledgeDomain()
    {
        // Arrange
        var persona = new Persona
        {
            Name = "AI Expert",
            Role = "Data Scientist",
            Description = "Expert in AI and ML",
            SystemPrompt = "You are an AI expert."
        };
        persona.AddKnowledgeDomain("machine_learning");

        // Act
        var hassDomain = persona.HasKnowledgeDomain("machine_learning");

        // Assert
        hassDomain.ShouldBeTrue();
    }

    /// <summary>
    /// Tests checking knowledge domain is case insensitive
    /// </summary>
    [Fact]
    public void Should_BeCaseInsensitive_When_CheckingKnowledgeDomain()
    {
        // Arrange
        var persona = new Persona
        {
            Name = "AI Expert",
            Role = "Data Scientist",
            Description = "Expert in AI and ML",
            SystemPrompt = "You are an AI expert."
        };
        persona.AddKnowledgeDomain("Machine_Learning");

        // Act
        var hasDomain = persona.HasKnowledgeDomain("machine_learning");

        // Assert
        hasDomain.ShouldBeTrue();
    }

    /// <summary>
    /// Tests adding prompt template to persona
    /// </summary>
    [Fact]
    public void Should_AddTemplate_When_ValidTemplateProvided()
    {
        // Arrange
        var persona = new Persona
        {
            Name = "Code Reviewer",
            Role = "Senior Developer",
            Description = "Expert code reviewer",
            SystemPrompt = "You are a senior developer."
        };

        var template = new PromptTemplate
        {
            Name = "Code Review Template",
            TemplateText = "Please review this code: {{code}}",
            ContextTag = "code_review"
        };

        var originalUpdateTime = persona.UpdatedAt;

        // Act
        persona.AddTemplate(template);

        // Assert
        persona.Templates.ShouldContain(template);
        persona.UpdatedAt.ShouldBeGreaterThan(originalUpdateTime);
    }

    /// <summary>
    /// Tests adding null template throws exception
    /// </summary>
    [Fact]
    public void Should_ThrowException_When_AddingNullTemplate()
    {
        // Arrange
        var persona = new Persona
        {
            Name = "Test Persona",
            Role = "Tester",
            Description = "Test description",
            SystemPrompt = "Test prompt"
        };

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => persona.AddTemplate(null));
    }

    /// <summary>
    /// Tests retrieving template by context tag
    /// </summary>
    [Fact]
    public void Should_ReturnTemplate_When_ContextTagMatches()
    {
        // Arrange
        var persona = new Persona
        {
            Name = "Code Reviewer",
            Role = "Senior Developer",
            Description = "Expert code reviewer",
            SystemPrompt = "You are a senior developer."
        };

        var template = new PromptTemplate
        {
            Name = "Code Review Template",
            TemplateText = "Please review this code: {{code}}",
            ContextTag = "code_review"
        };

        persona.AddTemplate(template);

        // Act
        var foundTemplate = persona.GetTemplate("code_review");

        // Assert
        foundTemplate.ShouldNotBeNull();
        foundTemplate.ShouldBe(template);
    }

    /// <summary>
    /// Tests retrieving template with case insensitive context tag
    /// </summary>
    [Fact]
    public void Should_BeCaseInsensitive_When_RetrievingTemplateByContextTag()
    {
        // Arrange
        var persona = new Persona
        {
            Name = "Code Reviewer",
            Role = "Senior Developer",
            Description = "Expert code reviewer",
            SystemPrompt = "You are a senior developer."
        };

        var template = new PromptTemplate
        {
            Name = "Code Review Template",
            TemplateText = "Please review this code: {{code}}",
            ContextTag = "Code_Review"
        };

        persona.AddTemplate(template);

        // Act
        var foundTemplate = persona.GetTemplate("code_review");

        // Assert
        foundTemplate.ShouldNotBeNull();
        foundTemplate.ShouldBe(template);
    }

    /// <summary>
    /// Tests retrieving non-existent template returns null
    /// </summary>
    [Fact]
    public void Should_ReturnNull_When_TemplateNotFound()
    {
        // Arrange
        var persona = new Persona
        {
            Name = "Code Reviewer",
            Role = "Senior Developer",
            Description = "Expert code reviewer",
            SystemPrompt = "You are a senior developer."
        };

        // Act
        var foundTemplate = persona.GetTemplate("non_existent");

        // Assert
        foundTemplate.ShouldBeNull();
    }

    /// <summary>
    /// Tests setting and getting metadata
    /// </summary>
    [Fact]
    public void Should_SetAndGetMetadata_When_ValidKeyValueProvided()
    {
        // Arrange
        var persona = new Persona
        {
            Name = "Test Persona",
            Role = "Tester",
            Description = "Test description",
            SystemPrompt = "Test prompt"
        };
        var originalUpdateTime = persona.UpdatedAt;

        // Act
        persona.SetMetadata("version", "1.0");
        persona.SetMetadata("author", "Test Author");
        var version = persona.GetMetadata<string>("version");
        var author = persona.GetMetadata<string>("author");

        // Assert
        version.ShouldBe("1.0");
        author.ShouldBe("Test Author");
        persona.UpdatedAt.ShouldBeGreaterThan(originalUpdateTime);
    }

    /// <summary>
    /// Tests setting metadata with null key throws exception
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_ThrowException_When_SettingMetadataWithInvalidKey(string invalidKey)
    {
        // Arrange
        var persona = new Persona
        {
            Name = "Test Persona",
            Role = "Tester",
            Description = "Test description",
            SystemPrompt = "Test prompt"
        };

        // Act & Assert
        Should.Throw<ArgumentException>(() => persona.SetMetadata(invalidKey, "value"));
    }

    /// <summary>
    /// Tests getting non-existent metadata returns default value
    /// </summary>
    [Fact]
    public void Should_ReturnDefault_When_MetadataKeyDoesNotExist()
    {
        // Arrange
        var persona = new Persona
        {
            Name = "Test Persona",
            Role = "Tester",
            Description = "Test description",
            SystemPrompt = "Test prompt"
        };

        // Act
        var value = persona.GetMetadata<string>("non_existent");

        // Assert
        value.ShouldBeNull();
    }

    /// <summary>
    /// Tests deactivating persona
    /// </summary>
    [Fact]
    public void Should_DeactivatePersona_When_DeactivateCalled()
    {
        // Arrange
        var persona = new Persona
        {
            Name = "Test Persona",
            Role = "Tester",
            Description = "Test description",
            SystemPrompt = "Test prompt"
        };
        var originalUpdateTime = persona.UpdatedAt;

        // Act
        persona.Deactivate();

        // Assert
        persona.IsActive.ShouldBeFalse();
        persona.UpdatedAt.ShouldBeGreaterThan(originalUpdateTime);
    }

    /// <summary>
    /// Tests activating persona
    /// </summary>
    [Fact]
    public void Should_ActivatePersona_When_ActivateCalled()
    {
        // Arrange
        var persona = new Persona
        {
            Name = "Test Persona",
            Role = "Tester",
            Description = "Test description",
            SystemPrompt = "Test prompt",
            IsActive = false
        };
        var originalUpdateTime = persona.UpdatedAt;

        // Act
        persona.Activate();

        // Assert
        persona.IsActive.ShouldBeTrue();
        persona.UpdatedAt.ShouldBeGreaterThan(originalUpdateTime);
    }

    /// <summary>
    /// Tests persona initialization sets correct default values
    /// </summary>
    [Fact]
    public void Should_SetCorrectDefaults_When_PersonaInitialized()
    {
        // Arrange & Act
        var persona = new Persona();

        // Assert
        persona.Id.ShouldNotBe(Guid.Empty);
        persona.Name.ShouldBe(string.Empty);
        persona.Role.ShouldBe(string.Empty);
        persona.Description.ShouldBe(string.Empty);
        persona.SystemPrompt.ShouldBe(string.Empty);
        persona.IsActive.ShouldBeTrue();
        persona.Traits.ShouldNotBeNull();
        persona.Traits.ShouldBeEmpty();
        persona.KnowledgeDomains.ShouldNotBeNull();
        persona.KnowledgeDomains.ShouldBeEmpty();
        persona.Templates.ShouldNotBeNull();
        persona.Templates.ShouldBeEmpty();
        persona.Metadata.ShouldNotBeNull();
        persona.Metadata.ShouldBeEmpty();
    }
}