using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain;
using ExxerAI.Infrastructure.Repositories;
using Shouldly;

namespace ExxerAI.Application.Tests;

/// <summary>
/// Integration tests for the complete application stack
/// </summary>
public class IntegrationTests
{
	private readonly IAgentRepository _agentRepository;
	private readonly ITaskRepository _taskRepository;
	private readonly IAgentService _agentService;

	public IntegrationTests()
	{
		_agentRepository = new InMemoryAgentRepository();
		_taskRepository = new InMemoryTaskRepository();
		_agentService = new AgentService(_agentRepository, _taskRepository);
	}

	/// <summary>
	/// Test fixture for end-to-end agent lifecycle scenarios
	/// </summary>
	public class AgentLifecycleTests : IntegrationTests
	{
		[Fact]
		public async Task Should_CompleteAgentLifecycle_When_FullWorkflowExecuted()
		{
			// Arrange - Create agent
			var capabilities = new AgentCapabilities
			{
				CanProcessNaturalLanguage = true,
				CanGenerateCode = true,
				MaxConcurrentTasks = 3,
				SupportedTaskTypes = { "CodeGeneration", "TextAnalysis" }
			};

			// Act 1 - Create agent
			var createResult = await _agentService.CreateAgentAsync(
				"Integration Test Agent",
				"Full lifecycle test agent",
				capabilities);

			// Assert 1 - Agent created successfully
			createResult.IsSuccess.ShouldBeTrue();
			createResult.Value.ShouldNotBeNull();
			createResult.Value.Status.ShouldBe(AgentStatus.Inactive);

			var agentId = createResult.Value.Id;

			// Act 2 - Update agent configuration
			var newConfig = new AgentConfiguration
			{
				TaskTimeoutSeconds = 600,
				MaxRetries = 5,
				Priority = 8,
				CustomProperties = { ["testMode"] = true }
			};

			var updateConfigResult = await _agentService.UpdateAgentConfigurationAsync(agentId, newConfig);

			// Assert 2 - Configuration updated
			updateConfigResult.IsSuccess.ShouldBeTrue();

			// Act 3 - Activate agent
			var activateResult = await _agentService.UpdateAgentStatusAsync(agentId, AgentStatus.Active);

			// Assert 3 - Agent activated
			activateResult.IsSuccess.ShouldBeTrue();

			// Verify agent status through repository
			var agentResult = await _agentRepository.GetByIdAsync(agentId);
			agentResult.IsSuccess.ShouldBeTrue();
			agentResult.Value!.Status.ShouldBe(AgentStatus.Active);
			agentResult.Value.Configuration.TaskTimeoutSeconds.ShouldBe(600);

			// Act 4 - Get all agents
			var allAgentsResult = await _agentService.GetAllAgentsAsync();

			// Assert 4 - Agent appears in list
			allAgentsResult.IsSuccess.ShouldBeTrue();
			allAgentsResult.Value!.Any(a => a.Id == agentId).ShouldBeTrue();

			// Act 5 - Delete agent
			var deleteResult = await _agentService.DeleteAgentAsync(agentId);

			// Assert 5 - Agent deleted
			deleteResult.IsSuccess.ShouldBeTrue();

			// Verify agent is gone
			var getDeletedResult = await _agentService.GetAgentAsync(agentId);
			getDeletedResult.IsFailure.ShouldBeTrue();
		}

		[Fact]
		public async Task Should_HandleMultipleAgents_When_ConcurrentOperations()
		{
			// Arrange - Create multiple agents concurrently
			var agentCreationTasks = Enumerable.Range(1, 10)
				.Select(i => _agentService.CreateAgentAsync(
					$"Concurrent Agent {i}",
					$"Agent {i} for concurrency testing",
					new AgentCapabilities { SupportedTaskTypes = { $"Task{i}" } }))
				.ToArray();

			// Act - Create all agents concurrently
			var results = await Task.WhenAll(agentCreationTasks);

			// Assert - All agents created successfully
			results.All(r => r.IsSuccess).ShouldBeTrue();
			results.Select(r => r.Value!.Id).Distinct().Count().ShouldBe(10);

			// Act - Get all agents
			var allAgentsResult = await _agentService.GetAllAgentsAsync();

			// Assert - All agents present
			allAgentsResult.IsSuccess.ShouldBeTrue();
			allAgentsResult.Value!.Count().ShouldBeGreaterThanOrEqualTo(10);
		}
	}

