using System.Collections.Generic;
using Xunit;
using Shouldly;
using ExxerAI.Domain;

namespace ExxerAI.Domain.Tests
{
    /// <summary>
    /// Comprehensive tests for Result and Result<T> classes following I-TDD principles.
    /// These tests cover the fundamental error handling pattern used throughout ExxerAI.
    /// </summary>
    public class ResultTests
    {
        /// <summary>
        /// Contract Test: Result.Success() should create successful result
        /// </summary>
        [Fact]
        public void Success_ShouldCreateSuccessfulResult_When_Called()
        {
            // Act
            var result = Result.Success();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.IsFailure.ShouldBeFalse();
            result.Errors.ShouldBeEmpty();
            result.Error.ShouldBeNull();
        }

        /// <summary>
        /// Contract Test: Result.WithFailure() should create failed result with error
        /// </summary>
        [Fact]
        public void WithFailure_ShouldCreateFailedResult_When_ErrorProvided()
        {
            // Arrange
            const string errorMessage = "Something went wrong";

            // Act
            var result = Result.WithFailure(errorMessage);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe(errorMessage);
            result.Errors.ShouldContain(errorMessage);
        }

        /// <summary>
        /// Contract Test: Result.WithFailure() should handle multiple errors
        /// </summary>
        [Fact]
        public void WithFailure_ShouldCreateFailedResultWithMultipleErrors_When_ErrorCollectionProvided()
        {
            // Arrange
            var errors = new[] { "Error 1", "Error 2", "Error 3" };

            // Act
            var result = Result.WithFailure(errors);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Error 1");
            result.Errors.ShouldBe(errors);
        }

        /// <summary>
        /// Contract Test: Result.ToString() should show meaningful representation
        /// </summary>
        [Theory]
        [InlineData(true, null, "Success")]
        [InlineData(false, "Test error", "WithFailure: Test error")]
        public void ToString_ShouldReturnMeaningfulRepresentation_When_Called(bool isSuccess, string errorMessage, string expected)
        {
            // Arrange
            var result = isSuccess ? Result.Success() : Result.WithFailure(errorMessage);

            // Act
            var stringResult = result.ToString();

            // Assert
            stringResult.ShouldBe(expected);
        }

        /// <summary>
        /// Behavior Test: OnSuccess should execute action when successful
        /// </summary>
        [Fact]
        public void OnSuccess_ShouldExecuteAction_When_ResultIsSuccessful()
        {
            // Arrange
            var result = Result.Success();
            var executed = false;

            // Act
            var returnedResult = result.OnSuccess(() => executed = true);

            // Assert
            executed.ShouldBeTrue();
            returnedResult.ShouldBeSameAs(result); // Fluent interface
        }

        /// <summary>
        /// Behavior Test: OnSuccess should not execute action when failed
        /// </summary>
        [Fact]
        public void OnSuccess_ShouldNotExecuteAction_When_ResultIsFailed()
        {
            // Arrange
            var result = Result.WithFailure("Error");
            var executed = false;

            // Act
            var returnedResult = result.OnSuccess(() => executed = true);

            // Assert
            executed.ShouldBeFalse();
            returnedResult.ShouldBeSameAs(result);
        }

        /// <summary>
        /// Behavior Test: OnFailure should execute action when failed
        /// </summary>
        [Fact]
        public void OnFailure_ShouldExecuteAction_When_ResultIsFailed()
        {
            // Arrange
            var result = Result.WithFailure("Test error");
            var capturedErrors = new List<string>();

            // Act
            var returnedResult = result.OnFailure(errors => capturedErrors.AddRange(errors));

            // Assert
            capturedErrors.ShouldContain("Test error");
            returnedResult.ShouldBeSameAs(result);
        }

        /// <summary>
        /// Behavior Test: Map should transform successful result
        /// </summary>
        [Fact]
        public void Map_ShouldTransformValue_When_ResultIsSuccessful()
        {
            // Arrange
            var result = Result.Success();

            // Act
            var mappedResult = result.Map(() => "mapped value");

            // Assert
            mappedResult.IsSuccess.ShouldBeTrue();
            mappedResult.Data.ShouldBe("mapped value");
        }

