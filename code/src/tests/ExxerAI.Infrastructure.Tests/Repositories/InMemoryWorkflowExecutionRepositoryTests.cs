using Microsoft.Extensions.Logging;
using NSubstitute;
using ExxerAI.Infrastructure.Repositories;
using ExxerAI.Domain.Entities;
using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.Infrastructure.Tests.Repositories;

/// <summary>
/// Unit tests for InMemoryWorkflowExecutionRepository
/// Tests all CRUD operations, error scenarios, and repository behavior
/// </summary>
public class InMemoryWorkflowExecutionRepositoryTests : IDisposable
{
    private readonly InMemoryWorkflowExecutionRepository _repository;
    private readonly ILogger<InMemoryWorkflowExecutionRepository> _logger;

    public InMemoryWorkflowExecutionRepositoryTests()
    {
        _logger = Substitute.For<ILogger<InMemoryWorkflowExecutionRepository>>();
        _repository = new InMemoryWorkflowExecutionRepository(_logger);
    }

    public void Dispose()
    {
        _repository.Clear();
    }

    #region CreateExecutionAsync Tests

    [Fact]
    public async Task CreateExecutionAsync_WithValidExecution_ShouldReturnSuccess()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var execution = new WorkflowExecution
        {
            WorkflowId = workflowId,
            Status = WorkflowExecutionStatus.Starting,
            Input = new Dictionary<string, object> { ["key"] = "value" }
        };

