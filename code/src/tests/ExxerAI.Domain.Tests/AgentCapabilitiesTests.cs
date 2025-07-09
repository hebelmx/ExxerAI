using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests;

/// <summary>
/// Unit tests for AgentCapabilities domain model
/// </summary>
public class AgentCapabilitiesTests
{
    /// <summary>
    /// Tests that default constructor initializes all properties with expected default values
    /// </summary>
    [Fact]
    public void Should_Initialize_With_Default_Values_When_Using_Default_Constructor()
    {
        // Act
        var capabilities = new AgentCapabilities();

        // Assert
        capabilities.CanProcessNaturalLanguage.ShouldBeTrue();
        capabilities.CanGenerateCode.ShouldBeFalse();
        capabilities.CanAnalyzeData.ShouldBeFalse();
        capabilities.CanCallExternalAPIs.ShouldBeFalse();
        capabilities.MaxConcurrentTasks.ShouldBe(1);
        capabilities.SupportedTaskTypes.ShouldNotBeNull();
        capabilities.SupportedTaskTypes.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests property round-trip for CanProcessNaturalLanguage
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Should_Set_And_Get_CanProcessNaturalLanguage_When_Value_Provided(bool canProcess)
    {
        // Arrange
        var capabilities = new AgentCapabilities();

        // Act
        capabilities.CanProcessNaturalLanguage = canProcess;

        // Assert
        capabilities.CanProcessNaturalLanguage.ShouldBe(canProcess);
    }

    /// <summary>
    /// Tests property round-trip for CanGenerateCode
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Should_Set_And_Get_CanGenerateCode_When_Value_Provided(bool canGenerate)
    {
        // Arrange
        var capabilities = new AgentCapabilities();

        // Act
        capabilities.CanGenerateCode = canGenerate;

        // Assert
        capabilities.CanGenerateCode.ShouldBe(canGenerate);
    }

    /// <summary>
    /// Tests property round-trip for CanAnalyzeData
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Should_Set_And_Get_CanAnalyzeData_When_Value_Provided(bool canAnalyze)
    {
        // Arrange
        var capabilities = new AgentCapabilities();

        // Act
        capabilities.CanAnalyzeData = canAnalyze;

        // Assert
        capabilities.CanAnalyzeData.ShouldBe(canAnalyze);
    }

    /// <summary>
    /// Tests property round-trip for CanCallExternalAPIs
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Should_Set_And_Get_CanCallExternalAPIs_When_Value_Provided(bool canCall)
    {
        // Arrange
        var capabilities = new AgentCapabilities();

        // Act
        capabilities.CanCallExternalAPIs = canCall;

        // Assert
        capabilities.CanCallExternalAPIs.ShouldBe(canCall);
    }

    /// <summary>
    /// Tests property round-trip for MaxConcurrentTasks
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(100)]
    public void Should_Set_And_Get_MaxConcurrentTasks_When_Valid_Value_Provided(int maxTasks)
    {
        // Arrange
        var capabilities = new AgentCapabilities();

        // Act
        capabilities.MaxConcurrentTasks = maxTasks;

        // Assert
        capabilities.MaxConcurrentTasks.ShouldBe(maxTasks);
    }

    /// <summary>
    /// Tests boundary values for MaxConcurrentTasks
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void Should_Accept_Any_MaxConcurrentTasks_Value_When_No_Validation_Applied(int maxTasks)
    {
        // Arrange
        var capabilities = new AgentCapabilities();

        // Act
        capabilities.MaxConcurrentTasks = maxTasks;

        // Assert
        capabilities.MaxConcurrentTasks.ShouldBe(maxTasks);
    }

    /// <summary>
    /// Tests property round-trip for SupportedTaskTypes with list
    /// </summary>
    [Fact]
    public void Should_Set_And_Get_SupportedTaskTypes_When_List_Provided()
    {
        // Arrange
        var taskTypes = new List<string> { "DataProcessing", "NLP", "CodeGeneration", "Analysis" };

        // Act - Use object initializer for init-only property
        var capabilities = new AgentCapabilities
        {
            SupportedTaskTypes = taskTypes
        };

        // Assert
        capabilities.SupportedTaskTypes.ShouldBe(taskTypes);
        capabilities.SupportedTaskTypes.Count.ShouldBe(4);
        capabilities.SupportedTaskTypes.ShouldContain("DataProcessing");
        capabilities.SupportedTaskTypes.ShouldContain("NLP");
        capabilities.SupportedTaskTypes.ShouldContain("CodeGeneration");
        capabilities.SupportedTaskTypes.ShouldContain("Analysis");
    }

    /// <summary>
    /// Tests that SupportedTaskTypes can be modified after initialization
    /// </summary>
    [Fact]
    public void Should_Allow_SupportedTaskTypes_Modification_When_Already_Initialized()
    {
        // Arrange
        var capabilities = new AgentCapabilities();
        
        // Act
        capabilities.SupportedTaskTypes.Add("NewTaskType");

        // Assert
        capabilities.SupportedTaskTypes.Count.ShouldBe(1);
        capabilities.SupportedTaskTypes.ShouldContain("NewTaskType");
    }

