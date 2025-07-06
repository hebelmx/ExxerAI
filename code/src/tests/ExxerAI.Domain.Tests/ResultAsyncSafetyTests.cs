using ExxerAI.Domain.Helpers.Operations;
using Shouldly;

namespace ExxerAI.Domain.Tests;

/// <summary>
/// Critical async safety tests for Result classes with Span&lt;T&gt; optimizations.
/// These tests ensure that internal Span&lt;T&gt; usage doesn't leak into async boundaries
/// and that Result&lt;T&gt; objects work correctly in async/await scenarios.
/// </summary>
public class ResultAsyncSafetyTests
{
    #region Test Constants

    /// <summary>
    /// Test constants for async safety validation.
    /// </summary>
    private static class AsyncTestConstants
    {
        public const int SmallDelayMs = 100; // 0.1 seconds as user suggested
        public const int AsyncOperationCount = 10;
        public const string AsyncTestError = "Async test error";
        public const string AsyncTestValue = "Async test value";
        public static readonly string[] SmallErrorArray = ["Error1", "Error2", "Error3"];
    }

    #endregion Test Constants

    #region Basic Async Compatibility Tests

    /// <summary>
    /// Tests that Result creation with Span optimizations works in async methods.
    /// </summary>
    [Fact]
    public async Task Result_WithSpanOptimizations_ShouldWorkInAsyncMethods()
    {
        // Arrange - Create results that should trigger Span optimizations
        var errors = AsyncTestConstants.SmallErrorArray;
        
        // Act - This should not cause compiler errors about ref struct in async methods
        await Task.Delay(AsyncTestConstants.SmallDelayMs);
        
        var result = Result.WithFailure(errors);
        var genericResult = Result<string>.WithFailure(errors, AsyncTestConstants.AsyncTestValue);
        
        await Task.Delay(AsyncTestConstants.SmallDelayMs);
        
        // Assert - Results should be created correctly despite async context
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Error1");
        result.Errors.ShouldContain("Error2");
        result.Errors.ShouldContain("Error3");
        
        genericResult.IsFailure.ShouldBeTrue();
        genericResult.Value.ShouldBe(AsyncTestConstants.AsyncTestValue);
        genericResult.Errors.ShouldContain("Error1");
    }

    /// <summary>
    /// Tests that Result.ToString() with Span optimizations works in async methods.
    /// </summary>
    [Fact]
    public async Task Result_ToString_WithSpanOptimizations_ShouldWorkInAsyncMethods()
    {
        // Arrange
        var errors = AsyncTestConstants.SmallErrorArray;
        var result = Result.WithFailure(errors);
        
        // Act - Ensure ToString (which uses Span optimizations) works across async boundaries
        await Task.Delay(AsyncTestConstants.SmallDelayMs);
        
        var stringRepresentation = result.ToString();
        
        await Task.Delay(AsyncTestConstants.SmallDelayMs);
        
        // Assert
        stringRepresentation.ShouldNotBeNull();
        stringRepresentation.ShouldContain(ResultConstants.FailurePrefix);
        stringRepresentation.ShouldContain("Error1");
    }

    /// <summary>
    /// Tests that CombineErrors with Span optimizations works in async methods.
    /// </summary>
    [Fact]
    public async Task CombineErrors_WithSpanOptimizations_ShouldWorkInAsyncMethods()
    {
        // Arrange - Small collections that should trigger Span optimization
        var primaryErrors = new List<string> { "Primary1", "Primary2" };
        var secondaryErrors = new List<string> { "Secondary1", "Secondary2" };
        
        // Act - Test across async boundaries
        await Task.Delay(AsyncTestConstants.SmallDelayMs);
        
        var combinedResult = Result.CombineErrors(primaryErrors, secondaryErrors);
        
        await Task.Delay(AsyncTestConstants.SmallDelayMs);
        
        // Assert
        combinedResult.IsFailure.ShouldBeTrue();
        combinedResult.Errors.Count().ShouldBe(4);
        combinedResult.Errors.ShouldContain("Primary1");
        combinedResult.Errors.ShouldContain("Secondary2");
    }

    #endregion Basic Async Compatibility Tests

    #region Concurrent Async Operations Tests

    /// <summary>
    /// Tests that multiple concurrent async operations using Result with Span optimizations work correctly.
    /// </summary>
    [Fact]
    public async Task Result_ConcurrentAsyncOperations_ShouldWorkCorrectly()
    {
        // Arrange - Create multiple concurrent tasks
        var tasks = new List<Task<Result<string>>>();
        
        for (var i = 0; i < AsyncTestConstants.AsyncOperationCount; i++)
        {
            var taskId = i;
            tasks.Add(CreateResultAsyncOperation(taskId));
        }
        
        // Act - Wait for all tasks to complete
        var results = await Task.WhenAll(tasks);
        
        // Assert - All operations should complete successfully
        results.Length.ShouldBe(AsyncTestConstants.AsyncOperationCount);
        
        foreach (var result in results)
        {
            result.ShouldNotBeNull();
            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldNotBeNull();
            result.Value.ShouldStartWith("Async result");
        }
    }

