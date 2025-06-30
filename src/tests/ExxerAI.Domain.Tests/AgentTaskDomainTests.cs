using ExxerAI.Domain;
using Shouldly;

namespace ExxerAI.Domain.Tests;

/// <summary>
/// Comprehensive unit tests for AgentTask domain entity
/// </summary>
public class AgentTaskDomainTests
{
	/// <summary>
	/// Test fixture for AgentTask creation scenarios
	/// </summary>
	public class CreationTests
	{
		[Fact]
		public void Should_CreateTaskWithDefaults_When_InstantiatedEmpty()
		{
			// Arrange & Act
			var task = new AgentTask();

			// Assert
			task.Id.ShouldNotBe(Guid.Empty);
			task.Title.ShouldBe(string.Empty);
			task.Description.ShouldBe(string.Empty);
			task.TaskType.ShouldBe(string.Empty);
			task.Status.ShouldBe(TaskStatus.Pending);
			task.Priority.ShouldBe(TaskPriority.Normal);
			task.AssignedAgentId.ShouldBeNull();
			task.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
			task.StartedAt.ShouldBeNull();
			task.CompletedAt.ShouldBeNull();
			task.Deadline.ShouldBeNull();
			task.ErrorMessage.ShouldBeNull();
			task.RetryCount.ShouldBe(0);
			task.Input.ShouldNotBeNull();
			task.Output.ShouldNotBeNull();
			task.Metadata.ShouldNotBeNull();
		}

		[Fact]
		public void Should_CreateTaskWithValues_When_PropertiesSet()
		{
			// Arrange
			var taskId = Guid.NewGuid();
			var agentId = Guid.NewGuid();
			var deadline = DateTime.UtcNow.AddHours(4);

			// Act
			var task = new AgentTask
			{
				Id = taskId,
				Title = "Test Task",
				Description = "A comprehensive test task",
				TaskType = "DataAnalysis",
				Status = TaskStatus.InProgress,
				Priority = TaskPriority.High,
				AssignedAgentId = agentId,
				Deadline = deadline,
				RetryCount = 2
			};

			// Assert
			task.Id.ShouldBe(taskId);
			task.Title.ShouldBe("Test Task");
			task.Description.ShouldBe("A comprehensive test task");
			task.TaskType.ShouldBe("DataAnalysis");
			task.Status.ShouldBe(TaskStatus.InProgress);
			task.Priority.ShouldBe(TaskPriority.High);
			task.AssignedAgentId.ShouldBe(agentId);
			task.Deadline.ShouldBe(deadline);
			task.RetryCount.ShouldBe(2);
		}
	}

	/// <summary>
	/// Test fixture for AgentTask status transitions
	/// </summary>
	public class StatusTransitionTests
	{
		[Theory]
		[InlineData(TaskStatus.Pending)]
		[InlineData(TaskStatus.InProgress)]
		[InlineData(TaskStatus.Completed)]
		[InlineData(TaskStatus.Failed)]
		[InlineData(TaskStatus.Cancelled)]
		[InlineData(TaskStatus.Paused)]
		public void Should_AllowStatusChange_When_ValidStatusProvided(TaskStatus newStatus)
		{
			// Arrange
			var task = new AgentTask { Title = "Status Test" };

			// Act
			task.Status = newStatus;

			// Assert
			task.Status.ShouldBe(newStatus);
		}

		[Fact]
		public void Should_TrackStatusHistory_When_StatusChangesMultipleTimes()
		{
			// Arrange
			var task = new AgentTask { Title = "Status History Test" };
			var statuses = new[] { TaskStatus.InProgress, TaskStatus.Paused, TaskStatus.InProgress, TaskStatus.Completed };

			// Act & Assert
			task.Status.ShouldBe(TaskStatus.Pending); // Initial
			
			foreach (var status in statuses)
			{
				task.Status = status;
				task.Status.ShouldBe(status);
			}
		}
	}

