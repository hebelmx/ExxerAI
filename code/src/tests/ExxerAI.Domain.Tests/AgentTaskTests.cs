using ExxerAI.Domain;
using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests;

/// <summary>
/// Comprehensive unit tests for AgentTask and related classes
/// </summary>
public class AgentTaskTests
{
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
			var startTime = DateTime.UtcNow.AddMinutes(-45);
			var endTime = startTime.AddMinutes(45);
			var task = new AgentTask
			{
				StartedAt = startTime,
				CompletedAt = endTime
			};

			// Act
			var duration = task.ExecutionDuration;

			// Assert
			duration.ShouldNotBeNull();
			duration.Value.TotalMinutes.ShouldBe(45, 0.001);
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
			duration.Value.TotalMinutes.ShouldBe(durationMinutes, 0.001);
		}

		[Fact]
		public void Should_HandleZeroDuration_When_StartAndEndTimesIdentical()
		{
			// Arrange
			var sameTime = DateTime.UtcNow;
			var task = new AgentTask
			{
				StartedAt = sameTime,
				CompletedAt = sameTime
			};

			// Act
			var duration = task.ExecutionDuration;

			// Assert
			duration.ShouldNotBeNull();
			duration.Value.TotalMilliseconds.ShouldBe(0, 1);
		}

		[Fact]
		public void Should_HandleNegativeDuration_When_EndTimeBeforeStartTime()
		{
			// Arrange
			var startTime = DateTime.UtcNow;
			var endTime = startTime.AddMinutes(-30);
			var task = new AgentTask
			{
				StartedAt = startTime,
				CompletedAt = endTime
			};

			// Act
			var duration = task.ExecutionDuration;

			// Assert
			duration.ShouldNotBeNull();
			duration.Value.TotalMinutes.ShouldBe(-30, 0.001);
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
				AgentStatus = TaskAgentStatus.Pending
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
				Deadline = DateTime.UtcNow.AddDays(1),
				AgentStatus = TaskAgentStatus.Pending
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
				Deadline = DateTime.UtcNow.AddDays(-1),
				AgentStatus = TaskAgentStatus.Pending
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
				Deadline = DateTime.UtcNow.AddDays(-1),
				AgentStatus = TaskAgentStatus.Completed
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
				Deadline = DateTime.UtcNow.AddDays(-1),
				AgentStatus = status
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
				Deadline = DateTime.UtcNow.AddDays(daysOverdue),
				AgentStatus = TaskAgentStatus.InProgress
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
				Deadline = DateTime.UtcNow.AddDays(daysRemaining),
				AgentStatus = TaskAgentStatus.InProgress
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
			var agentId = Guid.NewGuid();

			// Act & Assert - Initial state
			task.AgentStatus.ShouldBe(TaskAgentStatus.Pending);
			task.StartedAt.ShouldBeNull();
			task.CompletedAt.ShouldBeNull();
			task.ExecutionDuration.ShouldBeNull();

			// Act & Assert - Assignment
			task.AssignedAgentId = agentId;
			task.AssignedAgentId.ShouldBe(agentId);

			// Act & Assert - Start execution
			var startTime = DateTime.UtcNow;
			task.StartedAt = startTime;
			task.AgentStatus = TaskAgentStatus.InProgress;
			task.AgentStatus.ShouldBe(TaskAgentStatus.InProgress);
			task.StartedAt.ShouldBe(startTime);

			// Act & Assert - Complete execution
			var endTime = startTime.AddMinutes(30);
			task.CompletedAt = endTime;
			task.AgentStatus = TaskAgentStatus.Completed;
			task.AgentStatus.ShouldBe(TaskAgentStatus.Completed);
			task.ExecutionDuration.ShouldNotBeNull();
			task.ExecutionDuration.Value.TotalMinutes.ShouldBe(30, 0.001);
		}

		[Fact]
		public void Should_TrackFailureScenario_When_TaskEncountersError()
		{
			// Arrange
			var task = CreateValidAgentTask();
			const string errorMessage = "Network connection timeout";

			// Act - Start task
			task.AgentStatus = TaskAgentStatus.InProgress;
			task.StartedAt = DateTime.UtcNow;

			// Act - Failure occurs
			task.AgentStatus = TaskAgentStatus.Failed;
			task.ErrorMessage = errorMessage;
			task.RetryCount = 1;

			// Assert
			task.AgentStatus.ShouldBe(TaskAgentStatus.Failed);
			task.ErrorMessage.ShouldBe(errorMessage);
			task.RetryCount.ShouldBe(1);
			task.CompletedAt.ShouldBeNull();
			task.ExecutionDuration.ShouldBeNull();
		}

		[Fact]
		public void Should_HandleRetryScenario_When_TaskFailsMultipleTimes()
		{
			// Arrange
			var task = CreateValidAgentTask();

			// Act & Assert - First attempt
			task.RetryCount = 0;
			task.AgentStatus = TaskAgentStatus.InProgress;
			task.AgentStatus = TaskAgentStatus.Failed;
			task.RetryCount = 1;
			task.RetryCount.ShouldBe(1);

			// Act & Assert - Second attempt
			task.AgentStatus = TaskAgentStatus.InProgress;
			task.AgentStatus = TaskAgentStatus.Failed;
			task.RetryCount = 2;
			task.RetryCount.ShouldBe(2);

			// Act & Assert - Third attempt succeeds
			task.AgentStatus = TaskAgentStatus.InProgress;
			task.AgentStatus = TaskAgentStatus.Completed;
			task.RetryCount.ShouldBe(2); // Retry count typically doesn't change on success
		}

		[Fact]
		public void Should_HandlePauseResumeScenario_When_TaskSuspended()
		{
			// Arrange
			var task = CreateValidAgentTask();

			// Act & Assert - Start and pause
			task.AgentStatus = TaskAgentStatus.InProgress;
			task.StartedAt = DateTime.UtcNow;
			task.AgentStatus = TaskAgentStatus.Paused;
			task.AgentStatus.ShouldBe(TaskAgentStatus.Paused);

			// Act & Assert - Resume and complete
			task.AgentStatus = TaskAgentStatus.InProgress;
			task.AgentStatus.ShouldBe(TaskAgentStatus.InProgress);
			task.AgentStatus = TaskAgentStatus.Completed;
			task.CompletedAt = DateTime.UtcNow.AddMinutes(45);
			task.AgentStatus.ShouldBe(TaskAgentStatus.Completed);
		}

		[Fact]
		public void Should_HandleCancellationScenario_When_TaskAborted()
		{
			// Arrange
			var task = CreateValidAgentTask();

			// Act - Start and cancel
			task.AgentStatus = TaskAgentStatus.InProgress;
			task.StartedAt = DateTime.UtcNow;
			task.AgentStatus = TaskAgentStatus.Cancelled;

			// Assert
			task.AgentStatus.ShouldBe(TaskAgentStatus.Cancelled);
			task.CompletedAt.ShouldBeNull();
			task.ExecutionDuration.ShouldBeNull();
		}

		[Fact]
		public void Should_HandlePriorityEscalation_When_TaskBecomesUrgent()
		{
			// Arrange
			var task = CreateValidAgentTask();
			task.Priority = TaskPriority.Low;

			// Act & Assert - Priority escalation
			task.Priority = TaskPriority.Normal;
			task.Priority.ShouldBe(TaskPriority.Normal);

			task.Priority = TaskPriority.High;
			task.Priority.ShouldBe(TaskPriority.High);

			task.Priority = TaskPriority.Critical;
			task.Priority.ShouldBe(TaskPriority.Critical);
		}
	}
}

