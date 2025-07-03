namespace IndTrace.Domain.UnitTests.EnumTests;

/// <summary>
/// Unit tests for EnumLookUpTable - Lookup table entity for enumeration mapping in manufacturing systems
/// </summary>
public class EnumLookUpTableTests
{
    [Fact]
    public void Constructor_WithDefaultParameters_ShouldCreateValidInstance()
    {
        // Arrange & Act
        var lookupTable = new EnumLookUpTable();

        // Assert
        lookupTable.ShouldNotBeNull();
        lookupTable.ShouldBeAssignableTo<ILookUpTable>();
    }

    [Fact]
    public void Constructor_WithParametrizedValues_ShouldSetProperties()
    {
        // Arrange
        const int expectedId = 1001;
        const string expectedName = "WeldingStation";
        const string expectedDisplayName = "Welding Station Alpha";

        // Act
        var lookupTable = new EnumLookUpTable(expectedId, expectedName, expectedDisplayName);

        // Assert
        lookupTable.Id.ShouldBe(expectedId);
        lookupTable.Name.ShouldBe(expectedName);
        lookupTable.DisplayName.ShouldBe(expectedDisplayName);
    }

    [Fact]
    public void Id_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var lookupTable = new EnumLookUpTable();
        const int expectedId = 2002;

        // Act
        lookupTable.Id = expectedId;

