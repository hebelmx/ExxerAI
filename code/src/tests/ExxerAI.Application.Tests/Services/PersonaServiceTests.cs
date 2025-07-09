using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.Operations;
using NSubstitute;
using Xunit;

namespace ExxerAI.Application.Tests.Services;

/// <summary>
/// Unit tests for PersonaService
/// </summary>
public class PersonaServiceTests
{
    private readonly IPersonaRepository _personaRepository;
    private readonly IPromptTemplateRepository _promptTemplateRepository;
    private readonly PersonaService _personaService;

    /// <summary>
    /// Initializes test dependencies
    /// </summary>
    public PersonaServiceTests()
    {
        _personaRepository = Substitute.For<IPersonaRepository>();
        _promptTemplateRepository = Substitute.For<IPromptTemplateRepository>();
        _personaService = new PersonaService(_personaRepository, _promptTemplateRepository);
    }

    /// <summary>
    /// Tests successful persona creation
    /// </summary>
    [Fact]
    public async Task CreatePersonaAsync_Should_ReturnSuccess_When_ValidDataProvided()
    {
        // Arrange
        var name = "Technical Expert";
        var role = "Software Architect";
        var description = "Expert in software design patterns";
        var systemPrompt = "You are a technical expert specializing in software architecture.";
        var traits = new Dictionary<string, string> { { "tone", "professional" } };
        var domains = new List<string> { "software_architecture" };

        var expectedPersona = new Persona
        {
            Id = Guid.NewGuid(),
            Name = name,
            Role = role,
            Description = description,
            SystemPrompt = systemPrompt
        };

        _personaRepository.AddAsync(Arg.Any<Persona>(), Arg.Any<CancellationToken>())
            .Returns(Result<Persona>.WithSuccess(expectedPersona));

        // Act
        var result = await _personaService.CreatePersonaAsync(
            name, role, description, systemPrompt, traits, domains, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ShouldNotBeNull();
        result.Value!.Name.ShouldBe(name);
        result.Value!.Role.ShouldBe(role);
        result.Value!.Description.ShouldBe(description);
        result.Value!.SystemPrompt.ShouldBe(systemPrompt);

        await _personaRepository.Received(1)
            .AddAsync(Arg.Any<Persona>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests persona creation with null or empty name fails
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreatePersonaAsync_Should_ReturnFailure_When_NameIsInvalid(string? invalidName)
    {
        // Arrange
        var role = "Test Role";
        var description = "Test description";
        var systemPrompt = "Test prompt";

        // Act
        var result = await _personaService.CreatePersonaAsync(
            invalidName!, role, description, systemPrompt, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("name cannot be empty");

        await _personaRepository.DidNotReceive()
            .AddAsync(Arg.Any<Persona>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests persona creation with null or empty role fails
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreatePersonaAsync_Should_ReturnFailure_When_RoleIsInvalid(string? invalidRole)
    {
        // Arrange
        var name = "Test Name";
        var description = "Test description";
        var systemPrompt = "Test prompt";

        // Act
        var result = await _personaService.CreatePersonaAsync(
            name, invalidRole!, description, systemPrompt, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("role cannot be empty");

        await _personaRepository.DidNotReceive()
            .AddAsync(Arg.Any<Persona>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests persona creation with repository failure
    /// </summary>
    [Fact]
    public async Task CreatePersonaAsync_Should_ReturnFailure_When_RepositoryFails()
    {
        // Arrange
        var name = "Test Name";
        var role = "Test Role";
        var description = "Test description";
        var systemPrompt = "Test prompt";

        _personaRepository.AddAsync(Arg.Any<Persona>(), Arg.Any<CancellationToken>())
            .Returns(Result<Persona>.WithFailure("Database error"));

        // Act
        var result = await _personaService.CreatePersonaAsync(
            name, role, description, systemPrompt, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Failed to create persona");
    }

    /// <summary>
    /// Tests successful persona update
    /// </summary>
    [Fact]
    public async Task UpdatePersonaAsync_Should_ReturnSuccess_When_ValidDataProvided()
    {
        // Arrange
        var personaId = Guid.NewGuid();
        var existingPersona = new Persona
        {
            Id = personaId,
            Name = "Old Name",
            Role = "Old Role",
            Description = "Old description",
            SystemPrompt = "Old prompt"
        };

        var updatedPersona = new Persona
        {
            Id = personaId,
            Name = "New Name",
            Role = "New Role",
            Description = "New description",
            SystemPrompt = "New prompt"
        };

        _personaRepository.GetByIdAsync(personaId, Arg.Any<CancellationToken>())
            .Returns(Result<Persona>.WithSuccess(existingPersona));
        _personaRepository.UpdateAsync(Arg.Any<Persona>(), Arg.Any<CancellationToken>())
            .Returns(Result<Persona>.WithSuccess(updatedPersona));

        // Act
        var result = await _personaService.UpdatePersonaAsync(
            personaId, "New Name", "New Role", "New description", "New prompt", cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ShouldNotBeNull();

        await _personaRepository.Received(1)
            .GetByIdAsync(personaId, Arg.Any<CancellationToken>());
        await _personaRepository.Received(1)
            .UpdateAsync(Arg.Any<Persona>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests update with invalid persona ID fails
    /// </summary>
    [Fact]
    public async Task UpdatePersonaAsync_Should_ReturnFailure_When_PersonaIdIsEmpty()
    {
        // Act
        var result = await _personaService.UpdatePersonaAsync(Guid.Empty, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Invalid persona identifier");

        await _personaRepository.DidNotReceive()
            .GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests getting persona by ID successfully
    /// </summary>
    [Fact]
    public async Task GetPersonaByIdAsync_Should_ReturnSuccess_When_PersonaExists()
    {
        // Arrange
        var personaId = Guid.NewGuid();
        var expectedPersona = new Persona
        {
            Id = personaId,
            Name = "Test Persona",
            Role = "Test Role"
        };

        _personaRepository.GetByIdAsync(personaId, Arg.Any<CancellationToken>())
            .Returns(Result<Persona>.WithSuccess(expectedPersona));

        // Act
        var result = await _personaService.GetPersonaByIdAsync(personaId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ShouldBe(expectedPersona);

        await _personaRepository.Received(1)
            .GetByIdAsync(personaId, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests getting active personas successfully
    /// </summary>
    [Fact]
    public async Task GetActivePersonasAsync_Should_ReturnSuccess_When_PersonasExist()
    {
        // Arrange
        var personas = new List<Persona>
        {
            new() { Id = Guid.NewGuid(), Name = "Persona 1", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Persona 2", IsActive = true }
        };

        _personaRepository.GetActivePersonasAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Persona>>.WithSuccess(personas));

        // Act
        var result = await _personaService.GetActivePersonasAsync(TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Count().ShouldBe(2);

        await _personaRepository.Received(1)
            .GetActivePersonasAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests adding trait to persona successfully
    /// </summary>
    [Fact]
    public async Task AddPersonaTraitAsync_Should_ReturnSuccess_When_ValidDataProvided()
    {
        // Arrange
        var personaId = Guid.NewGuid();
        var persona = new Persona
        {
            Id = personaId,
            Name = "Test Persona",
            Role = "Test Role"
        };

        _personaRepository.GetByIdAsync(personaId, Arg.Any<CancellationToken>())
            .Returns(Result<Persona>.WithSuccess(persona));
        _personaRepository.UpdateAsync(Arg.Any<Persona>(), Arg.Any<CancellationToken>())
            .Returns(Result<Persona>.WithSuccess(persona));

        // Act
        var result = await _personaService.AddPersonaTraitAsync(
            personaId, "tone", "professional", cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ShouldBeTrue();
        persona.Traits.ShouldContainKey("tone");
        persona.Traits["tone"].ShouldBe("professional");

        await _personaRepository.Received(1)
            .UpdateAsync(Arg.Any<Persona>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests adding trait with invalid persona ID fails
    /// </summary>
    [Fact]
    public async Task AddPersonaTraitAsync_Should_ReturnFailure_When_PersonaIdIsEmpty()
    {
        // Act
        var result = await _personaService.AddPersonaTraitAsync(
            Guid.Empty, "tone", "professional", cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Invalid persona identifier");
    }

    /// <summary>
    /// Tests adding trait with empty key fails
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AddPersonaTraitAsync_Should_ReturnFailure_When_TraitKeyIsInvalid(string? invalidKey)
    {
        // Arrange
        var personaId = Guid.NewGuid();

        // Act
        var result = await _personaService.AddPersonaTraitAsync(
            personaId, invalidKey!, "value", cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Trait key cannot be empty");
    }

    /// <summary>
    /// Tests removing trait from persona successfully
    /// </summary>
    [Fact]
    public async Task RemovePersonaTraitAsync_Should_ReturnSuccess_When_TraitExists()
    {
        // Arrange
        var personaId = Guid.NewGuid();
        var persona = new Persona
        {
            Id = personaId,
            Name = "Test Persona",
            Role = "Test Role"
        };
        persona.AddTrait("tone", "professional");

        _personaRepository.GetByIdAsync(personaId, Arg.Any<CancellationToken>())
            .Returns(Result<Persona>.WithSuccess(persona));
        _personaRepository.UpdateAsync(Arg.Any<Persona>(), Arg.Any<CancellationToken>())
            .Returns(Result<Persona>.WithSuccess(persona));

        // Act
        var result = await _personaService.RemovePersonaTraitAsync(
            personaId, "tone", cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ShouldBeTrue();
        persona.Traits.ShouldNotContainKey("tone");

        await _personaRepository.Received(1)
            .UpdateAsync(Arg.Any<Persona>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests adding knowledge domain to persona successfully
    /// </summary>
    [Fact]
    public async Task AddKnowledgeDomainAsync_Should_ReturnSuccess_When_ValidDomainProvided()
    {
        // Arrange
        var personaId = Guid.NewGuid();
        var persona = new Persona
        {
            Id = personaId,
            Name = "AI Expert",
            Role = "Value Scientist"
        };

        _personaRepository.GetByIdAsync(personaId, Arg.Any<CancellationToken>())
            .Returns(Result<Persona>.WithSuccess(persona));
        _personaRepository.UpdateAsync(Arg.Any<Persona>(), Arg.Any<CancellationToken>())
            .Returns(Result<Persona>.WithSuccess(persona));

        // Act
        var result = await _personaService.AddKnowledgeDomainAsync(
            personaId, "machine_learning", cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ShouldBeTrue();
        persona.KnowledgeDomains.ShouldContain("machine_learning");

        await _personaRepository.Received(1)
            .UpdateAsync(Arg.Any<Persona>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests removing knowledge domain from persona successfully
    /// </summary>
    [Fact]
    public async Task RemoveKnowledgeDomainAsync_Should_ReturnSuccess_When_DomainExists()
    {
        // Arrange
        var personaId = Guid.NewGuid();
        var persona = new Persona
        {
            Id = personaId,
            Name = "AI Expert",
            Role = "Value Scientist"
        };
        persona.AddKnowledgeDomain("machine_learning");

        _personaRepository.GetByIdAsync(personaId, Arg.Any<CancellationToken>())
            .Returns(Result<Persona>.WithSuccess(persona));
        _personaRepository.UpdateAsync(Arg.Any<Persona>(), Arg.Any<CancellationToken>())
            .Returns(Result<Persona>.WithSuccess(persona));

        // Act
        var result = await _personaService.RemoveKnowledgeDomainAsync(
            personaId, "machine_learning", cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ShouldBeTrue();
        persona.KnowledgeDomains.ShouldNotContain("machine_learning");

        await _personaRepository.Received(1)
            .UpdateAsync(Arg.Any<Persona>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests creating template for persona successfully
    /// </summary>
    [Fact]
    public async Task CreateTemplateForPersonaAsync_Should_ReturnSuccess_When_ValidDataProvided()
    {
        // Arrange
        var personaId = Guid.NewGuid();
        var persona = new Persona
        {
            Id = personaId,
            Name = "Code Reviewer",
            Role = "Senior Developer"
        };

        var expectedTemplate = new PromptTemplate
        {
            Id = Guid.NewGuid(),
            Name = "Code Review Template",
            TemplateText = "Please review this {{language}} code: {{code}}",
            ContextTag = "code_review",
            PersonaId = personaId
        };

        _personaRepository.GetByIdAsync(personaId, Arg.Any<CancellationToken>())
            .Returns(Result<Persona>.WithSuccess(persona));
        _promptTemplateRepository.AddAsync(Arg.Any<PromptTemplate>(), Arg.Any<CancellationToken>())
            .Returns(Result<PromptTemplate>.WithSuccess(expectedTemplate));
        _personaRepository.UpdateAsync(Arg.Any<Persona>(), Arg.Any<CancellationToken>())
            .Returns(Result<Persona>.WithSuccess(persona));

        // Act
        var result = await _personaService.CreateTemplateForPersonaAsync(
            personaId,
            "Code Review Template",
            "Please review this {{language}} code: {{code}}",
            "code_review", cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ShouldNotBeNull();
        result.Value!.Name.ShouldBe("Code Review Template");
        result.Value!.ContextTag.ShouldBe("code_review");
        result.Value!.PersonaId.ShouldBe(personaId);

        await _promptTemplateRepository.Received(1)
            .AddAsync(Arg.Any<PromptTemplate>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests creating template with invalid template name fails
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateTemplateForPersonaAsync_Should_ReturnFailure_When_TemplateNameIsInvalid(string? invalidName)
    {
        // Arrange
        var personaId = Guid.NewGuid();

        // Act
        var result = await _personaService.CreateTemplateForPersonaAsync(
            personaId, invalidName!, "Template text", "context", cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Template name cannot be empty");

        await _promptTemplateRepository.DidNotReceive()
            .AddAsync(Arg.Any<PromptTemplate>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests getting persona templates successfully
    /// </summary>
    [Fact]
    public async Task GetPersonaTemplatesAsync_Should_ReturnSuccess_When_TemplatesExist()
    {
        // Arrange
        var personaId = Guid.NewGuid();
        var templates = new List<PromptTemplate>
        {
            new() { Id = Guid.NewGuid(), Name = "Template 1", PersonaId = personaId },
            new() { Id = Guid.NewGuid(), Name = "Template 2", PersonaId = personaId }
        };

        _promptTemplateRepository.GetByPersonaIdAsync(personaId, false, Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<PromptTemplate>>.WithSuccess(templates));

        // Act
        var result = await _personaService.GetPersonaTemplatesAsync(personaId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.Count().ShouldBe(2);

        await _promptTemplateRepository.Received(1)
            .GetByPersonaIdAsync(personaId, false, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests activating persona successfully
    /// </summary>
    [Fact]
    public async Task ActivatePersonaAsync_Should_ReturnSuccess_When_PersonaExists()
    {
        // Arrange
        var personaId = Guid.NewGuid();
        var persona = new Persona
        {
            Id = personaId,
            Name = "Test Persona",
            IsActive = false
        };

        _personaRepository.GetByIdAsync(personaId, Arg.Any<CancellationToken>())
            .Returns(Result<Persona>.WithSuccess(persona));
        _personaRepository.UpdateAsync(Arg.Any<Persona>(), Arg.Any<CancellationToken>())
            .Returns(Result<Persona>.WithSuccess(persona));

        // Act
        var result = await _personaService.ActivatePersonaAsync(personaId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ShouldBeTrue();
        persona.IsActive.ShouldBeTrue();

        await _personaRepository.Received(1)
            .UpdateAsync(Arg.Any<Persona>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests deactivating persona successfully
    /// </summary>
    [Fact]
    public async Task DeactivatePersonaAsync_Should_ReturnSuccess_When_PersonaExists()
    {
        // Arrange
        var personaId = Guid.NewGuid();
        var persona = new Persona
        {
            Id = personaId,
            Name = "Test Persona",
            IsActive = true
        };

        _personaRepository.GetByIdAsync(personaId, Arg.Any<CancellationToken>())
            .Returns(Result<Persona>.WithSuccess(persona));
        _personaRepository.UpdateAsync(Arg.Any<Persona>(), Arg.Any<CancellationToken>())
            .Returns(Result<Persona>.WithSuccess(persona));

        // Act
        var result = await _personaService.DeactivatePersonaAsync(personaId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ShouldBeTrue();
        persona.IsActive.ShouldBeFalse();

        await _personaRepository.Received(1)
            .UpdateAsync(Arg.Any<Persona>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests deleting persona successfully
    /// </summary>
    [Fact]
    public async Task DeletePersonaAsync_Should_ReturnSuccess_When_PersonaExists()
    {
        // Arrange
        var personaId = Guid.NewGuid();
        var templates = new List<PromptTemplate>
        {
            new() { Id = Guid.NewGuid(), PersonaId = personaId }
        };

        _promptTemplateRepository.GetByPersonaIdAsync(personaId, true, Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<PromptTemplate>>.WithSuccess(templates));
        _promptTemplateRepository.DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result<bool>.WithSuccess(true));
        _personaRepository.DeleteAsync(personaId, Arg.Any<CancellationToken>())
            .Returns(Result<bool>.WithSuccess(true));

        // Act
        var result = await _personaService.DeletePersonaAsync(personaId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ShouldBeTrue();

        await _promptTemplateRepository.Received(1)
            .DeleteAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _personaRepository.Received(1)
            .DeleteAsync(personaId, Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests validating persona successfully
    /// </summary>
    [Fact]
    public async Task ValidatePersonaAsync_Should_ReturnValidationResult_When_PersonaExists()
    {
        // Arrange
        var personaId = Guid.NewGuid();
        var persona = new Persona
        {
            Id = personaId,
            Name = "Test Persona",
            Role = "Test Role",
            SystemPrompt = "You are a test persona with comprehensive capabilities."
        };
        persona.AddTrait("tone", "professional");
        persona.AddKnowledgeDomain("testing");

        var templates = new List<PromptTemplate>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Test Template",
                TemplateText = "Test {{param}}",
                ContextTag = "test",
                PersonaId = personaId
            }
        };

        _personaRepository.GetByIdAsync(personaId, Arg.Any<CancellationToken>())
            .Returns(Result<Persona>.WithSuccess(persona));
        _promptTemplateRepository.GetByPersonaIdAsync(personaId, true, Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<PromptTemplate>>.WithSuccess(templates));

        // Act
        var result = await _personaService.ValidatePersonaAsync(personaId, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ShouldNotBeNull();
        result.Value!.IsValid.ShouldBeTrue();
        result.Value!.Errors.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests search personas with criteria successfully
    /// </summary>
    [Fact]
    public async Task SearchPersonasAsync_Should_ReturnSuccess_When_ValidCriteriaProvided()
    {
        // Arrange
        var searchCriteria = new PersonaSearchCriteria
        {
            Role = "Developer",
            RequiredKnowledgeDomains = ["software_development"],
            IncludeInactive = false
        };

        var personas = new List<Persona>
        {
            new() { Id = Guid.NewGuid(), Name = "Developer 1", Role = "Developer" },
            new() { Id = Guid.NewGuid(), Name = "Developer 2", Role = "Developer" }
        };

        // Mock all repository methods that will be called
        _personaRepository.GetActivePersonasAsync(Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Persona>>.WithSuccess(personas));

        _personaRepository.GetByRoleAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Persona>>.WithSuccess(personas));

        _personaRepository.FindByKnowledgeDomainsAsync(
            Arg.Any<IEnumerable<string>>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<Persona>>.WithSuccess(personas));

        // Act
        var result = await _personaService.SearchPersonasAsync(searchCriteria, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ShouldNotBeNull();

        await _personaRepository.Received(1)
            .GetActivePersonasAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests search personas with null criteria fails
    /// </summary>
    [Fact]
    public async Task SearchPersonasAsync_Should_ReturnFailure_When_CriteriaIsNull()
    {
        // Act
        var result = await _personaService.SearchPersonasAsync(null!, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Error!.ShouldContain("Search criteria cannot be null");

        await _personaRepository.DidNotReceive()
            .GetActivePersonasAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Tests constructor with null persona repository throws exception
    /// </summary>
    [Fact]
    public void Constructor_Should_ThrowException_When_PersonaRepositoryIsNull()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new PersonaService(null!, _promptTemplateRepository));
    }

    /// <summary>
    /// Tests constructor with null prompt template repository throws exception
    /// </summary>
    [Fact]
    public void Constructor_Should_ThrowException_When_PromptTemplateRepositoryIsNull()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new PersonaService(_personaRepository, null!));
    }
}