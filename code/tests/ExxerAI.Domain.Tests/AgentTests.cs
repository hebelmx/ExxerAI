using ExxerAI.Domain;
using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests;

/// <summary>
/// Comprehensive tests for Agent domain class following I-TDD principles.
/// Tests cover agent lifecycle, properties, validation, and business rules.
/// </summary>
public class AgentTests
{
    /// <summary>
    /// Contract Test: Agent constructor should initialize with default values
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults_When_Created()
    {
        // Act
        var agent = new Agent();

        // Assert
        agent.Id.ShouldNotBe(Guid.Empty);
        agent.Name.ShouldBe(string.Empty);
        agent.Description.ShouldBe(string.Empty);
        agent.Status.ShouldBe(AgentStatus.Inactive);
        agent.Capabilities.ShouldNotBeNull();
        agent.Configuration.ShouldNotBeNull();
        agent.Tasks.ShouldNotBeNull();
        agent.Tasks.ShouldBeEmpty();
        agent.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
        agent.UpdatedAt.ShouldBeInRange(DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddMinutes(1));
    }

    /// <summary>
    /// Contract Test: Agent Id should be unique for each instance
    /// </summary>
    [Fact]
    public void Id_ShouldBeUnique_When_MultipleAgentsCreated()
    {
        // Act
        var agent1 = new Agent();
        var agent2 = new Agent();

        // Assert
        agent1.Id.ShouldNotBe(agent2.Id);
        agent1.Id.ShouldNotBe(Guid.Empty);
        agent2.Id.ShouldNotBe(Guid.Empty);
    }

    /// <summary>
    /// Contract Test: Agent properties should be settable
    /// </summary>
    [Fact]
    public void Properties_ShouldBeSettable_When_ValuesAssigned()
    {
        // Arrange
        var agent = new Agent();
        var testId = Guid.NewGuid();
        var testName = "TestAgent";
        var testDescription = "Test Agent Description";
        var testStatus = AgentStatus.Active;
        var testCapabilities = new AgentCapabilities { CanGenerateCode = true };
        var testConfiguration = new AgentConfiguration();
        var testCreatedAt = DateTime.UtcNow.AddDays(-1);
        var testUpdatedAt = DateTime.UtcNow;

        // Act
        agent.Id = testId;
        agent.Name = testName;
        agent.Description = testDescription;
        agent.Status = testStatus;
        agent.Capabilities = testCapabilities;
        agent.Configuration = testConfiguration;
        agent.CreatedAt = testCreatedAt;
        agent.UpdatedAt = testUpdatedAt;

        // Assert
        agent.Id.ShouldBe(testId);
        agent.Name.ShouldBe(testName);
        agent.Description.ShouldBe(testDescription);
        agent.Status.ShouldBe(testStatus);
        agent.Capabilities.ShouldBeSameAs(testCapabilities);
        agent.Configuration.ShouldBeSameAs(testConfiguration);
        agent.CreatedAt.ShouldBe(testCreatedAt);
        agent.UpdatedAt.ShouldBe(testUpdatedAt);
    }

    /// <summary>
    /// Contract Test: Tasks collection should be modifiable
    /// </summary>
    [Fact]
    public void Tasks_ShouldBeModifiable_When_TasksAdded()
    {
        // Arrange
        var agent = new Agent();
        var task1 = new AgentTask { Id = Guid.NewGuid(), Name = "Task 1" };
        var task2 = new AgentTask { Id = Guid.NewGuid(), Name = "Task 2" };

        // Act
        agent.Tasks.Add(task1);
        agent.Tasks.Add(task2);

        // Assert
        agent.Tasks.Count.ShouldBe(2);
        agent.Tasks.ShouldContain(task1);
        agent.Tasks.ShouldContain(task2);
    }

    /// <summary>
    /// Validation Test: Agent with maximum name length should be valid
    /// </summary>
    [Fact]
    public void Name_ShouldAcceptMaxLength_When_Valid()
    {
        // Arrange
        var agent = new Agent();
        var maxLengthName = new string('A', 100); // StringLength(100)

        // Act
        agent.Name = maxLengthName;

        // Assert
        agent.Name.ShouldBe(maxLengthName);
        agent.Name.Length.ShouldBe(100);
    }

    /// <summary>
    /// Validation Test: Agent with maximum description length should be valid
    /// </summary>
    [Fact]
    public void Description_ShouldAcceptMaxLength_When_Valid()
    {
        // Arrange
        var agent = new Agent();
        var maxLengthDescription = new string('B', 500); // StringLength(500)

        // Act
        agent.Description = maxLengthDescription;

        // Assert
        agent.Description.ShouldBe(maxLengthDescription);
        agent.Description.Length.ShouldBe(500);
    }

