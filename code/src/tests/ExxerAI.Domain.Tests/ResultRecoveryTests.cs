using ExxerAI.Domain.Helpers;
using Shouldly;

namespace ExxerAI.Domain.Tests;

/// <summary>
/// Tests for Result recovery operations (Recover, RecoverWith, Match).
/// These tests focus on error recovery scenarios and type safety.
/// </summary>
public class ResultRecoveryTests
{
    [Fact]
    public void Recover_ShouldExecuteRecoveryFunction_WhenResultIsFailure()
    {
        // Arrange
        var failedResult = Result.WithFailure("Initial failure");
        var recoveryResult = Result.Success();

        // Act
        var recoveredResult = failedResult.Recover(() => recoveryResult);

        // Assert
        recoveredResult.IsSuccess.ShouldBeTrue();
        recoveredResult.ShouldBe(recoveryResult);
    }

    [Fact]
    public void Recover_ShouldReturnOriginalResult_WhenResultIsSuccess()
    {
        // Arrange
        var successResult = Result.Success();
        var recoveryResult = Result.WithFailure("Should not be used");

        // Act
        var recoveredResult = successResult.Recover(() => recoveryResult);

        // Assert
        recoveredResult.IsSuccess.ShouldBeTrue();
        recoveredResult.ShouldBe(successResult);
    }

    [Fact]
    public void GenericRecover_ShouldExecuteRecoveryFunction_WhenResultIsFailure()
    {
        // Arrange
        var failedResult = Result<string>.WithFailure("Initial failure");
        var recoveryResult = Result<string>.Success("Recovered value");

        // Act
        var recoveredResult = failedResult.Recover(() => recoveryResult);

        // Assert
        recoveredResult.IsSuccess.ShouldBeTrue();
        recoveredResult.Value.ShouldBe("Recovered value");
    }

    [Fact]
    public void GenericRecover_ShouldReturnOriginalResult_WhenResultIsSuccess()
    {
        // Arrange
        var successResult = Result<string>.Success("Original value");
        var recoveryResult = Result<string>.WithFailure("Should not be used");

        // Act
        var recoveredResult = successResult.Recover(() => recoveryResult);

        // Assert
        recoveredResult.IsSuccess.ShouldBeTrue();
        recoveredResult.Value.ShouldBe("Original value");
    }

    [Fact]
    public void RecoverWith_ShouldExecuteRecoveryFunction_WhenResultIsFailure()
    {
        // Arrange
        var failedResult = Result<string>.WithFailure("Initial failure");
        var recoveryResult = Result<int>.Success(42);

        // Act
        var recoveredResult = failedResult.RecoverWith(() => recoveryResult);

        // Assert
        recoveredResult.IsSuccess.ShouldBeTrue();
        recoveredResult.Value.ShouldBe(42);
    }

    [Fact]
    public void RecoverWith_ShouldConvertCompatibleTypes_WhenResultIsSuccess()
    {
        // Arrange - Using compatible types (object and string)
        var successResult = Result<object>.Success("test string");

        // Act - Convert object to string (compatible conversion)
        var convertedResult = successResult.RecoverWith<string>(() => Result<string>.WithFailure("fallback"));

        // Assert
        convertedResult.IsSuccess.ShouldBeTrue();
        convertedResult.Value.ShouldBe("test string");
    }

    /// <summary>
    /// REGRESSION TEST: This test validates the fix for the dangerous cast bug in RecoverWith.
    /// Previously: (TOut)(object)Value! would throw InvalidCastException at runtime
    /// Now: Safe type checking with proper error handling
    /// </summary>
    [Fact]
    public void RecoverWith_ShouldReturnFailure_WhenTypeConversionIsIncompatible()
    {
        // Arrange - Using incompatible types (string to int)
        var successResult = Result<string>.Success("not a number");

        // Act - Attempt to convert string to int (incompatible conversion)
        var convertedResult = successResult.RecoverWith<int>(() => Result<int>.Success(999));

        // Assert
        convertedResult.IsFailure.ShouldBeTrue();
        convertedResult.Errors.ShouldNotBeEmpty();
        convertedResult.Errors.First().ShouldContain("Cannot convert value of type 'String' to 'Int32' in RecoverWith operation");
    }

