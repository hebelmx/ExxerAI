using ExxerAI.Infrastructure.Repositories;

namespace ExxerAI.IntegrationTests;

/// <summary>
/// Comprehensive unit tests for InMemoryTaskRepository
/// </summary>
public class InMemoryTaskRepositoryTests
{
    private readonly InMemoryTaskRepository _repository;

    public InMemoryTaskRepositoryTests()
    {
        _repository = new InMemoryTaskRepository();
    }

    /// <summary>
    /// Test class for basic CRUD operations
    /// </summary>
    public class CrudOperationsTests : InMemoryTaskRepositoryTests
    {
        [Fact]
        public async Task Should_AddTask_When_ValidTaskProvidedAsync()
        {
            // Arrange
            var task = new AgentTask
            {
                Title = "Test Task",
                Description = "Test Description",
                TaskType = "Analysis",
                Priority = TaskPriority.High,
                AgentStatus = TaskAgentStatus.Pending
            };

            // Act
            var result = await _repository.AddAsync(task, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.Id.ShouldNotBe(Guid.Empty);
            result.Value.Title.ShouldBe(task.Title);
            result.Value.CreatedAt.ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Fact]
        public async Task Should_GenerateId_When_TaskHasEmptyIdAsync()
        {
            // Arrange
            var task = new AgentTask
            {
                Id = Guid.Empty,
                Title = "Auto ID Task",
                TaskType = "AutoTest"
            };

            // Act
            var result = await _repository.AddAsync(task, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.Id.ShouldNotBe(Guid.Empty);
            result.Value.Id.ShouldNotBe(task.Id); // Should have new ID
        }

        [Fact]
        public async Task Should_PreserveId_When_TaskHasValidIdAsync()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var task = new AgentTask
            {
                Id = taskId,
                Title = "Preserve ID Task",
                TaskType = "PreserveTest"
            };

            // Act
            var result = await _repository.AddAsync(task, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.Id.ShouldBe(taskId);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_TaskIdAlreadyExistsAsync()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var task1 = new AgentTask { Id = taskId, Title = "First Task", TaskType = "Duplicate" };
            var task2 = new AgentTask { Id = taskId, Title = "Second Task", TaskType = "Duplicate" };

            await _repository.AddAsync(task1, cancellationToken: TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.AddAsync(task2, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain($"Task with ID {taskId} already exists");
        }

        [Fact]
        public async Task Should_GetTaskById_When_TaskExistsAsync()
        {
            // Arrange
            var task = new AgentTask
            {
                Title = "Retrievable Task",
                Description = "Can be retrieved",
                TaskType = "Retrieval",
                Priority = TaskPriority.Normal
            };

            var addResult = await _repository.AddAsync(task, cancellationToken: TestContext.Current.CancellationToken);
            var taskId = addResult.Value!.Id;

            // Act
            var result = await _repository.GetByIdAsync(taskId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.Id.ShouldBe(taskId);
            result.Value.Title.ShouldBe(task.Title);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_TaskNotFoundAsync()
        {
            // Act
            var result = await _repository.GetByIdAsync(Guid.NewGuid(), cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Task not found");
        }

        [Fact]
        public async Task Should_UpdateTask_When_TaskExistsAsync()
        {
            // Arrange
            var task = new AgentTask
            {
                Title = "Original Task",
                Description = "Original Description",
                TaskType = "UpdateTest",
                AgentStatus = TaskAgentStatus.Pending
            };

            var addResult = await _repository.AddAsync(task, cancellationToken: TestContext.Current.CancellationToken);
            var addedTask = addResult.Value!;

            addedTask.Title = "Updated Task";
            addedTask.Description = "Updated Description";
            addedTask.AgentStatus = TaskAgentStatus.InProgress;

            // Act
            var result = await _repository.UpdateAsync(addedTask, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.Title.ShouldBe("Updated Task");
            result.Value.Description.ShouldBe("Updated Description");
            result.Value.AgentStatus.ShouldBe(TaskAgentStatus.InProgress);
        }

        [Fact]
        public async Task Should_DeleteTask_When_TaskExistsAsync()
        {
            // Arrange
            var task = new AgentTask
            {
                Title = "Deletable Task",
                TaskType = "Deletion"
            };

            var addResult = await _repository.AddAsync(task, cancellationToken: TestContext.Current.CancellationToken);
            var taskId = addResult.Value!.Id;

            // Act
            var deleteResult = await _repository.DeleteAsync(taskId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            deleteResult.IsSuccess.ShouldBeTrue();

            // Verify task is deleted
            var getResult = await _repository.GetByIdAsync(taskId, cancellationToken: TestContext.Current.CancellationToken);
            getResult.IsFailure.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_GetAllTasks_When_TasksExistAsync()
        {
            // Arrange
            var tasks = new[]
            {
                new AgentTask { Title = "Task 1", TaskType = "Test1" },
                new AgentTask { Title = "Task 2", TaskType = "Test2" },
                new AgentTask { Title = "Task 3", TaskType = "Test3" }
            };

            foreach (var task in tasks)
            {
                await _repository.AddAsync(task, cancellationToken: TestContext.Current.CancellationToken);
            }

            // Act
            var result = await _repository.GetAllAsync(cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.Count().ShouldBeGreaterThanOrEqualTo(3);
        }
    }

    /// <summary>
    /// Test class for specialized query operations
    /// </summary>
    public class QueryOperationsTests : InMemoryTaskRepositoryTests
    {
        [Fact]
        public async Task Should_GetTasksByStatus_When_StatusMatchesAsync()
        {
            // Arrange
            var pendingTasks = new[]
            {
                new AgentTask { Title = "Pending 1", AgentStatus = TaskAgentStatus.Pending, TaskType = "StatusTest" },
                new AgentTask { Title = "Pending 2", AgentStatus = TaskAgentStatus.Pending, TaskType = "StatusTest" }
            };
            var inProgressTask = new AgentTask { Title = "In Progress", AgentStatus = TaskAgentStatus.InProgress, TaskType = "StatusTest" };
            var completedTask = new AgentTask { Title = "Completed", AgentStatus = TaskAgentStatus.Completed, TaskType = "StatusTest" };

            foreach (var task in pendingTasks.Concat(new[] { inProgressTask, completedTask }))
            {
                await _repository.AddAsync(task, cancellationToken: TestContext.Current.CancellationToken);
            }

            // Act
            var result = await _repository.GetByStatusAsync(TaskAgentStatus.Pending, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.Count().ShouldBeGreaterThanOrEqualTo(2);
            result.Value.All(t => t.AgentStatus == TaskAgentStatus.Pending).ShouldBeTrue();
        }

        [Fact]
        public async Task Should_GetTasksByAgent_When_AgentIdMatchesAsync()
        {
            // Arrange
            var agentId1 = Guid.NewGuid();
            var agentId2 = Guid.NewGuid();

            var agent1Tasks = new[]
            {
                new AgentTask { Title = "Agent1 Task1", AssignedAgentId = agentId1, TaskType = "AgentTest" },
                new AgentTask { Title = "Agent1 Task2", AssignedAgentId = agentId1, TaskType = "AgentTest" }
            };
            var agent2Task = new AgentTask { Title = "Agent2 Task", AssignedAgentId = agentId2, TaskType = "AgentTest" };
            var unassignedTask = new AgentTask { Title = "Unassigned", TaskType = "AgentTest" };

            foreach (var task in agent1Tasks.Concat(new[] { agent2Task, unassignedTask }))
            {
                await _repository.AddAsync(task, cancellationToken: TestContext.Current.CancellationToken);
            }

            // Act
            var result = await _repository.GetByAgentAsync(agentId1, null, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.Count().ShouldBe(2);
            result.Value.All(t => t.AssignedAgentId == agentId1).ShouldBeTrue();
        }

        [Fact]
        public async Task Should_GetTasksByAgentAndStatus_When_BothFiltersAppliedAsync()
        {
            // Arrange
            var agentId = Guid.NewGuid();

            var tasks = new[]
            {
                new AgentTask { Title = "Agent Pending", AssignedAgentId = agentId, AgentStatus = TaskAgentStatus.Pending, TaskType = "FilterTest" },
                new AgentTask { Title = "Agent InProgress", AssignedAgentId = agentId, AgentStatus = TaskAgentStatus.InProgress, TaskType = "FilterTest" },
                new AgentTask { Title = "Agent Completed", AssignedAgentId = agentId, AgentStatus = TaskAgentStatus.Completed, TaskType = "FilterTest" }
            };

            foreach (var task in tasks)
            {
                await _repository.AddAsync(task, cancellationToken: TestContext.Current.CancellationToken);
            }

            // Act
            var result = await _repository.GetByAgentAsync(agentId, TaskAgentStatus.InProgress, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.Count().ShouldBe(1);
            result.Value.First().AgentStatus.ShouldBe(TaskAgentStatus.InProgress);
            result.Value.First().AssignedAgentId.ShouldBe(agentId);
        }

        [Fact]
        public async Task Should_GetTasksByType_When_TaskTypeMatchesAsync()
        {
            // Arrange
            var analysisTasks = new[]
            {
                new AgentTask { Title = "Analysis 1", TaskType = "DataAnalysis" },
                new AgentTask { Title = "Analysis 2", TaskType = "DataAnalysis" }
            };
            var codingTask = new AgentTask { Title = "Coding", TaskType = "CodeGeneration" };
            var reportingTask = new AgentTask { Title = "Reporting", TaskType = "ReportGeneration" };

            foreach (var task in analysisTasks.Concat(new[] { codingTask, reportingTask }))
            {
                await _repository.AddAsync(task, cancellationToken: TestContext.Current.CancellationToken);
            }

            // Act
            var result = await _repository.GetByTypeAsync("DataAnalysis", null, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.Count().ShouldBe(2);
            result.Value.All(t => t.TaskType == "DataAnalysis").ShouldBeTrue();
        }

        [Fact]
        public async Task Should_GetOverdueTasks_When_TasksPastDeadlineAsync()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var overdueTasks = new[]
            {
                new AgentTask
                {
                    Title = "Overdue 1",
                    Deadline = now.AddHours(-2),
                    AgentStatus = TaskAgentStatus.Pending,
                    TaskType = "OverdueTest"
                },
                new AgentTask
                {
                    Title = "Overdue 2",
                    Deadline = now.AddHours(-1),
                    AgentStatus = TaskAgentStatus.InProgress,
                    TaskType = "OverdueTest"
                }
            };
            var upcomingTask = new AgentTask
            {
                Title = "Future",
                Deadline = now.AddHours(2),
                AgentStatus = TaskAgentStatus.Pending,
                TaskType = "OverdueTest"
            };
            var overdueButCompletedTask = new AgentTask
            {
                Title = "Overdue Completed",
                Deadline = now.AddHours(-3),
                AgentStatus = TaskAgentStatus.Completed,
                TaskType = "OverdueTest"
            };

            foreach (var task in overdueTasks.Concat(new[] { upcomingTask, overdueButCompletedTask }))
            {
                await _repository.AddAsync(task, cancellationToken: TestContext.Current.CancellationToken);
            }

            // Act
            var result = await _repository.GetOverdueTasksAsync(cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value!.Count().ShouldBe(2);
            result.Value.All(t => t.Deadline < now).ShouldBeTrue();
            result.Value.All(t => t.AgentStatus != TaskAgentStatus.Completed).ShouldBeTrue();
        }
    }

    /// <summary>
    /// Test class for validation and error scenarios
    /// </summary>
    public class ValidationTests : InMemoryTaskRepositoryTests
    {
        [Fact]
        public async Task Should_ReturnFailure_When_AddingNullTaskAsync()
        {
            // Act
            var result = await _repository.AddAsync(null!, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Task cannot be null");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_UpdatingNullTaskAsync()
        {
            // Act
            var result = await _repository.UpdateAsync(null!, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Task cannot be null");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_UpdatingNonExistentTaskAsync()
        {
            // Arrange
            var task = new AgentTask
            {
                Id = Guid.NewGuid(),
                Title = "Non-existent Task",
                TaskType = "ValidationTest"
            };

            // Act
            var result = await _repository.UpdateAsync(task, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain($"Task not found with ID: {task.Id}");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_UpdatingTaskWithEmptyIdAsync()
        {
            // Arrange
            var task = new AgentTask
            {
                Id = Guid.Empty,
                Title = "Empty ID Task",
                TaskType = "ValidationTest"
            };

            // Act
            var result = await _repository.UpdateAsync(task, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Task ID cannot be empty");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_DeletingNonExistentTaskAsync()
        {
            // Act
            var result = await _repository.DeleteAsync(Guid.NewGuid(), cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Task not found with ID:");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_DeletingWithEmptyIdAsync()
        {
            // Act
            var result = await _repository.DeleteAsync(Guid.Empty, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Task ID cannot be empty");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_GetByAgentWithEmptyIdAsync()
        {
            // Act
            var result = await _repository.GetByAgentAsync(Guid.Empty, null, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Agent ID cannot be empty");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_GetByTypeWithEmptyTypeAsync()
        {
            // Act
            var result = await _repository.GetByTypeAsync("", null, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Task type cannot be empty");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_ExistsWithEmptyIdAsync()
        {
            // Act
            var result = await _repository.ExistsAsync(Guid.Empty, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Task ID cannot be empty");
        }
    }

    /// <summary>
    /// Test class for existence checking operations
    /// </summary>
    public class ExistenceTests : InMemoryTaskRepositoryTests
    {
        [Fact]
        public async Task Should_ReturnTrue_When_TaskExistsAsync()
        {
            // Arrange
            var task = new AgentTask
            {
                Title = "Existence Test Task",
                TaskType = "ExistenceTest"
            };

            var addResult = await _repository.AddAsync(task, cancellationToken: TestContext.Current.CancellationToken);
            var taskId = addResult.Value!.Id;

            // Act
            var result = await _repository.ExistsAsync(taskId, cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_ReturnFalse_When_TaskDoesNotExistAsync()
        {
            // Act
            var result = await _repository.ExistsAsync(Guid.NewGuid(), cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldBeFalse();
        }
    }

    /// <summary>
    /// Test class for concurrent operations and thread safety
    /// </summary>
    public class ConcurrencyTests : InMemoryTaskRepositoryTests
    {
        [Fact]
        public async Task Should_HandleConcurrentAdds_When_MultipleThreadsAddingTasksAsync()
        {
            // Arrange
            var tasks = Enumerable.Range(1, 100)
                .Select(i => new AgentTask
                {
                    Title = $"Concurrent Task {i}",
                    TaskType = "ConcurrencyTest"
                })
                .ToArray();

            // Act
            var addTasks = tasks.Select(task => _repository.AddAsync(task, TestContext.Current.CancellationToken));
            var results = await Task.WhenAll(addTasks);

            // Assert
            results.All(r => r.IsSuccess).ShouldBeTrue();

            var allTasks = await _repository.GetAllAsync(cancellationToken: TestContext.Current.CancellationToken);
            allTasks.IsSuccess.ShouldBeTrue();
            allTasks.Value!.Count().ShouldBeGreaterThanOrEqualTo(100);
        }

        [Fact]
        public async Task Should_HandleConcurrentReads_When_MultipleThreadsReadingAsync()
        {
            // Arrange
            var task = new AgentTask
            {
                Title = "Concurrent Read Test",
                TaskType = "ConcurrencyTest"
            };

            var addResult = await _repository.AddAsync(task, cancellationToken: TestContext.Current.CancellationToken);
            var taskId = addResult.Value!.Id;

            // Act - Multiple concurrent reads
            var readTasks = Enumerable.Range(1, 50)
                .Select(_ => _repository.GetByIdAsync(taskId, TestContext.Current.CancellationToken));
            var results = await Task.WhenAll(readTasks);

            // Assert
            results.All(r => r.IsSuccess).ShouldBeTrue();
            results.All(r => r.Value!.Id == taskId).ShouldBeTrue();
        }

        [Fact]
        public async Task Should_HandleConcurrentUpdates_When_MultipleThreadsUpdatingAsync()
        {
            // Arrange
            var task = new AgentTask
            {
                Title = "Concurrent Update Test",
                TaskType = "ConcurrencyTest"
            };

            var addResult = await _repository.AddAsync(task, cancellationToken: TestContext.Current.CancellationToken);
            var taskToUpdate = addResult.Value!;

            // Act - Multiple concurrent updates
            var updateTasks = Enumerable.Range(1, 10)
                .Select(i =>
                {
                    var updatedTask = new AgentTask
                    {
                        Id = taskToUpdate.Id,
                        Title = $"Updated Title {i}",
                        TaskType = taskToUpdate.TaskType,
                        AgentStatus = TaskAgentStatus.InProgress,
                        CreatedAt = taskToUpdate.CreatedAt
                    };
                    return _repository.UpdateAsync(updatedTask, cancellationToken: TestContext.Current.CancellationToken);
                });

            var results = await Task.WhenAll(updateTasks);

            // Assert - All updates should succeed (last one wins in concurrent dictionary)
            results.All(r => r.IsSuccess).ShouldBeTrue();
        }
    }

    /// <summary>
    /// Test class for performance and scalability scenarios
    /// </summary>
    public class PerformanceTests : InMemoryTaskRepositoryTests
    {
        [Fact]
        public async Task Should_HandleLargeTaskCollections_When_ManyTasksAddedAsync()
        {
            // Arrange
            var taskCount = 10000;
            var tasks = Enumerable.Range(1, taskCount)
                .Select(i => new AgentTask
                {
                    Title = $"Performance Task {i}",
                    TaskType = $"Type{i % 10}", // 10 different types
                    Priority = (TaskPriority)(i % 4 + 1),
                    AgentStatus = (TaskAgentStatus)(i % 6)
                })
                .ToArray();

            // Act - Add all tasks
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            foreach (var task in tasks)
            {
                await _repository.AddAsync(task, cancellationToken: TestContext.Current.CancellationToken);
            }

            stopwatch.Stop();

            // Assert
            stopwatch.ElapsedMilliseconds.ShouldBeLessThan(5000); // Should complete within 5 seconds

            var allTasks = await _repository.GetAllAsync(cancellationToken: TestContext.Current.CancellationToken);
            allTasks.IsSuccess.ShouldBeTrue();
            allTasks.Value!.Count().ShouldBeGreaterThanOrEqualTo(taskCount);
        }

        [Fact]
        public async Task Should_HandleComplexQueries_When_LargeDataSetAsync()
        {
            // Arrange - Add a large dataset
            var agentIds = Enumerable.Range(1, 100).Select(_ => Guid.NewGuid()).ToArray();
            var tasks = new List<AgentTask>();

            for (int i = 0; i < 5000; i++)
            {
                tasks.Add(new AgentTask
                {
                    Title = $"Query Test Task {i}",
                    TaskType = $"QueryType{i % 20}",
                    AssignedAgentId = agentIds[i % agentIds.Length],
                    AgentStatus = (TaskAgentStatus)(i % 6),
                    Priority = (TaskPriority)(i % 4 + 1),
                    Deadline = i % 10 == 0 ? DateTime.UtcNow.AddHours(-1) : DateTime.UtcNow.AddHours(1)
                });
            }

            foreach (var task in tasks)
            {
                await _repository.AddAsync(task, cancellationToken: TestContext.Current.CancellationToken);
            }

            // Act & Assert - Complex queries should complete quickly
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var statusQuery = await _repository.GetByStatusAsync(TaskAgentStatus.InProgress, cancellationToken: TestContext.Current.CancellationToken);
            var agentQuery = await _repository.GetByAgentAsync(agentIds[0], null, cancellationToken: TestContext.Current.CancellationToken);
            var typeQuery = await _repository.GetByTypeAsync("QueryType5", null, cancellationToken: TestContext.Current.CancellationToken);
            var overdueQuery = await _repository.GetOverdueTasksAsync(cancellationToken: TestContext.Current.CancellationToken);

            stopwatch.Stop();

            // Assert
            stopwatch.ElapsedMilliseconds.ShouldBeLessThan(1000); // Should complete within 1 second
            statusQuery.IsSuccess.ShouldBeTrue();
            agentQuery.IsSuccess.ShouldBeTrue();
            typeQuery.IsSuccess.ShouldBeTrue();
            overdueQuery.IsSuccess.ShouldBeTrue();
        }
    }

    /// <summary>
    /// Test class for edge cases and boundary conditions
    /// </summary>
    public class EdgeCaseTests : InMemoryTaskRepositoryTests
    {
        [Fact]
        public async Task Should_HandleEmptyRepository_When_NoTasksAddedAsync()
        {
            // Act
            var allTasks = await _repository.GetAllAsync(TestContext.Current.CancellationToken);
            var statusTasks = await _repository.GetByStatusAsync(TaskAgentStatus.Pending, cancellationToken: TestContext.Current.CancellationToken);
            var agentTasks = await _repository.GetByAgentAsync(Guid.NewGuid(), null, cancellationToken: TestContext.Current.CancellationToken);
            var typeTasks = await _repository.GetByTypeAsync("NonExistent", null, cancellationToken: TestContext.Current.CancellationToken);
            var overdueTasks = await _repository.GetOverdueTasksAsync(cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            allTasks.IsSuccess.ShouldBeTrue();
            allTasks.Value!.ShouldBeEmpty();

            statusTasks.IsSuccess.ShouldBeTrue();
            statusTasks.Value!.ShouldBeEmpty();

            agentTasks.IsSuccess.ShouldBeTrue();
            agentTasks.Value!.ShouldBeEmpty();

            typeTasks.IsSuccess.ShouldBeTrue();
            typeTasks.Value!.ShouldBeEmpty();

            overdueTasks.IsSuccess.ShouldBeTrue();
            overdueTasks.Value!.ShouldBeEmpty();
        }

        [Fact]
        public async Task Should_HandleTasksWithNoDeadline_When_CheckingOverdueAsync()
        {
            // Arrange
            var tasksWithoutDeadline = new[]
            {
                new AgentTask { Title = "No Deadline 1", AgentStatus = TaskAgentStatus.Pending, TaskType = "EdgeTest" },
                new AgentTask { Title = "No Deadline 2", AgentStatus = TaskAgentStatus.InProgress, TaskType = "EdgeTest" }
            };

            foreach (var task in tasksWithoutDeadline)
            {
                await _repository.AddAsync(task, cancellationToken: TestContext.Current.CancellationToken);
            }

            // Act
            var result = await _repository.GetOverdueTasksAsync(cancellationToken: TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            // Tasks without deadlines should not appear in overdue results
            result.Value.ShouldNotContain(t => tasksWithoutDeadline.Contains(t));
        }

        [Fact]
        public async Task Should_HandleTasksWithSameTitle_When_AddingDuplicateTitlesAsync()
        {
            // Arrange
            var tasks = new[]
            {
                new AgentTask { Title = "Duplicate Title", TaskType = "Type1" },
                new AgentTask { Title = "Duplicate Title", TaskType = "Type2" },
                new AgentTask { Title = "Duplicate Title", TaskType = "Type3" }
            };

            // Act
            foreach (var task in tasks)
            {
                var result = await _repository.AddAsync(task, cancellationToken: TestContext.Current.CancellationToken);
                result.IsSuccess.ShouldBeTrue(); // Should allow duplicate titles
            }

            // Assert
            var allTasks = await _repository.GetAllAsync(cancellationToken: TestContext.Current.CancellationToken);
            allTasks.IsSuccess.ShouldBeTrue();
            allTasks.Value!.Count(t => t.Title == "Duplicate Title").ShouldBeGreaterThanOrEqualTo(3);
        }
    }
}