    /// <summary>
    /// Tests that SupportedTaskTypes collection initialization with init works correctly
    /// </summary>
    [Fact]
    public void Should_Initialize_SupportedTaskTypes_With_Init_Syntax_When_Creating_Object()
    {
        // Act
        var capabilities = new AgentCapabilities
        {
            SupportedTaskTypes = ["Task1", "Task2", "Task3"]
        };

        // Assert
        capabilities.SupportedTaskTypes.Count.ShouldBe(3);
        capabilities.SupportedTaskTypes.ShouldContain("Task1");
        capabilities.SupportedTaskTypes.ShouldContain("Task2");
        capabilities.SupportedTaskTypes.ShouldContain("Task3");
    }

    /// <summary>
    /// Tests all properties can be set together
    /// </summary>
    [Fact]
    public void Should_Set_All_Properties_When_Creating_Complete_Capabilities()
    {
        // Arrange
        var taskTypes = new List<string> { "ComplexAnalysis", "APIIntegration" };

        // Act
        var capabilities = new AgentCapabilities
        {
            CanProcessNaturalLanguage = false,
            CanGenerateCode = true,
            CanAnalyzeData = true,
            CanCallExternalAPIs = true,
            MaxConcurrentTasks = 5,
            SupportedTaskTypes = taskTypes
        };

        // Assert
        capabilities.CanProcessNaturalLanguage.ShouldBeFalse();
        capabilities.CanGenerateCode.ShouldBeTrue();
        capabilities.CanAnalyzeData.ShouldBeTrue();
        capabilities.CanCallExternalAPIs.ShouldBeTrue();
        capabilities.MaxConcurrentTasks.ShouldBe(5);
        capabilities.SupportedTaskTypes.ShouldBe(taskTypes);
        capabilities.SupportedTaskTypes.Count.ShouldBe(2);
    }

    /// <summary>
    /// Tests enabling all capabilities
    /// </summary>
    [Fact]
    public void Should_Enable_All_Capabilities_When_Setting_All_To_True()
    {
        // Arrange
        var capabilities = new AgentCapabilities();

        // Act
        capabilities.CanProcessNaturalLanguage = true;
        capabilities.CanGenerateCode = true;
        capabilities.CanAnalyzeData = true;
        capabilities.CanCallExternalAPIs = true;

        // Assert
        capabilities.CanProcessNaturalLanguage.ShouldBeTrue();
        capabilities.CanGenerateCode.ShouldBeTrue();
        capabilities.CanAnalyzeData.ShouldBeTrue();
        capabilities.CanCallExternalAPIs.ShouldBeTrue();
    }

    /// <summary>
    /// Tests disabling all capabilities
    /// </summary>
    [Fact]
    public void Should_Disable_All_Capabilities_When_Setting_All_To_False()
    {
        // Arrange
        var capabilities = new AgentCapabilities
        {
            CanProcessNaturalLanguage = true,
            CanGenerateCode = true,
            CanAnalyzeData = true,
            CanCallExternalAPIs = true
        };

        // Act
        capabilities.CanProcessNaturalLanguage = false;
        capabilities.CanGenerateCode = false;
        capabilities.CanAnalyzeData = false;
        capabilities.CanCallExternalAPIs = false;

        // Assert
        capabilities.CanProcessNaturalLanguage.ShouldBeFalse();
        capabilities.CanGenerateCode.ShouldBeFalse();
        capabilities.CanAnalyzeData.ShouldBeFalse();
        capabilities.CanCallExternalAPIs.ShouldBeFalse();
    }

    /// <summary>
    /// Tests adding multiple task types one by one
    /// </summary>
    [Fact]
    public void Should_Support_Adding_Multiple_TaskTypes_When_Adding_One_By_One()
    {
        // Arrange
        var capabilities = new AgentCapabilities();

        // Act
        capabilities.SupportedTaskTypes.Add("DataMining");
        capabilities.SupportedTaskTypes.Add("TextAnalysis");
        capabilities.SupportedTaskTypes.Add("ImageProcessing");

        // Assert
        capabilities.SupportedTaskTypes.Count.ShouldBe(3);
        capabilities.SupportedTaskTypes.ShouldContain("DataMining");
        capabilities.SupportedTaskTypes.ShouldContain("TextAnalysis");
        capabilities.SupportedTaskTypes.ShouldContain("ImageProcessing");
    }

    /// <summary>
    /// Tests SupportedTaskTypes with empty list
    /// </summary>
    [Fact]
    public void Should_Accept_Empty_SupportedTaskTypes_When_Empty_List_Provided()
    {
        // Arrange
        var emptyList = new List<string>();

        // Act - Use object initializer for init-only property
        var capabilities = new AgentCapabilities
        {
            SupportedTaskTypes = emptyList
        };

        // Assert
        capabilities.SupportedTaskTypes.ShouldBe(emptyList);
        capabilities.SupportedTaskTypes.ShouldBeEmpty();
    }

    /// <summary>
    /// Tests SupportedTaskTypes with duplicate values
    /// </summary>
    [Fact]
    public void Should_Allow_Duplicate_TaskTypes_When_Adding_Same_Type_Multiple_Times()
    {
        // Arrange
        var capabilities = new AgentCapabilities();

        // Act
        capabilities.SupportedTaskTypes.Add("DuplicateTask");
        capabilities.SupportedTaskTypes.Add("DuplicateTask");
        capabilities.SupportedTaskTypes.Add("DuplicateTask");

        // Assert
        capabilities.SupportedTaskTypes.Count.ShouldBe(3);
        capabilities.SupportedTaskTypes.ShouldAllBe(taskType => taskType == "DuplicateTask");
    }
} 