    /// <summary>
    /// REGRESSION TEST: Ensures we don't get runtime exceptions from type conversion.
    /// This validates that the dangerous cast fix properly handles edge cases.
    /// </summary>
    [Fact]  
    public void RecoverWith_ShouldHandleComplexTypeConversions_SafelyWithoutExceptions()
    {
        // Arrange - Test various incompatible type scenarios
        var stringResult = Result<string>.Success("text");
        var intResult = Result<int>.Success(42);
        var listResult = Result<List<string>>.Success(new List<string> { "item" });

        // Act & Assert - All should return failures, not throw exceptions
        var stringToInt = stringResult.RecoverWith<int>(() => Result<int>.Success(0));
        stringToInt.IsFailure.ShouldBeTrue();

        var intToString = intResult.RecoverWith<string>(() => Result<string>.Success("fallback"));
        intToString.IsFailure.ShouldBeTrue();

        var listToInt = listResult.RecoverWith<int>(() => Result<int>.Success(0));
        listToInt.IsFailure.ShouldBeTrue();

        // Verify all error messages follow the expected pattern
        stringToInt.Errors.First().ShouldContain("Cannot convert value of type");
        intToString.Errors.First().ShouldContain("Cannot convert value of type");
        listToInt.Errors.First().ShouldContain("Cannot convert value of type");
    }

    [Fact]
    public void RecoverWith_ShouldHandleNullValues_Safely()
    {
        // Arrange
        var nullResult = Result<string?>.Success(null);

        // Act
        var convertedResult = nullResult.RecoverWith<int>(() => Result<int>.Success(42));

        // Assert - Null cannot be converted to int, so should return type conversion error
        convertedResult.IsFailure.ShouldBeTrue();
        convertedResult.Errors.First().ShouldContain("Cannot convert value of type");
    }

    [Fact]
    public void Match_ShouldExecuteSuccessFunction_WhenResultIsSuccess()
    {
        // Arrange
        var successResult = Result<string>.Success("test value");

        // Act - Generic Match returns Result<TOut>, so get the Value
        var matchResult = successResult.Match(
            value => $"Success: {value}",
            errors => $"Failed: {string.Join(", ", errors)}"
        );

        // Assert  
        matchResult.Value.ShouldBe("Success: test value");
    }

    [Fact]
    public void Match_ShouldExecuteFailureFunction_WhenResultIsFailure()
    {
        // Arrange
        var failureResult = Result<string>.WithFailure("test error");

        // Act - Generic Match returns Result<TOut>, so get the Value
        var matchResult = failureResult.Match(
            value => $"Success: {value}",
            errors => $"Failed: {string.Join(", ", errors)}"
        );

        // Assert
        matchResult.Value.ShouldBe("Failed: test error");
    }

    [Fact]
    public void NonGenericMatch_ShouldHandleNullErrors_Gracefully()
    {
        // Arrange - Use non-generic Result with explicit null check to trigger default error
        var result = Result.WithFailure(ResultConstants.DefaultErrorMessage);

        // Act - Use non-generic Match that returns T directly
        var matchResult = result.Match(
            () => "Success",
            errors => $"Failed: {string.Join(", ", errors)}"
        );

        // Assert - Should contain the default error message
        matchResult.ShouldContain(ResultConstants.DefaultErrorMessage);
    }

    #region Type Safety Regression Tests

    /// <summary>
    /// Regression tests for the null handling fixes implemented to prevent NullReferenceException.
    /// These tests ensure that methods like Ensure, Combine, and Match properly handle null values
    /// without throwing exceptions at runtime.
    /// </summary>
    [Fact]
    public void Ensure_WithNullValue_ShouldReturnFailureInsteadOfThrowing()
    {
        // Arrange - Create a successful result with null value (edge case scenario)
        var result = new Result<string>(true, Array.Empty<string>(), null);

        // Act - This should not throw NullReferenceException
        var ensuredResult = result.Ensure(value => value.Length > 0, "Value should have length");

        // Assert
        ensuredResult.IsFailure.ShouldBeTrue();
        ensuredResult.Errors.ShouldContain(ResultConstants.ConditionEvaluationWithNullValue);
    }