        /// <summary>
        /// Behavior Test: Map should propagate errors
        /// </summary>
        [Fact]
        public void Map_ShouldPropagateErrors_When_ResultIsFailed()
        {
            // Arrange
            var result = Result.WithFailure("Original error");

            // Act
            var mappedResult = result.Map(() => "mapped value");

            // Assert
            mappedResult.IsSuccess.ShouldBeFalse();
            mappedResult.Error.ShouldBe("Original error");
        }

        /// <summary>
        /// Behavior Test: Bind should chain successful operations
        /// </summary>
        [Fact]
        public void Bind_ShouldChainOperations_When_ResultIsSuccessful()
        {
            // Arrange
            var result = Result.Success();

            // Act
            var boundResult = result.Bind(() => Result<string>.WithSuccess("bound value"));

            // Assert
            boundResult.IsSuccess.ShouldBeTrue();
            boundResult.Data.ShouldBe("bound value");
        }

        /// <summary>
        /// Behavior Test: Ensure should validate conditions
        /// </summary>
        [Theory]
        [InlineData(true, true, "Success")]
        [InlineData(true, false, "Condition failed")]
        [InlineData(false, true, "Original error")]
        public void Ensure_ShouldValidateCondition_When_Called(bool initialSuccess, bool conditionResult, string expectedError)
        {
            // Arrange
            var result = initialSuccess ? Result.Success() : Result.WithFailure("Original error");

            // Act
            var ensuredResult = result.Ensure(() => conditionResult, "Condition failed");

            // Assert
            if (expectedError == "Success")
            {
                ensuredResult.IsSuccess.ShouldBeTrue();
            }
            else
            {
                ensuredResult.IsSuccess.ShouldBeFalse();
                ensuredResult.Error.ShouldBe(expectedError);
            }
        }

        /// <summary>
        /// Behavior Test: Combine should aggregate results
        /// </summary>
        [Fact]
        public void Combine_ShouldAggregateResults_When_MultipleResultsProvided()
        {
            // Arrange
            var successResult = Result.Success();
            var results = new[]
            {
                Result.Success(),
                Result.WithFailure("Error 1"),
                Result.WithFailure("Error 2")
            };

            // Act
            var combinedResult = successResult.Combine(results);

            // Assert
            combinedResult.IsSuccess.ShouldBeFalse();
            combinedResult.Errors.ShouldContain("Error 1");
            combinedResult.Errors.ShouldContain("Error 2");
        }

        /// <summary>
        /// Behavior Test: Match should execute appropriate function
        /// </summary>
        [Theory]
        [InlineData(true, "success")]
        [InlineData(false, "failure")]
        public void Match_ShouldExecuteAppropriateFunction_When_Called(bool isSuccess, string expected)
        {
            // Arrange
            var result = isSuccess ? Result.Success() : Result.WithFailure("error");

            // Act
            var matchResult = result.Match(
                onSuccess: () => "success",
                onFailure: errors => "failure");

            // Assert
            matchResult.ShouldBe(expected);
        }

        /// <summary>
        /// Recovery Test: Recover should handle failures
        /// </summary>
        [Fact]
        public void Recover_ShouldProvideRecovery_When_ResultIsFailed()
        {
            // Arrange
            var result = Result.WithFailure("Error");

            // Act
            var recoveredResult = result.Recover(() => Result.Success());

            // Assert
            recoveredResult.IsSuccess.ShouldBeTrue();
        }

        /// <summary>
        /// Static Method Test: CombineErrors should merge error collections
        /// </summary>
        [Fact]
        public void CombineErrors_ShouldMergeErrorCollections_When_ErrorsProvided()
        {
            // Arrange
            var primaryErrors = new[] { "Primary 1", "Primary 2" };
            var secondaryErrors = new[] { "Secondary 1" };

            // Act
            var result = Result.CombineErrors(primaryErrors, secondaryErrors);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Errors.ShouldContain("Primary 1");
            result.Errors.ShouldContain("Primary 2");
            result.Errors.ShouldContain("Secondary 1");
        }
    }

    /// <summary>
    /// Comprehensive tests for Result<T> class covering typed results
    /// </summary>
    public class ResultOfTTests
    {
        /// <summary>
        /// Contract Test: Result<T>.Success() should create successful typed result
        /// </summary>
        [Fact]
        public void Success_ShouldCreateSuccessfulTypedResult_When_ValueProvided()
        {
            // Arrange
            const string value = "test value";

            // Act
            var result = Result<string>.Success(value);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.IsFailure.ShouldBeFalse();
            result.Data.ShouldBe(value);
            result.Value.ShouldBe(value);
            result.Errors.ShouldBeNullOrEmpty();
            result.Error.ShouldBeNull();
        }