/// <summary>
/// Unit tests for TaskData class
/// </summary>
public class TaskDataTests
{
	public class ConstructorAndDefaultsTests
	{
		[Fact]
		public void Should_InitializeDefaults_When_Created()
		{
			// Act
			var taskData = new TaskData();

			// Assert
			taskData.ContentType.ShouldBe("application/json");
			taskData.Content.ShouldBe(string.Empty);
			taskData.Properties.ShouldNotBeNull();
			taskData.Properties.ShouldBeEmpty();
		}
	}

	public class ContentTypeTests
	{
		[Theory]
		[InlineData("application/json")]
		[InlineData("application/xml")]
		[InlineData("text/plain")]
		[InlineData("text/html")]
		[InlineData("application/octet-stream")]
		[InlineData("custom/type")]
		public void Should_AcceptValidContentTypes_When_Set(string contentType)
		{
			// Arrange
			var taskData = new TaskData();

			// Act
			taskData.ContentType = contentType;

			// Assert
			taskData.ContentType.ShouldBe(contentType);
		}

		[Fact]
		public void Should_DefaultToApplicationJson_When_Created()
		{
			// Act
			var taskData = new TaskData();

			// Assert
			taskData.ContentType.ShouldBe("application/json");
		}
	}

	public class ContentTests
	{
		[Theory]
		[InlineData("")]
		[InlineData("simple text")]
		[InlineData("{\"key\": \"value\"}")]
		[InlineData("<xml><data>value</data></xml>")]
		[InlineData("Multi-line\ncontent\nwith\nbreaks")]
		public void Should_AcceptValidContent_When_Set(string content)
		{
			// Arrange
			var taskData = new TaskData();

			// Act
			taskData.Content = content;

			// Assert
			taskData.Content.ShouldBe(content);
		}