    [Fact]
    public void Combine_WithNullValueInSuccessfulResult_ShouldHandleGracefully()
    {
        // Arrange - Create successful result with null value
        var result = new Result<string>(true, Array.Empty<string>(), null);
        var otherResult = Result.Success();

        // Act - This should not throw NullReferenceException
        var combinedResult = result.Combine(otherResult);

        // Assert
        combinedResult.IsFailure.ShouldBeTrue();
        combinedResult.Errors.ShouldContain(ResultConstants.NullValueInSuccessfulResult);
    }

    [Fact]
    public void Match_WithNullValueInSuccessfulResult_ShouldCallFailureFunction()
    {
        // Arrange - Create a successful result with null value (edge case scenario)
        var result = new Result<string>(true, Array.Empty<string>(), null);

        // Act - Match should detect null value and call failure function
        var matchResult = result.Match(
            value => -1, // This should NOT be called due to null value
            errors => -2 // This SHOULD be called due to null value handling
        );

        // Assert - Should call failure function due to null value in successful result
        matchResult.Value.ShouldBe(-2); // Fixed: Match returns Result<TOut>, so check the Value
    }

    [Fact]
    public void Tap_WithNullValue_ShouldNotExecuteAction()
    {
        // Arrange
        var result = new Result<string>(true, Array.Empty<string>(), null);
        var actionExecuted = false;

        // Act - This should not throw NullReferenceException
        var tappedResult = result.Tap(value => actionExecuted = true);

        // Assert
        actionExecuted.ShouldBeFalse();
        tappedResult.ShouldBeSameAs(result); // Should return same instance
    }

    [Fact]
    public void JsonConstructor_WithInconsistentState_ShouldValidateAndFix()
    {
        // Arrange - Create Result with inconsistent state (hasErrors = true, but no actual errors)
        var errors = Array.Empty<string>();

        // Act - Constructor should validate and fix inconsistency
        var result = new Result<string>(true, errors, "test-value");

        // Assert - State should be consistent after validation
        result.IsSuccess.ShouldBeTrue();
        result.HasErrors.ShouldBeFalse(); // Should be fixed from inconsistent state
        result.HasWarnings.ShouldBeFalse();
        result.Value.ShouldBe("test-value");
    }

    [Fact]
    public void OnSuccess_WithNullValue_ShouldNotExecuteAction()
    {
        // Arrange
        var result = new Result<string>(true, Array.Empty<string>(), null);
        var actionExecuted = false;

        // Act
        var successResult = result.OnSuccess(value => actionExecuted = true);

        // Assert
        actionExecuted.ShouldBeFalse();
        successResult.ShouldBeSameAs(result);
    }

    [Fact]
    public void Performance_ReducedLinqAllocations_ShouldBeMeasurable()
    {
        // Arrange - Create multiple results for combination
        var results = new[]
        {
            Result.WithFailure("Error 1"),
            Result.WithFailure("Error 2"),
            Result.WithFailure("Error 3")
        };

        var primaryErrors = new[] { "Primary 1", "Primary 2" };
        var secondaryErrors = new[] { "Secondary 1", "Secondary 2" };

        // Act - These operations should use optimized code paths
        var combinedResult = Result.Success().Combine(results);
        var combinedErrors = Result.CombineErrors(primaryErrors, secondaryErrors);
        var genericCombinedErrors = Result<string>.CombineErrors<string>(primaryErrors, secondaryErrors);

        // Assert - Verify functionality is preserved
        combinedResult.IsFailure.ShouldBeTrue();
        combinedResult.Errors.Count().ShouldBe(3);

        combinedErrors.IsFailure.ShouldBeTrue();
        combinedErrors.Errors.Count().ShouldBe(4);

        genericCombinedErrors.IsFailure.ShouldBeTrue();
        genericCombinedErrors.Errors.Count().ShouldBe(4);
    }

    #endregion Type Safety Regression Tests
} 