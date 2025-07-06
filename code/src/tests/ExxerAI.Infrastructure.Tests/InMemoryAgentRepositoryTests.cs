using ExxerAI.Domain;
using ExxerAI.Domain.Entities;
using ExxerAI.Infrastructure.Repositories;
using Shouldly;

namespace ExxerAI.Infrastructure.Tests;

/// <summary>
/// Unit tests for InMemoryAgentRepository
/// </summary>
public class InMemoryAgentRepositoryTests
{
    private readonly InMemoryAgentRepository _repository;

    public InMemoryAgentRepositoryTests()
    {
        _repository = new InMemoryAgentRepository();
    }

    /// <summary>
    /// Test class for basic CRUD operations
    /// </summary>
    public class CrudOperationsTests : InMemoryAgentRepositoryTests
    {
        [Fact]
        public async Task Should_AddAgent_When_ValidAgentProvided()
        {
            // Arrange
            var agent = new Agent
            {
                Name = "Test Agent",
                Description = "Test Description",
                Capabilities = new AgentCapabilities { SupportedTaskTypes = ["test"] },
                Status = AgentStatus.Active
            };

            // Act
            var result = await _repository.AddAsync(agent, TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Id.ShouldNotBe(Guid.Empty);
            result.Data.Name.ShouldBe(agent.Name);
            result.Data.CreatedAt.ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Fact]
        public async Task Should_GetAgentById_When_AgentExists()
        {
            // Arrange
            var agent = new Agent
            {
                Name = "Test Agent",
                Description = "Test Description",
                Capabilities = new AgentCapabilities { SupportedTaskTypes = ["test"] },
                Status = AgentStatus.Active
            };

            var addResult = await _repository.AddAsync(agent, TestContext.Current.CancellationToken);
            var agentId = addResult.Data!.Id;

            // Act
            var result = await _repository.GetByIdAsync(agentId, TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Id.ShouldBe(agentId);
            result.Data.Name.ShouldBe(agent.Name);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_AgentNotFound()
        {
            // Act
            var result = await _repository.GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Agent not found");
        }

        [Fact]
        public async Task Should_UpdateAgent_When_AgentExists()
        {
            // Arrange
            var agent = new Agent
            {
                Name = "Test Agent",
                Description = "Test Description",
                Capabilities = new AgentCapabilities { SupportedTaskTypes = ["test"] },
                Status = AgentStatus.Active
            };

            var addResult = await _repository.AddAsync(agent, TestContext.Current.CancellationToken);
            var addedAgent = addResult.Data!;

            addedAgent.Name = "Updated Agent Name";
            addedAgent.Description = "Updated Description";

            // Act
            var result = await _repository.UpdateAsync(addedAgent, TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Name.ShouldBe("Updated Agent Name");
            result.Data.Description.ShouldBe("Updated Description");
            result.Data.UpdatedAt.ShouldBeGreaterThan(addedAgent.CreatedAt);
        }

        [Fact]
        public async Task Should_DeleteAgent_When_AgentExists()
        {
            // Arrange
            var agent = new Agent
            {
                Name = "Test Agent",
                Description = "Test Description",
                Capabilities = new AgentCapabilities { SupportedTaskTypes = ["test"] },
                Status = AgentStatus.Active
            };

            var addResult = await _repository.AddAsync(agent, TestContext.Current.CancellationToken);
            var agentId = addResult.Data!.Id;

            // Act
            var deleteResult = await _repository.DeleteAsync(agentId, TestContext.Current.CancellationToken);

            // Assert
            deleteResult.IsSuccess.ShouldBeTrue();

            // Verify agent is deleted
            var getResult = await _repository.GetByIdAsync(agentId, TestContext.Current.CancellationToken);
            getResult.IsFailure.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_GetAllAgents_When_AgentsExist()
        {
            // Arrange
            var agent1 = new Agent { Name = "Agent 1", Description = "Desc 1", Capabilities = new AgentCapabilities { SupportedTaskTypes = ["test"] } };
            var agent2 = new Agent { Name = "Agent 2", Description = "Desc 2", Capabilities = new AgentCapabilities { SupportedTaskTypes = ["test"] } };

            await _repository.AddAsync(agent1, TestContext.Current.CancellationToken);
            await _repository.AddAsync(agent2, TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.GetAllAsync(TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count().ShouldBeGreaterThanOrEqualTo(2);
        }
    }

    /// <summary>
    /// Test class for specialized query operations
    /// </summary>
    public class QueryOperationsTests : InMemoryAgentRepositoryTests
    {
        [Fact]
        public async Task Should_GetAgentsByStatus_When_StatusMatches()
        {
            // Arrange
            var activeAgent = new Agent
            {
                Name = "Active Agent",
                Description = "Active",
                Capabilities = new AgentCapabilities { SupportedTaskTypes = ["test"] },
                Status = AgentStatus.Active
            };
            var inactiveAgent = new Agent
            {
                Name = "Inactive Agent",
                Description = "Inactive",
                Capabilities = new AgentCapabilities { SupportedTaskTypes = ["test"] },
                Status = AgentStatus.Inactive
            };

            await _repository.AddAsync(activeAgent, TestContext.Current.CancellationToken);
            await _repository.AddAsync(inactiveAgent, TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.GetByStatusAsync(AgentStatus.Active, TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.All(a => a.Status == AgentStatus.Active).ShouldBeTrue();
            result.Data.Any(a => a.Name == "Active Agent").ShouldBeTrue();
            result.Data.Any(a => a.Name == "Inactive Agent").ShouldBeFalse();
        }

        [Fact]
        public async Task Should_FindAgentsByTaskType_When_TaskTypeSupported()
        {
            // Arrange
            var webAgent = new Agent
            {
                Name = "Web Agent",
                Description = "Web tasks",
                Capabilities = new AgentCapabilities { SupportedTaskTypes = ["web", "api"] },
                Status = AgentStatus.Active
            };
            var dataAgent = new Agent
            {
                Name = "Value Agent",
                Description = "Value tasks",
                Capabilities = new AgentCapabilities { SupportedTaskTypes = ["data", "analysis"] },
                Status = AgentStatus.Active
            };

            await _repository.AddAsync(webAgent, TestContext.Current.CancellationToken);
            await _repository.AddAsync(dataAgent, TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.FindByTaskTypeAsync("web", TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count().ShouldBe(1);
            result.Data.First().Name.ShouldBe("Web Agent");
        }

        [Fact]
        public async Task Should_ReturnEmptyList_When_NoAgentsMatchTaskType()
        {
            // Arrange
            var agent = new Agent
            {
                Name = "Test Agent",
                Description = "Test",
                Capabilities = new AgentCapabilities { SupportedTaskTypes = ["test"] },
                Status = AgentStatus.Active
            };

            await _repository.AddAsync(agent, TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.FindByTaskTypeAsync("nonexistent", TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count().ShouldBe(0);
        }

        [Fact]
        public async Task Should_GetAgentsWithTaskCount_When_Called()
        {
            // Arrange
            var agent = new Agent
            {
                Name = "Test Agent",
                Description = "Test",
                Capabilities = new AgentCapabilities { SupportedTaskTypes = ["test"] },
                Status = AgentStatus.Active
            };

            await _repository.AddAsync(agent, TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.GetAgentsWithTaskCountAsync(TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data.Count().ShouldBeGreaterThanOrEqualTo(1);

            var agentWithCount = result.Data.FirstOrDefault(atc => atc.Agent.Name == "Test Agent");
            agentWithCount.Agent.ShouldNotBeNull();
            agentWithCount.TaskCount.ShouldBe(0); // No tasks initially
        }
    }

    /// <summary>
    /// Test class for validation and error scenarios
    /// </summary>
    public class ValidationTests : InMemoryAgentRepositoryTests
    {
        [Fact]
        public async Task Should_ReturnFailure_When_AddingNullAgent()
        {
            // Act
            var result = await _repository.AddAsync(null!, TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Agent cannot be null");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_UpdatingNullAgent()
        {
            // Act
            var result = await _repository.UpdateAsync(null!, TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Agent cannot be null");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_UpdatingNonExistentAgent()
        {
            // Arrange
            var agent = new Agent
            {
                Id = Guid.NewGuid(),
                Name = "Test Agent",
                Description = "Test",
                Capabilities = new AgentCapabilities { SupportedTaskTypes = ["test"] }
            };

            // Act
            var result = await _repository.UpdateAsync(agent, TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Agent not found");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_DeletingNonExistentAgent()
        {
            // Act
            var result = await _repository.DeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Agent not found");
        }

        [Fact]
        public async Task Should_CheckExistence_When_AgentExists()
        {
            // Arrange
            var agent = new Agent
            {
                Name = "Test Agent",
                Description = "Test",
                Capabilities = new AgentCapabilities { SupportedTaskTypes = ["test"] }
            };

            var addResult = await _repository.AddAsync(agent, TestContext.Current.CancellationToken);
            var agentId = addResult.Data!.Id;

            // Act
            var existsResult = await _repository.ExistsAsync(agentId, TestContext.Current.CancellationToken);

            // Assert
            existsResult.IsSuccess.ShouldBeTrue();
            existsResult.Data.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_CheckNonExistence_When_AgentDoesNotExist()
        {
            // Act
            var existsResult = await _repository.ExistsAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

            // Assert
            existsResult.IsSuccess.ShouldBeTrue();
            existsResult.Data.ShouldBeFalse();
        }
    }
}