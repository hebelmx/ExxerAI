using ExxerAI.Domain;
using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests;

/// <summary>
/// Comprehensive unit tests for AgentTask entity
/// </summary>
public class AgentTaskEntityTests
{
	/// <summary>
	/// Creates a valid AgentTask for testing purposes
	/// </summary>
	/// <returns>A properly initialized AgentTask instance</returns>
	private static AgentTask CreateValidAgentTask()
	{
		return new AgentTask
		{
			Title = "Test Task",
			Description = "A comprehensive test task",
			TaskType = "TestType",
			Priority = TaskPriority.Normal,
			AgentStatus = TaskAgentStatus.Pending
		};
	}

	public class ConstructorAndDefaultsTests
	{
		[Fact]
		public void Should_InitializeWithDefaults_When_UsingParameterlessConstructor()
		{
			// Act
			var task = new AgentTask();

			// Assert
			task.Id.ShouldNotBe(Guid.Empty);
			task.Title.ShouldBe(string.Empty);
			task.Description.ShouldBe(string.Empty);
			task.TaskType.ShouldBe(string.Empty);
			task.AgentStatus.ShouldBe(TaskAgentStatus.Pending);
			task.Priority.ShouldBe(TaskPriority.Normal);
			task.AssignedAgentId.ShouldBeNull();
			task.AssignedAgent.ShouldBeNull();
			task.Input.ShouldNotBeNull();
			task.Output.ShouldNotBeNull();
			task.StartedAt.ShouldBeNull();
			task.CompletedAt.ShouldBeNull();
			task.Deadline.ShouldBeNull();
			task.ErrorMessage.ShouldBeNull();
			task.RetryCount.ShouldBe(0);
			task.Metadata.ShouldNotBeNull();
			(DateTime.UtcNow - task.CreatedAt).ShouldBeLessThan(TimeSpan.FromSeconds(1));
		}

		[Fact]
		public void Should_GenerateUniqueIds_When_MultipleInstancesCreated()
		{
			// Act
			var task1 = new AgentTask();
			var task2 = new AgentTask();
			var task3 = new AgentTask();

			// Assert
			task1.Id.ShouldNotBe(task2.Id);
			task1.Id.ShouldNotBe(task3.Id);
			task2.Id.ShouldNotBe(task3.Id);
		}

		[Fact]
		public void Should_InitializeDataObjects_When_TaskCreated()
		{
			// Act
			var task = new AgentTask();

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

			task.Metadata.ShouldNotBeNull();
			task.Metadata.Properties.ShouldNotBeNull();
			task.Metadata.Properties.ShouldBeEmpty();
			task.Metadata.Context.ShouldNotBeNull();
			task.Metadata.Context.ShouldBeEmpty();
			task.Metadata.Metrics.ShouldNotBeNull();
			task.Metadata.Metrics.ShouldBeEmpty();
		}
	}

	public class PropertyValidationTests
	{
		[Theory]
		[InlineData("Simple Task")]
		[InlineData("A")]
		[InlineData("Complex Task with Numbers 123 and Symbols @#$")]
		[InlineData("Very Long Task Title That Exceeds Normal Length Expectations But Still Within 200 Character Limit For Comprehensive Testing Purpose And Edge Case Coverage Validation")]
		public void Should_AcceptValidTitles_When_WithinLengthLimits(string title)
		{
			// Arrange
			var task = new AgentTask();

			// Act
			task.Title = title;

			// Assert
			task.Title.ShouldBe(title);
		}

		[Theory]
		[InlineData("")]
		[InlineData("Short")]
		[InlineData("Medium length description")]
		[InlineData("Very detailed and comprehensive description that explains the task in full detail including all requirements, expectations, acceptance criteria, and any additional notes that might be relevant for task execution and completion validation purposes.")]
		public void Should_AcceptValidDescriptions_When_WithinLengthLimits(string description)
		{
			// Arrange
			var task = new AgentTask();

			// Act
			task.Description = description;

			// Assert
			task.Description.ShouldBe(description);
		}

