namespace ExxerAI.Api.Tests.Models;

/// <summary>
/// Unit tests for mapping extension methods
/// </summary>
public class MappingExtensionTests
{
    /// <summary>
    /// Tests ToResponse extension method mapping logic
    /// </summary>
    [Fact]
    public void Should_Map_Agent_To_AgentResponse_When_ToResponse_Called()
    {
        // Arrange
        var capabilities = new AgentCapabilities
        {
            CanProcessNaturalLanguage = true,
            CanGenerateCode = false,
            MaxConcurrentTasks = 5,
            SupportedTaskTypes = ["NLP"]
        };

        var configuration = new AgentConfiguration
        {
            TaskTimeoutSeconds = 600,
            MaxRetries = 5,
            Priority = 8,
            CustomProperties = new Dictionary<string, object> { { "key", "value" } }
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
        response.Capabilities.ShouldNotBeNull();
        response.Configuration.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests ToDto extension method for AgentCapabilities
    /// </summary>
    [Fact]
    public void Should_Map_AgentCapabilities_To_Dto_When_ToDto_Called()
    {
        // Arrange
        var capabilities = new AgentCapabilities
        {
            CanProcessNaturalLanguage = false,
            CanGenerateCode = true,
            CanAnalyzeData = true,
            CanCallExternalAPIs = true,
            MaxConcurrentTasks = 10,
            SupportedTaskTypes = ["CodeGen", "API"]
        };

        // Act
        var dto = capabilities.ToDto();

        // Assert
        dto.ShouldNotBeNull();
        dto.CanProcessNaturalLanguage.ShouldBe(false);
        dto.CanGenerateCode.ShouldBe(true);
        dto.CanAnalyzeData.ShouldBe(true);
        dto.CanCallExternalAPIs.ShouldBe(true);
        dto.MaxConcurrentTasks.ShouldBe(10);
        dto.SupportedTaskTypes.Count.ShouldBe(2);
        dto.SupportedTaskTypes.ShouldContain("CodeGen");
        dto.SupportedTaskTypes.ShouldContain("API");
    }

    /// <summary>
    /// Tests ToDto extension method for AgentCapabilities with null SupportedTaskTypes
    /// </summary>
    [Fact]
    public void Should_Handle_Null_SupportedTaskTypes_When_Mapping_AgentCapabilities_ToDto()
    {
        // Arrange
        var capabilities = new AgentCapabilities
        {
            CanProcessNaturalLanguage = true,
            MaxConcurrentTasks = 2,
            SupportedTaskTypes = null!
        };

        // Act
        var dto = capabilities.ToDto();

        // Assert
        dto.ShouldNotBeNull();
        dto.SupportedTaskTypes.ShouldNotBeNull();
        dto.SupportedTaskTypes.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests ToDto extension method for AgentConfiguration
    /// </summary>
    [Fact]
    public void Should_Map_AgentConfiguration_To_Dto_When_ToDto_Called()
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
                { "timeout", 5000 }
            }
        };

        // Act
        var dto = configuration.ToDto();

        // Assert
        dto.ShouldNotBeNull();
        dto.TaskTimeoutSeconds.ShouldBe(1200);
        dto.MaxRetries.ShouldBe(10);
        dto.Priority.ShouldBe(3);
        dto.CustomProperties.Count.ShouldBe(2);
        dto.CustomProperties["endpoint"].ShouldBe("https://api.test.com");
        dto.CustomProperties["timeout"].ShouldBe(5000);
    }

