using ExxerAI.Domain.Operations;
using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests;

/// <summary>
/// Tests for functional cancellation handling patterns using Result&lt;T&gt;.
/// Validates that cancellation is handled functionally without throwing exceptions,
/// maintaining the functional programming principles throughout async operations.
/// </summary>
public class CancellationHandlingTests
{
    #region Test Constants

    /// <summary>
    /// Test constants for cancellation scenarios.
    /// </summary>
    private static class CancellationTestConstants
    {
        public const int ShortDelayMs = 50;
        public const int MediumDelayMs = 200;
        public const int LongDelayMs = 1000;
        public const string TestValue = "Cancellation test value";
        public const string TestError = "Simulated operation error";
    }

    #endregion Test Constants

    #region ResultExtensions Cancellation Tests

    /// <summary>
    /// Tests the basic cancellation factory methods and predicates.
    /// </summary>
    [Fact]
    public void Cancelled_ShouldCreateFailedResult_WithCancellationError()
    {
        // Act - Test non-generic cancellation
        var result = ResultExtensions.Cancelled();
        var genericResult = ResultExtensions.Cancelled<string>();

        // Assert - Both should be failures with cancellation error
        result.IsFailure.ShouldBeTrue();
        result.IsCancelled().ShouldBeTrue();
        result.Errors.ShouldContain(ResultErrors.OperationCancelled);

        genericResult.IsFailure.ShouldBeTrue();
        genericResult.IsCancelled().ShouldBeTrue();
        genericResult.Errors.ShouldContain(ResultErrors.OperationCancelled);
        genericResult.Value!.ShouldBeNull();
    }

    /// <summary>
    /// Tests the IsCancelled predicate methods.
    /// </summary>
    [Fact]
    public void IsCancelled_ShouldDetectCancellationCorrectly()
    {
        // Arrange
        var cancelledResult = ResultExtensions.Cancelled();
        var cancelledGenericResult = ResultExtensions.Cancelled<int>();
        var successResult = Result.Success();
        var failureResult = Result.WithFailure("Regular error");
        var successGenericResult = Result<int>.Success(42);
        var failureGenericResult = Result<int>.WithFailure("Regular error");

        // Assert - Only cancelled results should be detected
        cancelledResult.IsCancelled().ShouldBeTrue();
        cancelledGenericResult.IsCancelled().ShouldBeTrue();
        
        successResult.IsCancelled().ShouldBeFalse();
        failureResult.IsCancelled().ShouldBeFalse();
        successGenericResult.IsCancelled().ShouldBeFalse();
        failureGenericResult.IsCancelled().ShouldBeFalse();
    }

    /// <summary>
    /// Tests cancellation detection with mixed error messages.
    /// </summary>
    [Fact]
    public void IsCancelled_WithMixedErrors_ShouldDetectCancellationCorrectly()
    {
        // Arrange - Result with both cancellation and other errors
        var mixedErrors = new[]
        {
            "Some other error",
            ResultErrors.OperationCancelled,
            "Another error"
        };
        var mixedResult = Result.WithFailure(mixedErrors);
        var mixedGenericResult = Result<string>.WithFailure(mixedErrors, "test");

        // Assert - Should detect cancellation even with other errors
        mixedResult.IsCancelled().ShouldBeTrue();
        mixedGenericResult.IsCancelled().ShouldBeTrue();
    }

    #endregion ResultExtensions Cancellation Tests

    #region CancellationAwareResult Tests

