using ExxerAI.Api.Models;
using ExxerAI.Domain;

namespace ExxerAI.Api.Tests.Models;

/// <summary>
/// Unit tests for API Agent Models and DTOs
/// </summary>
public class AgentModelsDtoTests
{
    /// <summary>
    /// Tests CreateAgentRequest default constructor initialization
    /// </summary>
    [Fact]
    public void Should_Initialize_CreateAgentRequest_With_Default_Values_When_Using_Default_Constructor()
    {
        // Act
        var request = new CreateAgentRequest();

        // Assert
        request.Name.ShouldBe(string.Empty);
        request.Description.ShouldBe(string.Empty);
        request.Capabilities.ShouldNotBeNull();
    }

    /// <summary>
    /// Tests CreateAgentRequest property round-trip for Name
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("Test Agent")]
    [InlineData("Data Processing Agent")]
    [InlineData("Document Intelligence Agent")]
    public void Should_Set_And_Get_Name_When_Valid_String_Provided_For_CreateAgentRequest(string name)
    {
        // Arrange
        var request = new CreateAgentRequest();

        // Act
        request.Name = name;

        // Assert
        request.Name.ShouldBe(name);
    }

    /// <summary>
    /// Tests CreateAgentRequest property round-trip for Description
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("Simple description")]
    [InlineData("A more complex description with multiple words and punctuation!")]
    public void Should_Set_And_Get_Description_When_Valid_String_Provided_For_CreateAgentRequest(string description)
    {
        // Arrange
        var request = new CreateAgentRequest();

        // Act
        request.Description = description;

        // Assert
        request.Description.ShouldBe(description);
    }

    /// <summary>
    /// Tests CreateAgentRequest property round-trip for Capabilities
    /// </summary>
    [Fact]
    public void Should_Set_And_Get_Capabilities_When_Valid_Object_Provided_For_CreateAgentRequest()
    {
        // Arrange
        var request = new CreateAgentRequest();
        var capabilities = new AgentCapabilitiesDto
        {
            CanProcessNaturalLanguage = true,
            CanGenerateCode = false,
            MaxConcurrentTasks = 5
        };

        // Act
        request.Capabilities = capabilities;

        // Assert
        request.Capabilities.ShouldBe(capabilities);
        request.Capabilities.CanProcessNaturalLanguage.ShouldBeTrue();
        request.Capabilities.MaxConcurrentTasks.ShouldBe(5);
    }

    /// <summary>
    /// Tests UpdateAgentConfigurationRequest default constructor initialization
    /// </summary>
    [Fact]
    public void Should_Initialize_UpdateAgentConfigurationRequest_With_Default_Values_When_Using_Default_Constructor()
    {
        // Act
        var request = new UpdateAgentConfigurationRequest();

        // Assert
        request.TaskTimeoutSeconds.ShouldBe(300);
        request.MaxRetries.ShouldBe(3);
        request.Priority.ShouldBe(1);
        request.CustomProperties.ShouldNotBeNull();
        request.CustomProperties.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests UpdateAgentConfigurationRequest property round-trips
    /// </summary>
    [Theory]
    [InlineData(60, 1, 5)]
    [InlineData(300, 3, 1)]
    [InlineData(1800, 10, 10)]
    public void Should_Set_And_Get_Properties_When_Valid_Values_Provided_For_UpdateAgentConfigurationRequest(
        int timeoutSeconds, int maxRetries, int priority)
    {
        // Arrange
        var request = new UpdateAgentConfigurationRequest();

        // Act
        request.TaskTimeoutSeconds = timeoutSeconds;
        request.MaxRetries = maxRetries;
        request.Priority = priority;

        // Assert
        request.TaskTimeoutSeconds.ShouldBe(timeoutSeconds);
        request.MaxRetries.ShouldBe(maxRetries);
        request.Priority.ShouldBe(priority);
    }

    /// <summary>
    /// Tests UpdateAgentStatusRequest constructor and property
    /// </summary>
    [Theory]
    [InlineData(AgentStatus.Active)]
    [InlineData(AgentStatus.Inactive)]
    [InlineData(AgentStatus.Busy)]
    [InlineData(AgentStatus.Error)]
    [InlineData(AgentStatus.Paused)]
    public void Should_Set_And_Get_Status_When_Valid_Status_Provided_For_UpdateAgentStatusRequest(AgentStatus status)
    {
        // Arrange
        var request = new UpdateAgentStatusRequest();

        // Act
        request.Status = status;

        // Assert
        request.Status.ShouldBe(status);
    }

    /// <summary>
    /// Tests AssignTaskRequest default constructor and property
    /// </summary>
    [Fact]
    public void Should_Set_And_Get_TaskId_When_Valid_Guid_Provided_For_AssignTaskRequest()
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
    /// Tests AgentResponse default constructor initialization
    /// </summary>
    [Fact]
    public void Should_Initialize_AgentResponse_With_Default_Values_When_Using_Default_Constructor()
    {
        // Act
        var response = new AgentResponse();

        // Assert
        response.Id.ShouldBe(default(Guid));
        response.Name.ShouldBe(string.Empty);
        response.Description.ShouldBe(string.Empty);
        response.Status.ShouldBe(default(AgentStatus));
        response.Capabilities.ShouldNotBeNull();
        response.Configuration.ShouldNotBeNull();
        response.CreatedAt.ShouldBe(default(DateTime));
        response.UpdatedAt.ShouldBe(default(DateTime));
    }

    /// <summary>
    /// Tests AgentResponse all properties can be set
    /// </summary>
    [Fact]
    public void Should_Set_All_Properties_When_Creating_Complete_AgentResponse()
    {
        // Arrange
        var id = Guid.NewGuid();
        var capabilities = new AgentCapabilitiesDto();
        var configuration = new AgentConfigurationDto();
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;

        // Act
        var response = new AgentResponse
        {
            Id = id,
            Name = "Test Agent",
            Description = "Test Description",
            Status = AgentStatus.Active,
            Capabilities = capabilities,
            Configuration = configuration,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        // Assert
        response.Id.ShouldBe(id);
        response.Name.ShouldBe("Test Agent");
        response.Description.ShouldBe("Test Description");
        response.Status.ShouldBe(AgentStatus.Active);
        response.Capabilities.ShouldBe(capabilities);
        response.Configuration.ShouldBe(configuration);
        response.CreatedAt.ShouldBe(createdAt);
        response.UpdatedAt.ShouldBe(updatedAt);
    }

    /// <summary>
    /// Tests AgentCapabilitiesDto default constructor initialization
    /// </summary>
    [Fact]
    public void Should_Initialize_AgentCapabilitiesDto_With_Default_Values_When_Using_Default_Constructor()
    {
        // Act
        var dto = new AgentCapabilitiesDto();

        // Assert
        dto.CanProcessNaturalLanguage.ShouldBeTrue();
        dto.CanGenerateCode.ShouldBeFalse();
        dto.CanAnalyzeData.ShouldBeFalse();
        dto.CanCallExternalAPIs.ShouldBeFalse();
        dto.MaxConcurrentTasks.ShouldBe(1);
        dto.SupportedTaskTypes.ShouldNotBeNull();
        dto.SupportedTaskTypes.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests AgentCapabilitiesDto boolean properties
    /// </summary>
    [Theory]
    [InlineData(true, false, true, false)]
    [InlineData(false, true, false, true)]
    [InlineData(true, true, true, true)]
    [InlineData(false, false, false, false)]
    public void Should_Set_And_Get_Boolean_Properties_When_Values_Provided_For_AgentCapabilitiesDto(
        bool canProcessNL, bool canGenerateCode, bool canAnalyzeData, bool canCallAPIs)
    {
        // Arrange
        var dto = new AgentCapabilitiesDto();

        // Act
        dto.CanProcessNaturalLanguage = canProcessNL;
        dto.CanGenerateCode = canGenerateCode;
        dto.CanAnalyzeData = canAnalyzeData;
        dto.CanCallExternalAPIs = canCallAPIs;

        // Assert
        dto.CanProcessNaturalLanguage.ShouldBe(canProcessNL);
        dto.CanGenerateCode.ShouldBe(canGenerateCode);
        dto.CanAnalyzeData.ShouldBe(canAnalyzeData);
        dto.CanCallExternalAPIs.ShouldBe(canCallAPIs);
    }

    /// <summary>
    /// Tests AgentCapabilitiesDto SupportedTaskTypes collection
    /// </summary>
    [Fact]
    public void Should_Manage_SupportedTaskTypes_Collection_When_Adding_Items_For_AgentCapabilitiesDto()
    {
        // Arrange
        var dto = new AgentCapabilitiesDto();

        // Act
        dto.SupportedTaskTypes.Add("DataProcessing");
        dto.SupportedTaskTypes.Add("NLP");
        dto.SupportedTaskTypes.Add("CodeGeneration");

        // Assert
        dto.SupportedTaskTypes.Count.ShouldBe(3);
        dto.SupportedTaskTypes.ShouldContain("DataProcessing");
        dto.SupportedTaskTypes.ShouldContain("NLP");
        dto.SupportedTaskTypes.ShouldContain("CodeGeneration");
    }

    /// <summary>
    /// Tests AgentConfigurationDto default constructor initialization
    /// </summary>
    [Fact]
    public void Should_Initialize_AgentConfigurationDto_With_Default_Values_When_Using_Default_Constructor()
    {
        // Act
        var dto = new AgentConfigurationDto();

        // Assert
        dto.TaskTimeoutSeconds.ShouldBe(300);
        dto.MaxRetries.ShouldBe(3);
        dto.Priority.ShouldBe(1);
        dto.CustomProperties.ShouldNotBeNull();
        dto.CustomProperties.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests AgentConfigurationDto CustomProperties manipulation
    /// </summary>
    [Fact]
    public void Should_Manage_CustomProperties_Dictionary_When_Adding_Items_For_AgentConfigurationDto()
    {
        // Arrange
        var dto = new AgentConfigurationDto();

        // Act
        dto.CustomProperties.Add("apiEndpoint", "https://api.example.com");
        dto.CustomProperties.Add("retryDelay", 5000);
        dto.CustomProperties.Add("enableLogging", true);

        // Assert
        dto.CustomProperties.Count.ShouldBe(3);
        dto.CustomProperties["apiEndpoint"].ShouldBe("https://api.example.com");
        dto.CustomProperties["retryDelay"].ShouldBe(5000);
        dto.CustomProperties["enableLogging"].ShouldBe(true);
    }

    /// <summary>
    /// Tests ApiResponse generic wrapper default constructor
    /// </summary>
    [Fact]
    public void Should_Initialize_ApiResponse_With_Default_Values_When_Using_Default_Constructor()
    {
        // Act
        var response = new ApiResponse<string>();

        // Assert
        response.Success.ShouldBeFalse();
        response.Data.ShouldBeNull();
        response.Errors.ShouldNotBeNull();
        response.Errors.ShouldBeEmpty();
        response.Message.ShouldBe(string.Empty);
    }

    /// <summary>
    /// Tests ApiResponse property round-trips
    /// </summary>
    [Fact]
    public void Should_Set_And_Get_All_Properties_When_Creating_Complete_ApiResponse()
    {
        // Arrange
        var data = "Test Data";
        var errors = new List<string> { "Error1", "Error2" };

        // Act
        var response = new ApiResponse<string>
        {
            Success = true,
            Data = data,
            Errors = errors,
            Message = "Operation completed"
        };

        // Assert
        response.Success.ShouldBeTrue();
        response.Data.ShouldBe(data);
        response.Errors.ShouldBe(errors);
        response.Message.ShouldBe("Operation completed");
        response.Errors.Count.ShouldBe(2);
    }

    /// <summary>
    /// Tests ApiResponse with different generic types
    /// </summary>
    [Fact]
    public void Should_Work_With_Different_Generic_Types_When_Creating_ApiResponse()
    {
        // Act
        var stringResponse = new ApiResponse<string> { Data = "test" };
        var intResponse = new ApiResponse<int> { Data = 42 };
        var agentResponse = new ApiResponse<AgentResponse> { Data = new AgentResponse() };

        // Assert
        stringResponse.Data.ShouldBe("test");
        intResponse.Data.ShouldBe(42);
        agentResponse.Data.ShouldNotBeNull();
    }
} 