		[Theory]
		[InlineData("API")]
		[InlineData("DataProcessing")]
		[InlineData("ML")]
		[InlineData("DocumentProcessing")]
		[InlineData("Integration")]
		public void Should_AcceptValidTaskTypes_When_WithinLengthLimits(string taskType)
		{
			// Arrange
			var task = new AgentTask();

			// Act
			task.TaskType = taskType;

			// Assert
			task.TaskType.ShouldBe(taskType);
		}

		[Fact]
		public void Should_AllowAssignedAgentId_When_ValidGuidProvided()
		{
			// Arrange
			var task = new AgentTask();
			var agentId = Guid.NewGuid();

			// Act
			task.AssignedAgentId = agentId;

			// Assert
			task.AssignedAgentId.ShouldBe(agentId);
		}

		[Fact]
		public void Should_AllowNullAssignedAgentId_When_NoAgentAssigned()
		{
			// Arrange
			var task = new AgentTask();

			// Act
			task.AssignedAgentId = null;

			// Assert
			task.AssignedAgentId.ShouldBeNull();
		}

		[Fact]
		public void Should_AllowErrorMessage_When_TaskFails()
		{
			// Arrange
			var task = new AgentTask();
			const string errorMessage = "Task failed due to insufficient resources";

			// Act
			task.ErrorMessage = errorMessage;

			// Assert
			task.ErrorMessage.ShouldBe(errorMessage);
		}

		[Theory]
		[InlineData(0)]
		[InlineData(1)]
		[InlineData(3)]
		[InlineData(10)]
		[InlineData(100)]
		public void Should_AcceptValidRetryCount_When_WithinReasonableLimits(int retryCount)
		{
			// Arrange
			var task = new AgentTask();

			// Act
			task.RetryCount = retryCount;

			// Assert
			task.RetryCount.ShouldBe(retryCount);
		}
	}

	public class TaskAgentStatusTests
	{
		[Theory]
		[InlineData(nameof(TaskAgentStatus.Pending))]
		[InlineData(nameof(TaskAgentStatus.InProgress))]
		[InlineData(nameof(TaskAgentStatus.Completed))]
		[InlineData(nameof(TaskAgentStatus.Failed))]
		[InlineData(nameof(TaskAgentStatus.Cancelled))]
		[InlineData(nameof(TaskAgentStatus.Paused))]
		public void Should_HandleAllTaskStatuses_When_ValidStatusProvided(string statusName)
		{
			// Arrange
			var task = new AgentTask();
			var expectedStatus = Enum.Parse<TaskAgentStatus>(statusName);

			// Act
			task.AgentStatus = expectedStatus;

			// Assert
			task.AgentStatus.ShouldBe(expectedStatus);
			Enum.IsDefined(typeof(TaskAgentStatus), task.AgentStatus).ShouldBeTrue();
		}

		[Fact]
		public void Should_StartWithPendingStatus_When_TaskCreated()
		{
			// Act
			var task = new AgentTask();

			// Assert
			task.AgentStatus.ShouldBe(TaskAgentStatus.Pending);
		}

		[Fact]
		public void Should_AllowStatusTransitions_When_WorkflowProgresses()
		{
			// Arrange
			var task = new AgentTask();

			// Act & Assert - Typical workflow progression
			task.AgentStatus = TaskAgentStatus.Pending;
			task.AgentStatus.ShouldBe(TaskAgentStatus.Pending);

			task.AgentStatus = TaskAgentStatus.InProgress;
			task.AgentStatus.ShouldBe(TaskAgentStatus.InProgress);

			task.AgentStatus = TaskAgentStatus.Completed;
			task.AgentStatus.ShouldBe(TaskAgentStatus.Completed);
		}

		[Fact]
		public void Should_AllowStatusTransitionsToFailed_When_ErrorsOccur()
		{
			// Arrange
			var task = new AgentTask();

			// Act & Assert - Error workflow
			task.AgentStatus = TaskAgentStatus.InProgress;
			task.AgentStatus = TaskAgentStatus.Failed;
			task.AgentStatus.ShouldBe(TaskAgentStatus.Failed);
		}

