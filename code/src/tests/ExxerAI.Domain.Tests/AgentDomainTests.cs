using ExxerAI.Domain;
using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests;

/// <summary>
/// Comprehensive unit tests for Agent domain entity
/// </summary>
public class AgentDomainTests
{
    /// <summary>
    /// Test fixture for Agent creation and initialization
    /// </summary>
    public class CreationTests
    {
        [Fact]
        public void Should_CreateAgentWithDefaults_When_InstantiatedEmpty()
        {
            // Arrange & Act
            var agent = new Agent();

            // Assert
            agent.Id.ShouldNotBe(Guid.Empty);
            agent.Name.ShouldBe(string.Empty);
            agent.Description.ShouldBe(string.Empty);
            agent.Status.ShouldBe(AgentStatus.Inactive);
            agent.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
            agent.UpdatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));

            // Verify nested objects are initialized
            agent.Capabilities.ShouldNotBeNull();
            agent.Configuration.ShouldNotBeNull();
            agent.Tasks.ShouldNotBeNull();
            agent.Tasks.ShouldBeEmpty();
        }

        [Fact]
        public void Should_CreateAgentWithValues_When_PropertiesSet()
        {
            // Arrange
            var agentId = Guid.NewGuid();
            var createdTime = DateTime.UtcNow;

            // Act
            var agent = new Agent
            {
                Id = agentId,
                Name = "TestBot",
                Description = "A comprehensive test agent",
                Status = AgentStatus.Active,
                CreatedAt = createdTime,
                UpdatedAt = createdTime
            };

            // Assert
            agent.Id.ShouldBe(agentId);
            agent.Name.ShouldBe("TestBot");
            agent.Description.ShouldBe("A comprehensive test agent");
            agent.Status.ShouldBe(AgentStatus.Active);
            agent.CreatedAt.ShouldBe(createdTime);
            agent.UpdatedAt.ShouldBe(createdTime);
        }
    }

    /// <summary>
    /// Test fixture for Agent agentStatus management
    /// </summary>
    public class StatusManagementTests
    {
        [Theory]
        [InlineData(AgentStatus.Inactive)]
        [InlineData(AgentStatus.Active)]
        [InlineData(AgentStatus.Busy)]
        [InlineData(AgentStatus.Error)]
        [InlineData(AgentStatus.Paused)]
        public void Should_AllowStatusChange_When_ValidStatusProvided(AgentStatus newStatus)
        {
            // Arrange
            var agent = new Agent { Name = "AgentStatus Test Agent" };

            // Act
            agent.Status = newStatus;

            // Assert
            agent.Status.ShouldBe(newStatus);
        }

        [Fact]
        public void Should_AllowStatusTransitions_When_ValidWorkflow()
        {
            // Arrange
            var agent = new Agent { Name = "Workflow Test Agent" };
            var statusFlow = new[]
            {
                AgentStatus.Active,
                AgentStatus.Busy,
                AgentStatus.Active,
                AgentStatus.Paused,
                AgentStatus.Active,
                AgentStatus.Error,
                AgentStatus.Inactive
            };

            // Act & Assert
            agent.Status.ShouldBe(AgentStatus.Inactive); // Initial state

            foreach (var status in statusFlow)
            {
                agent.Status = status;
                agent.Status.ShouldBe(status);
            }
        }
    }

    /// <summary>
    /// Test fixture for Agent capabilities
    /// </summary>
    public class CapabilitiesTests
    {
        [Fact]
        public void Should_InitializeDefaultCapabilities_When_AgentCreated()
        {
            // Arrange & Act
            var agent = new Agent { Name = "Capabilities Test" };

            // Assert
            agent.Capabilities.CanProcessNaturalLanguage.ShouldBeTrue();
            agent.Capabilities.CanGenerateCode.ShouldBeFalse();
            agent.Capabilities.CanAnalyzeData.ShouldBeFalse();
            agent.Capabilities.CanCallExternalAPIs.ShouldBeFalse();
            agent.Capabilities.MaxConcurrentTasks.ShouldBe(1);
            agent.Capabilities.SupportedTaskTypes.ShouldNotBeNull();
            agent.Capabilities.SupportedTaskTypes.ShouldBeEmpty();
        }

        [Fact]
        public void Should_ConfigureCapabilities_When_CapabilitiesSet()
        {
            // Arrange
            var agent = new Agent { Name = "Advanced Agent" };

            // Act
            agent.Capabilities.CanProcessNaturalLanguage = true;
            agent.Capabilities.CanGenerateCode = true;
            agent.Capabilities.CanAnalyzeData = true;
            agent.Capabilities.CanCallExternalAPIs = true;
            agent.Capabilities.MaxConcurrentTasks = 5;
            agent.Capabilities.SupportedTaskTypes.Add("CodeGeneration");
            agent.Capabilities.SupportedTaskTypes.Add("DataAnalysis");
            agent.Capabilities.SupportedTaskTypes.Add("APIIntegration");

            // Assert
            agent.Capabilities.CanProcessNaturalLanguage.ShouldBeTrue();
            agent.Capabilities.CanGenerateCode.ShouldBeTrue();
            agent.Capabilities.CanAnalyzeData.ShouldBeTrue();
            agent.Capabilities.CanCallExternalAPIs.ShouldBeTrue();
            agent.Capabilities.MaxConcurrentTasks.ShouldBe(5);
            agent.Capabilities.SupportedTaskTypes.Count.ShouldBe(3);
            agent.Capabilities.SupportedTaskTypes.ShouldContain("CodeGeneration");
            agent.Capabilities.SupportedTaskTypes.ShouldContain("DataAnalysis");
            agent.Capabilities.SupportedTaskTypes.ShouldContain("APIIntegration");
        }

        [Fact]
        public void Should_AllowTaskTypeManagement_When_SupportedTaskTypesModified()
        {
            // Arrange
            var agent = new Agent { Name = "Task Type Test" };

            // Act - Add task types
            agent.Capabilities.SupportedTaskTypes.Add("WebScraping");
            agent.Capabilities.SupportedTaskTypes.Add("ImageProcessing");
            agent.Capabilities.SupportedTaskTypes.Add("TextAnalysis");

            // Assert initial state
            agent.Capabilities.SupportedTaskTypes.Count.ShouldBe(3);

            // Act - Remove a task type
            agent.Capabilities.SupportedTaskTypes.Remove("ImageProcessing");

            // Assert final state
            agent.Capabilities.SupportedTaskTypes.Count.ShouldBe(2);
            agent.Capabilities.SupportedTaskTypes.ShouldContain("WebScraping");
            agent.Capabilities.SupportedTaskTypes.ShouldContain("TextAnalysis");
            agent.Capabilities.SupportedTaskTypes.ShouldNotContain("ImageProcessing");
        }
    }

    /// <summary>
    /// Test fixture for Agent configuration
    /// </summary>
    public class ConfigurationTests
    {
        [Fact]
        public void Should_InitializeDefaultConfiguration_When_AgentCreated()
        {
            // Arrange & Act
            var agent = new Agent { Name = "Configuration Test" };

            // Assert
            agent.Configuration.TaskTimeoutSeconds.ShouldBe(300);
            agent.Configuration.MaxRetries.ShouldBe(3);
            agent.Configuration.Priority.ShouldBe(1);
            agent.Configuration.CustomProperties.ShouldNotBeNull();
            agent.Configuration.CustomProperties.ShouldBeEmpty();
        }

        [Fact]
        public void Should_ConfigureSettings_When_ConfigurationModified()
        {
            // Arrange
            var agent = new Agent { Name = "Custom Config Agent" };

            // Act
            agent.Configuration.TaskTimeoutSeconds = 600;
            agent.Configuration.MaxRetries = 5;
            agent.Configuration.Priority = 10;
            agent.Configuration.CustomProperties["environment"] = "production";
            agent.Configuration.CustomProperties["version"] = "2.0";
            agent.Configuration.CustomProperties["maxMemoryMB"] = 1024;

            // Assert
            agent.Configuration.TaskTimeoutSeconds.ShouldBe(600);
            agent.Configuration.MaxRetries.ShouldBe(5);
            agent.Configuration.Priority.ShouldBe(10);
            agent.Configuration.CustomProperties.Count.ShouldBe(3);
            agent.Configuration.CustomProperties["environment"].ShouldBe("production");
            agent.Configuration.CustomProperties["version"].ShouldBe("2.0");
            agent.Configuration.CustomProperties["maxMemoryMB"].ShouldBe(1024);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(60, 60)]
        [InlineData(3600, 3600)]
        [InlineData(86400, 86400)] // 24 hours
        public void Should_AcceptValidTimeouts_When_TimeoutSecondsSet(int timeoutSeconds, int expectedValue)
        {
            // Arrange
            var agent = new Agent { Name = "Timeout Test" };

            // Act
            agent.Configuration.TaskTimeoutSeconds = timeoutSeconds;

            // Assert
            agent.Configuration.TaskTimeoutSeconds.ShouldBe(expectedValue);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public void Should_AcceptValidRetries_When_MaxRetriesSet(int maxRetries)
        {
            // Arrange
            var agent = new Agent { Name = "Retry Test" };

            // Act
            agent.Configuration.MaxRetries = maxRetries;

            // Assert
            agent.Configuration.MaxRetries.ShouldBe(maxRetries);
        }
    }

    /// <summary>
    /// Test fixture for Agent task management
    /// </summary>
    public class TaskManagementTests
    {
        [Fact]
        public void Should_InitializeEmptyTaskCollection_When_AgentCreated()
        {
            // Arrange & Act
            var agent = new Agent { Name = "Task Collection Test" };

            // Assert
            agent.Tasks.ShouldNotBeNull();
            agent.Tasks.ShouldBeEmpty();
            agent.Tasks.Count.ShouldBe(0);
        }

        [Fact]
        public void Should_AllowTaskAssignment_When_TasksAdded()
        {
            // Arrange
            var agent = new Agent { Name = "Task Assignment Test" };
            var task1 = new AgentTask { Title = "Task 1", TaskType = "Analysis" };
            var task2 = new AgentTask { Title = "Task 2", TaskType = "Processing" };

            // Act
            agent.Tasks.Add(task1);
            agent.Tasks.Add(task2);

            // Assert
            agent.Tasks.Count.ShouldBe(2);
            agent.Tasks.ShouldContain(task1);
            agent.Tasks.ShouldContain(task2);
        }

        [Fact]
        public void Should_AllowTaskRemoval_When_TasksRemoved()
        {
            // Arrange
            var agent = new Agent { Name = "Task Removal Test" };
            var task1 = new AgentTask { Title = "Task 1", TaskType = "Analysis" };
            var task2 = new AgentTask { Title = "Task 2", TaskType = "Processing" };
            var task3 = new AgentTask { Title = "Task 3", TaskType = "Reporting" };

            agent.Tasks.Add(task1);
            agent.Tasks.Add(task2);
            agent.Tasks.Add(task3);

            // Act
            agent.Tasks.Remove(task2);

            // Assert
            agent.Tasks.Count.ShouldBe(2);
            agent.Tasks.ShouldContain(task1);
            agent.Tasks.ShouldNotContain(task2);
            agent.Tasks.ShouldContain(task3);
        }
    }

    /// <summary>
    /// Test fixture for Agent business rules and validation
    /// </summary>
    public class BusinessRulesTests
    {
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null!)]
        public void Should_AllowEmptyName_When_ValidationNotEnforced(string? name)
        {
            // Arrange & Act
            var agent = new Agent { Name = name ?? string.Empty };

            // Assert - Domain model allows empty values, validation happens at service/API level
            agent.Name.ShouldBe(name ?? string.Empty);
        }

        [Fact]
        public void Should_HandleLongStrings_When_WithinLimits()
        {
            // Arrange
            var longName = new string('A', 100); // Max length per data annotation
            var longDescription = new string('B', 500); // Max length per data annotation

            // Act
            var agent = new Agent
            {
                Name = longName,
                Description = longDescription
            };

            // Assert
            agent.Name.ShouldBe(longName);
            agent.Description.ShouldBe(longDescription);
        }

        [Fact]
        public void Should_TrackTimestamps_When_AgentModified()
        {
            // Arrange
            var agent = new Agent
            {
                Name = "Timestamp Test",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10)
            };
            var originalCreatedAt = agent.CreatedAt;

            // Act
            agent.UpdatedAt = DateTime.UtcNow;
            agent.Name = "Updated Agent";

            // Assert
            agent.CreatedAt.ShouldBe(originalCreatedAt); // Should not change
            agent.UpdatedAt.ShouldBeGreaterThan(originalCreatedAt);
        }
    }

    /// <summary>
    /// Test fixture for Agent edge cases and extreme scenarios
    /// </summary>
    public class EdgeCaseTests
    {
        [Fact]
        public void Should_HandleExtremeTaskCounts_When_MaxConcurrentTasksSet()
        {
            // Arrange
            var agent = new Agent { Name = "Extreme Task Count Test" };

            // Act
            agent.Capabilities.MaxConcurrentTasks = int.MaxValue;

            // Assert
            agent.Capabilities.MaxConcurrentTasks.ShouldBe(int.MaxValue);
        }

        [Fact]
        public void Should_HandleLargeCustomProperties_When_ManyPropertiesAdded()
        {
            // Arrange
            var agent = new Agent { Name = "Large Properties Test" };

            // Act - Add many custom properties
            for (int i = 0; i < 1000; i++)
            {
                agent.Configuration.CustomProperties[$"prop{i}"] = $"value{i}";
            }

            // Assert
            agent.Configuration.CustomProperties.Count.ShouldBe(1000);
            agent.Configuration.CustomProperties["prop0"].ShouldBe("value0");
            agent.Configuration.CustomProperties["prop999"].ShouldBe("value999");
        }

        [Fact]
        public void Should_HandleComplexCustomProperties_When_DifferentTypesUsed()
        {
            // Arrange
            var agent = new Agent { Name = "Complex Properties Test" };

            // Act
            agent.Configuration.CustomProperties["string"] = "text value";
            agent.Configuration.CustomProperties["int"] = 42;
            agent.Configuration.CustomProperties["double"] = 3.14159;
            agent.Configuration.CustomProperties["bool"] = true;
            agent.Configuration.CustomProperties["date"] = DateTime.UtcNow;
            agent.Configuration.CustomProperties["guid"] = Guid.NewGuid();
            agent.Configuration.CustomProperties["array"] = new[] { 1, 2, 3, 4, 5 };

            // Assert
            agent.Configuration.CustomProperties.Count.ShouldBe(7);
            agent.Configuration.CustomProperties["string"].ShouldBe("text value");
            agent.Configuration.CustomProperties["int"].ShouldBe(42);
            agent.Configuration.CustomProperties["double"].ShouldBe(3.14159);
            agent.Configuration.CustomProperties["bool"].ShouldBe(true);
            agent.Configuration.CustomProperties["date"].ShouldBeOfType<DateTime>();
            agent.Configuration.CustomProperties["guid"].ShouldBeOfType<Guid>();
            agent.Configuration.CustomProperties["array"].ShouldBeOfType<int[]>();
        }

        [Fact]
        public void Should_HandleExtremeDates_When_TimestampsSet()
        {
            // Arrange
            var agent = new Agent { Name = "Extreme Dates Test" };
            var veryEarlyDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var veryLateDate = new DateTime(2200, 12, 31, 23, 59, 59, DateTimeKind.Utc);

            // Act
            agent.CreatedAt = veryEarlyDate;
            agent.UpdatedAt = veryLateDate;

            // Assert
            agent.CreatedAt.ShouldBe(veryEarlyDate);
            agent.UpdatedAt.ShouldBe(veryLateDate);
        }
    }

    /// <summary>
    /// Test fixture for Agent collections and relationships
    /// </summary>
    public class RelationshipTests
    {
        [Fact]
        public void Should_MaintainTaskReferences_When_TasksAssociatedWithAgent()
        {
            // Arrange
            var agent = new Agent { Name = "Relationship Test Agent" };
            var tasks = Enumerable.Range(1, 5)
                .Select(i => new AgentTask
                {
                    Title = $"Task {i}",
                    TaskType = "TestType",
                    AssignedAgentId = agent.Id
                })
                .ToArray();

            // Act
            foreach (var task in tasks)
            {
                agent.Tasks.Add(task);
                task.AssignedAgent = agent;
            }

            // Assert
            agent.Tasks.Count.ShouldBe(5);
            foreach (var task in agent.Tasks)
            {
                task.AssignedAgentId.ShouldBe(agent.Id);
                task.AssignedAgent.ShouldBe(agent);
            }
        }

        [Fact]
        public void Should_AllowBidirectionalRelationship_When_AgentAndTaskLinked()
        {
            // Arrange
            var agent = new Agent { Name = "Bidirectional Test Agent" };
            var task = new AgentTask
            {
                Title = "Bidirectional Task",
                TaskType = "TestType"
            };

            // Act
            task.AssignedAgentId = agent.Id;
            task.AssignedAgent = agent;
            agent.Tasks.Add(task);

            // Assert
            task.AssignedAgentId.ShouldBe(agent.Id);
            task.AssignedAgent.ShouldBe(agent);
            agent.Tasks.ShouldContain(task);
            agent.Tasks.Count.ShouldBe(1);
        }
    }

    /// <summary>
    /// Test fixture for Agent comparison and equality
    /// </summary>
    public class EqualityTests
    {
        [Fact]
        public void Should_BeEqual_When_SameAgentInstance()
        {
            // Arrange
            var agent = new Agent { Name = "Equality Test" };

            // Act & Assert
            agent.ShouldBe(agent);
            ReferenceEquals(agent, agent).ShouldBeTrue();
        }

        [Fact]
        public void Should_BeDifferent_When_DifferentAgentInstances()
        {
            // Arrange
            var agent1 = new Agent { Name = "Agent 1" };
            var agent2 = new Agent { Name = "Agent 2" };

            // Act & Assert
            agent1.ShouldNotBe(agent2);
            agent1.Id.ShouldNotBe(agent2.Id);
            ReferenceEquals(agent1, agent2).ShouldBeFalse();
        }

        [Fact]
        public void Should_HaveUniqueIds_When_MultipleAgentsCreated()
        {
            // Arrange & Act
            var agents = Enumerable.Range(1, 100)
                .Select(i => new Agent { Name = $"Agent {i}" })
                .ToArray();

            // Assert
            var uniqueIds = agents.Select(a => a.Id).Distinct().ToArray();
            uniqueIds.Length.ShouldBe(100);
        }
    }
}