    /// <summary>
    /// Tests ToDto extension method for AgentConfiguration with null CustomProperties
    /// </summary>
    [Fact]
    public void Should_Handle_Null_CustomProperties_When_Mapping_AgentConfiguration_ToDto()
    {
        // Arrange
        var configuration = new AgentConfiguration
        {
            TaskTimeoutSeconds = 300,
            MaxRetries = 3,
            Priority = 1,
            CustomProperties = null!
        };

        // Act
        var dto = configuration.ToDto();

        // Assert
        dto.ShouldNotBeNull();
        dto.CustomProperties.ShouldNotBeNull();
        dto.CustomProperties.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests ToDomain extension method for AgentCapabilitiesDto
    /// </summary>
    [Fact]
    public void Should_Map_AgentCapabilitiesDto_To_Domain_When_ToDomain_Called()
    {
        // Arrange
        var dto = new AgentCapabilitiesDto
        {
            CanProcessNaturalLanguage = true,
            CanGenerateCode = true,
            CanAnalyzeData = false,
            CanCallExternalAPIs = true,
            MaxConcurrentTasks = 7,
            SupportedTaskTypes = ["NLP", "CodeGen"]
        };

        // Act
        var domain = dto.ToDomain();

        // Assert
        domain.ShouldNotBeNull();
        domain.CanProcessNaturalLanguage.ShouldBe(true);
        domain.CanGenerateCode.ShouldBe(true);
        domain.CanAnalyzeData.ShouldBe(false);
        domain.CanCallExternalAPIs.ShouldBe(true);
        domain.MaxConcurrentTasks.ShouldBe(7);
        domain.SupportedTaskTypes.Count.ShouldBe(2);
    }

    /// <summary>
    /// Tests ToDomain extension method for AgentCapabilitiesDto with null SupportedTaskTypes
    /// </summary>
    [Fact]
    public void Should_Handle_Null_SupportedTaskTypes_When_Mapping_AgentCapabilitiesDto_ToDomain()
    {
        // Arrange
        var dto = new AgentCapabilitiesDto
        {
            CanProcessNaturalLanguage = false,
            MaxConcurrentTasks = 1,
            SupportedTaskTypes = null!
        };

        // Act
        var domain = dto.ToDomain();

        // Assert
        domain.ShouldNotBeNull();
        domain.SupportedTaskTypes.ShouldNotBeNull();
        domain.SupportedTaskTypes.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests ToDomain extension method for UpdateAgentConfigurationRequest
    /// </summary>
    [Fact]
    public void Should_Map_UpdateAgentConfigurationRequest_To_Domain_When_ToDomain_Called()
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
        domain.TaskTimeoutSeconds.ShouldBe(1800);
        domain.MaxRetries.ShouldBe(8);
        domain.Priority.ShouldBe(6);
        domain.CustomProperties.Count.ShouldBe(2);
        domain.CustomProperties["authToken"].ShouldBe("abc123");
        domain.CustomProperties["retryDelay"].ShouldBe(2000);
    }

    /// <summary>
    /// Tests ToDomain extension method for UpdateAgentConfigurationRequest with null CustomProperties
    /// </summary>
    [Fact]
    public void Should_Handle_Null_CustomProperties_When_Mapping_UpdateAgentConfigurationRequest_ToDomain()
    {
        // Arrange
        var request = new UpdateAgentConfigurationRequest
        {
            TaskTimeoutSeconds = 450,
            MaxRetries = 2,
            Priority = 4,
            CustomProperties = null!
        };

        // Act
        var domain = request.ToDomain();

        // Assert
        domain.ShouldNotBeNull();
        domain.CustomProperties.ShouldNotBeNull();
        domain.CustomProperties.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests boolean value preservation in bidirectional mapping
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
            SupportedTaskTypes = []
        };

        // Act
        var dto = originalCapabilities.ToDto();
        var roundTrip = dto.ToDomain();

        // Assert
        roundTrip.CanProcessNaturalLanguage.ShouldBe(canProcessNL);
        roundTrip.CanGenerateCode.ShouldBe(canGenerateCode);
        roundTrip.CanAnalyzeData.ShouldBe(canAnalyzeData);
        roundTrip.CanCallExternalAPIs.ShouldBe(canCallAPIs);
    }

    /// <summary>
    /// Tests that mapping creates new collection instances
    /// </summary>
    [Fact]
    public void Should_Create_New_Collection_Instances_When_Mapping_Collections()
    {
        // Arrange
        var originalTaskTypes = new List<string> { "Original" };
        var capabilities = new AgentCapabilities
        {
            SupportedTaskTypes = originalTaskTypes
        };

        // Act
        var dto = capabilities.ToDto();
        dto.SupportedTaskTypes.Add("Added");

        // Assert
        originalTaskTypes.Count.ShouldBe(1);
        dto.SupportedTaskTypes.Count.ShouldBe(2);
    }

    /// <summary>
    /// Tests that mapping creates new dictionary instances
    /// </summary>
    [Fact]
    public void Should_Create_New_Dictionary_Instances_When_Mapping_Dictionaries()
    {
        // Arrange
        var originalProps = new Dictionary<string, object> { { "Original", "Value" } };
        var configuration = new AgentConfiguration
        {
            CustomProperties = originalProps
        };

        // Act
        var dto = configuration.ToDto();
        dto.CustomProperties.Add("Added", "Value");

        // Assert
        originalProps.Count.ShouldBe(1);
        dto.CustomProperties.Count.ShouldBe(2);
    }
} 