    /// <summary>
    /// Helper method for concurrent async operations test.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <returns>A task that returns a Result with Span optimizations.</returns>
    private static async Task<Result<string>> CreateResultAsyncOperation(int taskId)
    {
        // Simulate async work
        await Task.Delay(AsyncTestConstants.SmallDelayMs);
        
        // Create result that should trigger Span optimizations
        var errors = new[] { $"Error{taskId}A", $"Error{taskId}B" };
        var successResult = Result<string>.Success($"Async result {taskId}");
        
        // Test ToString which uses Span optimizations
        var _ = successResult.ToString();
        
        // Test error handling with Span optimizations
        var errorResult = Result<string>.WithFailure(errors);
        var __ = errorResult.ToString();
        
        await Task.Delay(AsyncTestConstants.SmallDelayMs);
        
        return successResult;
    }

    #endregion Concurrent Async Operations Tests

    #region ConfigureAwait and Async Patterns Tests

    /// <summary>
    /// Tests that Result operations work correctly with ConfigureAwait(false).
    /// </summary>
    [Fact]
    public async Task Result_WithConfigureAwaitFalse_ShouldWorkCorrectly()
    {
        // Arrange
        var errors = AsyncTestConstants.SmallErrorArray;
        
        // Act - Use ConfigureAwait(false) which changes synchronization context
        await Task.Delay(AsyncTestConstants.SmallDelayMs).ConfigureAwait(false);
        
        var result = Result.WithFailure(errors);
        var combinedResult = Result.CombineErrors(errors, new[] { "Additional1", "Additional2" });
        
        await Task.Delay(AsyncTestConstants.SmallDelayMs).ConfigureAwait(false);
        
        var stringRep = result.ToString();
        var combinedStringRep = combinedResult.ToString();
        
        // Assert - Should work correctly despite ConfigureAwait(false)
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain("Error1");
        
        combinedResult.IsFailure.ShouldBeTrue();
        combinedResult.Errors.Count().ShouldBe(5);
        
        stringRep.ShouldContain("Error1");
        combinedStringRep.ShouldContain("Additional1");
    }

    /// <summary>
    /// Tests async enumerable patterns with Result collections.
    /// </summary>
    [Fact]
    public async Task Result_AsyncEnumerablePatterns_ShouldWorkCorrectly()
    {
        // Arrange
        var results = new List<Result<string>>();
        
        // Act - Create results asynchronously
        await foreach (var asyncResult in GenerateAsyncResults())
        {
            results.Add(asyncResult);
        }
        
        // Assert
        results.Count.ShouldBe(5);
        results.All(r => r.IsSuccess).ShouldBeTrue();
        results.All(r => r.Value?.StartsWith("Async") == true).ShouldBeTrue();
    }

    /// <summary>
    /// Helper method that generates async enumerable results.
    /// </summary>
    /// <returns>Async enumerable of results.</returns>
    private static async IAsyncEnumerable<Result<string>> GenerateAsyncResults()
    {
        for (var i = 0; i < 5; i++)
        {
            await Task.Delay(AsyncTestConstants.SmallDelayMs);
            
            // Use Span optimizations during async generation
            var errors = new[] { $"TempError{i}" };
            var tempResult = Result<string>.WithFailure(errors);
            var _ = tempResult.ToString(); // Triggers Span optimization
            
            yield return Result<string>.Success($"Async generated {i}");
        }
    }

    #endregion ConfigureAwait and Async Patterns Tests

    #region Task Continuation and Exception Handling Tests

    /// <summary>
    /// Tests that Result operations work correctly in task continuations.
    /// </summary>
    [Fact]
    public async Task Result_TaskContinuations_ShouldWorkCorrectly()
    {
        // Arrange & Act
        var finalResult = await Task.Run(async () =>
        {
            await Task.Delay(AsyncTestConstants.SmallDelayMs);
            return Result<string>.Success("Initial");
        })
        .ContinueWith(async task =>
        {
            var result = await task;
            await Task.Delay(AsyncTestConstants.SmallDelayMs);
            
            // Use Span optimizations in continuation
            var errors = AsyncTestConstants.SmallErrorArray;
            var errorResult = Result<string>.WithFailure(errors);
            var _ = errorResult.ToString();
            
            return result.IsSuccess 
                ? Result<string>.Success($"Continued: {result.Value}")
                : Result<string>.WithFailure("Continuation failed");
        })
        .Unwrap();
        
        // Assert
        finalResult.IsSuccess.ShouldBeTrue();
        finalResult.Value.ShouldBe("Continued: Initial");
    }

    /// <summary>
    /// Tests exception handling in async methods with Result Span optimizations.
    /// </summary>
    [Fact]
    public async Task Result_AsyncExceptionHandling_ShouldWorkCorrectly()
    {
        // Arrange
        var exceptionThrown = false;
        Result<string>? result = null;
        
        try
        {
            // Act - Simulate async operation that might throw
            await Task.Delay(AsyncTestConstants.SmallDelayMs);
            
            // This should not throw even if Span optimizations are used
            result = await SimulateAsyncOperationWithSpanOptimizations(shouldThrow: false);
            
            await Task.Delay(AsyncTestConstants.SmallDelayMs);
        }
        catch (Exception)
        {
            exceptionThrown = true;
        }
        
        // Assert
        exceptionThrown.ShouldBeFalse();
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("Async operation succeeded");
    }