        /// <summary>
        /// Contract Test: Result<T>.WithFailure() should create failed typed result
        /// </summary>
        [Fact]
        public void WithFailure_ShouldCreateFailedTypedResult_When_ErrorProvided()
        {
            // Arrange
            const string errorMessage = "Something went wrong";

            // Act
            var result = Result<string>.WithFailure(errorMessage);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.IsFailure.ShouldBeTrue();
            result.Data.ShouldBeNull();
            result.Value.ShouldBeNull();
            result.Error.ShouldBe(errorMessage);
        }

        /// <summary>
        /// Contract Test: Result<T> with null value should be failure
        /// </summary>
        [Fact]
        public void Result_ShouldBeFailed_When_ValueIsNull()
        {
            // Act
            var result = Result<string>.Success(null!);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.IsFailure.ShouldBeTrue();
            result.Data.ShouldBeNull();
        }

        /// <summary>
        /// Contract Test: Result<T>.WithWarnings() should create successful result with warnings
        /// </summary>
        [Fact]
        public void WithWarnings_ShouldCreateSuccessfulResultWithWarnings_When_WarningsProvided()
        {
            // Arrange
            const string value = "test value";
            var warnings = new List<string> { "Warning 1", "Warning 2" };

            // Act
            var result = Result<string>.WithWarnings(warnings, value);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.HasWarnings.ShouldBeTrue();
            result.IsRecoverable.ShouldBeTrue();
            result.Data.ShouldBe(value);
            result.Errors.ShouldBe(warnings);
        }

        /// <summary>
        /// Behavior Test: Implicit conversion from T to Result<T>
        /// </summary>
        [Fact]
        public void ImplicitConversion_ShouldCreateSuccessfulResult_When_ValueAssigned()
        {
            // Act
            Result<string> result = "test value";

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldBe("test value");
        }

        /// <summary>
        /// Behavior Test: Implicit conversion to non-generic Result
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ImplicitConversionToResult_ShouldPreserveState_When_Converted(bool isSuccess)
        {
            // Arrange
            var typedResult = isSuccess 
                ? Result<string>.Success("value") 
                : Result<string>.WithFailure("error");

            // Act
            Result result = typedResult;

            // Assert
            result.IsSuccess.ShouldBe(isSuccess);
            result.IsFailure.ShouldBe(!isSuccess);
        }

        /// <summary>
        /// Behavior Test: Deconstruction should expose all components
        /// </summary>
        [Fact]
        public void Deconstruct_ShouldExposeAllComponents_When_Called()
        {
            // Arrange
            var result = Result<string>.Success("test value");

            // Act
            var (succeeded, data, errors) = result;

            // Assert
            succeeded.ShouldBeTrue();
            data.ShouldBe("test value");
            errors.ShouldBeEmpty();
        }

        /// <summary>
        /// Behavior Test: OnSuccess should execute action with value
        /// </summary>
        [Fact]
        public void OnSuccess_ShouldExecuteActionWithValue_When_Successful()
        {
            // Arrange
            var result = Result<string>.Success("test");
            string? capturedValue = null;

            // Act
            var returnedResult = result.OnSuccess(value => capturedValue = value);

            // Assert
            capturedValue.ShouldBe("test");
            returnedResult.ShouldBeSameAs(result);
        }

        /// <summary>
        /// Behavior Test: Map should transform typed values
        /// </summary>
        [Fact]
        public void Map_ShouldTransformTypedValue_When_Successful()
        {
            // Arrange
            var result = Result<string>.Success("123");

            // Act
            var mappedResult = result.Map(value => int.Parse(value));

            // Assert
            mappedResult.IsSuccess.ShouldBeTrue();
            mappedResult.Data.ShouldBe(123);
        }

        /// <summary>
        /// Behavior Test: Bind should chain typed operations
        /// </summary>
        [Fact]
        public void Bind_ShouldChainTypedOperations_When_Successful()
        {
            // Arrange
            var result = Result<string>.Success("test");

            // Act
            var boundResult = result.Bind(value => Result<int>.Success(value.Length));

            // Assert
            boundResult.IsSuccess.ShouldBeTrue();
            boundResult.Data.ShouldBe(4);
        }

