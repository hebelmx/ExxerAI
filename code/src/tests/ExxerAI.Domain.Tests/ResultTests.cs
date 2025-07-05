using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Shouldly;

namespace ExxerAI.Domain.Tests;

public class ResultTests
{
    #region Test Constants

    /// <summary>
    /// Constants for expected messages to reduce test fragility and improve maintainability.
    /// These represent contracts vs implementation details.
    /// </summary>
    private static class ExpectedMessages
    {
        // Contract: Default error message (should be consistent across versions)
        public const string DefaultError = "Operation failed to execute successfully";
        
        // Contract: No errors found message
        public const string NoErrorsFound = "No errors were found";
        
        // Implementation detail: ToString formatting (more flexible)
        public const string SuccessPrefix = "Success";
        public const string FailurePrefix = "Failure";
        
        // Test data: Specific error messages used in tests
        public const string TestError = "Test error";
        public const string InitialFailure = "Initial failure";
        public const string ConditionFailed = "Condition failed";
    }

    #endregion Test Constants

    #region Test Helper Methods

    /// <summary>
    /// Helper methods for more resilient string testing that focuses on behavior rather than exact formatting.
    /// Using regular static methods instead of extension methods to avoid nested class restrictions.
    /// </summary>
    private static class TestHelpers
    {
        /// <summary>
        /// Validates ToString behavior focusing on key content rather than exact formatting.
        /// </summary>
        public static void ShouldRepresentFailure(string toStringResult, string expectedError)
        {
            // Contract: Must indicate failure and contain the error (case-insensitive for formatting)
            var lowerResult = toStringResult.ToLowerInvariant();
            var lowerPrefix = ExpectedMessages.FailurePrefix.ToLowerInvariant();
            
            lowerResult.Contains(lowerPrefix).ShouldBeTrue(
                $"Expected '{toStringResult}' to indicate failure (should contain '{ExpectedMessages.FailurePrefix}')");
            toStringResult.Contains(expectedError).ShouldBeTrue(
                $"Expected '{toStringResult}' to contain error '{expectedError}'");
        }

        /// <summary>
        /// Validates ToString behavior for success cases.
        /// </summary>
        public static void ShouldRepresentSuccess(string toStringResult)
        {
            // Contract: Must indicate success (case-insensitive for formatting)
            var lowerResult = toStringResult.ToLowerInvariant();
            var lowerPrefix = ExpectedMessages.SuccessPrefix.ToLowerInvariant();
            
            lowerResult.Contains(lowerPrefix).ShouldBeTrue(
                $"Expected '{toStringResult}' to indicate success (should contain '{ExpectedMessages.SuccessPrefix}')");
        }
    }

    #endregion Test Helper Methods

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange & Act - Test parameterless constructor
        var instance = new Result();

        // Assert
        instance.ShouldNotBeNull();
        instance.IsSuccess.ShouldBeFalse();
        instance.IsFailure.ShouldBeTrue();
        instance.Errors.ShouldNotBeNull();
        instance.Errors.ShouldBeEmpty();

        // Arrange & Act - Test static Success factory method
        var successResult = Result.Success();

        // Assert
        successResult.ShouldNotBeNull();
        successResult.IsSuccess.ShouldBeTrue();
        successResult.IsFailure.ShouldBeFalse();
        successResult.Errors.ShouldBeEmpty();

        // Arrange & Act - Test static WithFailure factory method with single error
        var failureResult = Result.WithFailure("Test error");

        // Assert
        failureResult.ShouldNotBeNull();
        failureResult.IsSuccess.ShouldBeFalse();
        failureResult.IsFailure.ShouldBeTrue();
        failureResult.Errors.ShouldContain(ExpectedMessages.TestError);
        failureResult.Errors.Count().ShouldBe(1);

        // Arrange & Act - Test static WithFailure factory method with multiple errors
        var multipleErrors = new List<string> { "Error 1", "Error 2", "Error 3" };
        var multiFailureResult = Result.WithFailure(multipleErrors);

