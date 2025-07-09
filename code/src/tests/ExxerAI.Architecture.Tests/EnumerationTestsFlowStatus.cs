namespace ExxerAI.Architecture.Tests;

public class EnumerationTestsFlowStatus
{
    [Fact]
    public void GetInvalidEnum()
    {
        // Arrange & Act
        var statusCicloEnums = EnumModel.FromValue<FlowStatus>(-1);

        // Assert
        Assert.NotNull(statusCicloEnums);
        Assert.Equal("Invalid", statusCicloEnums.Name);
    }

    [Fact]
    public void GetInvalidEnumWhenValueIsNotValid()
    {
        // Arrange & Act
        var statusCicloEnums = EnumModel.FromValue<FlowStatus>(-10);

        // Assert
        Assert.NotNull(statusCicloEnums);
        Assert.Equal("Invalid", statusCicloEnums.Name);
    }
}