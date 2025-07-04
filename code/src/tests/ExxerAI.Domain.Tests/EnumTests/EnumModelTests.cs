namespace ExxerAI.Domain.Tests.EnumTests;

/// <summary>
/// Unit tests for EnumModel - Base enumeration class for strongly-typed enumerations in manufacturing systems
/// </summary>
public class EnumModelTests
{
    [Fact]
    public void Constructor_WithDefaultParameters_ShouldCreateValidInstance()
    {
        // Arrange & Act
        var enumModel = new TestEnumModel();

        // Assert
        enumModel.ShouldNotBeNull();
        enumModel.ShouldBeAssignableTo<IComparable>();
        enumModel.ShouldBeAssignableTo<IEnumModel>();
    }

    [Fact]
    public void Invalid_StaticProperty_ShouldReturnInvalidInstance()
    {
        // Arrange & Act
        var invalid = EnumModel.Invalid;

        // Assert
        invalid.ShouldNotBeNull();
        invalid.Value.ShouldBe(1);
        invalid.Name.ShouldBe("Invalid Value");
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllComponents()
    {
        // Arrange
        var flowStatus = FlowStatus.InProcess;

        // Act
        var (value, name, displayName) = flowStatus;

        // Assert
        value.ShouldBe(1);
        name.ShouldBe("Ok");
        displayName.ShouldBe("Ok");
    }

    [Fact]
    public void ImplicitConversion_ToInt_ShouldReturnValue()
    {
        // Arrange
        var flowStatus = FlowStatus.InProcess;

        // Act
        int value = flowStatus;

        // Assert
        value.ShouldBe(1);
    }

    [Fact]
    public void ToString_WhenCalled_ShouldReturnDisplayNameOrName()
    {
        // Arrange
        var flowStatus = FlowStatus.InProcess;

        // Act
        var result = flowStatus.ToString(1);

        // Assert
        result.ShouldBe("Ok");
    }

    [Theory]
    [InlineData(1, true)]    // Ok
    [InlineData(2, true)]    // NOk
    [InlineData(4, true)]    // Restored
    [InlineData(999, false)] // Non-existent
    public void Exists_WhenCalledWithValues_ShouldReturnCorrectResult(int value, bool expected)
    {
        // Arrange & Act
        var result = EnumModel.Exists<FlowStatus>(value);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void GetAll_WhenCalled_ShouldReturnAllInstancesOfType()
    {
        // Arrange & Act
        var allFlowStatuses = EnumModel.GetAll<FlowStatus>().ToList();

        // Assert
        allFlowStatuses.ShouldNotBeEmpty();
        allFlowStatuses.ShouldContain(ps => ps.Name == "Ok");
        allFlowStatuses.ShouldContain(ps => ps.Name == "nOK");
        allFlowStatuses.ShouldContain(ps => ps.Name == "Restored");
        allFlowStatuses.ShouldContain(ps => ps.Name == "Rejected");
        allFlowStatuses.ShouldContain(ps => ps.Name == "Scrap");
    }

    [Fact]
    public void ToLookUpTable_NonGeneric_ShouldReturnEnumLookUpTableList()
    {
        // Arrange & Act
        var lookupTables = EnumModel.ToLookUpTable<FlowStatus>();

        // Assert
        lookupTables.ShouldNotBeEmpty();
        lookupTables.ShouldAllBe(lt => lt != null);
        lookupTables.ShouldContain(lt => lt.Name == "Ok");
    }

    [Fact]
    public void Equals_WithSameInstance_ShouldReturnTrue()
    {
        // Arrange
        var flowStatus1 = FlowStatus.InProcess;
        var flowStatus2 = FlowStatus.InProcess;

        // Act & Assert
        flowStatus1.Equals(flowStatus2).ShouldBeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var flowStatus1 = FlowStatus.InProcess;
        var flowStatus2 = FlowStatus.Created;

        // Act & Assert
        flowStatus1.Equals(flowStatus2).ShouldBeFalse();
    }

    [Fact]
    public void Equals_WithNullObject_ShouldReturnFalse()
    {
        // Arrange
        var flowStatus = FlowStatus.InProcess;

        // Act & Assert
        flowStatus.Equals(null).ShouldBeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldReturnSameHashCode()
    {
        // Arrange
        var flowStatus1 = FlowStatus.InProcess;
        var flowStatus2 = FlowStatus.InProcess;

        // Act
        var hash1 = flowStatus1.GetHashCode();
        var hash2 = flowStatus2.GetHashCode();

        // Assert
        hash1.ShouldBe(hash2);
    }

    [Fact]
    public void AbsoluteDifference_BetweenValues_ShouldReturnCorrectDifference()
    {
        // Arrange
        var flowStatus1 = FlowStatus.InProcess;       // Value = 1
        var flowStatus2 = FlowStatus.Finished; // Value = 4

        // Act
        var difference = EnumModel.AbsoluteDifference(flowStatus1, flowStatus2);

        // Assert
        difference.ShouldBe(3);
    }

    [Theory]
    [InlineData(1)]     // Ok
    [InlineData(2)]     // NOk
    [InlineData(4)]     // Restored
    [InlineData(8)]     // Rejected
    [InlineData(512)]   // Scrap
    public void FromValue_WithValidValues_ShouldReturnCorrectInstance(int value)
    {
        // Arrange & Act
        var result = EnumModel.FromValue<FlowStatus>(value);

        // Assert
        result.ShouldNotBeNull();
        result.Value.ShouldBe(value);
    }

    [Fact]
    public void FromValue_WithInvalidValue_ShouldReturnInvalidInstance()
    {
        // Arrange & Act
        var result = EnumModel.FromValue<FlowStatus>(999);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Invalid Value");
    }

    [Theory]
    [InlineData("None")]
    [InlineData("Created")]
    [InlineData("InProcess")]
    [InlineData("Finished")]
    [InlineData("Invalid")]
    public void FromName_WithValidNames_ShouldReturnCorrectInstance(string name)
    {
        // Arrange & Act
        var result = EnumModel.FromName<FlowStatus>(name);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe(name);
    }

    [Fact]
    public void FromName_WithInvalidName_ShouldReturnInvalidInstance()
    {
        // Arrange & Act
        var result = EnumModel.FromName<FlowStatus>("NonExistentStatus");

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Invalid Value");
    }

    [Fact]
    public void FromDisplayName_WithValidDisplayName_ShouldReturnCorrectInstance()
    {
        // Arrange & Act
        var result = EnumModel.FromDisplayName<FlowStatus>("Ok");

        // Assert
        result.ShouldNotBeNull();
        result.DisplayName.ShouldBe("Ok");
    }

    [Fact]
    public void FromDisplayName_WithInvalidDisplayName_ShouldReturnInvalidInstance()
    {
        // Arrange & Act
        var result = EnumModel.FromDisplayName<FlowStatus>("NonExistentDisplayName");

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Invalid Value");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(null)]
    public void FromValue_WithNullableInt_ShouldHandleCorrectly(int? value)
    {
        // Arrange & Act
        var result = EnumModel.FromValue<FlowStatus>(value);

        // Assert
        result.ShouldNotBeNull();
        if (value.HasValue && EnumModel.Exists<FlowStatus>(value.Value))
        {
            result.Value.ShouldBe(value.Value);
        }
        else
        {
            result.Name.ShouldBe("Invalid Value");
        }
    }

    [Fact]
    public void InvalidValue_WhenCalled_ShouldReturnInvalidInstance()
    {
        // Arrange & Act
        var result = EnumModel.InvalidValue<FlowStatus>();

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Invalid Value");
    }

    [Fact]
    public void CompareTo_WithSameType_ShouldCompareByValue()
    {
        // Arrange
        var flowStatus1 = FlowStatus.InProcess;       // Value = 1
        var flowStatus2 = FlowStatus.Finished; // Value = 4

        // Act
        var comparison = flowStatus1.CompareTo(flowStatus2);

        // Assert
        comparison.ShouldBeLessThan(0);
    }

    [Fact]
    public void CompareTo_WithNull_ShouldReturnDefault()
    {
        // Arrange
        var flowStatus = FlowStatus.InProcess;

        // Act
        var comparison = flowStatus.CompareTo(null);

        // Assert
        comparison.ShouldBe(0);
    }

    [Theory]
    [InlineData(1, 2)]    // Ok vs NOk
    [InlineData(2, 4)]    // NOk vs Restored
    [InlineData(4, 8)]    // Restored vs Rejected
    public void ManufacturingWorkflow_WithDifferentFlowStatuses_ShouldMaintainOrder(int value1, int value2)
    {
        // Arrange
        var status1 = EnumModel.FromValue<FlowStatus>(value1);
        var status2 = EnumModel.FromValue<FlowStatus>(value2);

        // Act
        var comparison = status1.CompareTo(status2);

        // Assert
        comparison.ShouldBeLessThan(0);
        status1.Value.ShouldBeLessThan(status2.Value);
    }

    /// <summary>
    /// Test enumeration for testing purposes
    /// </summary>
    private class TestEnumModel : ExxerAI.Domain.Enums.EnumModel
    {
        public TestEnumModel() : base()
        {
        }
    }
}