	/// <summary>
	/// Test fixture for AgentTask priority handling
	/// </summary>
	public class PriorityTests
	{
		[Theory]
		[InlineData(TaskPriority.Low, 1)]
		[InlineData(TaskPriority.Normal, 2)]
		[InlineData(TaskPriority.High, 3)]
		[InlineData(TaskPriority.Critical, 4)]
		public void Should_RespectPriorityOrder_When_PrioritySet(TaskPriority priority, int expectedValue)
		{
			// Arrange
			var task = new AgentTask { Title = "Priority Test" };

			// Act
			task.Priority = priority;

			// Assert
			task.Priority.ShouldBe(priority);
			((int)task.Priority).ShouldBe(expectedValue);
		}

		[Fact]
		public void Should_SortByPriority_When_MultipleTasksCompared()
		{
			// Arrange
			var tasks = new[]
			{
				new AgentTask { Title = "Low", Priority = TaskPriority.Low },
				new AgentTask { Title = "Critical", Priority = TaskPriority.Critical },
				new AgentTask { Title = "Normal", Priority = TaskPriority.Normal },
				new AgentTask { Title = "High", Priority = TaskPriority.High }
			};

			// Act
			var sorted = tasks.OrderByDescending(t => t.Priority).ToArray();

			// Assert
			sorted[0].Priority.ShouldBe(TaskPriority.Critical);
			sorted[1].Priority.ShouldBe(TaskPriority.High);
			sorted[2].Priority.ShouldBe(TaskPriority.Normal);
			sorted[3].Priority.ShouldBe(TaskPriority.Low);
		}
	}

	/// <summary>
	/// Test fixture for AgentTask timing calculations
	/// </summary>
	public class TimingTests
	{
		[Fact]
		public void Should_CalculateExecutionDuration_When_StartAndEndTimesSet()
		{
			// Arrange
			var task = new AgentTask { Title = "Duration Test" };
			var startTime = new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc);
			var endTime = new DateTime(2024, 1, 1, 12, 30, 45, DateTimeKind.Utc);

			// Act
			task.StartedAt = startTime;
			task.CompletedAt = endTime;

			// Assert
			task.ExecutionDuration.ShouldNotBeNull();
			task.ExecutionDuration.Value.TotalHours.ShouldBe(2.5125, 0.0001);
			task.ExecutionDuration.Value.TotalMinutes.ShouldBe(150.75, 0.01);
		}

		[Fact]
		public void Should_ReturnNullDuration_When_StartTimeNotSet()
		{
			// Arrange
			var task = new AgentTask 
			{ 
				Title = "No Start Test",
				CompletedAt = DateTime.UtcNow
			};

			// Act & Assert
			task.ExecutionDuration.ShouldBeNull();
		}