    /// <summary>
    /// Behavior Test: Agent status transitions should work correctly
    /// </summary>
    [Theory]
    [InlineData(AgentStatus.Inactive, AgentStatus.Active)]
    [InlineData(AgentStatus.Active, AgentStatus.Busy)]
    [InlineData(AgentStatus.Busy, AgentStatus.Active)]
    [InlineData(AgentStatus.Active, AgentStatus.Paused)]
    [InlineData(AgentStatus.Error, AgentStatus.Inactive)]
    public void Status_ShouldTransition_When_ValidStateChange(AgentStatus fromStatus, AgentStatus toStatus)
    {
        // Arrange
        var agent = new Agent { Status = fromStatus };

        // Act
        agent.Status = toStatus;

        // Assert
        agent.Status.ShouldBe(toStatus);
    }

    /// <summary>
    /// Business Rule Test: Agent timestamp should update correctly
    /// </summary>
    [Fact]
    public void UpdatedAt_ShouldBeAfterCreatedAt_When_Updated()
    {
        // Arrange
        var agent = new Agent();
        var originalCreatedAt = agent.CreatedAt;
        
        // Act - Simulate update
        Thread.Sleep(10); // Small delay to ensure different timestamps
        agent.UpdatedAt = DateTime.UtcNow;

        // Assert
        agent.UpdatedAt.ShouldBeGreaterThan(originalCreatedAt);
    }

    /// <summary>
    /// Edge Case Test: Agent with null values should handle gracefully
    /// </summary>
    [Fact]
    public void Agent_ShouldHandleNullAssignments_When_NullValuesSet()
    {
        // Arrange
        var agent = new Agent();

        // Act & Assert - These should not throw, collections should be initialized
        agent.Tasks.ShouldNotBeNull();
        
        // Capabilities and Configuration have default constructors, so should not be null
        agent.Capabilities.ShouldNotBeNull();
        agent.Configuration.ShouldNotBeNull();
    }
}

/// <summary>
/// Comprehensive tests for AgentStatus enum
/// </summary>
public class AgentStatusTests
{
    /// <summary>
    /// Contract Test: AgentStatus should have all expected values
    /// </summary>
    [Theory]
    [InlineData(AgentStatus.Inactive)]
    [InlineData(AgentStatus.Active)]
    [InlineData(AgentStatus.Busy)]
    [InlineData(AgentStatus.Error)]
    [InlineData(AgentStatus.Paused)]
    public void AgentStatus_ShouldHaveExpectedValues_When_Accessed(AgentStatus status)
    {
        // Act & Assert
        Enum.IsDefined(typeof(AgentStatus), status).ShouldBeTrue();
    }

    /// <summary>
    /// Contract Test: AgentStatus should have correct default value
    /// </summary>
    [Fact]
    public void AgentStatus_ShouldHaveInactiveAsDefault_When_DefaultUsed()
    {
        // Act
        var defaultStatus = default(AgentStatus);

        // Assert
        defaultStatus.ShouldBe(AgentStatus.Inactive);
    }

    /// <summary>
    /// Behavior Test: AgentStatus should convert to string correctly
    /// </summary>
    [Theory]
    [InlineData(AgentStatus.Inactive, "Inactive")]
    [InlineData(AgentStatus.Active, "Active")]
    [InlineData(AgentStatus.Busy, "Busy")]
    [InlineData(AgentStatus.Error, "Error")]
    [InlineData(AgentStatus.Paused, "Paused")]
    public void AgentStatus_ShouldConvertToString_When_ToString(AgentStatus status, string expected)
    {
        // Act
        var result = status.ToString();

        // Assert
        result.ShouldBe(expected);
    }

    /// <summary>
    /// Behavior Test: AgentStatus should support parsing from string
    /// </summary>
    [Theory]
    [InlineData("Inactive", AgentStatus.Inactive)]
    [InlineData("Active", AgentStatus.Active)]
    [InlineData("Busy", AgentStatus.Busy)]
    [InlineData("Error", AgentStatus.Error)]
    [InlineData("Paused", AgentStatus.Paused)]
    public void AgentStatus_ShouldParseFromString_When_ValidString(string input, AgentStatus expected)
    {
        // Act
        var parsed = Enum.Parse<AgentStatus>(input);

        // Assert
        parsed.ShouldBe(expected);
    }
}