        /// <summary>
        /// Behavior Test: Ensure should validate typed conditions
        /// </summary>
        [Fact]
        public void Ensure_ShouldValidateTypedCondition_When_Called()
        {
            // Arrange
            var result = Result<string>.Success("test");

            // Act
            var ensuredResult = result.Ensure(value => value.Length > 3, "Too short");

            // Assert
            ensuredResult.IsSuccess.ShouldBeTrue();
            ensuredResult.Data.ShouldBe("test");
        }

        /// <summary>
        /// Behavior Test: Match should handle typed results
        /// </summary>
        [Fact]
        public void Match_ShouldHandleTypedResults_When_Called()
        {
            // Arrange
            var result = Result<string>.Success("test");

            // Act
            var matchResult = result.Match(
                onSuccess: value => value.ToUpper(),
                onFailure: errors => "FAILED");

            // Assert
            matchResult.ShouldBe("TEST");
        }

        /// <summary>
        /// Recovery Test: Recover should handle typed failures
        /// </summary>
        [Fact]
        public void Recover_ShouldHandleTypedFailures_When_Failed()
        {
            // Arrange
            var result = Result<string>.WithFailure("Error");

            // Act
            var recoveredResult = result.Recover(() => Result<string>.Success("recovered"));

            // Assert
            recoveredResult.IsSuccess.ShouldBeTrue();
            recoveredResult.Data.ShouldBe("recovered");
        }

        /// <summary>
        /// Recovery Test: RecoverWith should handle cross-type recovery
        /// </summary>
        [Fact]
        public void RecoverWith_ShouldHandleCrossTypeRecovery_When_Failed()
        {
            // Arrange
            var result = Result<string>.WithFailure("Error");

            // Act
            var recoveredResult = result.RecoverWith(() => Result<int>.Success(42));

            // Assert
            recoveredResult.IsSuccess.ShouldBeTrue();
            recoveredResult.Data.ShouldBe(42);
        }

        /// <summary>
        /// Edge Case Test: Empty string should be successful
        /// </summary>
        [Fact]
        public void EmptyString_ShouldBeSuccessful_When_Provided()
        {
            // Act
            var result = Result<string>.Success(string.Empty);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldBe(string.Empty);
        }

        /// <summary>
        /// Edge Case Test: Empty collection should be successful
        /// </summary>
        [Fact]
        public void EmptyCollection_ShouldBeSuccessful_When_Provided()
        {
            // Act
            var result = Result<List<string>>.Success(new List<string>());

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.Data.ShouldNotBeNull();
            result.Data!.Count.ShouldBe(0);
        }

        /// <summary>
        /// Static Method Test: CombineErrors should work with typed results
        /// </summary>
        [Fact]
        public void CombineErrors_ShouldWorkWithTypedResults_When_ErrorsProvided()
        {
            // Arrange
            var primaryErrors = new[] { "Error 1" };
            var secondaryErrors = new[] { "Error 2" };

            // Act
            var result = Result<string>.CombineErrors(primaryErrors, secondaryErrors);

            // Assert
            result.IsSuccess.ShouldBeFalse();
            result.Errors.ShouldContain("Error 1");
            result.Errors.ShouldContain("Error 2");
        }

        /// <summary>
        /// ToString Test: Should show meaningful representation for typed results
        /// </summary>
        [Theory]
        [InlineData("test", "Success: test")]
        [InlineData(42, "Success: 42")]
        public void ToString_ShouldShowMeaningfulRepresentation_When_Successful<T>(T value, string expected)
        {
            // Arrange
            var result = Result<T>.Success(value);

            // Act
            var stringResult = result.ToString();

            // Assert
            stringResult.ShouldBe(expected);
        }
    }

    /// <summary>
    /// Tests for EnumerableExtensions utility methods
    /// </summary>
    public class EnumerableExtensionsTests
    {
        /// <summary>
        /// Contract Test: NotNullOrEmpty should validate collections correctly
        /// </summary>
        [Theory]
        [InlineData(new[] { "item" }, true)]
        [InlineData(new string[0], false)]
        [InlineData(null, false)]
        public void NotNullOrEmpty_ShouldValidateCollections_When_Called(IEnumerable<string>? collection, bool expected)
        {
            // Act
            var result = collection.NotNullOrEmpty();

            // Assert
            result.ShouldBe(expected);
        }
    }
} 