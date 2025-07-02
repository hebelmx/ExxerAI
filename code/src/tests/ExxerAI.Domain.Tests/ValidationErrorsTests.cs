using ExxerAI.Domain;
using Shouldly;

namespace ExxerAI.Domain.Tests;

/// <summary>
/// Tests for null argument validation errors and Result<T> extensions
/// </summary>
public class ValidationErrorsTests
{
	/// <summary>
	/// Tests for NullArgumentError functionality
	/// </summary>
	public class NullArgumentErrorTests
	{
		[Fact]
		public void Constructor_WithParameterName_ShouldSetProperties()
		{
			// Arrange
			const string parameterName = "testParam";

			// Act
			var error = new NullArgumentError(parameterName);

			// Assert
			error.ParameterName.ShouldBe(parameterName);
			error.Message.ShouldBe($"Parameter '{parameterName}' cannot be null");
		}

		[Fact]
		public void Constructor_WithParameterNameAndMessage_ShouldSetProperties()
		{
			// Arrange
			const string parameterName = "testParam";
			const string customMessage = "Custom error message";

			// Act
			var error = new NullArgumentError(parameterName, customMessage);

			// Assert
			error.ParameterName.ShouldBe(parameterName);
			error.Message.ShouldBe(customMessage);
		}

		[Fact]
		public void ToString_WithDefaultMessage_ShouldReturnFormattedMessage()
		{
			// Arrange
			const string parameterName = "testParam";
			var error = new NullArgumentError(parameterName);

			// Act
			var result = error.ToString();

			// Assert
			result.ShouldBe($"Parameter '{parameterName}' cannot be null");
		}

		[Fact]
		public void ToString_WithCustomMessage_ShouldReturnCustomMessage()
		{
			// Arrange
			const string parameterName = "testParam";
			const string customMessage = "Custom error message";
			var error = new NullArgumentError(parameterName, customMessage);

			// Act
			var result = error.ToString();

			// Assert
			result.ShouldBe(customMessage);
		}
	}

	/// <summary>
	/// Tests for MultipleNullArgumentsError functionality
	/// </summary>
	public class MultipleNullArgumentsErrorTests
	{
		[Fact]
		public void Constructor_WithParameterNames_ShouldCreateErrors()
		{
			// Arrange
			var parameterNames = new[] { "param1", "param2", "param3" };

			// Act
			var error = new MultipleNullArgumentsError(parameterNames);

			// Assert
			error.Errors.Count.ShouldBe(3);
			error.Errors[0].ParameterName.ShouldBe("param1");
			error.Errors[1].ParameterName.ShouldBe("param2");
			error.Errors[2].ParameterName.ShouldBe("param3");
		}

		[Fact]
		public void Constructor_WithErrorObjects_ShouldSetErrors()
		{
			// Arrange
			var errors = new[]
			{
				new NullArgumentError("param1", "Custom message 1"),
				new NullArgumentError("param2", "Custom message 2")
			};

			// Act
			var multipleError = new MultipleNullArgumentsError(errors);

			// Assert
			multipleError.Errors.Count.ShouldBe(2);
			multipleError.Errors[0].Message.ShouldBe("Custom message 1");
			multipleError.Errors[1].Message.ShouldBe("Custom message 2");
		}

		[Fact]
		public void ToString_ShouldReturnFormattedMessage()
		{
			// Arrange
			var parameterNames = new[] { "param1", "param2", "param3" };
			var error = new MultipleNullArgumentsError(parameterNames);

			// Act
			var result = error.ToString();

			// Assert
			result.ShouldBe("Multiple null parameters: param1, param2, param3");
		}
	}

	/// <summary>
	/// Tests for Result<T> extension methods
	/// </summary>
	public class ResultExtensionsTests
	{
		[Fact]
		public void FailForNullArgument_Generic_ShouldReturnFailureResult()
		{
			// Act
			var result = ResultExtensions.FailForNullArgument<string>("testParam");

			// Assert
			result.IsFailure.ShouldBeTrue();
			result.Error.ShouldContain("Parameter 'testParam' cannot be null");
		}

		[Fact]
		public void FailForNullArgument_WithCustomMessage_ShouldReturnFailureWithMessage()
		{
			// Arrange
			const string customMessage = "Custom validation error";

			// Act
			var result = ResultExtensions.FailForNullArgument<string>("testParam", customMessage);

			// Assert
			result.IsFailure.ShouldBeTrue();
			result.Error.ShouldContain(customMessage);
		}