		[Fact]
		public void Should_AllowStatusTransitionsToCancelled_When_TaskAborted()
		{
			// Arrange
			var task = new AgentTask();

			// Act & Assert - Cancellation workflow
			task.AgentStatus = TaskAgentStatus.Pending;
			task.AgentStatus = TaskAgentStatus.Cancelled;
			task.AgentStatus.ShouldBe(TaskAgentStatus.Cancelled);
		}

		[Fact]
		public void Should_AllowStatusTransitionsToPaused_When_TaskSuspended()
		{
			// Arrange
			var task = new AgentTask();

			// Act & Assert - Pause/resume workflow
			task.AgentStatus = TaskAgentStatus.InProgress;
			task.AgentStatus = TaskAgentStatus.Paused;
			task.AgentStatus.ShouldBe(TaskAgentStatus.Paused);

			task.AgentStatus = TaskAgentStatus.InProgress;
			task.AgentStatus.ShouldBe(TaskAgentStatus.InProgress);
		}
	}

	public class TaskPriorityTests
	{
		[Theory]
		[InlineData(nameof(TaskPriority.Low), 1)]
		[InlineData(nameof(TaskPriority.Normal), 2)]
		[InlineData(nameof(TaskPriority.High), 3)]
		[InlineData(nameof(TaskPriority.Critical), 4)]
		public void Should_HandleAllTaskPriorities_When_ValidPriorityProvided(string priorityName, int expectedValue)
		{
			// Arrange
			var task = new AgentTask();
			var expectedPriority = Enum.Parse<TaskPriority>(priorityName);

			// Act
			task.Priority = expectedPriority;

			// Assert
			task.Priority.ShouldBe(expectedPriority);
			((int)task.Priority).ShouldBe(expectedValue);
			Enum.IsDefined(typeof(TaskPriority), task.Priority).ShouldBeTrue();
		}

		[Fact]
		public void Should_StartWithNormalPriority_When_TaskCreated()
		{
			// Act
			var task = new AgentTask();

			// Assert
			task.Priority.ShouldBe(TaskPriority.Normal);
			((int)task.Priority).ShouldBe(2);
		}

		[Fact]
		public void Should_OrderPrioritiesCorrectly_When_ComparingValues()
		{
			// Arrange & Act & Assert
			((int)TaskPriority.Low).ShouldBeLessThan((int)TaskPriority.Normal);
			((int)TaskPriority.Normal).ShouldBeLessThan((int)TaskPriority.High);
			((int)TaskPriority.High).ShouldBeLessThan((int)TaskPriority.Critical);
		}

		[Fact]
		public void Should_AllowPriorityChanges_When_TaskRequirementsChange()
		{
			// Arrange
			var task = new AgentTask { Priority = TaskPriority.Low };

			// Act & Assert - Priority escalation
			task.Priority = TaskPriority.Normal;
			task.Priority.ShouldBe(TaskPriority.Normal);

			task.Priority = TaskPriority.High;
			task.Priority.ShouldBe(TaskPriority.High);

			task.Priority = TaskPriority.Critical;
			task.Priority.ShouldBe(TaskPriority.Critical);

			// Act & Assert - Priority de-escalation
			task.Priority = TaskPriority.Low;
			task.Priority.ShouldBe(TaskPriority.Low);
		}
	}

	public class DateTimeAndDurationTests
	{
		[Fact]
		public void Should_SetCreatedAtNearCurrentTime_When_TaskCreated()
		{
			// Arrange
			var beforeCreation = DateTime.UtcNow;

			// Act
			var task = new AgentTask();
			var afterCreation = DateTime.UtcNow;

			// Assert
			task.CreatedAt.ShouldBeInRange(beforeCreation, afterCreation);
		}

		[Fact]
		public void Should_AllowCustomCreatedAt_When_ExplicitlySet()
		{
			// Arrange
			var task = new AgentTask();
			var customDate = DateTime.UtcNow.AddDays(-5);

			// Act
			task.CreatedAt = customDate;

			// Assert
			task.CreatedAt.ShouldBe(customDate);
		}

		[Fact]
		public void Should_AllowNullStartedAt_When_TaskNotStarted()
		{
			// Arrange & Act
			var task = new AgentTask();

			// Assert
			task.StartedAt.ShouldBeNull();
		}

