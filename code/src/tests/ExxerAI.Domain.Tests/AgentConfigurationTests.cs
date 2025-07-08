using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests;

/// <summary>
/// Unit tests for AgentConfiguration domain model
/// </summary>
public class AgentConfigurationTests
{
    /// <summary>
    /// Tests that default constructor initializes all properties with expected default values
    /// </summary>
    [Fact]
    public void Should_Initialize_With_Default_Values_When_Using_Default_Constructor()
    {
        // Act
        var configuration = new AgentConfiguration();

        // Assert
        configuration.TaskTimeoutSeconds.ShouldBe(300);
        configuration.MaxRetries.ShouldBe(3);
        configuration.Priority.ShouldBe(1);
        configuration.CustomProperties.ShouldNotBeNull();
        configuration.CustomProperties.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests property round-trip for TaskTimeoutSeconds
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(300)]
    [InlineData(3600)]
    [InlineData(7200)]
    public void Should_Set_And_Get_TaskTimeoutSeconds_When_Valid_Value_Provided(int timeoutSeconds)
    {
        // Arrange
        var configuration = new AgentConfiguration();

        // Act
        configuration.TaskTimeoutSeconds = timeoutSeconds;

        // Assert
        configuration.TaskTimeoutSeconds.ShouldBe(timeoutSeconds);
    }

    /// <summary>
    /// Tests property round-trip for MaxRetries
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(10)]
    public void Should_Set_And_Get_MaxRetries_When_Valid_Value_Provided(int maxRetries)
    {
        // Arrange
        var configuration = new AgentConfiguration();

        // Act
        configuration.MaxRetries = maxRetries;

        // Assert
        configuration.MaxRetries.ShouldBe(maxRetries);
    }

    /// <summary>
    /// Tests property round-trip for Priority
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void Should_Set_And_Get_Priority_When_Valid_Value_Provided(int priority)
    {
        // Arrange
        var configuration = new AgentConfiguration();

        // Act
        configuration.Priority = priority;

        // Assert
        configuration.Priority.ShouldBe(priority);
    }

    /// <summary>
    /// Tests property round-trip for CustomProperties
    /// </summary>
    [Fact]
    public void Should_Set_And_Get_CustomProperties_When_Valid_Dictionary_Provided()
    {
        // Arrange
        var customProperties = new Dictionary<string, object>
        {
            { "apiKey", "test-key-123" },
            { "retryDelay", 1000 },
            { "enableLogging", true },
            { "endpoint", "https://api.example.com" }
        };

        // Act - Use object initializer for init-only property
        var configuration = new AgentConfiguration
        {
            CustomProperties = customProperties
        };

        // Assert
        configuration.CustomProperties.ShouldBe(customProperties);
        configuration.CustomProperties.Count.ShouldBe(4);
        configuration.CustomProperties["apiKey"].ShouldBe("test-key-123");
        configuration.CustomProperties["retryDelay"].ShouldBe(1000);
        configuration.CustomProperties["enableLogging"].ShouldBe(true);
        configuration.CustomProperties["endpoint"].ShouldBe("https://api.example.com");
    }

    /// <summary>
    /// Tests that CustomProperties can be modified after initialization
    /// </summary>
    [Fact]
    public void Should_Allow_CustomProperties_Modification_When_Already_Initialized()
    {
        // Arrange
        var configuration = new AgentConfiguration();
        
        // Act
        configuration.CustomProperties.Add("newKey", "newValue");

        // Assert
        configuration.CustomProperties.Count.ShouldBe(1);
        configuration.CustomProperties["newKey"].ShouldBe("newValue");
    }

    /// <summary>
    /// Tests that CustomProperties collection initialization with init works correctly
    /// </summary>
    [Fact]
    public void Should_Initialize_CustomProperties_With_Init_Syntax_When_Creating_Object()
    {
        // Act
        var configuration = new AgentConfiguration
        {
            CustomProperties = new Dictionary<string, object>
            {
                { "initKey", "initValue" },
                { "count", 42 }
            }
        };

        // Assert
        configuration.CustomProperties.Count.ShouldBe(2);
        configuration.CustomProperties["initKey"].ShouldBe("initValue");
        configuration.CustomProperties["count"].ShouldBe(42);
    }

    /// <summary>
    /// Tests all properties can be set together
    /// </summary>
    [Fact]
    public void Should_Set_All_Properties_When_Creating_Complete_Configuration()
    {
        // Arrange
        var customProps = new Dictionary<string, object> { { "test", "value" } };

        // Act
        var configuration = new AgentConfiguration
        {
            TaskTimeoutSeconds = 600,
            MaxRetries = 5,
            Priority = 8,
            CustomProperties = customProps
        };

        // Assert
        configuration.TaskTimeoutSeconds.ShouldBe(600);
        configuration.MaxRetries.ShouldBe(5);
        configuration.Priority.ShouldBe(8);
        configuration.CustomProperties.ShouldBe(customProps);
        configuration.CustomProperties["test"].ShouldBe("value");
    }

    /// <summary>
    /// Tests boundary values for TaskTimeoutSeconds
    /// </summary>
    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public void Should_Accept_Any_TaskTimeoutSeconds_Value_When_No_Validation_Applied(int timeout)
    {
        // Arrange
        var configuration = new AgentConfiguration();

        // Act
        configuration.TaskTimeoutSeconds = timeout;

        // Assert
        configuration.TaskTimeoutSeconds.ShouldBe(timeout);
    }

    /// <summary>
    /// Tests boundary values for MaxRetries
    /// </summary>
    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public void Should_Accept_Any_MaxRetries_Value_When_No_Validation_Applied(int retries)
    {
        // Arrange
        var configuration = new AgentConfiguration();

        // Act
        configuration.MaxRetries = retries;

        // Assert
        configuration.MaxRetries.ShouldBe(retries);
    }

    /// <summary>
    /// Tests boundary values for Priority
    /// </summary>
    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public void Should_Accept_Any_Priority_Value_When_No_Validation_Applied(int priority)
    {
        // Arrange
        var configuration = new AgentConfiguration();

        // Act
        configuration.Priority = priority;

        // Assert
        configuration.Priority.ShouldBe(priority);
    }
} 