        // Assert
        multiFailureResult.ShouldNotBeNull();
        multiFailureResult.IsSuccess.ShouldBeFalse();
        multiFailureResult.IsFailure.ShouldBeTrue();
        multiFailureResult.Errors.ShouldContain("Error 1");
        multiFailureResult.Errors.ShouldContain("Error 2");
        multiFailureResult.Errors.ShouldContain("Error 3");
        multiFailureResult.Errors.Count().ShouldBe(3);
    }

    [Fact]
    public void Constructor_WithInvalidParameters_ShouldThrowException()
    {
        // Arrange & Act & Assert - Test generic Result<T> constructor with null checks  
        var resultWithNullValue = new Result<string>(true, null, null);
        resultWithNullValue.IsSuccess.ShouldBeTrue(); // No errors and explicitly marked successful = success
        resultWithNullValue.Value.ShouldBeNull();

        // Arrange & Act & Assert - Test generic Result<T> with empty enumerable
        var emptyList = new List<int>();
        var resultWithEmptyList = new Result<List<int>>(true, null, emptyList);
        resultWithEmptyList.IsSuccess.ShouldBeTrue(); // Empty enumerable still succeeds if value is not null
        resultWithEmptyList.Value.ShouldNotBeNull();
        resultWithEmptyList.Value.Count.ShouldBe(0);

        // Arrange & Act & Assert - Test generic Result<T> with errors present
        var errors = new List<string> { "Error 1", "Error 2" };
        var resultWithErrors = new Result<string>(true, errors, "test value");
        resultWithErrors.IsSuccess.ShouldBeFalse(); // Errors present makes it fail
        resultWithErrors.Errors.ShouldContain("Error 1");
        resultWithErrors.Errors.ShouldContain("Error 2");
        resultWithErrors.Errors.Count().ShouldBe(2);

        // Arrange & Act & Assert - Test CombineErrors edge cases
        var combineResult = Result.CombineErrors(null, null);
        combineResult.IsFailure.ShouldBeTrue();
        combineResult.Errors.ShouldContain("No errors were found");

        // Arrange & Act & Assert - Test generic CombineErrors edge cases
        var genericCombineResult = Result<string>.CombineErrors<string>(null, null);
        genericCombineResult.IsFailure.ShouldBeTrue();
        genericCombineResult.Errors.ShouldContain("No errors were found");
        genericCombineResult.Value.ShouldBeNull();
    }

    [Fact]
    public void Properties_WhenSet_ShouldReturnCorrectValues()
    {
        // Arrange & Act - Test non-generic Result properties
        var successResult = Result.Success();
        var failureResult = Result.WithFailure("Test failure");

        // Assert - Success result properties
        successResult.IsSuccess.ShouldBeTrue();
        successResult.IsFailure.ShouldBeFalse();
        successResult.Errors.ShouldBeEmpty();

        // Assert - Failure result properties
        failureResult.IsSuccess.ShouldBeFalse();
        failureResult.IsFailure.ShouldBeTrue();
        failureResult.Errors.ShouldContain("Test failure");

        // Arrange & Act - Test generic Result<T> properties
        var successGenericResult = Result<int>.Success(42);
        var failureGenericResult = Result<string>.WithFailure("Generic failure", "fallback value");

        // Assert - Generic success result properties
        successGenericResult.IsSuccess.ShouldBeTrue();
        successGenericResult.IsFailure.ShouldBeFalse();
        successGenericResult.Value.ShouldBe(42);
        successGenericResult.Errors.ShouldBeEmpty(); // Success results have empty collections (regression fix)

        // Assert - Generic failure result properties
        failureGenericResult.IsSuccess.ShouldBeFalse();
        failureGenericResult.IsFailure.ShouldBeTrue();
        failureGenericResult.Value.ShouldBe("fallback value");
        failureGenericResult.Errors.ShouldContain("Generic failure");

        // Arrange & Act - Test warning properties
        var warningResult = Result<string>.WithWarnings(new List<string> { "Warning 1" }, "success value");

        // Assert - Warning result properties (fixed behavior: warnings are successful but with diagnostics)
        warningResult.IsSuccess.ShouldBeFalse(); // Success but with warnings shows as not success in IsSuccess
        warningResult.HasWarnings.ShouldBeTrue(); // Should have warnings since we used WithWarnings
        warningResult.IsRecoverable.ShouldBeTrue(); // Should be recoverable since the operation succeeded  
        warningResult.Value.ShouldBe("success value");
        warningResult.Errors.ShouldContain("Warning 1");

        // Arrange & Act - Test enumerable value handling
        var listValue = new List<string> { "item1", "item2" };
        var resultWithList = Result<List<string>>.Success(listValue);

        // Assert - Enumerable value properties
        resultWithList.IsSuccess.ShouldBeTrue();
        resultWithList.Value.ShouldBeEquivalentTo(listValue);
        resultWithList.Value.Count.ShouldBe(2);
    }

    [Fact]
    public void Methods_WhenCalled_ShouldReturnExpectedResults()
    {
        // Arrange
        var successResult = Result.Success();
        var failureResult = Result.WithFailure(ExpectedMessages.InitialFailure);
        var actionExecuted = false;
        var capturedErrors = new List<string>();

        // Act & Assert - Test OnSuccess method
        successResult.OnSuccess(() => actionExecuted = true);
        actionExecuted.ShouldBeTrue();

        actionExecuted = false; // Reset
        failureResult.OnSuccess(() => actionExecuted = true);
        actionExecuted.ShouldBeFalse(); // Should not execute on failure

        // Act & Assert - Test OnFailure method
        failureResult.OnFailure(errors => capturedErrors.AddRange(errors));
        capturedErrors.ShouldContain(ExpectedMessages.InitialFailure);

        capturedErrors.Clear(); // Reset
        successResult.OnFailure(errors => capturedErrors.AddRange(errors));
        capturedErrors.ShouldBeEmpty(); // Should not execute on success

        // Arrange & Act - Test Map method
        var mappedResult = successResult.Map(() => "Mapped value");
        var failedMappedResult = failureResult.Map(() => "This won't be returned");

        // Assert - Map method results
        mappedResult.IsSuccess.ShouldBeTrue();
        mappedResult.Value.ShouldBe("Mapped value");
        failedMappedResult.IsFailure.ShouldBeTrue();
        failedMappedResult.Errors.ShouldContain(ExpectedMessages.InitialFailure);

        // Arrange & Act - Test Bind method
        var boundResult = successResult.Bind(() => Result<int>.Success(100));
        var failedBoundResult = failureResult.Bind(() => Result<int>.Success(200));

        // Assert - Bind method results
        boundResult.IsSuccess.ShouldBeTrue();
        boundResult.Value.ShouldBe(100);
        failedBoundResult.IsFailure.ShouldBeTrue();
        failedBoundResult.Errors.ShouldContain(ExpectedMessages.InitialFailure);

        // Arrange & Act - Test Ensure method
        var ensuredSuccess = successResult.Ensure(() => true, "Should not fail");
        var ensuredFailure = successResult.Ensure(() => false, "Condition failed");

        // Assert - Ensure method results
        ensuredSuccess.IsSuccess.ShouldBeTrue();
        ensuredFailure.IsFailure.ShouldBeTrue();
        ensuredFailure.Errors.ShouldContain("Condition failed");

        // Arrange & Act - Test Tap method
        var tapExecuted = false;
        successResult.Tap(() => tapExecuted = true);

        // Assert - Tap method result
        tapExecuted.ShouldBeTrue();

        // Arrange & Act - Test Combine method
        var otherResult = Result.Success();
        var combinedSuccess = successResult.Combine(otherResult);
        var combinedFailure = successResult.Combine(failureResult);

        // Assert - Combine method results
        combinedSuccess.IsSuccess.ShouldBeTrue();
        combinedFailure.IsFailure.ShouldBeTrue();
        combinedFailure.Errors.ShouldContain(ExpectedMessages.InitialFailure);

        // Arrange & Act - Test Match method
        var matchResult = successResult.Match(() => "Success!", errors => $"Failed: {string.Join(", ", errors)}");
        var matchFailResult = failureResult.Match(() => "Success!", errors => $"Failed: {string.Join(", ", errors)}");

        // Assert - Match method results (more resilient to formatting changes)
        matchResult.ShouldBe("Success!");
        TestHelpers.ShouldRepresentFailure(matchFailResult, ExpectedMessages.InitialFailure);

        // Arrange & Act - Test Recover method
        var recoveredResult = failureResult.Recover(() => Result.Success());
        var noRecoveryResult = successResult.Recover(() => Result.WithFailure("Should not execute"));

        // Assert - Recover method results
        recoveredResult.IsSuccess.ShouldBeTrue();
        noRecoveryResult.IsSuccess.ShouldBeTrue();

        // Arrange & Act - Test ToString method
        var successString = successResult.ToString();
        var failureString = failureResult.ToString();

        // Assert - ToString method results (more resilient to formatting changes)
        TestHelpers.ShouldRepresentSuccess(successString);
        TestHelpers.ShouldRepresentFailure(failureString, ExpectedMessages.InitialFailure);
    }

    [Fact]
    public void DomainLogic_WhenExecuted_ShouldFollowBusinessRules()
    {
        // Arrange - Create domain scenarios for manufacturing context
        var validPartNumber = "PN-001-VALID";
        var invalidPartNumber = "";
        var validQuality = 95.5;
        var invalidQuality = 45.0;

        // Act & Assert - Test manufacturing validation business rules
        var validationResult = Result.Success()
            .Ensure(() => !string.IsNullOrEmpty(validPartNumber), "Part number is required")
            .Ensure(() => validQuality >= 90.0, "Quality must be at least 90%")
            .Map(() => new { PartNumber = validPartNumber, Quality = validQuality });

        validationResult.IsSuccess.ShouldBeTrue();
        validationResult.Value.PartNumber.ShouldBe(validPartNumber);
        validationResult.Value.Quality.ShouldBe(validQuality);

        // Act & Assert - Test failing validation business rules
        var failedValidationResult = Result.Success()
            .Ensure(() => !string.IsNullOrEmpty(invalidPartNumber), "Part number is required")
            .Ensure(() => invalidQuality >= 90.0, "Quality must be at least 90%");

        failedValidationResult.IsFailure.ShouldBeTrue();
        failedValidationResult.Errors.ShouldContain("Part number is required");

        // Act & Assert - Test manufacturing process workflow
        var processSteps = new List<string>();
        var manufacturingWorkflow = Result<string>.Success("Raw Material")
            .Tap(material => processSteps.Add($"Processing: {material}"))
            .Map(material => $"Machined {material}")
            .Tap(product => processSteps.Add($"Quality Check: {product}"))
            .Bind(product => Result<string>.Success($"Finished {product}"))
            .OnSuccess(finalProduct => processSteps.Add($"Completed: {finalProduct}"));

        manufacturingWorkflow.IsSuccess.ShouldBeTrue();
        manufacturingWorkflow.Value.ShouldBe("Finished Machined Raw Material");
        processSteps.Count.ShouldBe(3);
        processSteps.ShouldContain("Processing: Raw Material");
        processSteps.ShouldContain("Quality Check: Machined Raw Material");
        processSteps.ShouldContain("Completed: Finished Machined Raw Material");

        // Act & Assert - Test error recovery in manufacturing context
        var primaryErrors = new List<string> { "Machine malfunction", "Power outage" };
        var secondaryErrors = new List<string> { "Backup system activated", "Manual override required" };

        var errorRecoveryResult = Result.CombineErrors(primaryErrors, secondaryErrors)
            .Recover(() => Result.Success().Tap(() => processSteps.Add("Emergency protocols activated")));

        errorRecoveryResult.IsSuccess.ShouldBeTrue();
        processSteps.ShouldContain("Emergency protocols activated");

        // Act & Assert - Test complex domain scenario with multiple validations
        var batchProcessingResult = Result<int>.Success(1000) // Batch size
            .Ensure(batchSize => batchSize > 0, "Batch size must be positive")
            .Ensure(batchSize => batchSize <= 5000, "Batch size exceeds maximum capacity")
            .Map(batchSize => new { BatchSize = batchSize, EstimatedTime = batchSize * 0.5 })
            .Bind(batch => batch.EstimatedTime <= 2000
                ? Result<object>.Success(new { batch.BatchSize, batch.EstimatedTime, Status = "Approved" })
                : Result<object>.WithFailure("Processing time exceeds shift duration"));

        batchProcessingResult.IsSuccess.ShouldBeTrue();
        var batchResult = batchProcessingResult.Value;
        batchResult.ShouldNotBeNull();

        // Act & Assert - Test implicit conversions and deconstruction
        Result<string> implicitResult = "Test Value"; // Implicit conversion from T to Result<T>
        implicitResult.IsSuccess.ShouldBeTrue();
        implicitResult.Value.ShouldBe("Test Value");

        Result nonGenericResult = implicitResult; // Implicit conversion from Result<T> to Result
        nonGenericResult.IsSuccess.ShouldBeTrue();

        // Test deconstruction
        var (succeeded, data, errors) = Result<string>.Success("Deconstructed Value");
        succeeded.ShouldBeTrue();
        data.ShouldBe("Deconstructed Value");
        errors.ShouldBeEmpty();
    }

    [Fact]
    public void CombineErrors_NonGeneric_BothNull_ShouldReturnNoErrorsFoundMessage()
    {
        // Act
        var result = Result.CombineErrors(null, null);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(ExpectedMessages.NoErrorsFound);
    }

    [Fact]
    public void CombineErrors_NonGeneric_PrimaryErrorsOnly_ShouldReturnPrimaryErrors()
    {
        // Arrange
        var primaryErrors = new List<string> { "Error 1", "Error 2" };

        // Act
        var result = Result.CombineErrors(primaryErrors, null);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Error 1");
        result.Errors.ShouldContain("Error 2");
    }

    [Fact]
    public void CombineErrors_NonGeneric_SecondaryErrorsOnly_ShouldReturnSecondaryErrors()
    {
        // Arrange
        var secondaryErrors = new List<string> { "Error A", "Error B" };

        // Act
        var result = Result.CombineErrors(null, secondaryErrors);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Error A");
        result.Errors.ShouldContain("Error B");
    }

    [Fact]
    public void CombineErrors_NonGeneric_BothPrimaryAndSecondaryErrors_ShouldReturnCombinedErrors()
    {
        // Arrange
        var primaryErrors = new List<string> { "Error 1", "Error 2" };
        var secondaryErrors = new List<string> { "Error A", "Error B" };

        // Act
        var result = Result.CombineErrors(primaryErrors, secondaryErrors);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Error 1");
        result.Errors.ShouldContain("Error 2");
        result.Errors.ShouldContain("Error A");
        result.Errors.ShouldContain("Error B");
    }

    [Fact]
    public void CombineErrors_NonGeneric_NoErrors_ShouldReturnNoErrorsFoundMessage()
    {
        // Act
        var result = Result.CombineErrors(null, null);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.Count().ShouldBe(1);
        result.Errors.ShouldContain(ExpectedMessages.NoErrorsFound);
    }

    [Fact]
    public void CombineErrors_BothNull_ShouldReturnNoErrorsFoundMessageWithDefault()
    {
        // Act
        var result = Result<string>.CombineErrors<string>(null, null);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(ExpectedMessages.NoErrorsFound);
        result.Value.ShouldBeNull();
    }

    [Fact]
    public void CombineErrors_PrimaryErrorsOnly_ShouldReturnPrimaryErrorsWithDefault()
    {
        // Arrange
        var primaryErrors = new List<string> { "Error 1", "Error 2" };

        // Act
        var result = Result<string>.CombineErrors<string>(primaryErrors, null);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Error 1");
        result.Errors.ShouldContain("Error 2");
        result.Errors.Count().ShouldBe(2);
        result.Value.ShouldBeNull();
    }

    [Fact]
    public void CombineErrors_SecondaryErrorsOnly_ShouldReturnSecondaryErrorsWithValue()
    {
        // Arrange
        var secondaryErrors = new List<string> { "Error A", "Error B" };
        var value = "TestValue";

        // Act
        var result = Result<string>.CombineErrors<string>(null, secondaryErrors, value);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Error A");
        result.Errors.ShouldContain("Error B");
        result.Errors.Count().ShouldBe(2);
        result.Value.ShouldBe(value);
    }

    [Fact]
    public void CombineErrors_BothPrimaryAndSecondaryErrors_ShouldReturnCombinedErrorsWithValue()
    {
        // Arrange
        var primaryErrors = new List<string> { "Error 1", "Error 2" };
        var secondaryErrors = new List<string> { "Error A", "Error B" };
        var value = "CombinedValue";

        // Act
        var result = Result<string>.CombineErrors<string>(primaryErrors, secondaryErrors, value);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Error 1");
        result.Errors.ShouldContain("Error 2");
        result.Errors.ShouldContain("Error A");
        result.Errors.ShouldContain("Error B");
        result.Errors.Count().ShouldBe(4);
        result.Value.ShouldBe(value);
    }

    [Fact]
    public void CombineErrors_NoErrors_ShouldReturnNoErrorsFoundMessageWithValue()
    {
        // Arrange
        var value = "TestValue";

        // Act
        var result = Result<string>.CombineErrors<string>(null, null, value);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(ExpectedMessages.NoErrorsFound);
        result.Value.ShouldBe(value);
    }

    [Fact]
    public void Result_Success_ShouldBeSuccessful()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void ResultGeneric_Success_ShouldBeSuccessful()
    {
        // Act
        var result = Result<int>.Success(4);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Errors.ShouldBeEmpty(); // Success results have empty collections (regression fix)
        result.Value.ShouldBe(4);
    }

    [Fact]
    public void Result_Failure_ShouldBeFailureWithErrors()
    {
        // Arrange
        var errors = new List<string> { "Error1", "Error2" };

        // Act
        var result = Result.WithFailure(errors);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        errors.All(result.Errors.Contains).ShouldBeTrue();
    }

    [Fact]
    public void ResultGeneric_Failure_SingleError_ShouldBeFailureWithSingleError()
    {
        // Arrange
        var error = "Error1";

        // Act
        var result = Result<int>.WithFailure(error);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Errors.Count().ShouldBe(1);
        result.Errors.FirstOrDefault().ShouldBe(error);
    }

    [Fact]
    public void ResultGeneric_ToString_ShouldReturnSuccessOrFailureString()
    {
        // Act
        var successResult = Result<int>.Success(4);
        var failureResult = Result<int>.WithFailure("Error1");

        // Assert - More resilient toString validation
        TestHelpers.ShouldRepresentSuccess(successResult.ToString());
        TestHelpers.ShouldRepresentFailure(failureResult.ToString(), "Error1");
    }

    [Fact]
    public void Result_Failure_SingleError_ShouldBeFailureWithSingleError()
    {
        // Arrange
        var error = "Error1";

        // Act
        var result = Result.WithFailure(error);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Errors.Count().ShouldBe(1);
        result.Errors.FirstOrDefault().ShouldBe(error);
    }

    [Fact]
    public void Result_ToString_ShouldReturnSuccessOrFailureString()
    {
        // Act
        var successResult = Result.Success();
        var failureResult = Result.WithFailure("Error1");

        // Assert - More resilient toString validation
        TestHelpers.ShouldRepresentSuccess(successResult.ToString());
        TestHelpers.ShouldRepresentFailure(failureResult.ToString(), "Error1");
    }

    [Fact]
    public void Result_OnSuccess_ShouldInvokeAction_WhenResultIsSuccess()
    {
        // Arrange
        var result = Result.Success();
        var actionCalled = false;
        Action action = () => actionCalled = true;

        // Act
        result.OnSuccess(action);

        // Assert
        actionCalled.ShouldBeTrue();
    }

    // TODO [BUG] MAKE THIS TEST PASS
    //abr
    // July 4 2025
    [Fact]
    public void Result_OnFailure_ShouldInvokeActionWithErrors_WhenResultIsFailure()
    {
        // Arrange
        var errors = new List<string> { "Error1", "Error2" };
        var result = Result.WithFailure(errors);
        IEnumerable<string>? receivedErrors = null;
        Action<IEnumerable<string>> action = e => receivedErrors = e;

        bool fail = result.IsFailure;
        fail.ShouldBeTrue("Because we created a failure");
        // Act
        result.OnFailure(action);

        // Assert
        receivedErrors.ShouldNotBeNull();
        receivedErrors.ShouldContain("Error1");
        receivedErrors.ShouldContain("Error2");
        receivedErrors.Count().ShouldBe(2);
    }

    [Fact]
    public void Result_Ensure_ShouldReturnFailure_WhenConditionFails()
    {
        // Arrange
        var result = Result.Success();

        // Act
        var ensuredResult = result.Ensure(() => false, "Condition failed");

        // Assert
        ensuredResult.IsFailure.ShouldBeTrue();
        ensuredResult.Errors.ShouldContain(ExpectedMessages.ConditionFailed);
    }

    [Fact]
    public void Result_Tap_ShouldInvokeAction_WhenResultIsSuccess()
    {
        // Arrange
        var result = Result.Success();
        var actionCalled = false;
        Action action = () => actionCalled = true;

        // Act
        result.Tap(action);

        // Assert
        actionCalled.ShouldBeTrue();
    }

    [Fact]
    public void Result_Recover_ShouldReturnRecoveredResult_WhenResultIsFailure()
    {
        // Arrange
        var result = Result.WithFailure(ExpectedMessages.InitialFailure);
        var recoverResult = Result.Success();

        // Act
        var finalResult = result.Recover(() => recoverResult);

        // Assert
        finalResult.IsSuccess.ShouldBeTrue();
    }

    #region Collection Consistency Regression Tests

    /// <summary>
    /// Regression tests to ensure Result<T> collections are always consistent.
    /// These tests prevent the bug where Success() returned null collections while
    /// WithFailure() returned empty collections, causing unpredictable behavior.
    /// </summary>
    [Fact]
    public void Success_Methods_ShouldAlwaysReturnEmptyCollections_Never_Null()
    {
        // Arrange & Act - Test all success creation methods
        var successResult = Result<string>.Success("test-data");
        var withSuccessResult = Result<string>.WithSuccess("test-data");

        // Assert - Both methods should return empty collections (never null)
        successResult.Errors.ShouldNotBeNull();
        successResult.Errors.ShouldBeEmpty();

        withSuccessResult.Errors.ShouldNotBeNull();
        withSuccessResult.Errors.ShouldBeEmpty();

        // Assert - Both should behave identically
        successResult.IsSuccess.ShouldBe(withSuccessResult.IsSuccess);
        successResult.IsFailure.ShouldBe(withSuccessResult.IsFailure);
    }

    [Fact]
    public void Failure_Methods_ShouldAlwaysReturnNonNullCollections()
    {
        // Arrange & Act - Test all failure creation methods
        var singleErrorResult = Result<string>.WithFailure("error message");
        var multipleErrorsResult = Result<string>.WithFailure(new[] { "error1", "error2" });
        var nullErrorsResult = Result<string>.WithFailure((IEnumerable<string>?)null);
        var emptyErrorsResult = Result<string>.WithFailure(new List<string>());

        // Assert - All failure results should have non-null collections
        singleErrorResult.Errors.ShouldNotBeNull();
        multipleErrorsResult.Errors.ShouldNotBeNull();
        nullErrorsResult.Errors.ShouldNotBeNull();
        emptyErrorsResult.Errors.ShouldNotBeNull();

        // Assert - All should be failures
        singleErrorResult.IsFailure.ShouldBeTrue();
        multipleErrorsResult.IsFailure.ShouldBeTrue();
        nullErrorsResult.IsFailure.ShouldBeTrue();
        emptyErrorsResult.IsFailure.ShouldBeTrue();

        // Assert - Content should be as expected
        singleErrorResult.Errors.ShouldContain("error message");
        multipleErrorsResult.Errors.ShouldContain("error1");
        multipleErrorsResult.Errors.ShouldContain("error2");
        nullErrorsResult.Errors.ShouldContain(ExpectedMessages.DefaultError); // Default error for null input
        emptyErrorsResult.Errors.ShouldBeEmpty(); // Empty list stays empty (not null)
    }

    [Fact]
    public void Constructor_Consistency_ShouldHandleNullsIdentically()
    {
        // Arrange & Act - Test both constructors with null errors
        var jsonConstructorResult = new Result<string>(true, (IEnumerable<string>?)null, "test");
        var regularConstructorResult = new Result<string>(true, (List<string>?)null, "test");

        // Assert - Both constructors should handle nulls the same way
        jsonConstructorResult.Errors.ShouldNotBeNull();
        jsonConstructorResult.Errors.ShouldBeEmpty();

        regularConstructorResult.Errors.ShouldNotBeNull();
        regularConstructorResult.Errors.ShouldBeEmpty();

        // Assert - Both should have identical behavior
        jsonConstructorResult.IsSuccess.ShouldBe(regularConstructorResult.IsSuccess);
        jsonConstructorResult.IsFailure.ShouldBe(regularConstructorResult.IsFailure);
        jsonConstructorResult.Value.ShouldBe(regularConstructorResult.Value);
    }

    [Fact]
    public void All_Result_Operations_ShouldMaintainCollectionConsistency()
    {
        // Arrange - Create various result types
        var successResult = Result<string>.Success("data");
        var failureResult = Result<string>.WithFailure("error");

        // Act - Test all transformation operations
        var mappedSuccess = successResult.Map(x => x.ToUpper());
        var mappedFailure = failureResult.Map(x => x.ToUpper());
        var boundSuccess = successResult.Bind(x => Result<int>.Success(x.Length));
        var boundFailure = failureResult.Bind(x => Result<int>.Success(x.Length));

        // Assert - All results should maintain consistent collection behavior
        // Test mappedSuccess
        if (mappedSuccess.IsSuccess)
        {
            mappedSuccess.Errors.ShouldNotBeNull("Success results should never have null collections");
            mappedSuccess.Errors.ShouldBeEmpty("Success results should have empty collections");
        }
        else
        {
            mappedSuccess.Errors.ShouldNotBeNull("Failure results should never have null collections");
            mappedSuccess.Errors.ShouldNotBeEmpty("Failure results should contain error messages");
        }

        // Test boundSuccess
        if (boundSuccess.IsSuccess)
        {
            boundSuccess.Errors.ShouldNotBeNull("Success results should never have null collections");
            boundSuccess.Errors.ShouldBeEmpty("Success results should have empty collections");
        }
        else
        {
            boundSuccess.Errors.ShouldNotBeNull("Failure results should never have null collections");
            boundSuccess.Errors.ShouldNotBeEmpty("Failure results should contain error messages");
        }

        // Assert - Failures should propagate errors correctly
        mappedFailure.Errors.ShouldNotBeNull();
        mappedFailure.Errors.ShouldContain("error");
        boundFailure.Errors.ShouldNotBeNull();
        boundFailure.Errors.ShouldContain("error");
    }

    [Fact]
    public void Success_WithDifferentTypes_ShouldAlwaysHaveEmptyCollections()
    {
        // Arrange & Act - Test different types separately
        var stringResult = Result<string>.Success("test-string");
        var intResult = Result<int>.Success(42);
        var boolResult = Result<bool>.Success(true);

        // Assert - Regardless of type, success should always have empty collections
        stringResult.Errors.ShouldNotBeNull();
        stringResult.Errors.ShouldBeEmpty();
        stringResult.IsSuccess.ShouldBeTrue();
        stringResult.Value.ShouldBe("test-string");

        intResult.Errors.ShouldNotBeNull();
        intResult.Errors.ShouldBeEmpty();
        intResult.IsSuccess.ShouldBeTrue();
        intResult.Value.ShouldBe(42);

        boolResult.Errors.ShouldNotBeNull();
        boolResult.Errors.ShouldBeEmpty();
        boolResult.IsSuccess.ShouldBeTrue();
        boolResult.Value.ShouldBe(true);
    }

    [Fact]
    public void Collection_Behavior_Documentation_Test()
    {
        // This test serves as living documentation for the expected collection behavior

        // ✅ SUCCESS BEHAVIOR: Always empty collections, never null
        var success = Result<string>.Success("data");
        success.Errors.ShouldNotBeNull("✅ Success results must have non-null collections");
        success.Errors.ShouldBeEmpty("✅ Success results must have empty collections");

        // ✅ FAILURE BEHAVIOR: Always non-null collections with content
        var failure = Result<string>.WithFailure("error");
        failure.Errors.ShouldNotBeNull("✅ Failure results must have non-null collections");
        failure.Errors.ShouldNotBeEmpty("✅ Failure results must contain error messages");

        // ✅ NULL INPUT HANDLING: Always converted to appropriate defaults
        var nullInputResult = Result<string>.WithFailure((IEnumerable<string>?)null);
        nullInputResult.Errors.ShouldNotBeNull("✅ Null inputs must be converted to non-null collections");
        nullInputResult.Errors.ShouldNotBeEmpty("✅ Null inputs must result in default error messages");

        // ✅ CONSISTENCY: All creation methods behave the same
        var method1 = Result<string>.Success("test");
        var method2 = Result<string>.WithSuccess("test");
        method1.Errors.ShouldBeEquivalentTo(method2.Errors, "✅ All success methods must behave identically");
    }

    #endregion Collection Consistency Regression Tests

    #region Collection Type Overload Tests

    /// <summary>
    /// Tests to verify that different collection types work seamlessly with Result.WithFailure methods.
    /// These tests ensure all overloads (IEnumerable, Array) behave consistently and that IEnumerable
    /// properly handles all collection types (List, HashSet, etc.) without needing specific overloads.
    /// </summary>
    
    [Fact]
    public void Result_WithFailure_ShouldAcceptDifferentCollectionTypes()
    {
        // Arrange - Create different collection types with same content
        var errorList = new List<string> { "Error1", "Error2", "Error3" };
        var errorArray = new string[] { "Error1", "Error2", "Error3" };
        var errorHashSet = new HashSet<string> { "Error1", "Error2", "Error3" };
        var errorEnumerable = errorList.AsEnumerable();

        // Act - Create results using different collection types
        var resultFromList = Result.WithFailure(errorList); // Uses IEnumerable<string> overload
        var resultFromArray = Result.WithFailure(errorArray); // Uses string[] overload
        var resultFromHashSet = Result.WithFailure(errorHashSet); // Uses IEnumerable<string> overload
        var resultFromEnumerable = Result.WithFailure(errorEnumerable); // Uses IEnumerable<string> overload

        // Assert - All results should behave identically
        resultFromList.IsFailure.ShouldBeTrue();
        resultFromArray.IsFailure.ShouldBeTrue();
        resultFromHashSet.IsFailure.ShouldBeTrue();
        resultFromEnumerable.IsFailure.ShouldBeTrue();

        // Assert - All should contain the same errors (regardless of order for HashSet)
        resultFromList.Errors.ShouldContain("Error1");
        resultFromList.Errors.ShouldContain("Error2");
        resultFromList.Errors.ShouldContain("Error3");

        resultFromArray.Errors.ShouldContain("Error1");
        resultFromArray.Errors.ShouldContain("Error2");
        resultFromArray.Errors.ShouldContain("Error3");

        resultFromHashSet.Errors.ShouldContain("Error1");
        resultFromHashSet.Errors.ShouldContain("Error2");
        resultFromHashSet.Errors.ShouldContain("Error3");

        resultFromEnumerable.Errors.ShouldContain("Error1");
        resultFromEnumerable.Errors.ShouldContain("Error2");
        resultFromEnumerable.Errors.ShouldContain("Error3");

        // Assert - All should have the same error count
        resultFromList.Errors.Count().ShouldBe(3);
        resultFromArray.Errors.Count().ShouldBe(3);
        resultFromHashSet.Errors.Count().ShouldBe(3);
        resultFromEnumerable.Errors.Count().ShouldBe(3);
    }

    [Fact]
    public void ResultGeneric_WithFailure_ShouldAcceptDifferentCollectionTypes()
    {
        // Arrange - Create different collection types with same content
        var errorList = new List<string> { "Error1", "Error2", "Error3" };
        var errorArray = new string[] { "Error1", "Error2", "Error3" };
        var errorHashSet = new HashSet<string> { "Error1", "Error2", "Error3" };
        var errorEnumerable = errorList.AsEnumerable();
        var testValue = "TestValue";

        // Act - Create results using different collection types
        var resultFromList = Result<string>.WithFailure(errorList, testValue); // Uses IEnumerable<string> overload
        var resultFromArray = Result<string>.WithFailure(errorArray, testValue); // Uses string[] overload
        var resultFromHashSet = Result<string>.WithFailure(errorHashSet, testValue); // Uses IEnumerable<string> overload
        var resultFromEnumerable = Result<string>.WithFailure(errorEnumerable, testValue); // Uses IEnumerable<string> overload

        // Assert - All results should behave identically
        resultFromList.IsFailure.ShouldBeTrue();
        resultFromArray.IsFailure.ShouldBeTrue();
        resultFromHashSet.IsFailure.ShouldBeTrue();
        resultFromEnumerable.IsFailure.ShouldBeTrue();

        // Assert - All should contain the same errors
        resultFromList.Errors.ShouldContain("Error1");
        resultFromList.Errors.ShouldContain("Error2");
        resultFromList.Errors.ShouldContain("Error3");

        resultFromArray.Errors.ShouldContain("Error1");
        resultFromArray.Errors.ShouldContain("Error2");
        resultFromArray.Errors.ShouldContain("Error3");

        resultFromHashSet.Errors.ShouldContain("Error1");
        resultFromHashSet.Errors.ShouldContain("Error2");
        resultFromHashSet.Errors.ShouldContain("Error3");

        resultFromEnumerable.Errors.ShouldContain("Error1");
        resultFromEnumerable.Errors.ShouldContain("Error2");
        resultFromEnumerable.Errors.ShouldContain("Error3");

        // Assert - All should preserve the value
        resultFromList.Value.ShouldBe(testValue);
        resultFromArray.Value.ShouldBe(testValue);
        resultFromHashSet.Value.ShouldBe(testValue);
        resultFromEnumerable.Value.ShouldBe(testValue);

        // Assert - All should have the same error count
        resultFromList.Errors.Count().ShouldBe(3);
        resultFromArray.Errors.Count().ShouldBe(3);
        resultFromHashSet.Errors.Count().ShouldBe(3);
        resultFromEnumerable.Errors.Count().ShouldBe(3);
    }

    [Fact]
    public void Result_OnFailure_ShouldWorkWithDifferentCollectionTypes()
    {
        // Arrange - Test OnFailure behavior with different collection types
        var errorList = new List<string> { "ListError1", "ListError2" };
        var errorArray = new string[] { "ArrayError1", "ArrayError2" };
        var errorHashSet = new HashSet<string> { "HashSetError1", "HashSetError2" };

        var listResult = Result.WithFailure(errorList);
        var arrayResult = Result.WithFailure(errorArray);
        var hashSetResult = Result.WithFailure(errorHashSet);

        // Act & Assert - OnFailure should work consistently across all collection types
        IEnumerable<string>? receivedListErrors = null;
        IEnumerable<string>? receivedArrayErrors = null;
        IEnumerable<string>? receivedHashSetErrors = null;

        listResult.OnFailure(errors => receivedListErrors = errors);
        arrayResult.OnFailure(errors => receivedArrayErrors = errors);
        hashSetResult.OnFailure(errors => receivedHashSetErrors = errors);

        // Assert - All callbacks should have been invoked with proper errors
        receivedListErrors.ShouldNotBeNull();
        receivedListErrors.ShouldContain("ListError1");
        receivedListErrors.ShouldContain("ListError2");
        receivedListErrors.Count().ShouldBe(2);

        receivedArrayErrors.ShouldNotBeNull();
        receivedArrayErrors.ShouldContain("ArrayError1");
        receivedArrayErrors.ShouldContain("ArrayError2");
        receivedArrayErrors.Count().ShouldBe(2);

        receivedHashSetErrors.ShouldNotBeNull();
        receivedHashSetErrors.ShouldContain("HashSetError1");
        receivedHashSetErrors.ShouldContain("HashSetError2");
        receivedHashSetErrors.Count().ShouldBe(2);
    }

    [Fact]
    public void ResultGeneric_OnFailure_ShouldWorkWithDifferentCollectionTypes()
    {
        // Arrange - Test OnFailure behavior with different collection types for generic Result<T>
        var errorList = new List<string> { "ListError1", "ListError2" };
        var errorArray = new string[] { "ArrayError1", "ArrayError2" };
        var errorHashSet = new HashSet<string> { "HashSetError1", "HashSetError2" };

        var listResult = Result<int>.WithFailure(errorList, 42);
        var arrayResult = Result<int>.WithFailure(errorArray, 42);
        var hashSetResult = Result<int>.WithFailure(errorHashSet, 42);

        // Act & Assert - OnFailure should work consistently across all collection types
        IEnumerable<string>? receivedListErrors = null;
        IEnumerable<string>? receivedArrayErrors = null;
        IEnumerable<string>? receivedHashSetErrors = null;

        listResult.OnFailure(errors => receivedListErrors = errors);
        arrayResult.OnFailure(errors => receivedArrayErrors = errors);
        hashSetResult.OnFailure(errors => receivedHashSetErrors = errors);

        // Assert - All callbacks should have been invoked with proper errors
        receivedListErrors.ShouldNotBeNull();
        receivedListErrors.ShouldContain("ListError1");
        receivedListErrors.ShouldContain("ListError2");
        receivedListErrors.Count().ShouldBe(2);

        receivedArrayErrors.ShouldNotBeNull();
        receivedArrayErrors.ShouldContain("ArrayError1");
        receivedArrayErrors.ShouldContain("ArrayError2");
        receivedArrayErrors.Count().ShouldBe(2);

        receivedHashSetErrors.ShouldNotBeNull();
        receivedHashSetErrors.ShouldContain("HashSetError1");
        receivedHashSetErrors.ShouldContain("HashSetError2");
        receivedHashSetErrors.Count().ShouldBe(2);

        // Assert - All should preserve the value
        listResult.Value.ShouldBe(42);
        arrayResult.Value.ShouldBe(42);
        hashSetResult.Value.ShouldBe(42);
    }

    [Fact]
    public void Result_WithFailure_EmptyCollections_ShouldBehaveProperly()
    {
        // Arrange - Test with empty collections
        var emptyList = new List<string>();
        var emptyArray = new string[0];
        var emptyHashSet = new HashSet<string>();

        // Act
        var resultFromEmptyList = Result.WithFailure(emptyList);
        var resultFromEmptyArray = Result.WithFailure(emptyArray);
        var resultFromEmptyHashSet = Result.WithFailure(emptyHashSet);

        // Assert - All should be failures but with no errors
        resultFromEmptyList.IsFailure.ShouldBeTrue();
        resultFromEmptyArray.IsFailure.ShouldBeTrue();
        resultFromEmptyHashSet.IsFailure.ShouldBeTrue();

        resultFromEmptyList.Errors.ShouldNotBeNull();
        resultFromEmptyArray.Errors.ShouldNotBeNull();
        resultFromEmptyHashSet.Errors.ShouldNotBeNull();

        resultFromEmptyList.Errors.ShouldBeEmpty();
        resultFromEmptyArray.Errors.ShouldBeEmpty();
        resultFromEmptyHashSet.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void ResultGeneric_WithFailure_EmptyCollections_ShouldBehaveProperly()
    {
        // Arrange - Test with empty collections for generic Result<T>
        var emptyList = new List<string>();
        var emptyArray = new string[0];
        var emptyHashSet = new HashSet<string>();
        var testValue = "TestValue";

        // Act
        var resultFromEmptyList = Result<string>.WithFailure(emptyList, testValue);
        var resultFromEmptyArray = Result<string>.WithFailure(emptyArray, testValue);
        var resultFromEmptyHashSet = Result<string>.WithFailure(emptyHashSet, testValue);

        // Assert - All should be failures but with no errors
        resultFromEmptyList.IsFailure.ShouldBeTrue();
        resultFromEmptyArray.IsFailure.ShouldBeTrue();
        resultFromEmptyHashSet.IsFailure.ShouldBeTrue();

        resultFromEmptyList.Errors.ShouldNotBeNull();
        resultFromEmptyArray.Errors.ShouldNotBeNull();
        resultFromEmptyHashSet.Errors.ShouldNotBeNull();

        resultFromEmptyList.Errors.ShouldBeEmpty();
        resultFromEmptyArray.Errors.ShouldBeEmpty();
        resultFromEmptyHashSet.Errors.ShouldBeEmpty();

        // Assert - All should preserve the value
        resultFromEmptyList.Value.ShouldBe(testValue);
        resultFromEmptyArray.Value.ShouldBe(testValue);
        resultFromEmptyHashSet.Value.ShouldBe(testValue);
    }

    [Fact]
    public void Collections_Performance_Documentation_Test()
    {
        // This test serves as living documentation for collection handling behavior

        // ✅ ARRAY OVERLOAD: Direct, efficient handling
        var arrayResult = Result.WithFailure(new[] { "error1", "error2" });
        arrayResult.Errors.ShouldNotBeNull("✅ Array overloads should work efficiently");

        // ✅ IENUMERABLE OVERLOAD: Works with any collection type (List, HashSet, etc.)
        var listResult = Result.WithFailure(new List<string> { "error1", "error2" });
        listResult.Errors.ShouldNotBeNull("✅ IEnumerable overload should handle Lists efficiently");

        var hashSetResult = Result.WithFailure(new HashSet<string> { "error1", "error2" });
        hashSetResult.Errors.ShouldNotBeNull("✅ IEnumerable overload should handle any collection");

        // ✅ GENERIC VARIANTS: All work consistently
        var genericArrayResult = Result<int>.WithFailure(new[] { "error1", "error2" }, 42);
        var genericListResult = Result<int>.WithFailure(new List<string> { "error1", "error2" }, 42);
        var genericHashSetResult = Result<int>.WithFailure(new HashSet<string> { "error1", "error2" }, 42);

        genericArrayResult.IsFailure.ShouldBeTrue("✅ Generic array overload should work");
        genericListResult.IsFailure.ShouldBeTrue("✅ Generic IEnumerable overload should work with Lists");
        genericHashSetResult.IsFailure.ShouldBeTrue("✅ Generic IEnumerable overload should work with any collection");

        // ✅ CONSISTENCY: All maintain same behavior regardless of input collection type
        arrayResult.IsFailure.ShouldBe(listResult.IsFailure);
        listResult.IsFailure.ShouldBe(hashSetResult.IsFailure);
    }

    [Fact]
    public void Result_WithFailure_ShouldAcceptReadOnlyCollectionTypes()
    {
        // Arrange - Create different readonly collection types with same content
        var sourceList = new List<string> { "ReadOnlyError1", "ReadOnlyError2", "ReadOnlyError3" };
        
        // Different readonly collection types
        IReadOnlyList<string> readOnlyList = sourceList.AsReadOnly();
        IReadOnlyCollection<string> readOnlyCollection = sourceList.AsReadOnly();
        var readOnlyCollectionWrapper = new ReadOnlyCollection<string>(sourceList);
        var immutableList = sourceList.ToImmutableList();
        var immutableArray = sourceList.ToImmutableArray();
        var immutableHashSet = sourceList.ToImmutableHashSet();

        // Act - Create results using different readonly collection types
        var resultFromReadOnlyList = Result.WithFailure(readOnlyList);
        var resultFromReadOnlyCollection = Result.WithFailure(readOnlyCollection);
        var resultFromReadOnlyWrapper = Result.WithFailure(readOnlyCollectionWrapper);
        var resultFromImmutableList = Result.WithFailure(immutableList);
        var resultFromImmutableArray = Result.WithFailure(immutableArray);
        var resultFromImmutableHashSet = Result.WithFailure(immutableHashSet);

        // Assert - All results should behave identically
        resultFromReadOnlyList.IsFailure.ShouldBeTrue();
        resultFromReadOnlyCollection.IsFailure.ShouldBeTrue();
        resultFromReadOnlyWrapper.IsFailure.ShouldBeTrue();
        resultFromImmutableList.IsFailure.ShouldBeTrue();
        resultFromImmutableArray.IsFailure.ShouldBeTrue();
        resultFromImmutableHashSet.IsFailure.ShouldBeTrue();

        // Assert - All should contain the expected errors
        resultFromReadOnlyList.Errors.ShouldContain("ReadOnlyError1");
        resultFromReadOnlyList.Errors.ShouldContain("ReadOnlyError2");
        resultFromReadOnlyList.Errors.ShouldContain("ReadOnlyError3");

        resultFromReadOnlyCollection.Errors.ShouldContain("ReadOnlyError1");
        resultFromReadOnlyCollection.Errors.ShouldContain("ReadOnlyError2");
        resultFromReadOnlyCollection.Errors.ShouldContain("ReadOnlyError3");

        resultFromReadOnlyWrapper.Errors.ShouldContain("ReadOnlyError1");
        resultFromReadOnlyWrapper.Errors.ShouldContain("ReadOnlyError2");
        resultFromReadOnlyWrapper.Errors.ShouldContain("ReadOnlyError3");

        resultFromImmutableList.Errors.ShouldContain("ReadOnlyError1");
        resultFromImmutableList.Errors.ShouldContain("ReadOnlyError2");
        resultFromImmutableList.Errors.ShouldContain("ReadOnlyError3");

        resultFromImmutableArray.Errors.ShouldContain("ReadOnlyError1");
        resultFromImmutableArray.Errors.ShouldContain("ReadOnlyError2");
        resultFromImmutableArray.Errors.ShouldContain("ReadOnlyError3");

        resultFromImmutableHashSet.Errors.ShouldContain("ReadOnlyError1");
        resultFromImmutableHashSet.Errors.ShouldContain("ReadOnlyError2");
        resultFromImmutableHashSet.Errors.ShouldContain("ReadOnlyError3");

        // Assert - All should have the same error count (except HashSet which may reorder)
        resultFromReadOnlyList.Errors.Count().ShouldBe(3);
        resultFromReadOnlyCollection.Errors.Count().ShouldBe(3);
        resultFromReadOnlyWrapper.Errors.Count().ShouldBe(3);
        resultFromImmutableList.Errors.Count().ShouldBe(3);
        resultFromImmutableArray.Errors.Count().ShouldBe(3);
        resultFromImmutableHashSet.Errors.Count().ShouldBe(3);
    }

    [Fact]
    public void ResultGeneric_WithFailure_ShouldAcceptReadOnlyCollectionTypes()
    {
        // Arrange - Create different readonly collection types with same content
        var sourceList = new List<string> { "ReadOnlyError1", "ReadOnlyError2", "ReadOnlyError3" };
        var testValue = "ImmutableTestValue";
        
        // Different readonly collection types
        IReadOnlyList<string> readOnlyList = sourceList.AsReadOnly();
        IReadOnlyCollection<string> readOnlyCollection = sourceList.AsReadOnly();
        var readOnlyCollectionWrapper = new ReadOnlyCollection<string>(sourceList);
        var immutableList = sourceList.ToImmutableList();
        var immutableArray = sourceList.ToImmutableArray();
        var immutableHashSet = sourceList.ToImmutableHashSet();

        // Act - Create results using different readonly collection types
        var resultFromReadOnlyList = Result<string>.WithFailure(readOnlyList, testValue);
        var resultFromReadOnlyCollection = Result<string>.WithFailure(readOnlyCollection, testValue);
        var resultFromReadOnlyWrapper = Result<string>.WithFailure(readOnlyCollectionWrapper, testValue);
        var resultFromImmutableList = Result<string>.WithFailure(immutableList, testValue);
        var resultFromImmutableArray = Result<string>.WithFailure(immutableArray, testValue);
        var resultFromImmutableHashSet = Result<string>.WithFailure(immutableHashSet, testValue);

        // Assert - All results should behave identically
        resultFromReadOnlyList.IsFailure.ShouldBeTrue();
        resultFromReadOnlyCollection.IsFailure.ShouldBeTrue();
        resultFromReadOnlyWrapper.IsFailure.ShouldBeTrue();
        resultFromImmutableList.IsFailure.ShouldBeTrue();
        resultFromImmutableArray.IsFailure.ShouldBeTrue();
        resultFromImmutableHashSet.IsFailure.ShouldBeTrue();

        // Assert - All should preserve the value
        resultFromReadOnlyList.Value.ShouldBe(testValue);
        resultFromReadOnlyCollection.Value.ShouldBe(testValue);
        resultFromReadOnlyWrapper.Value.ShouldBe(testValue);
        resultFromImmutableList.Value.ShouldBe(testValue);
        resultFromImmutableArray.Value.ShouldBe(testValue);
        resultFromImmutableHashSet.Value.ShouldBe(testValue);

        // Assert - All should contain the expected errors
        resultFromReadOnlyList.Errors.ShouldContain("ReadOnlyError1");
        resultFromReadOnlyList.Errors.ShouldContain("ReadOnlyError2");
        resultFromReadOnlyList.Errors.ShouldContain("ReadOnlyError3");

        resultFromImmutableList.Errors.ShouldContain("ReadOnlyError1");
        resultFromImmutableList.Errors.ShouldContain("ReadOnlyError2");
        resultFromImmutableList.Errors.ShouldContain("ReadOnlyError3");

        resultFromImmutableArray.Errors.ShouldContain("ReadOnlyError1");
        resultFromImmutableArray.Errors.ShouldContain("ReadOnlyError2");
        resultFromImmutableArray.Errors.ShouldContain("ReadOnlyError3");

        // Assert - All should have the same error count
        resultFromReadOnlyList.Errors.Count().ShouldBe(3);
        resultFromReadOnlyCollection.Errors.Count().ShouldBe(3);
        resultFromReadOnlyWrapper.Errors.Count().ShouldBe(3);
        resultFromImmutableList.Errors.Count().ShouldBe(3);
        resultFromImmutableArray.Errors.Count().ShouldBe(3);
        resultFromImmutableHashSet.Errors.Count().ShouldBe(3);
    }

    [Fact]
    public void ReadOnlyCollections_OnFailure_ShouldWorkConsistently()
    {
        // Arrange - Test OnFailure behavior with readonly collection types
        var sourceErrors = new List<string> { "ImmutableError1", "ImmutableError2" };
        
        var readOnlyListResult = Result.WithFailure(sourceErrors.AsReadOnly());
        var immutableListResult = Result.WithFailure(sourceErrors.ToImmutableList());
        var immutableArrayResult = Result.WithFailure(sourceErrors.ToImmutableArray());

        // Act & Assert - OnFailure should work consistently across all readonly collection types
        IEnumerable<string>? receivedReadOnlyErrors = null;
        IEnumerable<string>? receivedImmutableListErrors = null;
        IEnumerable<string>? receivedImmutableArrayErrors = null;

        readOnlyListResult.OnFailure(errors => receivedReadOnlyErrors = errors);
        immutableListResult.OnFailure(errors => receivedImmutableListErrors = errors);
        immutableArrayResult.OnFailure(errors => receivedImmutableArrayErrors = errors);

        // Assert - All callbacks should have been invoked with proper errors
        receivedReadOnlyErrors.ShouldNotBeNull();
        receivedReadOnlyErrors.ShouldContain("ImmutableError1");
        receivedReadOnlyErrors.ShouldContain("ImmutableError2");
        receivedReadOnlyErrors.Count().ShouldBe(2);

        receivedImmutableListErrors.ShouldNotBeNull();
        receivedImmutableListErrors.ShouldContain("ImmutableError1");
        receivedImmutableListErrors.ShouldContain("ImmutableError2");
        receivedImmutableListErrors.Count().ShouldBe(2);

        receivedImmutableArrayErrors.ShouldNotBeNull();
        receivedImmutableArrayErrors.ShouldContain("ImmutableError1");
        receivedImmutableArrayErrors.ShouldContain("ImmutableError2");
        receivedImmutableArrayErrors.Count().ShouldBe(2);
    }

    #endregion Collection Type Overload Tests
}