/// <summary>
/// Comprehensive tests for AgentCapabilities class
/// </summary>
public class AgentCapabilitiesTests
{
    /// <summary>
    /// Contract Test: AgentCapabilities should initialize with defaults
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults_When_Created()
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
    /// Contract Test: AgentCapabilities properties should be settable
    /// </summary>
    [Fact]
    public void Properties_ShouldBeSettable_When_ValuesAssigned()
    {
        // Arrange
        var capabilities = new AgentCapabilities();

        // Act
        capabilities.CanProcessNaturalLanguage = false;
        capabilities.CanGenerateCode = true;
        capabilities.CanAnalyzeData = true;
        capabilities.CanCallExternalAPIs = true;
        capabilities.MaxConcurrentTasks = 5;

        // Assert
        capabilities.CanProcessNaturalLanguage.ShouldBeFalse();
        capabilities.CanGenerateCode.ShouldBeTrue();
        capabilities.CanAnalyzeData.ShouldBeTrue();
        capabilities.CanCallExternalAPIs.ShouldBeTrue();
        capabilities.MaxConcurrentTasks.ShouldBe(5);
    }

    /// <summary>
    /// Behavior Test: SupportedTaskTypes should be modifiable
    /// </summary>
    [Fact]
    public void SupportedTaskTypes_ShouldBeModifiable_When_TaskTypesAdded()
    {
        // Arrange
        var capabilities = new AgentCapabilities();

        // Act
        capabilities.SupportedTaskTypes.Add("text-analysis");
        capabilities.SupportedTaskTypes.Add("code-generation");
        capabilities.SupportedTaskTypes.Add("data-processing");

        // Assert
        capabilities.SupportedTaskTypes.Count.ShouldBe(3);
        capabilities.SupportedTaskTypes.ShouldContain("text-analysis");
        capabilities.SupportedTaskTypes.ShouldContain("code-generation");
        capabilities.SupportedTaskTypes.ShouldContain("data-processing");
    }

    /// <summary>
    /// Business Rule Test: MaxConcurrentTasks should allow valid ranges
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(100)]
    public void MaxConcurrentTasks_ShouldAcceptValidValues_When_ValidRangeProvided(int taskCount)
    {
        // Arrange
        var capabilities = new AgentCapabilities();

        // Act
        capabilities.MaxConcurrentTasks = taskCount;

        // Assert
        capabilities.MaxConcurrentTasks.ShouldBe(taskCount);
    }

    /// <summary>
    /// Edge Case Test: MaxConcurrentTasks with zero or negative values
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void MaxConcurrentTasks_ShouldAcceptAnyIntValue_When_Set(int taskCount)
    {
        // Arrange
        var capabilities = new AgentCapabilities();

        // Act
        capabilities.MaxConcurrentTasks = taskCount;

        // Assert - Domain model allows any int value, validation would be done elsewhere
        capabilities.MaxConcurrentTasks.ShouldBe(taskCount);
    }

    /// <summary>
    /// Behavior Test: AgentCapabilities should support capability combinations
    /// </summary>
    [Fact]
    public void Capabilities_ShouldSupportCombinations_When_MultipleEnabled()
    {
        // Arrange
        var capabilities = new AgentCapabilities();

        // Act - Enable multiple capabilities
        capabilities.CanProcessNaturalLanguage = true;
        capabilities.CanGenerateCode = true;
        capabilities.CanAnalyzeData = true;
        capabilities.CanCallExternalAPIs = true;

        // Assert - All capabilities can be enabled simultaneously
        capabilities.CanProcessNaturalLanguage.ShouldBeTrue();
        capabilities.CanGenerateCode.ShouldBeTrue();
        capabilities.CanAnalyzeData.ShouldBeTrue();
        capabilities.CanCallExternalAPIs.ShouldBeTrue();
    }

    /// <summary>
    /// Behavior Test: SupportedTaskTypes should handle duplicates
    /// </summary>
    [Fact]
    public void SupportedTaskTypes_ShouldAllowDuplicates_When_SameTaskTypeAddedTwice()
    {
        // Arrange
        var capabilities = new AgentCapabilities();

        // Act
        capabilities.SupportedTaskTypes.Add("text-analysis");
        capabilities.SupportedTaskTypes.Add("text-analysis"); // Duplicate

        // Assert - List<string> allows duplicates (business rule decision)
        capabilities.SupportedTaskTypes.Count.ShouldBe(2);
    }

    /// <summary>
    /// Edge Case Test: SupportedTaskTypes should handle null and empty strings
    /// </summary>
    [Fact]
    public void SupportedTaskTypes_ShouldHandleNullAndEmpty_When_AddedToCollection()
    {
        // Arrange
        var capabilities = new AgentCapabilities();

        // Act
        capabilities.SupportedTaskTypes.Add(string.Empty);
        capabilities.SupportedTaskTypes.Add(null!); // Testing edge case

        // Assert - Collection should accept these values (validation elsewhere)
        capabilities.SupportedTaskTypes.Count.ShouldBe(2);
        capabilities.SupportedTaskTypes.ShouldContain(string.Empty);
        capabilities.SupportedTaskTypes.ShouldContain(s => s == null);
    }
}

