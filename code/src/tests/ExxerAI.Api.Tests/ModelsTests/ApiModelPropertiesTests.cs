namespace ExxerAI.Api.Tests.Models;

/// <summary>
/// Unit tests for API Model properties to boost mutation score
/// </summary>
public class ApiModelPropertiesTests
{
    /// <summary>
    /// Tests CreateAgentRequest property round-trips
    /// </summary>
    [Fact]
    public void Should_Set_And_Get_CreateAgentRequest_Properties_When_Valid_Values_Provided()
    {
        // Arrange
        var request = new CreateAgentRequest();
        var capabilities = new AgentCapabilitiesDto();

        // Act
        request.Name = "Test Agent";
        request.Description = "Test Description";
        request.Capabilities = capabilities;

        // Assert
        request.Name.ShouldBe("Test Agent");
        request.Description.ShouldBe("Test Description");
        request.Capabilities.ShouldBe(capabilities);
    }

    /// <summary>
    /// Tests UpdateAgentConfigurationRequest property round-trips
    /// </summary>
    [Fact]
    public void Should_Set_And_Get_UpdateAgentConfigurationRequest_Properties_When_Valid_Values_Provided()
    {
        // Arrange
        var request = new UpdateAgentConfigurationRequest();
        var customProps = new Dictionary<string, object> { { "key", "value" } };

        // Act
        request.TaskTimeoutSeconds = 600;
        request.MaxRetries = 5;
        request.Priority = 8;
        request.CustomProperties = customProps;

        // Assert
        request.TaskTimeoutSeconds.ShouldBe(600);
        request.MaxRetries.ShouldBe(5);
        request.Priority.ShouldBe(8);
        request.CustomProperties.ShouldBe(customProps);
    }

    /// <summary>
    /// Tests UpdateAgentStatusRequest property round-trip
    /// </summary>
    [Theory]
    [InlineData(AgentStatus.Active)]
    [InlineData(AgentStatus.Inactive)]
    [InlineData(AgentStatus.Busy)]
    [InlineData(AgentStatus.Error)]
    [InlineData(AgentStatus.Paused)]
    public void Should_Set_And_Get_UpdateAgentStatusRequest_Status_When_Valid_Status_Provided(AgentStatus status)
    {
        // Arrange
        var request = new UpdateAgentStatusRequest();

        // Act
        request.Status = status;

        // Assert
        request.Status.ShouldBe(status);
    }

    /// <summary>
    /// Tests AssignTaskRequest property round-trip
    /// </summary>
    [Fact]
    public void Should_Set_And_Get_AssignTaskRequest_TaskId_When_Valid_Guid_Provided()
    {
        // Arrange
        var request = new AssignTaskRequest();
        var taskId = Guid.NewGuid();

        // Act
        request.TaskId = taskId;

        // Assert
        request.TaskId.ShouldBe(taskId);
    }

    /// <summary>
    /// Tests AgentResponse all properties
    /// </summary>
    [Fact]
    public void Should_Set_And_Get_AgentResponse_Properties_When_Valid_Values_Provided()
    {
        // Arrange
        var response = new AgentResponse();
        var id = Guid.NewGuid();
        var capabilities = new AgentCapabilitiesDto();
        var configuration = new AgentConfigurationDto();
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;

        // Act
        response.Id = id;
        response.Name = "Agent Name";
        response.Description = "Agent Description";
        response.Status = AgentStatus.Active;
        response.Capabilities = capabilities;
        response.Configuration = configuration;
        response.CreatedAt = createdAt;
        response.UpdatedAt = updatedAt;

        // Assert
        response.Id.ShouldBe(id);
        response.Name.ShouldBe("Agent Name");
        response.Description.ShouldBe("Agent Description");
        response.Status.ShouldBe(AgentStatus.Active);
        response.Capabilities.ShouldBe(capabilities);
        response.Configuration.ShouldBe(configuration);
        response.CreatedAt.ShouldBe(createdAt);
        response.UpdatedAt.ShouldBe(updatedAt);
    }

    /// <summary>
    /// Tests AgentCapabilitiesDto boolean properties
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Should_Set_And_Get_AgentCapabilitiesDto_Boolean_Properties_When_Values_Provided(bool value)
    {
        // Arrange
        var dto = new AgentCapabilitiesDto();

        // Act
        dto.CanProcessNaturalLanguage = value;
        dto.CanGenerateCode = value;
        dto.CanAnalyzeData = value;
        dto.CanCallExternalAPIs = value;

        // Assert
        dto.CanProcessNaturalLanguage.ShouldBe(value);
        dto.CanGenerateCode.ShouldBe(value);
        dto.CanAnalyzeData.ShouldBe(value);
        dto.CanCallExternalAPIs.ShouldBe(value);
    }