        // Assert
        lookupTable.Id.ShouldBe(expectedId);
    }

    [Fact]
    public void Name_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var lookupTable = new EnumLookUpTable();
        const string expectedName = "PaintingStation";

        // Act
        lookupTable.Name = expectedName;

        // Assert
        lookupTable.Name.ShouldBe(expectedName);
    }

    [Fact]
    public void DisplayName_WhenSet_ShouldReturnCorrectValue()
    {
        // Arrange
        var lookupTable = new EnumLookUpTable();
        const string expectedDisplayName = "Painting Station Beta";

        // Act
        lookupTable.DisplayName = expectedDisplayName;

        // Assert
        lookupTable.DisplayName.ShouldBe(expectedDisplayName);
    }

    [Fact]
    public void Deconstruct_WhenCalled_ShouldReturnAllComponents()
    {
        // Arrange
        const int expectedId = 3003;
        const string expectedName = "AssemblyStation";
        const string expectedDisplayName = "Assembly Station Gamma";
        var lookupTable = new EnumLookUpTable(expectedId, expectedName, expectedDisplayName);

        // Act
        var (value, name, displayName) = lookupTable;

        // Assert
        value.ShouldBe(expectedId);
        name.ShouldBe(expectedName);
        displayName.ShouldBe(expectedDisplayName);
    }

    [Theory]
    [InlineData(1, "Initial", "Initial Station")]
    [InlineData(2, "Process", "Process Station")]
    [InlineData(4, "Final", "Final Station")]
    [InlineData(8, "Inspection", "Inspection Station")]
    public void Constructor_WithValidManufacturingData_ShouldCreateCorrectInstance(
        int id, string name, string displayName)
    {
        // Arrange & Act
        var lookupTable = new EnumLookUpTable(id, name, displayName);

        // Assert
        lookupTable.Id.ShouldBe(id);
        lookupTable.Name.ShouldBe(name);
        lookupTable.DisplayName.ShouldBe(displayName);
    }

    [Fact]
    public void ToUpperClass_WhenCalled_ShouldConvertToSpecifiedType()
    {
        // Arrange
        var sourceLookupTable = new EnumLookUpTable(5005, "QualityControl", "Quality Control Station");

        // Act
        var result = sourceLookupTable.ToUpperClass<PartStatusEntity>(sourceLookupTable);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<PartStatusEntity>();
        result.Id.ShouldBe(5005);
        result.Name.ShouldBe("QualityControl");
        result.DisplayName.ShouldBe("Quality Control Station");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void StringProperties_WhenSetToNullOrEmpty_ShouldAcceptValues(string value)
    {
        // Arrange
        var lookupTable = new EnumLookUpTable();

        // Act & Assert (No exceptions should be thrown)
        lookupTable.Name = value;
        lookupTable.DisplayName = value;

        lookupTable.Name.ShouldBe(value);
        lookupTable.DisplayName.ShouldBe(value);
    }

    [Fact]
    public void NegativeId_WhenSet_ShouldBeAccepted()
    {
        // Arrange
        var lookupTable = new EnumLookUpTable();
        const int negativeId = -1;

        // Act
        lookupTable.Id = negativeId;

        // Assert
        lookupTable.Id.ShouldBe(negativeId);
    }

    [Fact]
    public void ZeroId_WhenSet_ShouldBeAccepted()
    {
        // Arrange
        var lookupTable = new EnumLookUpTable();
        const int zeroId = 0;

        // Act
        lookupTable.Id = zeroId;

        // Assert
        lookupTable.Id.ShouldBe(zeroId);
    }

    [Theory]
    [InlineData(-1, "Invalid", "Invalid Value")]
    [InlineData(0, "None", "None")]
    [InlineData(1, "Ok", "Good Part")]
    [InlineData(2, "NOk", "Defective Part")]
    [InlineData(512, "Scrap", "Scrap Part")]
    public void PartStatusScenarios_WhenCreated_ShouldHandleManufacturingStatuses(
        int id, string name, string displayName)
    {
        // Arrange & Act
        var lookupTable = new EnumLookUpTable(id, name, displayName);

        // Assert
        lookupTable.Id.ShouldBe(id);
        lookupTable.Name.ShouldBe(name);
        lookupTable.DisplayName.ShouldBe(displayName);
    }

    [Theory]
    [InlineData(1, "Printer", "Printer Station")]
    [InlineData(2, "Initial", "Initial Processing")]
    [InlineData(8, "Process", "Process Station")]
    [InlineData(16, "Final", "Final Assembly")]
    [InlineData(32, "Inspection", "Quality Inspection")]
    public void MachineTypeScenarios_WhenCreated_ShouldHandleManufacturingMachineTypes(
        int id, string name, string displayName)
    {
        // Arrange & Act
        var lookupTable = new EnumLookUpTable(id, name, displayName);

        // Assert
        lookupTable.Id.ShouldBe(id);
        lookupTable.Name.ShouldBe(name);
        lookupTable.DisplayName.ShouldBe(displayName);
    }

    [Fact]
    public void PropertyRoundTrip_WhenSetAndGet_ShouldMaintainValues()
    {
        // Arrange
        var lookupTable = new EnumLookUpTable();
        const int testId = 9999;
        const string testName = "TestStation";
        const string testDisplayName = "Test Station Delta";

        // Act
        lookupTable.Id = testId;
        lookupTable.Name = testName;
        lookupTable.DisplayName = testDisplayName;

        // Assert
        lookupTable.Id.ShouldBe(testId);
        lookupTable.Name.ShouldBe(testName);
        lookupTable.DisplayName.ShouldBe(testDisplayName);
    }

    [Fact]
    public void ToUpperClass_WithMachineTypeEntity_ShouldConvertCorrectly()
    {
        // Arrange
        var sourceLookupTable = new EnumLookUpTable(64, "DashBoard", "Dashboard Station");

        // Act
        var result = sourceLookupTable.ToUpperClass<MachineTypeEntity>(sourceLookupTable);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeOfType<MachineTypeEntity>();
        result.Id.ShouldBe(64);
        result.Name.ShouldBe("DashBoard");
        result.DisplayName.ShouldBe("Dashboard Station");
    }

    [Fact]
    public void CompleteWorkflow_WhenCreatedAndProcessed_ShouldMaintainDataIntegrity()
    {
        // Arrange
        const int workflowId = 7777;
        const string workflowName = "CompleteWorkflow";
        const string workflowDisplayName = "Complete Manufacturing Workflow";

        // Act
        var lookupTable = new EnumLookUpTable(workflowId, workflowName, workflowDisplayName);
        var (id, name, displayName) = lookupTable;

        // Assert
        id.ShouldBe(workflowId);
        name.ShouldBe(workflowName);
        displayName.ShouldBe(workflowDisplayName);
        lookupTable.Id.ShouldBe(workflowId);
        lookupTable.Name.ShouldBe(workflowName);
        lookupTable.DisplayName.ShouldBe(workflowDisplayName);
    }
}