		[Fact]
		public void Should_SetStartedAt_When_TaskBegins()
		{
			// Arrange
			var task = new AgentTask();
			var startTime = DateTime.UtcNow;

			// Act
			task.StartedAt = startTime;

			// Assert
			task.StartedAt.ShouldBe(startTime);
		}

		[Fact]
		public void Should_AllowNullCompletedAt_When_TaskNotCompleted()
		{
			// Arrange & Act
			var task = new AgentTask();

			// Assert
			task.CompletedAt.ShouldBeNull();
		}

		[Fact]
		public void Should_SetCompletedAt_When_TaskFinishes()
		{
			// Arrange
			var task = new AgentTask();
			var completionTime = DateTime.UtcNow;

			// Act
			task.CompletedAt = completionTime;

			// Assert
			task.CompletedAt.ShouldBe(completionTime);
		}

		[Fact]
		public void Should_AllowNullDeadline_When_NoDeadlineSet()
		{
			// Arrange & Act
			var task = new AgentTask();

			// Assert
			task.Deadline.ShouldBeNull();
		}

		[Fact]
		public void Should_SetDeadline_When_DeadlineProvided()
		{
			// Arrange
			var task = new AgentTask();
			var deadline = DateTime.UtcNow.AddDays(7);

			// Act
			task.Deadline = deadline;

			// Assert
			task.Deadline.ShouldBe(deadline);
		}
	}

	public class ExecutionDurationTests
	{
		[Fact]
		public void Should_ReturnNull_When_TaskNotStarted()
		{
			// Arrange
			var task = new AgentTask();

			// Act
			var duration = task.ExecutionDuration;

			// Assert
			duration.ShouldBeNull();
		}

		[Fact]
		public void Should_ReturnNull_When_TaskStartedButNotCompleted()
		{
			// Arrange
			var task = new AgentTask
			{
				StartedAt = DateTime.UtcNow.AddMinutes(-30)
			};

			// Act
			var duration = task.ExecutionDuration;

			// Assert
			duration.ShouldBeNull();
		}

		[Fact]
		public void Should_ReturnNull_When_TaskCompletedButNotStarted()
		{
			// Arrange
			var task = new AgentTask
			{
				CompletedAt = DateTime.UtcNow
			};

			// Act
			var duration = task.ExecutionDuration;

			// Assert
			duration.ShouldBeNull();
		}

		[Fact]
		public void Should_CalculateCorrectDuration_When_BothDatesSet()
		{
			// Arrange
			var startTime = DateTime.UtcNow.AddMinutes(-30);
			var endTime = DateTime.UtcNow;
			var task = new AgentTask
			{
				StartedAt = startTime,
				CompletedAt = endTime
			};

			// Act
			var duration = task.ExecutionDuration;

			// Assert
			duration.ShouldNotBeNull();
			duration.Value.TotalMinutes.ShouldBe(30, 0.1);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(15)]
		[InlineData(60)]
		[InlineData(180)]
		[InlineData(1440)] // 24 hours
		public void Should_CalculateVariousDurations_When_DifferentExecutionTimes(int durationMinutes)
		{
			// Arrange
			var startTime = DateTime.UtcNow;
			var endTime = startTime.AddMinutes(durationMinutes);
			var task = new AgentTask
			{
				StartedAt = startTime,
				CompletedAt = endTime
			};

			// Act
			var duration = task.ExecutionDuration;

			// Assert
			duration.ShouldNotBeNull();
			duration.Value.TotalMinutes.ShouldBe(durationMinutes, 0.1);
		}

		[Fact]
		public void Should_HandleZeroDuration_When_StartAndEndTimesIdentical()
		{
			// Arrange
			var timestamp = DateTime.UtcNow;
			var task = new AgentTask
			{
				StartedAt = timestamp,
				CompletedAt = timestamp
			};

			// Act
			var duration = task.ExecutionDuration;

			// Assert
			duration.ShouldNotBeNull();
			duration.Value.TotalMilliseconds.ShouldBe(0);
		}

