using ExxerAI.Application;
using ExxerAI.Domain;
using Shouldly;

namespace ExxerAI.Application.Tests;

/// <summary>
/// Unit tests for Result class
/// </summary>
public class ResultTests
{
    [Fact]
    public void Should_CreateSuccessResult_When_SuccessMethodCalled()
    {
        // Arrange & Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void Should_CreateFailureResult_When_SingleErrorProvided()
    {
        // Arrange
        var errorMessage = "Test error message";

        // Act
        var result = Result.WithFailure(errorMessage);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldContain(errorMessage);
        result.Errors.Count.ShouldBe(1);
    }

    [Fact]
    public void Should_CreateFailureResult_When_MultipleErrorsProvided()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2", "Error 3" };

        // Act
        var result = Result.WithFailure(errors);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldBe(errors);
        result.Errors.Count.ShouldBe(3);
    }

    [Fact]
    public void Should_CreateFailureResult_When_EmptyErrorCollectionProvided()
    {
        // Arrange
        var errors = Array.Empty<string>();

        // Act
        var result = Result.WithFailure(errors);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void Should_HandleNullErrors_When_ErrorCollectionIsNull()
    {
        // Arrange & Act
        var result = Result.WithFailure((IEnumerable<string>)null!);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void Should_ReturnReadOnlyList_When_AccessingErrors()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2" };
        var result = Result.WithFailure(errors);

        // Act & Assert
        result.Errors.ShouldBeOfType<List<string>>();
        result.Errors.Count.ShouldBe(2);
        
        // Verify it's read-only by checking interface
        result.Errors.ShouldBeAssignableTo<IReadOnlyList<string>>();
    }
}

/// <summary>
/// Unit tests for Result&lt;T&gt; class
/// </summary>
public class ResultOfTTests
{
    [Fact]
    public void Should_CreateSuccessResultWithValue_When_ValueProvided()
    {
        // Arrange
        var testValue = "Test Value";

        // Act
        var result = Result<string>.Success(testValue);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Value.ShouldBe(testValue);
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void Should_CreateFailureResultWithoutValue_When_ErrorProvided()
    {
        // Arrange
        var errorMessage = "Test error";

        // Act
        var result = Result<int>.WithFailure(errorMessage);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBe(default(int));
        result.Errors.ShouldContain(errorMessage);
    }

    [Fact]
    public void Should_CreateFailureResultWithMultipleErrors_When_ErrorCollectionProvided()
    {
        // Arrange
        var errors = new[] { "Error 1", "Error 2", "Error 3" };

        // Act
        var result = Result<string>.WithFailure(errors);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Errors.ShouldBe(errors);
        result.Errors.Count.ShouldBe(3);
    }

    [Fact]
    public void Should_ImplicitlyConvertValue_When_AssigningDirectly()
    {
        // Arrange
        var testValue = 42;

        // Act
        Result<int> result = testValue;

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(testValue);
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void Should_HandleComplexTypes_When_UsingCustomObjects()
    {
        // Arrange
        var testObject = new { Name = "Test", Value = 123 };

        // Act
        var result = Result<object>.Success(testObject);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(testObject);
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void Should_HandleNullValue_When_NullProvided()
    {
        // Arrange & Act
        var result = Result<string?>.Success(null);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeNull();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public void Should_InheritFromBaseResult_When_Created()
    {
        // Arrange & Act
        var result = Result<int>.Success(42);

        // Assert
        result.ShouldBeAssignableTo<Result>();
        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
    }

    [Theory]
    [InlineData("string", true)]
    [InlineData(42, true)]
    [InlineData(3.14, true)]
    [InlineData(true, true)]
    public void Should_HandleDifferentValueTypes_When_DifferentTypesProvided<T>(T value, bool expectedSuccess)
    {
        // Arrange & Act
        var result = Result<T>.Success(value);

        // Assert
        result.IsSuccess.ShouldBe(expectedSuccess);
        result.Value.ShouldBe(value);
    }

    [Fact]
    public void Should_ReturnDefaultValue_When_FailureCreated()
    {
        // Arrange & Act
        var stringResult = Result<string>.WithFailure("Error");
        var intResult = Result<int>.WithFailure("Error");
        var boolResult = Result<bool>.WithFailure("Error");

        // Assert
        stringResult.Value.ShouldBeNull();
        intResult.Value.ShouldBe(0);
        boolResult.Value.ShouldBeFalse();
    }

    [Fact]
    public void Should_HandleEmptyErrorCollection_When_FailureWithEmptyErrors()
    {
        // Arrange
        var emptyErrors = Array.Empty<string>();

        // Act
        var result = Result<string>.WithFailure(emptyErrors);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
        result.Value.ShouldBeNull();
    }
}