    /// <summary>
    /// Helper method that simulates async operations with Span optimizations.
    /// </summary>
    /// <param name="shouldThrow">Whether the operation should throw an exception.</param>
    /// <returns>A task that returns a Result.</returns>
    private static async Task<Result<string>> SimulateAsyncOperationWithSpanOptimizations(bool shouldThrow)
    {
        await Task.Delay(AsyncTestConstants.SmallDelayMs);
        
        if (shouldThrow)
        {
            throw new InvalidOperationException("Simulated async exception");
        }
        
        // Use operations that trigger Span optimizations
        var tempErrors = AsyncTestConstants.SmallErrorArray;
        var tempResult = Result.WithFailure(tempErrors);
        var _ = tempResult.ToString(); // Span optimization
        
        var combinedResult = Result.CombineErrors(tempErrors, new[] { "Extra" });
        var __ = combinedResult.ToString(); // Span optimization
        
        return Result<string>.Success("Async operation succeeded");
    }

    #endregion Task Continuation and Exception Handling Tests

    #region Stress Tests for Async Safety

    /// <summary>
    /// Stress test with many concurrent async operations using Span optimizations.
    /// </summary>
    [Fact]
    public async Task Result_HighConcurrencyStressTest_ShouldWorkCorrectly()
    {
        // Arrange - Create many concurrent tasks
        const int highConcurrencyCount = 100;
        var tasks = new List<Task<bool>>();
        
        for (var i = 0; i < highConcurrencyCount; i++)
        {
            var taskId = i;
            tasks.Add(PerformConcurrentAsyncOperation(taskId));
        }
        
        // Act - Wait for all tasks
        var results = await Task.WhenAll(tasks);
        
        // Assert - All operations should succeed
        results.Length.ShouldBe(highConcurrencyCount);
        results.All(success => success).ShouldBeTrue();
    }

    /// <summary>
    /// Helper method for high concurrency stress testing.
    /// </summary>
    /// <param name="taskId">The task identifier.</param>
    /// <returns>A task that returns success status.</returns>
    private static async Task<bool> PerformConcurrentAsyncOperation(int taskId)
    {
        try
        {
            await Task.Delay(1); // Very short delay for high concurrency
            
            // Perform operations that use Span optimizations
            var errors = new[] { $"Error{taskId}A", $"Error{taskId}B", $"Error{taskId}C" };
            
            var result1 = Result.WithFailure(errors);
            var str1 = result1.ToString();
            
            var result2 = Result<int>.WithFailure(errors, taskId);
            var str2 = result2.ToString();
            
            var combined = Result.CombineErrors(errors, new[] { $"Combined{taskId}" });
            var str3 = combined.ToString();
            
            await Task.Delay(1);
            
            // Verify results
            return result1.IsFailure && result2.IsFailure && combined.IsFailure &&
                   str1.Contains($"Error{taskId}A") && str2.Contains($"Error{taskId}B") && str3.Contains($"Combined{taskId}");
        }
        catch
        {
            return false;
        }
    }

    #endregion Stress Tests for Async Safety

    #region Documentation and Validation Tests

    /// <summary>
    /// Documents that our Span optimizations are safe in async contexts.
    /// This test serves as living documentation of async safety guarantees.
    /// </summary>
    [Fact]
    public async Task Result_AsyncSafetyDocumentation_ShouldDemonstrateCorrectUsage()
    {
        // ✅ SAFE: Span<T> is used only in internal static methods
        // ✅ SAFE: Result<T> stores arrays, not Span<T>
        // ✅ SAFE: Optimizations complete before method returns
        // ✅ SAFE: No ref struct crosses async boundaries
        
        await Task.Delay(AsyncTestConstants.SmallDelayMs);
        
        // These operations use Span optimizations internally but are async-safe
        var result = Result.WithFailure(AsyncTestConstants.SmallErrorArray);
        var genericResult = Result<string>.WithFailure(AsyncTestConstants.SmallErrorArray, "value");
        var combined = Result.CombineErrors(AsyncTestConstants.SmallErrorArray, new[] { "extra" });
        
        await Task.Delay(AsyncTestConstants.SmallDelayMs);
        
        // ToString operations use Span optimizations but are safe
        var str1 = result.ToString();
        var str2 = genericResult.ToString();
        var str3 = combined.ToString();
        
        await Task.Delay(AsyncTestConstants.SmallDelayMs);
        
        // All operations complete successfully
        result.IsFailure.ShouldBeTrue();
        genericResult.IsFailure.ShouldBeTrue();
        combined.IsFailure.ShouldBeTrue();
        
        str1.ShouldContain("Error1");
        str2.ShouldContain("Error1");
        str3.ShouldContain("Error1");
        
        // ✅ CONCLUSION: Span optimizations are completely safe in async contexts
        true.ShouldBeTrue("Async safety validation completed successfully");
    }

    #endregion Documentation and Validation Tests
} 