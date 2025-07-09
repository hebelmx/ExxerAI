using ExxerAI.Domain;
using ExxerAI.Infrastructure.Repositories;
using Shouldly;
using Xunit;

namespace ExxerAI.Infrastructure.Tests;

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
        public async Task Should_AddTask_When_ValidTaskProvided()
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
            var result = await _repository.AddAsync(task, TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data!.Id.ShouldNotBe(Guid.Empty);
            result.Data.Title.ShouldBe(task.Title);
            result.Data.CreatedAt.ShouldBeGreaterThan(DateTime.MinValue);
        }

        [Fact]
        public async Task Should_GenerateId_When_TaskHasEmptyId()
        {
            // Arrange
            var task = new AgentTask
            {
                Id = Guid.Empty,
                Title = "Auto ID Task",
                TaskType = "AutoTest"
            };

            // Act
            var result = await _repository.AddAsync(task, TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data!.Id.ShouldNotBe(Guid.Empty);
            result.Data.Id.ShouldNotBe(task.Id); // Should have new ID
        }

        [Fact]
        public async Task Should_PreserveId_When_TaskHasValidId()
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
            var result = await _repository.AddAsync(task, TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data!.Id.ShouldBe(taskId);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_TaskIdAlreadyExists()
        {
            // Arrange
            var taskId = Guid.NewGuid();
            var task1 = new AgentTask { Id = taskId, Title = "First Task", TaskType = "Duplicate" };
            var task2 = new AgentTask { Id = taskId, Title = "Second Task", TaskType = "Duplicate" };

            await _repository.AddAsync(task1, TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.AddAsync(task2, TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain($"Task with ID {taskId} already exists");
        }

        [Fact]
        public async Task Should_GetTaskById_When_TaskExists()
        {
            // Arrange
            var task = new AgentTask
            {
                Title = "Retrievable Task",
                Description = "Can be retrieved",
                TaskType = "Retrieval",
                Priority = TaskPriority.Normal
            };

            var addResult = await _repository.AddAsync(task, TestContext.Current.CancellationToken);
            var taskId = addResult.Data!.Id;

            // Act
            var result = await _repository.GetByIdAsync(taskId,  TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data!.Id.ShouldBe(taskId);
            result.Data.Title.ShouldBe(task.Title);
        }

        [Fact]
        public async Task Should_ReturnFailure_When_TaskNotFound()
        {
            // Act
            var result = await _repository.GetByIdAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Task not found");
        }

        [Fact]
        public async Task Should_UpdateTask_When_TaskExists()
        {
            // Arrange
            var task = new AgentTask
            {
                Title = "Original Task",
                Description = "Original Description",
                TaskType = "UpdateTest",
                AgentStatus = TaskAgentStatus.Pending
            };

            var addResult = await _repository.AddAsync(task, TestContext.Current.CancellationToken);
            var addedTask = addResult.Data!;

            addedTask.Title = "Updated Task";
            addedTask.Description = "Updated Description";
            addedTask.AgentStatus = TaskAgentStatus.InProgress;

            // Act
            var result = await _repository.UpdateAsync(addedTask, TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data!.Title.ShouldBe("Updated Task");
            result.Data.Description.ShouldBe("Updated Description");
            result.Data.AgentStatus.ShouldBe(TaskAgentStatus.InProgress);
        }

        [Fact]
        public async Task Should_DeleteTask_When_TaskExists()
        {
            // Arrange
            var task = new AgentTask
            {
                Title = "Deletable Task",
                TaskType = "Deletion"
            };

            var addResult = await _repository.AddAsync(task, TestContext.Current.CancellationToken);
            var taskId = addResult.Data!.Id;

            // Act
            var deleteResult = await _repository.DeleteAsync(taskId, TestContext.Current.CancellationToken);

            // Assert
            deleteResult.IsSuccess.ShouldBeTrue();

            // Verify task is deleted
            var getResult = await _repository.GetByIdAsync(taskId, TestContext.Current.CancellationToken);
            getResult.IsFailure.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_GetAllTasks_When_TasksExist()
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
                await _repository.AddAsync(task, TestContext.Current.CancellationToken);
            }

            // Act
            var result = await _repository.GetAllAsync(TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data!.Count().ShouldBeGreaterThanOrEqualTo(3);
        }
    }

    /// <summary>
    /// Test class for specialized query operations
    /// </summary>
    public class QueryOperationsTests : InMemoryTaskRepositoryTests
    {
        [Fact]
        public async Task Should_GetTasksByStatus_When_StatusMatches()
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
                await _repository.AddAsync(task, TestContext.Current.CancellationToken);
            }

            // Act
            var result = await _repository.GetByStatusAsync(TaskAgentStatus.Pending, TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data!.Count().ShouldBeGreaterThanOrEqualTo(2);
            result.Data.All(t => t.AgentStatus == TaskAgentStatus.Pending).ShouldBeTrue();
        }

        [Fact]
        public async Task Should_GetTasksByAgent_When_AgentIdMatches()
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
                await _repository.AddAsync(task, TestContext.Current.CancellationToken);
            }

            // Act
            var result = await _repository.GetByAgentAsync(agentId1, null, TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data!.Count().ShouldBe(2);
            result.Data.All(t => t.AssignedAgentId == agentId1).ShouldBeTrue();
        }

        [Fact]
        public async Task Should_GetTasksByAgentAndStatus_When_BothFiltersApplied()
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
                await _repository.AddAsync(task, TestContext.Current.CancellationToken);
            }

            // Act
            var result = await _repository.GetByAgentAsync(agentId, TaskAgentStatus.InProgress, TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data!.Count().ShouldBe(1);
            result.Data.First().AgentStatus.ShouldBe(TaskAgentStatus.InProgress);
            result.Data.First().AssignedAgentId.ShouldBe(agentId);
        }

        [Fact]
        public async Task Should_GetTasksByType_When_TaskTypeMatches()
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
                await _repository.AddAsync(task, TestContext.Current.CancellationToken);
            }

            // Act
            var result = await _repository.GetByTypeAsync("DataAnalysis", null, TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data!.Count().ShouldBe(2);
            result.Data.All(t => t.TaskType == "DataAnalysis").ShouldBeTrue();
        }

        [Fact]
        public async Task Should_GetOverdueTasks_When_TasksPastDeadline()
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
                await _repository.AddAsync(task, TestContext.Current.CancellationToken);
            }

            // Act
            var result = await _repository.GetOverdueTasksAsync(TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data!.Count().ShouldBe(2);
            result.Data.All(t => t.Deadline < now).ShouldBeTrue();
            result.Data.All(t => t.AgentStatus != TaskAgentStatus.Completed).ShouldBeTrue();
        }
    }

    /// <summary>
    /// Test class for validation and error scenarios
    /// </summary>
    public class ValidationTests : InMemoryTaskRepositoryTests
    {
        [Fact]
        public async Task Should_ReturnFailure_When_AddingNullTask()
        {
            // Act
            var result = await _repository.AddAsync(null!, TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Task cannot be null");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_UpdatingNullTask()
        {
            // Act
            var result = await _repository.UpdateAsync(null!, TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Task cannot be null");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_UpdatingNonExistentTask()
        {
            // Arrange
            var task = new AgentTask
            {
                Id = Guid.NewGuid(),
                Title = "Non-existent Task",
                TaskType = "ValidationTest"
            };

            // Act
            var result = await _repository.UpdateAsync(task, TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain($"Task not found with ID: {task.Id}");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_UpdatingTaskWithEmptyId()
        {
            // Arrange
            var task = new AgentTask
            {
                Id = Guid.Empty,
                Title = "Empty ID Task",
                TaskType = "ValidationTest"
            };

            // Act
            var result = await _repository.UpdateAsync(task, TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Task ID cannot be empty");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_DeletingNonExistentTask()
        {
            // Act
            var result = await _repository.DeleteAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Task not found with ID:");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_DeletingWithEmptyId()
        {
            // Act
            var result = await _repository.DeleteAsync(Guid.Empty, TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Task ID cannot be empty");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_GetByAgentWithEmptyId()
        {
            // Act
            var result = await _repository.GetByAgentAsync(Guid.Empty, null, TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Agent ID cannot be empty");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_GetByTypeWithEmptyType()
        {
            // Act
            var result = await _repository.GetByTypeAsync("", null, TestContext.Current.CancellationToken);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error!.ShouldContain("Task type cannot be empty");
        }

        [Fact]
        public async Task Should_ReturnFailure_When_ExistsWithEmptyId()
        {
            // Act
            var result = await _repository.ExistsAsync(Guid.Empty, TestContext.Current.CancellationToken);

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
        public async Task Should_ReturnTrue_When_TaskExists()
        {
            // Arrange
            var task = new AgentTask
            {
                Title = "Existence Test Task",
                TaskType = "ExistenceTest"
            };

            var addResult = await _repository.AddAsync(task, TestContext.Current.CancellationToken);
            var taskId = addResult.Data!.Id;

            // Act
            var result = await _repository.ExistsAsync(taskId, TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldBeTrue();
        }

        [Fact]
        public async Task Should_ReturnFalse_When_TaskDoesNotExist()
        {
            // Act
            var result = await _repository.ExistsAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldBeFalse();
        }
    }

    /// <summary>
    /// Test class for concurrent operations and thread safety
    /// </summary>
    public class ConcurrencyTests : InMemoryTaskRepositoryTests
    {
        [Fact]
        public async Task Should_HandleConcurrentAdds_When_MultipleThreadsAddingTasks()
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

            var allTasks = await _repository.GetAllAsync(TestContext.Current.CancellationToken);
            allTasks.IsSuccess.ShouldBeTrue();
            allTasks.Data!.Count().ShouldBeGreaterThanOrEqualTo(100);
        }

        [Fact]
        public async Task Should_HandleConcurrentReads_When_MultipleThreadsReading()
        {
            // Arrange
            var task = new AgentTask
            {
                Title = "Concurrent Read Test",
                TaskType = "ConcurrencyTest"
            };

            var addResult = await _repository.AddAsync(task, TestContext.Current.CancellationToken);
            var taskId = addResult.Data!.Id;

            // Act - Multiple concurrent reads
            var readTasks = Enumerable.Range(1, 50)
                .Select(_ => _repository.GetByIdAsync(taskId, TestContext.Current.CancellationToken));
            var results = await Task.WhenAll(readTasks);

            // Assert
            results.All(r => r.IsSuccess).ShouldBeTrue();
            results.All(r => r.Data!.Id == taskId).ShouldBeTrue();
        }

        [Fact]
        public async Task Should_HandleConcurrentUpdates_When_MultipleThreadsUpdating()
        {
            // Arrange
            var task = new AgentTask
            {
                Title = "Concurrent Update Test",
                TaskType = "ConcurrencyTest"
            };

            var addResult = await _repository.AddAsync(task, TestContext.Current.CancellationToken);
            var taskToUpdate = addResult.Data!;

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
                    return _repository.UpdateAsync(updatedTask, TestContext.Current.CancellationToken);
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
        public async Task Should_HandleLargeTaskCollections_When_ManyTasksAdded()
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
                await _repository.AddAsync(task, TestContext.Current.CancellationToken);
            }

            stopwatch.Stop();

            // Assert
            stopwatch.ElapsedMilliseconds.ShouldBeLessThan(5000); // Should complete within 5 seconds

            var allTasks = await _repository.GetAllAsync(TestContext.Current.CancellationToken);
            allTasks.IsSuccess.ShouldBeTrue();
            allTasks.Data!.Count().ShouldBeGreaterThanOrEqualTo(taskCount);
        }

        [Fact]
        public async Task Should_HandleComplexQueries_When_LargeDataSet()
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
                await _repository.AddAsync(task, TestContext.Current.CancellationToken);
            }

            // Act & Assert - Complex queries should complete quickly
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var statusQuery = await _repository.GetByStatusAsync(TaskAgentStatus.InProgress, TestContext.Current.CancellationToken);
            var agentQuery = await _repository.GetByAgentAsync(agentIds[0], null, TestContext.Current.CancellationToken);
            var typeQuery = await _repository.GetByTypeAsync("QueryType5", null, TestContext.Current.CancellationToken);
            var overdueQuery = await _repository.GetOverdueTasksAsync(TestContext.Current.CancellationToken);

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
        public async Task Should_HandleEmptyRepository_When_NoTasksAdded()
        {
            // Act
            var allTasks = await _repository.GetAllAsync(TestContext.Current.CancellationToken);
            var statusTasks = await _repository.GetByStatusAsync(TaskAgentStatus.Pending, TestContext.Current.CancellationToken);
            var agentTasks = await _repository.GetByAgentAsync(Guid.NewGuid(), null, TestContext.Current.CancellationToken);
            var typeTasks = await _repository.GetByTypeAsync("NonExistent", null, TestContext.Current.CancellationToken);
            var overdueTasks = await _repository.GetOverdueTasksAsync(TestContext.Current.CancellationToken);

            // Assert
            allTasks.IsSuccess.ShouldBeTrue();
            allTasks.Data!.ShouldBeEmpty();

            statusTasks.IsSuccess.ShouldBeTrue();
            statusTasks.Data!.ShouldBeEmpty();

            agentTasks.IsSuccess.ShouldBeTrue();
            agentTasks.Data!.ShouldBeEmpty();

            typeTasks.IsSuccess.ShouldBeTrue();
            typeTasks.Data!.ShouldBeEmpty();

            overdueTasks.IsSuccess.ShouldBeTrue();
            overdueTasks.Data!.ShouldBeEmpty();
        }

        [Fact]
        public async Task Should_HandleTasksWithNoDeadline_When_CheckingOverdue()
        {
            // Arrange
            var tasksWithoutDeadline = new[]
            {
                new AgentTask { Title = "No Deadline 1", AgentStatus = TaskAgentStatus.Pending, TaskType = "EdgeTest" },
                new AgentTask { Title = "No Deadline 2", AgentStatus = TaskAgentStatus.InProgress, TaskType = "EdgeTest" }
            };

            foreach (var task in tasksWithoutDeadline)
            {
                await _repository.AddAsync(task, TestContext.Current.CancellationToken);
            }

            // Act
            var result = await _repository.GetOverdueTasksAsync(TestContext.Current.CancellationToken);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            // Tasks without deadlines should not appear in overdue results
            result.Data.ShouldNotContain(t => tasksWithoutDeadline.Contains(t));
        }

        [Fact]
        public async Task Should_HandleTasksWithSameTitle_When_AddingDuplicateTitles()
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
                var result = await _repository.AddAsync(task, TestContext.Current.CancellationToken);
                result.IsSuccess.ShouldBeTrue(); // Should allow duplicate titles
            }

            // Assert
            var allTasks = await _repository.GetAllAsync(TestContext.Current.CancellationToken);
            allTasks.IsSuccess.ShouldBeTrue();
            allTasks.Data!.Count(t => t.Title == "Duplicate Title").ShouldBeGreaterThanOrEqualTo(3);
        }
    }
}