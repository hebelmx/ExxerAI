using ExxerAI.Domain;
using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests;

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