        // Act
        var result = await _repository.CreateExecutionAsync(execution, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().NotBe(Guid.Empty);
        result.Value.WorkflowId.Should().Be(workflowId);
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.Value.LastModified.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateExecutionAsync_WithEmptyGuid_ShouldGenerateNewId()
    {
        // Arrange
        var execution = new WorkflowExecution
        {
            Id = Guid.Empty,
            WorkflowId = Guid.NewGuid(),
            Status = WorkflowExecutionStatus.Starting
        };

        // Act
        var result = await _repository.CreateExecutionAsync(execution, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateExecutionAsync_WithDuplicateId_ShouldReturnFailure()
    {
        // Arrange
        var executionId = Guid.NewGuid();
        var execution1 = new WorkflowExecution { Id = executionId, WorkflowId = Guid.NewGuid() };
        var execution2 = new WorkflowExecution { Id = executionId, WorkflowId = Guid.NewGuid() };

        await _repository.CreateExecutionAsync(execution1, CancellationToken.None);

        // Act
        var result = await _repository.CreateExecutionAsync(execution2, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateExecutionAsync_WithNullExecution_ShouldReturnFailure()
    {
        // Act
        var result = await _repository.CreateExecutionAsync(null!, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("cannot be null");
    }

    [Fact]
    public async Task CreateExecutionAsync_WithCancellationToken_ShouldReturnCancelled()
    {
        // Arrange
        var execution = new WorkflowExecution { WorkflowId = Guid.NewGuid() };
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act
        var result = await _repository.CreateExecutionAsync(execution, cancellationTokenSource.Token);

        // Assert
        result.IsCancelled.Should().BeTrue();
    }

    #endregion

    #region GetExecutionAsync Tests

    [Fact]
    public async Task GetExecutionAsync_WithExistingId_ShouldReturnExecution()
    {
        // Arrange
        var execution = new WorkflowExecution { WorkflowId = Guid.NewGuid() };
        var createResult = await _repository.CreateExecutionAsync(execution, CancellationToken.None);
        var executionId = createResult.Value.Id;

        // Act
        var result = await _repository.GetExecutionAsync(executionId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(executionId);
    }

    [Fact]
    public async Task GetExecutionAsync_WithNonExistingId_ShouldReturnFailure()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.GetExecutionAsync(nonExistingId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task GetExecutionAsync_WithCancellationToken_ShouldReturnCancelled()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act
        var result = await _repository.GetExecutionAsync(Guid.NewGuid(), cancellationTokenSource.Token);

        // Assert
        result.IsCancelled.Should().BeTrue();
    }

    #endregion

    #region UpdateExecutionAsync Tests

    [Fact]
    public async Task UpdateExecutionAsync_WithExistingExecution_ShouldUpdateSuccessfully()
    {
        // Arrange
        var execution = new WorkflowExecution { WorkflowId = Guid.NewGuid(), Status = WorkflowExecutionStatus.Starting };
        var createResult = await _repository.CreateExecutionAsync(execution, CancellationToken.None);
        var createdExecution = createResult.Value;

        createdExecution.Status = WorkflowExecutionStatus.Running;
        createdExecution.ErrorMessage = "Test error";

        // Act
        var result = await _repository.UpdateExecutionAsync(createdExecution, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(WorkflowExecutionStatus.Running);
        result.Value.ErrorMessage.Should().Be("Test error");
        result.Value.LastModified.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateExecutionAsync_WithNonExistingExecution_ShouldReturnFailure()
    {
        // Arrange
        var execution = new WorkflowExecution { Id = Guid.NewGuid(), WorkflowId = Guid.NewGuid() };

        // Act
        var result = await _repository.UpdateExecutionAsync(execution, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    [Fact]
    public async Task UpdateExecutionAsync_WithNullExecution_ShouldReturnFailure()
    {
        // Act
        var result = await _repository.UpdateExecutionAsync(null!, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("cannot be null");
    }

    #endregion

    #region DeleteExecutionAsync Tests

    [Fact]
    public async Task DeleteExecutionAsync_WithExistingId_ShouldDeleteSuccessfully()
    {
        // Arrange
        var execution = new WorkflowExecution { WorkflowId = Guid.NewGuid() };
        var createResult = await _repository.CreateExecutionAsync(execution, CancellationToken.None);
        var executionId = createResult.Value.Id;

        // Act
        var result = await _repository.DeleteExecutionAsync(executionId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        // Verify deletion
        var getResult = await _repository.GetExecutionAsync(executionId, CancellationToken.None);
        getResult.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteExecutionAsync_WithNonExistingId_ShouldReturnFailure()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteExecutionAsync(nonExistingId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    #endregion

    #region GetExecutionsForWorkflowAsync Tests

    [Fact]
    public async Task GetExecutionsForWorkflowAsync_WithMultipleExecutions_ShouldReturnFiltered()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var otherWorkflowId = Guid.NewGuid();

        var execution1 = new WorkflowExecution { WorkflowId = workflowId, Status = WorkflowExecutionStatus.Running };
        var execution2 = new WorkflowExecution { WorkflowId = workflowId, Status = WorkflowExecutionStatus.Completed };
        var execution3 = new WorkflowExecution { WorkflowId = otherWorkflowId, Status = WorkflowExecutionStatus.Running };

        await _repository.CreateExecutionAsync(execution1, CancellationToken.None);
        await _repository.CreateExecutionAsync(execution2, CancellationToken.None);
        await _repository.CreateExecutionAsync(execution3, CancellationToken.None);

        // Act
        var result = await _repository.GetExecutionsForWorkflowAsync(workflowId, null, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().OnlyContain(e => e.WorkflowId == workflowId);
    }

    [Fact]
    public async Task GetExecutionsForWorkflowAsync_WithStatusFilter_ShouldReturnFilteredByStatus()
    {
        // Arrange
        var workflowId = Guid.NewGuid();

        var execution1 = new WorkflowExecution { WorkflowId = workflowId, Status = WorkflowExecutionStatus.Running };
        var execution2 = new WorkflowExecution { WorkflowId = workflowId, Status = WorkflowExecutionStatus.Completed };

        await _repository.CreateExecutionAsync(execution1, CancellationToken.None);
        await _repository.CreateExecutionAsync(execution2, CancellationToken.None);

        // Act
        var result = await _repository.GetExecutionsForWorkflowAsync(workflowId, WorkflowExecutionStatus.Running, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Status.Should().Be(WorkflowExecutionStatus.Running);
    }

    #endregion

    #region GetActiveExecutionsAsync Tests

    [Fact]
    public async Task GetActiveExecutionsAsync_WithMixedStatuses_ShouldReturnOnlyActive()
    {
        // Arrange
        var executions = new[]
        {
            new WorkflowExecution { WorkflowId = Guid.NewGuid(), Status = WorkflowExecutionStatus.Running },
            new WorkflowExecution { WorkflowId = Guid.NewGuid(), Status = WorkflowExecutionStatus.Paused },
            new WorkflowExecution { WorkflowId = Guid.NewGuid(), Status = WorkflowExecutionStatus.Completed },
            new WorkflowExecution { WorkflowId = Guid.NewGuid(), Status = WorkflowExecutionStatus.Failed }
        };

        foreach (var execution in executions)
        {
            await _repository.CreateExecutionAsync(execution, CancellationToken.None);
        }

        // Act
        var result = await _repository.GetActiveExecutionsAsync(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().OnlyContain(e => e.Status == WorkflowExecutionStatus.Running || e.Status == WorkflowExecutionStatus.Paused);
    }

    #endregion

    #region GetExecutionStatisticsAsync Tests

    [Fact]
    public async Task GetExecutionStatisticsAsync_WithVariousStatuses_ShouldReturnAccurateStatistics()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var executions = new[]
        {
            new WorkflowExecution { WorkflowId = workflowId, Status = WorkflowExecutionStatus.Running },
            new WorkflowExecution { WorkflowId = workflowId, Status = WorkflowExecutionStatus.Paused },
            new WorkflowExecution { WorkflowId = workflowId, Status = WorkflowExecutionStatus.Completed, CompletedAt = DateTime.UtcNow },
            new WorkflowExecution { WorkflowId = workflowId, Status = WorkflowExecutionStatus.Failed },
            new WorkflowExecution { WorkflowId = workflowId, Status = WorkflowExecutionStatus.Cancelled }
        };

        foreach (var execution in executions)
        {
            await _repository.CreateExecutionAsync(execution, CancellationToken.None);
        }

        // Act
        var result = await _repository.GetExecutionStatisticsAsync(workflowId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var stats = result.Value;
        stats.TotalExecutions.Should().Be(5);
        stats.RunningExecutions.Should().Be(1);
        stats.PausedExecutions.Should().Be(1);
        stats.CompletedExecutions.Should().Be(1);
        stats.FailedExecutions.Should().Be(1);
        stats.CancelledExecutions.Should().Be(1);
        stats.SuccessRate.Should().BeApproximately(0.33, 0.01); // 1/3 completed out of completed+failed+cancelled
    }

    [Fact]
    public async Task GetExecutionStatisticsAsync_WithNoExecutions_ShouldReturnZeroStatistics()
    {
        // Act
        var result = await _repository.GetExecutionStatisticsAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var stats = result.Value;
        stats.TotalExecutions.Should().Be(0);
        stats.RunningExecutions.Should().Be(0);
        stats.CompletedExecutions.Should().Be(0);
        stats.SuccessRate.Should().Be(0.0);
    }

    #endregion

    #region Thread Safety Tests

    [Fact]
    public async Task ConcurrentOperations_ShouldBeThreadSafe()
    {
        // Arrange
        var workflowId = Guid.NewGuid();
        var tasks = new List<Task>();

        // Act - Create multiple executions concurrently
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var execution = new WorkflowExecution { WorkflowId = workflowId, Status = WorkflowExecutionStatus.Starting };
                await _repository.CreateExecutionAsync(execution, CancellationToken.None);
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        var result = await _repository.GetExecutionsForWorkflowAsync(workflowId, null, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(10);
    }

    #endregion

    #region Utility Methods Tests

    [Fact]
    public void GetExecutionCount_AfterOperations_ShouldReturnCorrectCount()
    {
        // Arrange & Act
        var initialCount = _repository.GetExecutionCount();
        initialCount.Should().Be(0);

        var execution = new WorkflowExecution { WorkflowId = Guid.NewGuid() };
        _repository.CreateExecutionAsync(execution, CancellationToken.None).Wait();

        var countAfterCreate = _repository.GetExecutionCount();
        countAfterCreate.Should().Be(1);

        _repository.Clear();
        var countAfterClear = _repository.GetExecutionCount();
        countAfterClear.Should().Be(0);
    }

    #endregion
}