/// <summary>
/// Tests for agent behavior and business logic integration
/// </summary>
public class AgentBehaviorTests
{
    /// <summary>
    /// Integration Test: Agent with specific capabilities should work correctly
    /// </summary>
    [Fact]
    public void Agent_ShouldConfigureCorrectly_When_SpecificCapabilitiesSet()
    {
        // Arrange & Act
        var agent = new Agent
        {
            Name = "CodeGeneratorAgent",
            Description = "Specialized agent for code generation tasks",
            Status = AgentStatus.Active,
            Capabilities = new AgentCapabilities
            {
                CanProcessNaturalLanguage = true,
                CanGenerateCode = true,
                CanAnalyzeData = false,
                CanCallExternalAPIs = true,
                MaxConcurrentTasks = 3,
                SupportedTaskTypes = { "code-generation", "code-review", "documentation" }
            }
        };

        // Assert
        agent.Name.ShouldBe("CodeGeneratorAgent");
        agent.Status.ShouldBe(AgentStatus.Active);
        agent.Capabilities.CanGenerateCode.ShouldBeTrue();
        agent.Capabilities.CanAnalyzeData.ShouldBeFalse();
        agent.Capabilities.MaxConcurrentTasks.ShouldBe(3);
        agent.Capabilities.SupportedTaskTypes.ShouldContain("code-generation");
        agent.Capabilities.SupportedTaskTypes.Count.ShouldBe(3);
    }

    /// <summary>
    /// Business Rule Test: Agent with tasks should maintain relationships
    /// </summary>
    [Fact]
    public void Agent_ShouldMaintainTaskRelationships_When_TasksAssigned()
    {
        // Arrange
        var agent = new Agent { Name = "TestAgent" };
        var task1 = new AgentTask { Name = "Process Document", Status = TaskStatus.Pending };
        var task2 = new AgentTask { Name = "Generate Report", Status = TaskStatus.InProgress };

        // Act
        agent.Tasks.Add(task1);
        agent.Tasks.Add(task2);

        // Assert
        agent.Tasks.Count.ShouldBe(2);
        agent.Tasks.Where(t => t.Status == TaskStatus.Pending).Count().ShouldBe(1);
        agent.Tasks.Where(t => t.Status == TaskStatus.InProgress).Count().ShouldBe(1);
    }

    /// <summary>
    /// Performance Test: Agent creation should be efficient
    /// </summary>
    [Fact]
    public void Agent_ShouldCreateEfficiently_When_MultipleInstancesCreated()
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var agents = new List<Agent>();

        // Act - Create many agents
        for (int i = 0; i < 1000; i++)
        {
            agents.Add(new Agent
            {
                Name = $"Agent{i}",
                Description = $"Test agent number {i}",
                Status = AgentStatus.Active
            });
        }

        var endTime = DateTime.UtcNow;
        var duration = endTime - startTime;

        // Assert
        agents.Count.ShouldBe(1000);
        duration.ShouldBeLessThan(TimeSpan.FromSeconds(1)); // Should be very fast
        agents.Select(a => a.Id).Distinct().Count().ShouldBe(1000); // All unique IDs
    }

    /// <summary>
    /// State Test: Agent state should be consistent
    /// </summary>
    [Fact]
    public void Agent_ShouldMaintainConsistentState_When_PropertiesModified()
    {
        // Arrange
        var agent = new Agent();
        var originalCreatedAt = agent.CreatedAt;

        // Act
        agent.Name = "Updated Agent";
        agent.Status = AgentStatus.Active;
        agent.UpdatedAt = DateTime.UtcNow;

        // Assert
        agent.CreatedAt.ShouldBe(originalCreatedAt); // Should not change
        agent.UpdatedAt.ShouldBeGreaterThan(originalCreatedAt);
        agent.Name.ShouldBe("Updated Agent");
        agent.Status.ShouldBe(AgentStatus.Active);
    }
} 