		[Fact]
		public void Should_HandleNegativeDuration_When_EndTimeBeforeStartTime()
		{
			// Arrange
			var endTime = DateTime.UtcNow;
			var startTime = endTime.AddMinutes(30); // Start after end
			var task = new AgentTask
			{
				StartedAt = startTime,
				CompletedAt = endTime
			};

			// Act
			var duration = task.ExecutionDuration;

			// Assert
			duration.ShouldNotBeNull();
			duration.Value.TotalMinutes.ShouldBe(-30, 0.1);
		}
	}

	public class IsOverdueTests
	{
		[Fact]
		public void Should_ReturnFalse_When_NoDeadlineSet()
		{
			// Arrange
			var task = new AgentTask
			{
				AgentStatus = TaskAgentStatus.Pending,
				Deadline = null
			};

			// Act
			var isOverdue = task.IsOverdue;

			// Assert
			isOverdue.ShouldBeFalse();
		}

		[Fact]
		public void Should_ReturnFalse_When_DeadlineInFuture()
		{
			// Arrange
			var task = new AgentTask
			{
				AgentStatus = TaskAgentStatus.Pending,
				Deadline = DateTime.UtcNow.AddDays(1)
			};

			// Act
			var isOverdue = task.IsOverdue;

			// Assert
			isOverdue.ShouldBeFalse();
		}

		[Fact]
		public void Should_ReturnTrue_When_DeadlineInPastAndNotCompleted()
		{
			// Arrange
			var task = new AgentTask
			{
				AgentStatus = TaskAgentStatus.Pending,
				Deadline = DateTime.UtcNow.AddDays(-1)
			};

			// Act
			var isOverdue = task.IsOverdue;

			// Assert
			isOverdue.ShouldBeTrue();
		}

		[Fact]
		public void Should_ReturnFalse_When_DeadlineInPastButTaskCompleted()
		{
			// Arrange
			var task = new AgentTask
			{
				AgentStatus = TaskAgentStatus.Completed,
				Deadline = DateTime.UtcNow.AddDays(-1)
			};

			// Act
			var isOverdue = task.IsOverdue;

			// Assert
			isOverdue.ShouldBeFalse();
		}

		[Theory]
		[InlineData(nameof(TaskAgentStatus.Pending), true)]
		[InlineData(nameof(TaskAgentStatus.InProgress), true)]
		[InlineData(nameof(TaskAgentStatus.Failed), true)]
		[InlineData(nameof(TaskAgentStatus.Cancelled), true)]
		[InlineData(nameof(TaskAgentStatus.Paused), true)]
		[InlineData(nameof(TaskAgentStatus.Completed), false)]
		public void Should_HandleOverdueBasedOnStatus_When_DeadlineInPast(string statusName, bool expectedOverdue)
		{
			// Arrange
			var status = Enum.Parse<TaskAgentStatus>(statusName);
			var task = new AgentTask
			{
				AgentStatus = status,
				Deadline = DateTime.UtcNow.AddDays(-1)
			};

			// Act
			var isOverdue = task.IsOverdue;

			// Assert
			isOverdue.ShouldBe(expectedOverdue);
		}

		[Theory]
		[InlineData(-1)] // 1 day late
		[InlineData(-0.5)] // 12 hours late
		[InlineData(-0.0001)] // few minutes late
		public void Should_ReturnTrue_When_VariousOverdueTimes(double daysOverdue)
		{
			// Arrange
			var task = new AgentTask
			{
				AgentStatus = TaskAgentStatus.Pending,
				Deadline = DateTime.UtcNow.AddDays(daysOverdue)
			};

			// Act
			var isOverdue = task.IsOverdue;

			// Assert
			isOverdue.ShouldBeTrue();
		}

		[Theory]
		[InlineData(0.0001)] // few minutes remaining
		[InlineData(0.5)] // 12 hours remaining
		[InlineData(1)] // 1 day remaining
		[InlineData(7)] // 1 week remaining
		public void Should_ReturnFalse_When_VariousTimeRemaining(double daysRemaining)
		{
			// Arrange
			var task = new AgentTask
			{
				AgentStatus = TaskAgentStatus.Pending,
				Deadline = DateTime.UtcNow.AddDays(daysRemaining)
			};

			// Act
			var isOverdue = task.IsOverdue;

			// Assert
			isOverdue.ShouldBeFalse();
		}
	}

