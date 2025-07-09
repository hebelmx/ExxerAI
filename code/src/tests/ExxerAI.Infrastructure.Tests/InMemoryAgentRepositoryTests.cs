using ExxerAI.Domain;
using ExxerAI.Domain.Entities;
using ExxerAI.Infrastructure.Repositories;
using Shouldly;
using Xunit;

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
            var result = await _repository.AddAsync(agent, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.Id.ShouldNotBe(Guid.Empty);
            result.Value.Name.ShouldBe(agent.Name);
            result.Value.CreatedAt.ShouldBeGreaterThan(DateTime.MinValue);
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

            var addResult = await _repository.AddAsync(agent, cancellationToken: TestContext.Current.CancellationToken);
            var agentId = addResult.Value!.Id;

            // Act
            var result = await _repository.GetByIdAsync(agentId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.Id.ShouldBe(agentId);
            result.Value.Name.ShouldBe(agent.Name);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_AgentNotFound()
        {
            // Act
            var result = await _repository.GetByIdAsync(Guid.NewGuid(), cancellationToken: TestContext.Current.CancellationToken);

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

            var addResult = await _repository.AddAsync(agent, cancellationToken: TestContext.Current.CancellationToken);
            var addedAgent = addResult.Value!;

            addedAgent.Name = "Updated Agent Name";
            addedAgent.Description = "Updated Description";

            // Act
            var result = await _repository.UpdateAsync(addedAgent, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.Name.ShouldBe("Updated Agent Name");
            result.Value.Description.ShouldBe("Updated Description");
            result.Value.UpdatedAt.ShouldBeGreaterThan(addedAgent.CreatedAt);
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

            var addResult = await _repository.AddAsync(agent, cancellationToken: TestContext.Current.CancellationToken);
            var agentId = addResult.Value!.Id;

            // Act
            var deleteResult = await _repository.DeleteAsync(agentId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            deleteResult.IsSuccess.ShouldBeTrue();

            // Verify agent is deleted
            var getResult = await _repository.GetByIdAsync(agentId, cancellationToken: TestContext.Current.CancellationToken);
            getResult.IsFailure.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_GetAllAgents_When_AgentsExist()
        {
            // Arrange
            var agent1 = new Agent { Name = "Agent 1", Description = "Desc 1", Capabilities = new AgentCapabilities { SupportedTaskTypes = ["test"] } };
            var agent2 = new Agent { Name = "Agent 2", Description = "Desc 2", Capabilities = new AgentCapabilities { SupportedTaskTypes = ["test"] } };

            await _repository.AddAsync(agent1, cancellationToken: TestContext.Current.CancellationToken);
            await _repository.AddAsync(agent2, cancellationToken: TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.GetAllAsync(cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.Count().ShouldBeGreaterThanOrEqualTo(2);
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

            await _repository.AddAsync(activeAgent, cancellationToken: TestContext.Current.CancellationToken);
            await _repository.AddAsync(inactiveAgent, cancellationToken: TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.GetByStatusAsync(AgentStatus.Active, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.All(a => a.Status == AgentStatus.Active).ShouldBeTrue();
            result.Value.Any(a => a.Name == "Active Agent").ShouldBeTrue();
            result.Value.Any(a => a.Name == "Inactive Agent").ShouldBeFalse();
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

            await _repository.AddAsync(webAgent, cancellationToken: TestContext.Current.CancellationToken);
            await _repository.AddAsync(dataAgent, cancellationToken: TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.FindByTaskTypeAsync("web", cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.Count().ShouldBe(1);
            result.Value.First().Name.ShouldBe("Web Agent");
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

            await _repository.AddAsync(agent, cancellationToken: TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.FindByTaskTypeAsync("nonexistent", cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.Count().ShouldBe(0);
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

            await _repository.AddAsync(agent, cancellationToken: TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.GetAgentsWithTaskCountAsync(cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.Count().ShouldBeGreaterThanOrEqualTo(1);

            var agentWithCount = result.Value.FirstOrDefault(atc => atc.Agent.Name == "Test Agent");
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
            var result = await _repository.AddAsync(null!, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Agent cannot be null");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_UpdatingNullAgent()
        {
            // Act
            var result = await _repository.UpdateAsync(null!, cancellationToken: TestContext.Current.CancellationToken);

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
            var result = await _repository.UpdateAsync(agent, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Agent not found");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_DeletingNonExistentAgent()
        {
            // Act
            var result = await _repository.DeleteAsync(Guid.NewGuid(), cancellationToken: TestContext.Current.CancellationToken);

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

            var addResult = await _repository.AddAsync(agent, cancellationToken: TestContext.Current.CancellationToken);
            var agentId = addResult.Value!.Id;

            // Act
            var existsResult = await _repository.ExistsAsync(agentId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            existsResult.IsSuccess.ShouldBeTrue();
            existsResult.Value!.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_CheckNonExistence_When_AgentDoesNotExist()
        {
            // Act
            var existsResult = await _repository.ExistsAsync(Guid.NewGuid(), cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            existsResult.IsSuccess.ShouldBeTrue();
            existsResult.Value!.ShouldBeFalse();
        }
    }
}