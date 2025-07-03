using ExxerAI.Domain.Enums;

namespace ExxerAI.Domain.Tests.EnumTests;

public class EnumerationTestsFlowStatus
{
    [Fact]
    public void GetInvalidEnum()
    {
        // Arrange & Act
        var statusCicloEnums = Enums.EnumModel.FromValue<FlowStatus>(-1);

        // Assert
        statusCicloEnums.ShouldNotBeNull();
        statusCicloEnums.Name.ShouldBe("Invalid");
    }

    [Fact]
    public void GetInvalidEnumWhenValueIsNotValid()
    {
        // Arrange & Act
        var statusCicloEnums = Enums.EnumModel.FromValue<FlowStatus>(-10);

        // Assert
        statusCicloEnums.ShouldNotBeNull();
        statusCicloEnums.Name.ShouldBe("Invalid");
    }
}