    /// <summary>
    /// Tests AgentCapabilitiesDto MaxConcurrentTasks property
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(100)]
    public void Should_Set_And_Get_AgentCapabilitiesDto_MaxConcurrentTasks_When_Valid_Value_Provided(int maxTasks)
    {
        // Arrange
        var dto = new AgentCapabilitiesDto();

        // Act
        dto.MaxConcurrentTasks = maxTasks;

        // Assert
        dto.MaxConcurrentTasks.ShouldBe(maxTasks);
    }

    /// <summary>
    /// Tests AgentCapabilitiesDto SupportedTaskTypes collection
    /// </summary>
    [Fact]
    public void Should_Set_And_Get_AgentCapabilitiesDto_SupportedTaskTypes_When_Collection_Provided()
    {
        // Arrange
        var dto = new AgentCapabilitiesDto();
        var taskTypes = new List<string> { "Type1", "Type2", "Type3" };

        // Act
        dto.SupportedTaskTypes = taskTypes;

        // Assert
        dto.SupportedTaskTypes.ShouldBe(taskTypes);
        dto.SupportedTaskTypes.Count.ShouldBe(3);
    }

    /// <summary>
    /// Tests AgentConfigurationDto properties
    /// </summary>
    [Fact]
    public void Should_Set_And_Get_AgentConfigurationDto_Properties_When_Valid_Values_Provided()
    {
        // Arrange
        var dto = new AgentConfigurationDto();
        var customProps = new Dictionary<string, object> { { "test", 123 } };

        // Act
        dto.TaskTimeoutSeconds = 900;
        dto.MaxRetries = 7;
        dto.Priority = 9;
        dto.CustomProperties = customProps;

        // Assert
        dto.TaskTimeoutSeconds.ShouldBe(900);
        dto.MaxRetries.ShouldBe(7);
        dto.Priority.ShouldBe(9);
        dto.CustomProperties.ShouldBe(customProps);
    }

    /// <summary>
    /// Tests ApiResponse generic properties
    /// </summary>
    [Fact]
    public void Should_Set_And_Get_ApiResponse_Properties_When_Valid_Values_Provided()
    {
        // Arrange
        var response = new ApiResponse<string>();
        var errors = new List<string> { "Error1", "Error2" };

        // Act
        response.Success = true;
        response.Data = "Test Data";
        response.Errors = errors;
        response.Message = "Success message";

        // Assert
        response.Success.ShouldBeTrue();
        response.Data.ShouldBe("Test Data");
        response.Errors.ShouldBe(errors);
        response.Message.ShouldBe("Success message");
    }

    /// <summary>
    /// Tests ApiResponse with different data types
    /// </summary>
    [Fact]
    public void Should_Work_With_Different_Data_Types_When_Using_ApiResponse_Generic()
    {
        // Act
        var stringResponse = new ApiResponse<string> { Data = "test string" };
        var intResponse = new ApiResponse<int> { Data = 42 };
        var boolResponse = new ApiResponse<bool> { Data = true };

        // Assert
        stringResponse.Data.ShouldBe("test string");
        intResponse.Data.ShouldBe(42);
        boolResponse.Data.ShouldBeTrue();
    }

    /// <summary>
    /// Tests default values for all models
    /// </summary>
    [Fact]
    public void Should_Have_Expected_Default_Values_When_Using_Default_Constructors()
    {
        // Act
        var createRequest = new CreateAgentRequest();
        var updateConfigRequest = new UpdateAgentConfigurationRequest();
        var updateStatusRequest = new UpdateAgentStatusRequest();
        var assignTaskRequest = new AssignTaskRequest();
        var agentResponse = new AgentResponse();
        var capabilitiesDto = new AgentCapabilitiesDto();
        var configurationDto = new AgentConfigurationDto();
        var apiResponse = new ApiResponse<object>();

        // Assert - CreateAgentRequest
        createRequest.Name.ShouldBe(string.Empty);
        createRequest.Description.ShouldBe(string.Empty);
        createRequest.Capabilities.ShouldNotBeNull();

        // Assert - UpdateAgentConfigurationRequest
        updateConfigRequest.TaskTimeoutSeconds.ShouldBe(300);
        updateConfigRequest.MaxRetries.ShouldBe(3);
        updateConfigRequest.Priority.ShouldBe(1);
        updateConfigRequest.CustomProperties.ShouldNotBeNull();

        // Assert - UpdateAgentStatusRequest
        updateStatusRequest.Status.ShouldBe(default(AgentStatus));

        // Assert - AssignTaskRequest
        assignTaskRequest.TaskId.ShouldBe(default(Guid));

        // Assert - AgentResponse
        agentResponse.Id.ShouldBe(default(Guid));
        agentResponse.Name.ShouldBe(string.Empty);
        agentResponse.Capabilities.ShouldNotBeNull();

        // Assert - AgentCapabilitiesDto
        capabilitiesDto.CanProcessNaturalLanguage.ShouldBeTrue();
        capabilitiesDto.MaxConcurrentTasks.ShouldBe(1);
        capabilitiesDto.SupportedTaskTypes.ShouldNotBeNull();

        // Assert - AgentConfigurationDto
        configurationDto.TaskTimeoutSeconds.ShouldBe(300);
        configurationDto.CustomProperties.ShouldNotBeNull();

        // Assert - ApiResponse
        apiResponse.Success.ShouldBeFalse();
        apiResponse.Data.ShouldBeNull();
        apiResponse.Errors.ShouldNotBeNull();
        apiResponse.Message.ShouldBe(string.Empty);
    }
} 