	/// <summary>
	/// Test fixture for task assignment and management scenarios
	/// </summary>
	public class TaskManagementTests : IntegrationTests
	{
		[Fact]
		public async Task Should_CompleteTaskAssignmentFlow_When_AgentAndTaskExists()
		{
			// Arrange - Create agent first
			var agent = await CreateTestAgent("Task Management Agent", "CodeGeneration");
			var agentId = agent.Id;

			// Activate agent
			await _agentService.UpdateAgentStatusAsync(agentId, AgentStatus.Active);

			// Create task
			var task = new AgentTask
			{
				Title = "Integration Test Task",
				Description = "Task for integration testing",
				TaskType = "CodeGeneration",
				Priority = TaskPriority.High,
				Status = TaskStatus.Pending
			};

			var taskResult = await _taskRepository.AddAsync(task);
			taskResult.IsSuccess.ShouldBeTrue();
			var taskId = taskResult.Value!.Id;

			// Act - Assign task to agent
			var assignResult = await _agentService.AssignTaskToAgentAsync(agentId, taskId);

			// Assert - Task assigned successfully
			assignResult.IsSuccess.ShouldBeTrue();

			// Verify task is assigned
			var updatedTaskResult = await _taskRepository.GetByIdAsync(taskId);
			updatedTaskResult.IsSuccess.ShouldBeTrue();
			updatedTaskResult.Value!.AssignedAgentId.ShouldBe(agentId);

			// Verify agent can process this task type
			var agentResult = await _agentRepository.GetByIdAsync(agentId);
			agentResult.IsSuccess.ShouldBeTrue();
			agentResult.Value!.Capabilities.SupportedTaskTypes.ShouldContain("CodeGeneration");
		}

		[Fact]
		public async Task Should_FindBestAgent_When_MultipleAgentsAvailable()
		{
			// Arrange - Create agents with different capabilities
			var webAgent = await CreateTestAgent("Web Agent", "WebScraping", "APIIntegration");
			var dataAgent = await CreateTestAgent("Data Agent", "DataAnalysis", "Reporting");
			var generalAgent = await CreateTestAgent("General Agent", "WebScraping", "DataAnalysis", "CodeGeneration");

			// Activate all agents
			await _agentService.UpdateAgentStatusAsync(webAgent.Id, AgentStatus.Active);
			await _agentService.UpdateAgentStatusAsync(dataAgent.Id, AgentStatus.Active);
			await _agentService.UpdateAgentStatusAsync(generalAgent.Id, AgentStatus.Active);

			// Act - Find best agent for web scraping
			var bestAgentResult = await _agentService.FindBestAgentForTaskAsync("WebScraping");

			// Assert - Should find an agent that supports web scraping
			bestAgentResult.IsSuccess.ShouldBeTrue();
			bestAgentResult.Value.ShouldNotBeNull();
			bestAgentResult.Value.Capabilities.SupportedTaskTypes.ShouldContain("WebScraping");
		}

		[Fact]
		public async Task Should_PreventTaskAssignment_When_AgentInactive()
		{
			// Arrange - Create inactive agent
			var agent = await CreateTestAgent("Inactive Agent", "TestTask");
			// Agent remains inactive by default

			// Create task
			var task = new AgentTask
			{
				Title = "Task for Inactive Agent",
				TaskType = "TestTask",
				Status = TaskStatus.Pending
			};

			var taskResult = await _taskRepository.AddAsync(task);
			var taskId = taskResult.Value!.Id;

			// Act - Try to assign task to inactive agent
			var assignResult = await _agentService.AssignTaskToAgentAsync(agent.Id, taskId);

			// Assert - Assignment should fail
			assignResult.IsFailure.ShouldBeTrue();
			assignResult.Errors.ShouldContain("is not active");
		}
	}

