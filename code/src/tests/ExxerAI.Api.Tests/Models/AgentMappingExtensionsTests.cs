using ExxerAI.Api.Models;
using ExxerAI.Domain;

namespace ExxerAI.Api.Tests.Models;

/// <summary>
/// Unit tests for AgentMappingExtensions to test mapping logic
/// </summary>
public class AgentMappingExtensionsTests
{
    /// <summary>
    /// Tests ToResponse mapping from Agent domain model to AgentResponse DTO
    /// </summary>
    [Fact]
    public void Should_Map_Agent_To_AgentResponse_When_Valid_Agent_Provided()
    {
        // Arrange
        var capabilities = new AgentCapabilities
        {
            CanProcessNaturalLanguage = true,
            CanGenerateCode = false,
            CanAnalyzeData = true,
            CanCallExternalAPIs = false,
            MaxConcurrentTasks = 5,
            SupportedTaskTypes = new List<string> { "NLP", "DataAnalysis" }
        };

        var configuration = new AgentConfiguration
        {
            TaskTimeoutSeconds = 600,
            MaxRetries = 5,
            Priority = 8,
            CustomProperties = new Dictionary<string, object> { { "apiKey", "test123" } }
        };

        var agent = new Agent
        {
            Id = Guid.NewGuid(),
            Name = "Test Agent",
            Description = "Test Description",
            Status = AgentStatus.Active,
            Capabilities = capabilities,
            Configuration = configuration,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        var response = agent.ToResponse();

        // Assert
        response.ShouldNotBeNull();
        response.Id.ShouldBe(agent.Id);
        response.Name.ShouldBe(agent.Name);
        response.Description.ShouldBe(agent.Description);
        response.Status.ShouldBe(agent.Status);
        response.CreatedAt.ShouldBe(agent.CreatedAt);
        response.UpdatedAt.ShouldBe(agent.UpdatedAt);
        
        // Verify capabilities mapping
        response.Capabilities.ShouldNotBeNull();
        response.Capabilities.CanProcessNaturalLanguage.ShouldBe(capabilities.CanProcessNaturalLanguage);
        response.Capabilities.CanGenerateCode.ShouldBe(capabilities.CanGenerateCode);
        response.Capabilities.CanAnalyzeData.ShouldBe(capabilities.CanAnalyzeData);
        response.Capabilities.CanCallExternalAPIs.ShouldBe(capabilities.CanCallExternalAPIs);
        response.Capabilities.MaxConcurrentTasks.ShouldBe(capabilities.MaxConcurrentTasks);
        response.Capabilities.SupportedTaskTypes.Count.ShouldBe(2);
        response.Capabilities.SupportedTaskTypes.ShouldContain("NLP");
        response.Capabilities.SupportedTaskTypes.ShouldContain("DataAnalysis");

        // Verify configuration mapping
        response.Configuration.ShouldNotBeNull();
        response.Configuration.TaskTimeoutSeconds.ShouldBe(configuration.TaskTimeoutSeconds);
        response.Configuration.MaxRetries.ShouldBe(configuration.MaxRetries);
        response.Configuration.Priority.ShouldBe(configuration.Priority);
        response.Configuration.CustomProperties.Count.ShouldBe(1);
        response.Configuration.CustomProperties["apiKey"].ShouldBe("test123");
    }

    /// <summary>
    /// Tests ToDto mapping from AgentCapabilities domain model to AgentCapabilitiesDto
    /// </summary>
    [Fact]
    public void Should_Map_AgentCapabilities_To_AgentCapabilitiesDto_When_Valid_Capabilities_Provided()
    {
        // Arrange
        var capabilities = new AgentCapabilities
        {
            CanProcessNaturalLanguage = false,
            CanGenerateCode = true,
            CanAnalyzeData = true,
            CanCallExternalAPIs = true,
            MaxConcurrentTasks = 10,
            SupportedTaskTypes = new List<string> { "CodeGen", "API", "DataProc" }
        };

        // Act
        var dto = capabilities.ToDto();

        // Assert
        dto.ShouldNotBeNull();
        dto.CanProcessNaturalLanguage.ShouldBe(capabilities.CanProcessNaturalLanguage);
        dto.CanGenerateCode.ShouldBe(capabilities.CanGenerateCode);
        dto.CanAnalyzeData.ShouldBe(capabilities.CanAnalyzeData);
        dto.CanCallExternalAPIs.ShouldBe(capabilities.CanCallExternalAPIs);
        dto.MaxConcurrentTasks.ShouldBe(capabilities.MaxConcurrentTasks);
        dto.SupportedTaskTypes.Count.ShouldBe(3);
        dto.SupportedTaskTypes.ShouldContain("CodeGen");
        dto.SupportedTaskTypes.ShouldContain("API");
        dto.SupportedTaskTypes.ShouldContain("DataProc");
    }

    /// <summary>
    /// Tests ToDto mapping from AgentCapabilities with null SupportedTaskTypes
    /// </summary>
    [Fact]
    public void Should_Map_AgentCapabilities_To_AgentCapabilitiesDto_When_SupportedTaskTypes_Is_Null()
    {
        // Arrange
        var capabilities = new AgentCapabilities
        {
            CanProcessNaturalLanguage = true,
            CanGenerateCode = false,
            CanAnalyzeData = false,
            CanCallExternalAPIs = false,
            MaxConcurrentTasks = 2,
            SupportedTaskTypes = null
        };

        // Act
        var dto = capabilities.ToDto();

        // Assert
        dto.ShouldNotBeNull();
        dto.SupportedTaskTypes.ShouldNotBeNull();
        dto.SupportedTaskTypes.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests ToDto mapping from AgentConfiguration domain model to AgentConfigurationDto
    /// </summary>
    [Fact]
    public void Should_Map_AgentConfiguration_To_AgentConfigurationDto_When_Valid_Configuration_Provided()
    {
        // Arrange
        var configuration = new AgentConfiguration
        {
            TaskTimeoutSeconds = 1200,
            MaxRetries = 10,
            Priority = 3,
            CustomProperties = new Dictionary<string, object>
            {
                { "endpoint", "https://api.test.com" },
                { "timeout", 5000 },
                { "enabled", true }
            }
        };

        // Act
        var dto = configuration.ToDto();

        // Assert
        dto.ShouldNotBeNull();
        dto.TaskTimeoutSeconds.ShouldBe(configuration.TaskTimeoutSeconds);
        dto.MaxRetries.ShouldBe(configuration.MaxRetries);
        dto.Priority.ShouldBe(configuration.Priority);
        dto.CustomProperties.Count.ShouldBe(3);
        dto.CustomProperties["endpoint"].ShouldBe("https://api.test.com");
        dto.CustomProperties["timeout"].ShouldBe(5000);
        dto.CustomProperties["enabled"].ShouldBe(true);
    }

    /// <summary>
    /// Tests ToDto mapping from AgentConfiguration with null CustomProperties
    /// </summary>
    [Fact]
    public void Should_Map_AgentConfiguration_To_AgentConfigurationDto_When_CustomProperties_Is_Null()
    {
        // Arrange
        var configuration = new AgentConfiguration
        {
            TaskTimeoutSeconds = 300,
            MaxRetries = 3,
            Priority = 1,
            CustomProperties = null
        };

        // Act
        var dto = configuration.ToDto();

        // Assert
        dto.ShouldNotBeNull();
        dto.CustomProperties.ShouldNotBeNull();
        dto.CustomProperties.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests ToDomain mapping from AgentCapabilitiesDto to AgentCapabilities domain model
    /// </summary>
    [Fact]
    public void Should_Map_AgentCapabilitiesDto_To_AgentCapabilities_When_Valid_Dto_Provided()
    {
        // Arrange
        var dto = new AgentCapabilitiesDto
        {
            CanProcessNaturalLanguage = true,
            CanGenerateCode = true,
            CanAnalyzeData = false,
            CanCallExternalAPIs = true,
            MaxConcurrentTasks = 7,
            SupportedTaskTypes = new List<string> { "NLP", "CodeGen", "API" }
        };

        // Act
        var domain = dto.ToDomain();

        // Assert
        domain.ShouldNotBeNull();
        domain.CanProcessNaturalLanguage.ShouldBe(dto.CanProcessNaturalLanguage);
        domain.CanGenerateCode.ShouldBe(dto.CanGenerateCode);
        domain.CanAnalyzeData.ShouldBe(dto.CanAnalyzeData);
        domain.CanCallExternalAPIs.ShouldBe(dto.CanCallExternalAPIs);
        domain.MaxConcurrentTasks.ShouldBe(dto.MaxConcurrentTasks);
        domain.SupportedTaskTypes.Count.ShouldBe(3);
        domain.SupportedTaskTypes.ShouldContain("NLP");
        domain.SupportedTaskTypes.ShouldContain("CodeGen");
        domain.SupportedTaskTypes.ShouldContain("API");
    }

    /// <summary>
    /// Tests ToDomain mapping from AgentCapabilitiesDto with null SupportedTaskTypes
    /// </summary>
    [Fact]
    public void Should_Map_AgentCapabilitiesDto_To_AgentCapabilities_When_SupportedTaskTypes_Is_Null()
    {
        // Arrange
        var dto = new AgentCapabilitiesDto
        {
            CanProcessNaturalLanguage = false,
            CanGenerateCode = false,
            CanAnalyzeData = true,
            CanCallExternalAPIs = false,
            MaxConcurrentTasks = 1,
            SupportedTaskTypes = null
        };

        // Act
        var domain = dto.ToDomain();

        // Assert
        domain.ShouldNotBeNull();
        domain.SupportedTaskTypes.ShouldNotBeNull();
        domain.SupportedTaskTypes.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests ToDomain mapping from UpdateAgentConfigurationRequest to AgentConfiguration
    /// </summary>
    [Fact]
    public void Should_Map_UpdateAgentConfigurationRequest_To_AgentConfiguration_When_Valid_Request_Provided()
    {
        // Arrange
        var request = new UpdateAgentConfigurationRequest
        {
            TaskTimeoutSeconds = 1800,
            MaxRetries = 8,
            Priority = 6,
            CustomProperties = new Dictionary<string, object>
            {
                { "authToken", "abc123" },
                { "retryDelay", 2000 }
            }
        };

        // Act
        var domain = request.ToDomain();

        // Assert
        domain.ShouldNotBeNull();
        domain.TaskTimeoutSeconds.ShouldBe(request.TaskTimeoutSeconds);
        domain.MaxRetries.ShouldBe(request.MaxRetries);
        domain.Priority.ShouldBe(request.Priority);
        domain.CustomProperties.Count.ShouldBe(2);
        domain.CustomProperties["authToken"].ShouldBe("abc123");
        domain.CustomProperties["retryDelay"].ShouldBe(2000);
    }

    /// <summary>
    /// Tests ToDomain mapping from UpdateAgentConfigurationRequest with null CustomProperties
    /// </summary>
    [Fact]
    public void Should_Map_UpdateAgentConfigurationRequest_To_AgentConfiguration_When_CustomProperties_Is_Null()
    {
        // Arrange
        var request = new UpdateAgentConfigurationRequest
        {
            TaskTimeoutSeconds = 450,
            MaxRetries = 2,
            Priority = 4,
            CustomProperties = null
        };

        // Act
        var domain = request.ToDomain();

        // Assert
        domain.ShouldNotBeNull();
        domain.CustomProperties.ShouldNotBeNull();
        domain.CustomProperties.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests mapping preserves all boolean combinations for capabilities
    /// </summary>
    [Theory]
    [InlineData(true, true, true, true)]
    [InlineData(false, false, false, false)]
    [InlineData(true, false, true, false)]
    [InlineData(false, true, false, true)]
    public void Should_Preserve_Boolean_Values_When_Mapping_AgentCapabilities_Bidirectionally(
        bool canProcessNL, bool canGenerateCode, bool canAnalyzeData, bool canCallAPIs)
    {
        // Arrange
        var originalCapabilities = new AgentCapabilities
        {
            CanProcessNaturalLanguage = canProcessNL,
            CanGenerateCode = canGenerateCode,
            CanAnalyzeData = canAnalyzeData,
            CanCallExternalAPIs = canCallAPIs,
            MaxConcurrentTasks = 3,
            SupportedTaskTypes = new List<string> { "Test" }
        };

        // Act - Round trip mapping
        var dto = originalCapabilities.ToDto();
        var roundTripDomain = dto.ToDomain();

        // Assert
        roundTripDomain.CanProcessNaturalLanguage.ShouldBe(canProcessNL);
        roundTripDomain.CanGenerateCode.ShouldBe(canGenerateCode);
        roundTripDomain.CanAnalyzeData.ShouldBe(canAnalyzeData);
        roundTripDomain.CanCallExternalAPIs.ShouldBe(canCallAPIs);
    }

    /// <summary>
    /// Tests mapping with empty collections
    /// </summary>
    [Fact]
    public void Should_Handle_Empty_Collections_When_Mapping_AgentCapabilities()
    {
        // Arrange
        var capabilities = new AgentCapabilities
        {
            CanProcessNaturalLanguage = true,
            CanGenerateCode = false,
            CanAnalyzeData = true,
            CanCallExternalAPIs = false,
            MaxConcurrentTasks = 4,
            SupportedTaskTypes = new List<string>() // Empty but not null
        };

        // Act
        var dto = capabilities.ToDto();
        var roundTrip = dto.ToDomain();

        // Assert
        dto.SupportedTaskTypes.ShouldBeEmpty();
        roundTrip.SupportedTaskTypes.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests mapping with empty dictionaries
    /// </summary>
    [Fact]
    public void Should_Handle_Empty_Dictionaries_When_Mapping_AgentConfiguration()
    {
        // Arrange
        var configuration = new AgentConfiguration
        {
            TaskTimeoutSeconds = 500,
            MaxRetries = 4,
            Priority = 2,
            CustomProperties = new Dictionary<string, object>() // Empty but not null
        };

        // Act
        var dto = configuration.ToDto();

        // Assert
        dto.CustomProperties.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests that mapping creates new collection instances (not references)
    /// </summary>
    [Fact]
    public void Should_Create_New_Collection_Instances_When_Mapping_AgentCapabilities()
    {
        // Arrange
        var originalTaskTypes = new List<string> { "Original" };
        var capabilities = new AgentCapabilities
        {
            SupportedTaskTypes = originalTaskTypes
        };

        // Act
        var dto = capabilities.ToDto();
        dto.SupportedTaskTypes.Add("Added to DTO");

        // Assert
        originalTaskTypes.Count.ShouldBe(1); // Original collection should be unchanged
        dto.SupportedTaskTypes.Count.ShouldBe(2);
    }

    /// <summary>
    /// Tests that mapping creates new dictionary instances (not references)
    /// </summary>
    [Fact]
    public void Should_Create_New_Dictionary_Instances_When_Mapping_AgentConfiguration()
    {
        // Arrange
        var originalProperties = new Dictionary<string, object> { { "Original", "Value" } };
        var configuration = new AgentConfiguration
        {
            CustomProperties = originalProperties
        };

        // Act
        var dto = configuration.ToDto();
        dto.CustomProperties.Add("Added", "ToDTO");

        // Assert
        originalProperties.Count.ShouldBe(1); // Original dictionary should be unchanged
        dto.CustomProperties.Count.ShouldBe(2);
    }
} 