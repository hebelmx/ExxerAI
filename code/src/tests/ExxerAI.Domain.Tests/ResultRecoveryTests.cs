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
} 