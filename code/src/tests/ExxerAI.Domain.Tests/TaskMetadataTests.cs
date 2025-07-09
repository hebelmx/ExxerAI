using ExxerAI.Domain;
using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests;

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