		[Fact]
		public void Should_DefaultToEmptyString_When_Created()
		{
			// Act
			var taskData = new TaskData();

			// Assert
			taskData.Content.ShouldBe(string.Empty);
		}
	}

	public class PropertiesTests
	{
		[Fact]
		public void Should_AllowCustomProperties_When_Added()
		{
			// Arrange
			var taskData = new TaskData();

			// Act
			taskData.Properties["priority"] = "high";
			taskData.Properties["category"] = "development";
			taskData.Properties["estimatedDuration"] = 120;

			// Assert
			taskData.Properties.Count.ShouldBe(3);
			taskData.Properties["priority"].ShouldBe("high");
			taskData.Properties["category"].ShouldBe("development");
			taskData.Properties["estimatedDuration"].ShouldBe(120);
		}

		[Fact]
		public void Should_HandleVariousDataTypes_When_AddedToProperties()
		{
			// Arrange
			var taskData = new TaskData();

			// Act
			taskData.Properties["stringValue"] = "text";
			taskData.Properties["intValue"] = 42;
			taskData.Properties["doubleValue"] = 3.14;
			taskData.Properties["boolValue"] = true;
			taskData.Properties["dateValue"] = DateTime.UtcNow;
			taskData.Properties["guidValue"] = Guid.NewGuid();

			// Assert
			taskData.Properties.Count.ShouldBe(6);
			taskData.Properties["stringValue"].ShouldBe("text");
			taskData.Properties["intValue"].ShouldBe(42);
			taskData.Properties["doubleValue"].ShouldBe(3.14);
			taskData.Properties["boolValue"].ShouldBe(true);
			taskData.Properties["dateValue"].ShouldBeOfType<DateTime>();
			taskData.Properties["guidValue"].ShouldBeOfType<Guid>();
		}

		[Fact]
		public void Should_AllowPropertyModification_When_Updated()
		{
			// Arrange
			var taskData = new TaskData();
			taskData.Properties["status"] = "initial";

			// Act
			taskData.Properties["status"] = "updated";

			// Assert
			taskData.Properties["status"].ShouldBe("updated");
			taskData.Properties.Count.ShouldBe(1);
		}

		[Fact]
		public void Should_AllowPropertyRemoval_When_Deleted()
		{
			// Arrange
			var taskData = new TaskData();
			taskData.Properties["temp"] = "value";
			taskData.Properties.Count.ShouldBe(1);

			// Act
			var removed = taskData.Properties.Remove("temp");

			// Assert
			removed.ShouldBeTrue();
			taskData.Properties.Count.ShouldBe(0);
		}