    /// <summary>
    /// Tests wrapping a successful async operation.
    /// </summary>
    [Fact]
    public async Task WrapCancellationAware_WithSuccessfulOperation_ShouldReturnSuccess()
    {
        // Arrange
        var expectedValue = CancellationTestConstants.TestValue;

        // Act
        var result = await CancellationAwareResult.WrapCancellationAware<string>(
            async ct =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(CancellationTestConstants.ShortDelayMs), cancellationToken: TestContext.Current.CancellationToken);
                return expectedValue;
            }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsCancelled().ShouldBeFalse();
        result.Value!.ShouldBe(expectedValue);
    }

    /// <summary>
    /// Tests wrapping an operation that throws a regular exception.
    /// </summary>
    [Fact]
    public async Task WrapCancellationAware_WithException_ShouldReturnFailure()
    {
        // Act
        var result = await CancellationAwareResult.WrapCancellationAware<string>(
            ct => throw new InvalidOperationException(CancellationTestConstants.TestError), cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsCancelled().ShouldBeFalse();
        result.Errors.ShouldContain($"Operation failed: {CancellationTestConstants.TestError}");
    }

    /// <summary>
    /// Tests wrapping an operation with pre-cancelled token.
    /// </summary>
    [Fact]
    public async Task WrapCancellationAware_WithPreCancelledToken_ShouldReturnCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Pre-cancel the token

        // Act
        var result = await CancellationAwareResult.WrapCancellationAware<string>(
            async ct =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(CancellationTestConstants.LongDelayMs), cancellationToken: TestContext.Current.CancellationToken);
                return CancellationTestConstants.TestValue;
            },
            cts.Token);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsCancelled().ShouldBeTrue();
        result.Errors.ShouldContain(ResultErrors.OperationCancelled);
    }

    /// <summary>
    /// Tests wrapping an operation that gets cancelled during execution.
    /// </summary>
    [Fact]
    public async Task WrapCancellationAware_WithCancellationDuringExecution_ShouldReturnCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        // Act - Start operation and cancel after short delay
        var operationTask = CancellationAwareResult.WrapCancellationAware<string>(
            async ct =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(CancellationTestConstants.LongDelayMs), cancellationToken: TestContext.Current.CancellationToken);
                return CancellationTestConstants.TestValue;
            },
            cts.Token);

        // Cancel after a short delay
        _ = Task.Delay(TimeSpan.FromMilliseconds(CancellationTestConstants.ShortDelayMs), TestContext.Current.CancellationToken).ContinueWith(_ => cts.Cancel(), cancellationToken: TestContext.Current.CancellationToken);

        var result = await operationTask;

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsCancelled().ShouldBeTrue();
        result.Errors.ShouldContain(ResultErrors.OperationCancelled);
    }

    /// <summary>
    /// Tests wrapping a non-generic async operation.
    /// </summary>
    [Fact]
    public async Task WrapCancellationAware_NonGeneric_WithSuccessfulOperation_ShouldReturnSuccess()
    {
        // Arrange
        var operationExecuted = false;

        // Act
        var result = await CancellationAwareResult.WrapCancellationAware(
            async ct =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(CancellationTestConstants.ShortDelayMs), cancellationToken: TestContext.Current.CancellationToken);
                operationExecuted = true;
            }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsCancelled().ShouldBeFalse();
        operationExecuted.ShouldBeTrue();
    }

    /// <summary>
    /// Tests wrapping an operation that already returns Result&lt;T&gt;.
    /// </summary>
    [Fact]
    public async Task WrapResultOperation_WithSuccessfulResult_ShouldReturnOriginalResult()
    {
        // Arrange
        var expectedValue = CancellationTestConstants.TestValue;

        // Act
        var result = await CancellationAwareResult.WrapResultOperation<string>(
            async ct =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(CancellationTestConstants.ShortDelayMs), cancellationToken: TestContext.Current.CancellationToken);
                return Result<string>.Success(expectedValue);
            }, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsCancelled().ShouldBeFalse();
        result.Value!.ShouldBe(expectedValue);
    }

    /// <summary>
    /// Tests wrapping a Result operation that gets cancelled.
    /// </summary>
    [Fact]
    public async Task WrapResultOperation_WithCancellation_ShouldReturnCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        // Act
        var operationTask = CancellationAwareResult.WrapResultOperation<string>(
            async ct =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(CancellationTestConstants.LongDelayMs), cancellationToken: TestContext.Current.CancellationToken);
                return Result<string>.Success(CancellationTestConstants.TestValue);
            },
            cts.Token);

        // Cancel after short delay
        _ = Task.Delay(TimeSpan.FromMilliseconds(CancellationTestConstants.ShortDelayMs), TestContext.Current.CancellationToken).ContinueWith(_ => cts.Cancel(), cancellationToken: TestContext.Current.CancellationToken);

        var result = await operationTask;

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsCancelled().ShouldBeTrue();
    }

    #endregion CancellationAwareResult Tests

    #region Timeout Handling Tests

    /// <summary>
    /// Tests wrapping an operation with timeout that completes in time.
    /// </summary>
    [Fact]
    public async Task WrapWithTimeout_CompletesInTime_ShouldReturnSuccess()
    {
        // Arrange
        var timeout = TimeSpan.FromMilliseconds(CancellationTestConstants.MediumDelayMs);
        var expectedValue = CancellationTestConstants.TestValue;

        // Act
        var result = await CancellationAwareResult.WrapWithTimeout<string>(
            async ct =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(CancellationTestConstants.ShortDelayMs), cancellationToken: TestContext.Current.CancellationToken);
                return expectedValue;
            },
            timeout, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ShouldBe(expectedValue);
        result.IsCancelled().ShouldBeFalse();
    }

    /// <summary>
    /// Tests wrapping an operation that times out.
    /// </summary>
    [Fact]
    public async Task WrapWithTimeout_ExceedsTimeout_ShouldReturnTimeoutError()
    {
        // Arrange
        var timeout = TimeSpan.FromMilliseconds(CancellationTestConstants.ShortDelayMs);

        // Act
        var result = await CancellationAwareResult.WrapWithTimeout<string>(
            async ct =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(CancellationTestConstants.LongDelayMs), cancellationToken: TestContext.Current.CancellationToken);
                return CancellationTestConstants.TestValue;
            },
            timeout, cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(ResultErrors.OperationTimedOut);
        result.IsCancelled().ShouldBeFalse(); // It's a timeout, not cancellation
    }

    /// <summary>
    /// Tests wrapping with timeout and external cancellation.
    /// </summary>
    [Fact]
    public async Task WrapWithTimeout_WithExternalCancellation_ShouldReturnCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var timeout = TimeSpan.FromMilliseconds(CancellationTestConstants.LongDelayMs);

        // Act
        var operationTask = CancellationAwareResult.WrapWithTimeout<string>(
            async ct =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(CancellationTestConstants.LongDelayMs), cancellationToken: TestContext.Current.CancellationToken);
                return CancellationTestConstants.TestValue;
            },
            timeout,
            cts.Token);

        // Cancel externally before timeout
        _ = Task.Delay(TimeSpan.FromMilliseconds(CancellationTestConstants.ShortDelayMs), TestContext.Current.CancellationToken).ContinueWith(_ => cts.Cancel(), cancellationToken: TestContext.Current.CancellationToken);

        var result = await operationTask;

        // Assert
        result.IsFailure.ShouldBeTrue();
        result.IsCancelled().ShouldBeTrue();
    }

    #endregion Timeout Handling Tests

    #region Functional Programming Pattern Tests

    /// <summary>
    /// Tests the complete functional pattern: operation → cancellation check → result chaining.
    /// </summary>
    [Fact]
    public async Task FunctionalCancellationPattern_ShouldChainOperationsCorrectly()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var steps = new List<string>();

        // Act - Chain operations functionally
        var result = await CancellationAwareResult.WrapCancellationAware<string>(
                async ct =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(CancellationTestConstants.ShortDelayMs), cancellationToken: TestContext.Current.CancellationToken);
                    return "Step 1 Complete";
                },
                cts.Token)
            .ContinueWith(async task =>
            {
                var firstResult = await task;
                if (firstResult.IsCancelled())
                    return firstResult;

                steps.Add(firstResult.Value!);

                return await CancellationAwareResult.WrapCancellationAware<string>(
                    async ct =>
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(CancellationTestConstants.ShortDelayMs), cancellationToken: TestContext.Current.CancellationToken);
                        return "Step 2 Complete";
                    },
                    cts.Token);
            })
            .Unwrap();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value!.ShouldBe("Step 2 Complete");
        steps.ShouldContain("Step 1 Complete");
    }

    /// <summary>
    /// Tests semantic assertions as recommended in the rule.
    /// </summary>
    [Fact]
    public async Task SemanticAssertions_ShouldExpressIntentClearly()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await CancellationAwareResult.WrapCancellationAware<string>(
            ct => Task.FromResult(CancellationTestConstants.TestValue),
            cts.Token);

        // Assert - Semantic assertions over exception checks
        result.IsCancelled().ShouldBeTrue();
        result.IsFailure.ShouldBeTrue();
        result.IsSuccess.ShouldBeFalse();
        
        // This is much clearer than:
        // Should.Throw<OperationCanceledException>(() => ...)
    }

    #endregion Functional Programming Pattern Tests

    #region Edge Cases and Error Scenarios

    /// <summary>
    /// Tests null operation handling.
    /// </summary>
    [Fact]
    public async Task WrapCancellationAware_WithNullOperation_ShouldHandleGracefully()
    {
        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await CancellationAwareResult.WrapCancellationAware<string>(null!));
    }

    /// <summary>
    /// Tests multiple cancellation sources.
    /// </summary>
    [Fact]
    public async Task WrapCancellationAware_WithMultipleCancellationSources_ShouldHandleCorrectly()
    {
        // Arrange
        using var cts1 = new CancellationTokenSource();
        using var cts2 = new CancellationTokenSource();
        using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cts1.Token, cts2.Token);

        // Act
        var operationTask = CancellationAwareResult.WrapCancellationAware<string>(
            async ct =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(CancellationTestConstants.LongDelayMs), cancellationToken: TestContext.Current.CancellationToken);
                return CancellationTestConstants.TestValue;
            },
            combinedCts.Token);

        // Cancel one of the sources
        _ = Task.Delay(TimeSpan.FromMilliseconds(CancellationTestConstants.ShortDelayMs), TestContext.Current.CancellationToken).ContinueWith(_ => cts1.Cancel(), cancellationToken: TestContext.Current.CancellationToken);

        var result = await operationTask;

        // Assert
        result.IsCancelled().ShouldBeTrue();
    }

    #endregion Edge Cases and Error Scenarios
} 