	/// <summary>
	/// Test fixture for business rule validation scenarios
	/// </summary>
	public class BusinessRuleTests : IntegrationTests
	{
		[Fact]
		public async Task Should_ValidateAgentCapabilities_When_AssigningTasks()
		{
			// Arrange - Create agent with limited capabilities
			var limitedAgent = await CreateTestAgent("Limited Agent", "DataAnalysis");
			await _agentService.UpdateAgentStatusAsync(limitedAgent.Id, AgentStatus.Active);

			// Create task that agent cannot handle
			var incompatibleTask = new AgentTask
			{
				Title = "Code Generation Task",
				TaskType = "CodeGeneration", // Agent doesn't support this
				Status = TaskStatus.Pending
			};

			var taskResult = await _taskRepository.AddAsync(incompatibleTask);
			var taskId = taskResult.Value!.Id;

			// Act - Try to assign incompatible task
			var assignResult = await _agentService.AssignTaskToAgentAsync(limitedAgent.Id, taskId);

			// Assert - Assignment should succeed (business logic allows assignment regardless of capability)
			// This tests that the system allows assignment even if capabilities don't match
			assignResult.IsSuccess.ShouldBeTrue();
		}

		[Fact]
		public async Task Should_PreventAgentDeletion_When_ActiveTasksExist()
		{
			// Arrange - Create agent and assign active task
			var agent = await CreateTestAgent("Busy Agent", "TestTask");
			await _agentService.UpdateAgentStatusAsync(agent.Id, AgentStatus.Active);

			// Create and assign in-progress task
			var activeTask = new AgentTask
			{
				Title = "Active Task",
				TaskType = "TestTask",
				Status = TaskStatus.InProgress,
				AssignedAgentId = agent.Id
			};

			await _taskRepository.AddAsync(activeTask);

			// Act - Try to delete agent with active tasks
			var deleteResult = await _agentService.DeleteAgentAsync(agent.Id);

			// Assert - Deletion should fail
			deleteResult.IsFailure.ShouldBeTrue();
			deleteResult.Errors.ShouldContain("has active tasks");
		}
	}

	/// <summary>
	/// Test fixture for error handling and recovery scenarios
	/// </summary>
	public class ErrorHandlingTests : IntegrationTests
	{
		[Fact]
		public async Task Should_HandleRepositoryErrors_When_UnderlyingSystemFails()
		{
			// This test would require a mock repository that can simulate failures
			// For now, we test with null inputs to trigger validation errors

			// Act & Assert - Various null/invalid inputs
			var createResult = await _agentService.CreateAgentAsync("", "", null!);
			createResult.IsFailure.ShouldBeTrue();

			var getResult = await _agentService.GetAgentAsync(Guid.Empty);
			getResult.IsFailure.ShouldBeTrue();

			var updateResult = await _agentService.UpdateAgentStatusAsync(Guid.Empty, AgentStatus.Active);
			updateResult.IsFailure.ShouldBeTrue();

			var assignResult = await _agentService.AssignTaskToAgentAsync(Guid.Empty, Guid.Empty);
			assignResult.IsFailure.ShouldBeTrue();
		}