		[Fact]
		public void Should_ReturnNullDuration_When_EndTimeNotSet()
		{
			// Arrange
			var task = new AgentTask 
			{ 
				Title = "No End Test",
				StartedAt = DateTime.UtcNow
			};

			// Act & Assert
			task.ExecutionDuration.ShouldBeNull();
		}
	}

	/// <summary>
	/// Test fixture for AgentTask overdue detection
	/// </summary>
	public class OverdueDetectionTests
	{
		[Fact]
		public void Should_DetectOverdueTask_When_PastDeadlineAndNotCompleted()
		{
			// Arrange
			var task = new AgentTask
			{
				Title = "Overdue Test",
				Deadline = DateTime.UtcNow.AddHours(-2),
				Status = TaskStatus.InProgress
			};

			// Act & Assert
			task.IsOverdue.ShouldBeTrue();
		}

		[Fact]
		public void Should_NotDetectOverdue_When_NoDeadlineSet()
		{
			// Arrange
			var task = new AgentTask
			{
				Title = "No Deadline Test",
				Status = TaskStatus.InProgress
			};

			// Act & Assert
			task.IsOverdue.ShouldBeFalse();
		}

		[Fact]
		public void Should_NotDetectOverdue_When_TaskCompleted()
		{
			// Arrange
			var task = new AgentTask
			{
				Title = "Completed Test",
				Deadline = DateTime.UtcNow.AddHours(-2),
				Status = TaskStatus.Completed
			};

			// Act & Assert
			task.IsOverdue.ShouldBeFalse();
		}

		[Theory]
		[InlineData(TaskStatus.Pending)]
		[InlineData(TaskStatus.InProgress)]
		[InlineData(TaskStatus.Paused)]
		[InlineData(TaskStatus.Failed)]
		public void Should_DetectOverdue_When_NonCompletedStatusAndPastDeadline(TaskStatus status)
		{
			// Arrange
			var task = new AgentTask
			{
				Title = "Status Overdue Test",
				Deadline = DateTime.UtcNow.AddMinutes(-30),
				Status = status
			};

			// Act & Assert
			if (status == TaskStatus.Completed)
				task.IsOverdue.ShouldBeFalse();
			else
				task.IsOverdue.ShouldBeTrue();
		}
	}

	/// <summary>
	/// Test fixture for AgentTask data handling
	/// </summary>
	public class DataHandlingTests
	{
		[Fact]
		public void Should_InitializeEmptyTaskData_When_TaskCreated()
		{
			// Arrange & Act
			var task = new AgentTask { Title = "Data Test" };

			// Assert
			task.Input.ShouldNotBeNull();
			task.Input.ContentType.ShouldBe("application/json");
			task.Input.Content.ShouldBe(string.Empty);
			task.Input.Properties.ShouldNotBeNull();
			task.Input.Properties.ShouldBeEmpty();

			task.Output.ShouldNotBeNull();
			task.Output.ContentType.ShouldBe("application/json");
			task.Output.Content.ShouldBe(string.Empty);
			task.Output.Properties.ShouldNotBeNull();
			task.Output.Properties.ShouldBeEmpty();
		}

		[Fact]
		public void Should_StoreTaskData_When_DataProvided()
		{
			// Arrange
			var task = new AgentTask { Title = "Data Storage Test" };
			var inputData = new TaskData
			{
				ContentType = "text/plain",
				Content = "Input content",
				Properties = { ["key1"] = "value1", ["key2"] = 42 }
			};

			// Act
			task.Input = inputData;

			// Assert
			task.Input.ContentType.ShouldBe("text/plain");
			task.Input.Content.ShouldBe("Input content");
			task.Input.Properties["key1"].ShouldBe("value1");
			task.Input.Properties["key2"].ShouldBe(42);
		}
	}

	/// <summary>
	/// Test fixture for AgentTask metadata handling
	/// </summary>
	public class MetadataTests
	{
		[Fact]
		public void Should_InitializeEmptyMetadata_When_TaskCreated()
		{
			// Arrange & Act
			var task = new AgentTask { Title = "Metadata Test" };

			// Assert
			task.Metadata.ShouldNotBeNull();
			task.Metadata.Properties.ShouldNotBeNull();
			task.Metadata.Properties.ShouldBeEmpty();
			task.Metadata.Context.ShouldNotBeNull();
			task.Metadata.Context.ShouldBeEmpty();
			task.Metadata.Metrics.ShouldNotBeNull();
			task.Metadata.Metrics.ShouldBeEmpty();
		}

		[Fact]
		public void Should_StoreMetadata_When_MetadataProvided()
		{
			// Arrange
			var task = new AgentTask { Title = "Metadata Storage Test" };

			// Act
			task.Metadata.Properties["version"] = "1.0";
			task.Metadata.Context["environment"] = "test";
			task.Metadata.Metrics["performance"] = 95.5;

			// Assert
			task.Metadata.Properties["version"].ShouldBe("1.0");
			task.Metadata.Context["environment"].ShouldBe("test");
			task.Metadata.Metrics["performance"].ShouldBe(95.5);
		}
	}

	/// <summary>
	/// Test fixture for AgentTask validation scenarios
	/// </summary>
	public class ValidationTests
	{
		[Theory]
		[InlineData("")]
		[InlineData("   ")]
		[InlineData(null)]
		public void Should_AllowEmptyTitle_When_ValidationNotEnforced(string? title)
		{
			// Arrange & Act
			var task = new AgentTask { Title = title ?? string.Empty };

			// Assert - Domain model allows empty values, validation happens at service/API level
			task.Title.ShouldBe(title ?? string.Empty);
		}

		[Fact]
		public void Should_HandleVeryLongStrings_When_WithinLimits()
		{
			// Arrange
			var longTitle = new string('A', 200); // Max length per data annotation
			var longDescription = new string('B', 2000); // Max length per data annotation
			var longTaskType = new string('C', 50); // Max length per data annotation

			// Act
			var task = new AgentTask
			{
				Title = longTitle,
				Description = longDescription,
				TaskType = longTaskType
			};

			// Assert
			task.Title.ShouldBe(longTitle);
			task.Description.ShouldBe(longDescription);
			task.TaskType.ShouldBe(longTaskType);
		}
	}

	/// <summary>
	/// Test fixture for AgentTask edge cases
	/// </summary>
	public class EdgeCaseTests
	{
		[Fact]
		public void Should_HandleExtremeDates_When_SetCorrectly()
		{
			// Arrange
			var task = new AgentTask { Title = "Extreme Dates Test" };
			var veryEarlyDate = new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var veryLateDate = new DateTime(2100, 12, 31, 23, 59, 59, DateTimeKind.Utc);

			// Act
			task.CreatedAt = veryEarlyDate;
			task.Deadline = veryLateDate;

			// Assert
			task.CreatedAt.ShouldBe(veryEarlyDate);
			task.Deadline.ShouldBe(veryLateDate);
		}

		[Fact]
		public void Should_HandleMaxRetryCount_When_SetToLargeValue()
		{
			// Arrange
			var task = new AgentTask { Title = "Max Retry Test" };

			// Act
			task.RetryCount = int.MaxValue;

			// Assert
			task.RetryCount.ShouldBe(int.MaxValue);
		}

		[Fact]
		public void Should_HandleComplexTaskData_When_LargeObjectProvided()
		{
			// Arrange
			var task = new AgentTask { Title = "Complex Data Test" };
			var complexData = new TaskData
			{
				ContentType = "application/complex+json",
				Content = new string('X', 10000), // Large content
				Properties = Enumerable.Range(1, 100)
					.ToDictionary(i => $"key{i}", i => (object)$"value{i}")
			};

			// Act
			task.Input = complexData;

			// Assert
			task.Input.Content.Length.ShouldBe(10000);
			task.Input.Properties.Count.ShouldBe(100);
			task.Input.Properties["key50"].ShouldBe("value50");
		}
	}

	/// <summary>
	/// Test fixture for AgentTask assignment scenarios
	/// </summary>
	public class AssignmentTests
	{
		[Fact]
		public void Should_AllowAgentAssignment_When_ValidAgentIdProvided()
		{
			// Arrange
			var task = new AgentTask { Title = "Assignment Test" };
			var agentId = Guid.NewGuid();

			// Act
			task.AssignedAgentId = agentId;

			// Assert
			task.AssignedAgentId.ShouldBe(agentId);
		}

		[Fact]
		public void Should_AllowUnassignment_When_AgentIdSetToNull()
		{
			// Arrange
			var task = new AgentTask 
			{ 
				Title = "Unassignment Test",
				AssignedAgentId = Guid.NewGuid()
			};

			// Act
			task.AssignedAgentId = null;

			// Assert
			task.AssignedAgentId.ShouldBeNull();
		}

		[Fact]
		public void Should_AllowReassignment_When_DifferentAgentIdProvided()
		{
			// Arrange
			var task = new AgentTask 
			{ 
				Title = "Reassignment Test",
				AssignedAgentId = Guid.NewGuid()
			};
			var newAgentId = Guid.NewGuid();

			// Act
			var originalAgentId = task.AssignedAgentId;
			task.AssignedAgentId = newAgentId;

			// Assert
			task.AssignedAgentId.ShouldNotBe(originalAgentId);
			task.AssignedAgentId.ShouldBe(newAgentId);
		}
	}
} 