		[Fact]
		public void Should_HandleComplexObjects_When_AddedToProperties()
		{
			// Arrange
			var taskData = new TaskData();
			var complexObject = new { Name = "Test", Values = new[] { 1, 2, 3 } };

			// Act
			taskData.Properties["complex"] = complexObject;

			// Assert
			taskData.Properties["complex"].ShouldBe(complexObject);
		}
	}
}

/// <summary>
/// Unit tests for TaskMetadata class
/// </summary>
public class TaskMetadataTests
{
	public class ConstructorAndDefaultsTests
	{
		[Fact]
		public void Should_InitializeEmptyCollections_When_Created()
		{
			// Act
			var metadata = new TaskMetadata();

			// Assert
			metadata.Properties.ShouldNotBeNull();
			metadata.Properties.ShouldBeEmpty();
			metadata.Context.ShouldNotBeNull();
			metadata.Context.ShouldBeEmpty();
			metadata.Metrics.ShouldNotBeNull();
			metadata.Metrics.ShouldBeEmpty();
		}
	}

	public class PropertiesTests
	{
		[Fact]
		public void Should_AllowCustomProperties_When_Added()
		{
			// Arrange
			var metadata = new TaskMetadata();

			// Act
			metadata.Properties["environment"] = "production";
			metadata.Properties["version"] = "1.2.3";
			metadata.Properties["debug"] = true;

			// Assert
			metadata.Properties.Count.ShouldBe(3);
			metadata.Properties["environment"].ShouldBe("production");
			metadata.Properties["version"].ShouldBe("1.2.3");
			metadata.Properties["debug"].ShouldBe(true);
		}

		[Fact]
		public void Should_HandleVariousDataTypes_When_AddedToProperties()
		{
			// Arrange
			var metadata = new TaskMetadata();

			// Act
			metadata.Properties["string"] = "value";
			metadata.Properties["number"] = 42;
			metadata.Properties["decimal"] = 3.14m;
			metadata.Properties["boolean"] = false;
			metadata.Properties["array"] = new[] { "a", "b", "c" };

			// Assert
			metadata.Properties.Count.ShouldBe(5);
			metadata.Properties["string"].ShouldBe("value");
			metadata.Properties["number"].ShouldBe(42);
			metadata.Properties["decimal"].ShouldBe(3.14m);
			metadata.Properties["boolean"].ShouldBe(false);
			metadata.Properties["array"].ShouldBeOfType<string[]>();
		}
	}

	public class ContextTests
	{
		[Fact]
		public void Should_AllowContextInformation_When_Added()
		{
			// Arrange
			var metadata = new TaskMetadata();

			// Act
			metadata.Context["userId"] = "user123";
			metadata.Context["sessionId"] = "session456";
			metadata.Context["requestId"] = "req789";

			// Assert
			metadata.Context.Count.ShouldBe(3);
			metadata.Context["userId"].ShouldBe("user123");
			metadata.Context["sessionId"].ShouldBe("session456");
			metadata.Context["requestId"].ShouldBe("req789");
		}

		[Fact]
		public void Should_HandleEmptyAndNullValues_When_AddedToContext()
		{
			// Arrange
			var metadata = new TaskMetadata();

			// Act
			metadata.Context["empty"] = "";
			metadata.Context["whitespace"] = "   ";
			metadata.Context["normal"] = "value";

			// Assert
			metadata.Context.Count.ShouldBe(3);
			metadata.Context["empty"].ShouldBe("");
			metadata.Context["whitespace"].ShouldBe("   ");
			metadata.Context["normal"].ShouldBe("value");
		}

		[Fact]
		public void Should_AllowContextModification_When_Updated()
		{
			// Arrange
			var metadata = new TaskMetadata();
			metadata.Context["status"] = "initial";

			// Act
			metadata.Context["status"] = "updated";

			// Assert
			metadata.Context["status"].ShouldBe("updated");
			metadata.Context.Count.ShouldBe(1);
		}
	}

