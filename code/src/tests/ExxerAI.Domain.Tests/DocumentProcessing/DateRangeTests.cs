namespace ExxerAI.Domain.Tests.DocumentProcessing;

/// <summary>
/// Unit tests for DateRange class
/// </summary>
public class DateRangeTests
{
    /// <summary>
    /// Tests DateRange default constructor initialization
    /// </summary>
    [Fact]
    public void Should_Initialize_With_Default_Values_When_Using_Default_Constructor()
    {
        // Act
        var dateRange = new DateRange();

        // Assert
        dateRange.FromDate.ShouldBe(default(DateTime));
        dateRange.ToDate.ShouldBe(default(DateTime));
    }

    /// <summary>
    /// Tests property round-trip for FromDate
    /// </summary>
    [Theory]
    [InlineData("2024-01-01")]
    [InlineData("2024-06-15")]
    [InlineData("2024-12-31")]
    public void Should_Set_And_Get_FromDate_When_Valid_DateTime_Provided(string dateString)
    {
        // Arrange
        var dateRange = new DateRange();
        var fromDate = DateTime.Parse(dateString);

        // Act
        dateRange.FromDate = fromDate;

        // Assert
        dateRange.FromDate.ShouldBe(fromDate);
    }

    /// <summary>
    /// Tests property round-trip for ToDate
    /// </summary>
    [Theory]
    [InlineData("2024-01-01")]
    [InlineData("2024-06-15")]
    [InlineData("2024-12-31")]
    public void Should_Set_And_Get_ToDate_When_Valid_DateTime_Provided(string dateString)
    {
        // Arrange
        var dateRange = new DateRange();
        var toDate = DateTime.Parse(dateString);

        // Act
        dateRange.ToDate = toDate;

        // Assert
        dateRange.ToDate.ShouldBe(toDate);
    }

    /// <summary>
    /// Tests both properties can be set together
    /// </summary>
    [Fact]
    public void Should_Set_Both_Dates_When_Creating_Complete_DateRange()
    {
        // Arrange
        var fromDate = new DateTime(2024, 1, 1);
        var toDate = new DateTime(2024, 12, 31);

        // Act
        var dateRange = new DateRange
        {
            FromDate = fromDate,
            ToDate = toDate
        };

        // Assert
        dateRange.FromDate.ShouldBe(fromDate);
        dateRange.ToDate.ShouldBe(toDate);
    }
}