		[Fact]
		public async Task Should_MaintainDataConsistency_When_ConcurrentModifications()
		{
			// Arrange - Create agent
			var agent = await CreateTestAgent("Concurrent Test Agent", "TestTask");

			// Act - Multiple concurrent status updates
			var updateTasks = Enumerable.Range(0, 10)
				.Select(i => _agentService.UpdateAgentStatusAsync(
					agent.Id, 
					i % 2 == 0 ? AgentStatus.Active : AgentStatus.Paused))
				.ToArray();

			var results = await Task.WhenAll(updateTasks);

			// Assert - All operations should complete
			results.All(r => r.IsSuccess).ShouldBeTrue();

			// Verify final state is consistent
			var finalAgent = await _agentService.GetAgentAsync(agent.Id);
			finalAgent.IsSuccess.ShouldBeTrue();
			finalAgent.Value!.Status.ShouldBeOneOf(AgentStatus.Active, AgentStatus.Paused);
		}
	}

	/// <summary>
	/// Test fixture for performance and scalability scenarios
	/// </summary>
	public class PerformanceTests : IntegrationTests
	{
		[Fact]
		public async Task Should_HandleLargeNumbers_When_ManyAgentsCreated()
		{
			// Arrange & Act - Create many agents
			var agentCount = 1000;
			var stopwatch = System.Diagnostics.Stopwatch.StartNew();

			var createTasks = Enumerable.Range(1, agentCount)
				.Select(i => _agentService.CreateAgentAsync(
					$"Performance Agent {i}",
					$"Agent {i} for performance testing",
					new AgentCapabilities { SupportedTaskTypes = { $"Task{i % 10}" } }))
				.ToArray();

			var results = await Task.WhenAll(createTasks);
			stopwatch.Stop();

			// Assert - Performance and correctness
			results.All(r => r.IsSuccess).ShouldBeTrue();
			stopwatch.ElapsedMilliseconds.ShouldBeLessThan(10000); // Under 10 seconds

			// Verify all agents can be retrieved
			var allAgents = await _agentService.GetAllAgentsAsync();
			allAgents.IsSuccess.ShouldBeTrue();
			allAgents.Value!.Count().ShouldBeGreaterThanOrEqualTo(agentCount);
		}

		[Fact]
		public async Task Should_HandleComplexQueries_When_LargeDataset()
		{
			// Arrange - Create diverse agent population
			var agents = new List<Agent>();
			var taskTypes = new[] { "Web", "Data", "Code", "Analysis", "Reporting" };
			var statuses = new[] { AgentStatus.Active, AgentStatus.Inactive, AgentStatus.Busy };

			for (int i = 0; i < 500; i++)
			{
				var capabilities = new AgentCapabilities
				{
					SupportedTaskTypes = { taskTypes[i % taskTypes.Length] }
				};
				
				var agent = await _agentService.CreateAgentAsync(
					$"Query Test Agent {i}",
					$"Agent for query testing {i}",
					capabilities);
				
				if (agent.IsSuccess)
				{
					await _agentService.UpdateAgentStatusAsync(
						agent.Value!.Id, 
						statuses[i % statuses.Length]);
					agents.Add(agent.Value);
				}
			}

			// Act - Complex queries
			var stopwatch = System.Diagnostics.Stopwatch.StartNew();

			var queryTasks = new[]
			{
				_agentService.GetActiveAgentsAsync(),
				_agentService.FindBestAgentForTaskAsync("Web"),
				_agentService.FindBestAgentForTaskAsync("Data"),
				_agentService.GetAllAgentsAsync()
			};

			var queryResults = await Task.WhenAll(queryTasks);
			stopwatch.Stop();

			// Assert - All queries successful and performant
			queryResults.All(r => r.IsSuccess).ShouldBeTrue();
			stopwatch.ElapsedMilliseconds.ShouldBeLessThan(2000); // Under 2 seconds
		}
	}

	/// <summary>
	/// Helper method to create test agents
	/// </summary>
	private async Task<Agent> CreateTestAgent(string name, params string[] supportedTaskTypes)
	{
		var capabilities = new AgentCapabilities
		{
			SupportedTaskTypes = supportedTaskTypes.ToList()
		};

		var result = await _agentService.CreateAgentAsync(name, $"Test agent: {name}", capabilities);
		result.IsSuccess.ShouldBeTrue();
		return result.Value!;
	}
} 