		[Fact]
		public void FailForNullArguments_Generic_ShouldReturnFailureWithMultipleParams()
		{
			// Act
			var result = ResultExtensions.FailForNullArguments<string>("param1", "param2", "param3");

			// Assert
			result.IsFailure.ShouldBeTrue();
			result.Error.ShouldContain("Multiple null parameters: param1, param2, param3");
		}

		[Fact]
		public void FailForNullArgument_NonGeneric_ShouldReturnFailureResult()
		{
			// Act
			var result = ResultExtensions.FailForNullArgument("testParam");

			// Assert
			result.IsFailure.ShouldBeTrue();
			result.Error.ShouldContain("Parameter 'testParam' cannot be null");
		}

		[Fact]
		public void EnsureNotNull_WithValidValue_ShouldReturnSuccess()
		{
			// Arrange
			const string validValue = "test value";

			// Act
			var result = ResultExtensions.EnsureNotNull(validValue, "testParam");

			// Assert
			result.IsSuccess.ShouldBeTrue();
			result.Data.ShouldBe(validValue);
		}

		[Fact]
		public void EnsureNotNull_WithNullValue_ShouldReturnFailure()
		{
			// Arrange
			string? nullValue = null;

			// Act
			var result = ResultExtensions.EnsureNotNull(nullValue, "testParam");

			// Assert
			result.IsFailure.ShouldBeTrue();
			result.Error.ShouldContain("Parameter 'testParam' cannot be null");
		}

		[Fact]
		public void EnsureNotNull_Nullable_WithValue_ShouldReturnSuccess()
		{
			// Arrange
			int? validValue = 42;

			// Act
			var result = ResultExtensions.EnsureNotNull(validValue, "testParam");

			// Assert
			result.IsSuccess.ShouldBeTrue();
			result.Data.ShouldBe(42);
		}

		[Fact]
		public void EnsureNotNull_Nullable_WithNull_ShouldReturnFailure()
		{
			// Arrange
			int? nullValue = null;

			// Act
			var result = ResultExtensions.EnsureNotNull(nullValue, "testParam");

			// Assert
			result.IsFailure.ShouldBeTrue();
			result.Error.ShouldContain("Parameter 'testParam' cannot be null");
		}

		[Fact]
		public void ValidateNotNull_WithAllValidValues_ShouldReturnSuccess()
		{
			// Arrange
			var validations = new[]
			{
				((object?)"value1", "param1"),
				((object?)42, "param2"),
				((object?)new object(), "param3")
			};

			// Act
			var result = ResultExtensions.ValidateNotNull(validations);

			// Assert
			result.IsSuccess.ShouldBeTrue();
		}

		[Fact]
		public void ValidateNotNull_WithSomeNullValues_ShouldReturnFailure()
		{
			// Arrange
			var validations = new[]
			{
				((object?)"value1", "param1"),
				((object?)null, "param2"),
				((object?)null, "param3")
			};

			// Act
			var result = ResultExtensions.ValidateNotNull(validations);

			// Assert
			result.IsFailure.ShouldBeTrue();
			result.Error.ShouldContain("Multiple null parameters: param2, param3");
		}

		[Fact]
		public void CreateIfValid_WithAllValidValues_ShouldReturnFactoryResult()
		{
			// Arrange
			const string expectedValue = "Created Value";
			var validations = new[]
			{
				((object?)"value1", "param1"),
				((object?)42, "param2")
			};

			// Act
			var result = ResultExtensions.CreateIfValid(() => expectedValue, validations);

			// Assert
			result.IsSuccess.ShouldBeTrue();
			result.Data.ShouldBe(expectedValue);
		}

		[Fact]
		public void CreateIfValid_WithNullValues_ShouldReturnFailure()
		{
			// Arrange
			var validations = new[]
			{
				((object?)"value1", "param1"),
				((object?)null, "param2")
			};

			// Act
			var result = ResultExtensions.CreateIfValid(() => "Created Value", validations);

			// Assert
			result.IsFailure.ShouldBeTrue();
			result.Error.ShouldContain("Parameter 'param2' cannot be null");
		}
	}
} 