	public class MetricsTests
	{
		[Fact]
		public void Should_AllowMetricsStorage_When_Added()
		{
			// Arrange
			var metadata = new TaskMetadata();

			// Act
			metadata.Metrics["execution_time"] = 1500.5;
			metadata.Metrics["memory_usage"] = 256.0;
			metadata.Metrics["cpu_utilization"] = 85.7;

			// Assert
			metadata.Metrics.Count.ShouldBe(3);
			metadata.Metrics["execution_time"].ShouldBe(1500.5);
			metadata.Metrics["memory_usage"].ShouldBe(256.0);
			metadata.Metrics["cpu_utilization"].ShouldBe(85.7);
		}

		[Theory]
		[InlineData(0.0)]
		[InlineData(1.5)]
		[InlineData(100.0)]
		[InlineData(999999.99)]
		[InlineData(-1.0)]
		public void Should_AcceptVariousMetricValues_When_Added(double metricValue)
		{
			// Arrange
			var metadata = new TaskMetadata();

			// Act
			metadata.Metrics["test_metric"] = metricValue;

			// Assert
			metadata.Metrics["test_metric"].ShouldBe(metricValue);
		}

		[Fact]
		public void Should_HandlePerformanceMetrics_When_TrackingExecution()
		{
			// Arrange
			var metadata = new TaskMetadata();

			// Act - Simulate performance tracking
			metadata.Metrics["start_time"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			metadata.Metrics["peak_memory_mb"] = 128.5;
			metadata.Metrics["avg_cpu_percent"] = 23.7;
			metadata.Metrics["network_bytes_sent"] = 1024.0;
			metadata.Metrics["network_bytes_received"] = 2048.0;

			// Assert
			metadata.Metrics.Count.ShouldBe(5);
			metadata.Metrics["start_time"].ShouldBeGreaterThan(0);
			metadata.Metrics["peak_memory_mb"].ShouldBe(128.5);
			metadata.Metrics["avg_cpu_percent"].ShouldBe(23.7);
			metadata.Metrics["network_bytes_sent"].ShouldBe(1024.0);
			metadata.Metrics["network_bytes_received"].ShouldBe(2048.0);
		}

		[Fact]
		public void Should_AllowMetricUpdates_When_ValuesChange()
		{
			// Arrange
			var metadata = new TaskMetadata();
			metadata.Metrics["counter"] = 1.0;

			// Act
			metadata.Metrics["counter"] = 2.0;
			metadata.Metrics["counter"] = 3.0;

			// Assert
			metadata.Metrics["counter"].ShouldBe(3.0);
			metadata.Metrics.Count.ShouldBe(1);
		}
	}

	public class IntegratedTests
	{
		[Fact]
		public void Should_HandleAllCollectionsTogether_When_UsedSimultaneously()
		{
			// Arrange
			var metadata = new TaskMetadata();

			// Act
			metadata.Properties["environment"] = "test";
			metadata.Context["user"] = "testUser";
			metadata.Metrics["duration"] = 123.45;

			// Assert
			metadata.Properties.Count.ShouldBe(1);
			metadata.Context.Count.ShouldBe(1);
			metadata.Metrics.Count.ShouldBe(1);
			metadata.Properties["environment"].ShouldBe("test");
			metadata.Context["user"].ShouldBe("testUser");
			metadata.Metrics["duration"].ShouldBe(123.45);
		}

		[Fact]
		public void Should_MaintainSeparateCollections_When_SameKeysUsed()
		{
			// Arrange
			var metadata = new TaskMetadata();

			// Act
			metadata.Properties["key"] = "property_value";
			metadata.Context["key"] = "context_value";
			metadata.Metrics["key"] = 42.0;

			// Assert
			metadata.Properties["key"].ShouldBe("property_value");
			metadata.Context["key"].ShouldBe("context_value");
			metadata.Metrics["key"].ShouldBe(42.0);
		}
	}
} 