	public class BusinessLogicTests
	{
		[Fact]
		public void Should_TrackTaskLifecycle_When_ExecutingCompleteWorkflow()
		{
			// Arrange
			var task = CreateValidAgentTask();
			task.Deadline = DateTime.UtcNow.AddDays(1);

			// Act & Assert - Task lifecycle
			task.AgentStatus.ShouldBe(TaskAgentStatus.Pending);
			task.IsOverdue.ShouldBeFalse();

			// Start task
			task.AgentStatus = TaskAgentStatus.InProgress;
			task.StartedAt = DateTime.UtcNow;

			// Complete task
			task.AgentStatus = TaskAgentStatus.Completed;
			task.CompletedAt = DateTime.UtcNow;

			// Verify final state
			task.ExecutionDuration.ShouldNotBeNull();
			task.ExecutionDuration.Value.TotalSeconds.ShouldBeGreaterThanOrEqualTo(0);
			task.IsOverdue.ShouldBeFalse(); // Completed tasks are never overdue
		}

		[Fact]
		public void Should_TrackFailureScenario_When_TaskEncountersError()
		{
			// Arrange
			var task = CreateValidAgentTask();

			// Act & Assert - Failure scenario
			task.AgentStatus = TaskAgentStatus.InProgress;
			task.StartedAt = DateTime.UtcNow;

			task.AgentStatus = TaskAgentStatus.Failed;
			task.ErrorMessage = "Network timeout occurred";
			task.RetryCount = 1;

			// Verify error state
			task.ErrorMessage.ShouldNotBeNullOrEmpty();
			task.RetryCount.ShouldBeGreaterThan(0);
			task.ExecutionDuration.ShouldBeNull(); // No completion time set
		}

		[Fact]
		public void Should_HandleRetryScenario_When_TaskFailsMultipleTimes()
		{
			// Arrange
			var task = CreateValidAgentTask();

			// Act - Multiple retry scenario
			for (int retry = 1; retry <= 3; retry++)
			{
				task.AgentStatus = TaskAgentStatus.InProgress;
				task.AgentStatus = TaskAgentStatus.Failed;
				task.RetryCount = retry;
			}

			// Assert
			task.RetryCount.ShouldBe(3);
			task.AgentStatus.ShouldBe(TaskAgentStatus.Failed);
		}

		[Fact]
		public void Should_HandlePauseResumeScenario_When_TaskSuspended()
		{
			// Arrange
			var task = CreateValidAgentTask();

			// Act & Assert - Pause/resume workflow
			task.AgentStatus = TaskAgentStatus.InProgress;
			task.AgentStatus = TaskAgentStatus.Paused;
			task.AgentStatus.ShouldBe(TaskAgentStatus.Paused);

			task.AgentStatus = TaskAgentStatus.InProgress;
			task.AgentStatus.ShouldBe(TaskAgentStatus.InProgress);
		}

		[Fact]
		public void Should_HandleCancellationScenario_When_TaskAborted()
		{
			// Arrange
			var task = CreateValidAgentTask();

			// Act & Assert - Cancellation workflow
			task.AgentStatus = TaskAgentStatus.Pending;
			task.AgentStatus = TaskAgentStatus.Cancelled;
			task.AgentStatus.ShouldBe(TaskAgentStatus.Cancelled);
		}

		[Fact]
		public void Should_HandlePriorityEscalation_When_TaskBecomesUrgent()
		{
			// Arrange
			var task = CreateValidAgentTask();
			task.Priority = TaskPriority.Low;

			// Act - Priority escalation
			task.Priority = TaskPriority.Normal;
			task.Priority = TaskPriority.High;
			task.Priority = TaskPriority.Critical;

			// Assert
			task.Priority.ShouldBe(TaskPriority.Critical);
			((int)task.Priority).ShouldBe(4);
		}
	}
} 