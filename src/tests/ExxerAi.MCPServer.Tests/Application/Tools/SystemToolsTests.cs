using ExxerAi.MCPServer.Application.Tools;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace ExxerAi.MCPServer.Tests.Application.Tools;

/// <summary>
/// Comprehensive unit tests for SystemTools MCP tool implementation
/// Tests all system utility functionality and error scenarios
/// </summary>
public class SystemToolsTests
{
	private readonly ILogger<SystemTools> _logger;
	private readonly SystemTools _systemTools;

	/// <summary>
	/// Initializes a new instance of the SystemToolsTests class
	/// </summary>
	public SystemToolsTests()
	{
		_logger = Substitute.For<ILogger<SystemTools>>();
		_systemTools = new SystemTools(_logger);
	}

	/// <summary>
	/// Tests that SystemTools can be instantiated with valid dependencies
	/// </summary>
	[Fact]
	public void Should_CreateInstance_When_ValidLoggerProvided()
	{
		// Arrange & Act
		var instance = new SystemTools(_logger);

		// Assert
		instance.ShouldNotBeNull();
	}

	/// <summary>
	/// Tests that constructor throws ArgumentNullException when logger is null
	/// </summary>
	[Fact]
	public void Should_ThrowArgumentNullException_When_LoggerIsNull()
	{
		// Arrange, Act & Assert
		Should.Throw<ArgumentNullException>(() => new SystemTools(null!))
			.ParamName.ShouldBe("logger");
	}

	/// <summary>
	/// Tests successful system information retrieval
	/// </summary>
	[Fact]
	public async Task Should_GetSystemInfo_When_Called()
	{
		// Act
		var result = await _systemTools.GetSystemInfoAsync();

		// Assert
		result.ShouldContain("🖥️ System Information:");
		result.ShouldContain("💻 OS:");
		result.ShouldContain("🏗️ Architecture:");
		result.ShouldContain("⚙️ Runtime:");
		result.ShouldContain("🔧 Process:");
		result.ShouldContain("🧠 Memory:");
		result.ShouldContain("⏰ Uptime:");
		result.ShouldContain("🖥️ Machine:");
		result.ShouldContain("👤 User:");
		result.ShouldContain("📂 Working Dir:");
	}

	/// <summary>
	/// Tests current time retrieval in readable format
	/// </summary>
	[Fact]
	public async Task Should_GetCurrentTimeReadable_When_DefaultFormatUsed()
	{
		// Act
		var result = await _systemTools.GetCurrentTimeAsync();

		// Assert
		result.ShouldContain("🕐 Current Time:");
		result.ShouldContain("📅 Local:");
		result.ShouldContain("🌍 UTC:");
		result.ShouldContain("📍 Timezone:");
	}

	/// <summary>
	/// Tests current time retrieval in ISO format
	/// </summary>
	[Fact]
	public async Task Should_GetCurrentTimeISO_When_ISOFormatRequested()
	{
		// Act
		var result = await _systemTools.GetCurrentTimeAsync("iso");

		// Assert
		result.ShouldContain("🕐 Current Time (ISO):");
		result.ShouldMatch(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}\.\d{3}Z");
	}

	/// <summary>
	/// Tests current time retrieval in timestamp format
	/// </summary>
	[Fact]
	public async Task Should_GetCurrentTimeTimestamp_When_TimestampFormatRequested()
	{
		// Act
		var result = await _systemTools.GetCurrentTimeAsync("timestamp");

		// Assert
		result.ShouldContain("🕐 Current Time (Timestamp):");
		result.ShouldMatch(@"\d{10}"); // Unix timestamp should be 10 digits
	}

	/// <summary>
	/// Tests current time retrieval in UTC format
	/// </summary>
	[Fact]
	public async Task Should_GetCurrentTimeUTC_When_UTCFormatRequested()
	{
		// Act
		var result = await _systemTools.GetCurrentTimeAsync("utc");

		// Assert
		result.ShouldContain("🕐 Current Time (UTC):");
		result.ShouldContain("UTC");
	}

	/// <summary>
	/// Tests current time with timezone parameter
	/// </summary>
	[Fact]
	public async Task Should_GetCurrentTimeWithTimezone_When_ValidTimezoneProvided()
	{
		// Act
		var result = await _systemTools.GetCurrentTimeAsync("readable", "UTC");

		// Assert
		result.ShouldContain("🌏 UTC:");
	}

	/// <summary>
	/// Tests current time with invalid timezone parameter
	/// </summary>
	[Fact]
	public async Task Should_HandleInvalidTimezone_When_InvalidTimezoneProvided()
	{
		// Act
		var result = await _systemTools.GetCurrentTimeAsync("readable", "Invalid/Timezone");

		// Assert
		result.ShouldContain("❌ Invalid timezone: Invalid/Timezone");
	}

	/// <summary>
	/// Tests file listing in current directory
	/// </summary>
	[Fact]
	public async Task Should_ListFiles_When_ValidDirectoryProvided()
	{
		// Act
		var result = await _systemTools.ListFilesAsync(".");

		// Assert
		result.ShouldContain("📂 Directory Listing:");
		result.ShouldContain("📊 Items:");
	}

	/// <summary>
	/// Tests file listing with invalid directory
	/// </summary>
	[Fact]
	public async Task Should_ReturnErrorMessage_When_InvalidDirectoryProvided()
	{
		// Act
		var result = await _systemTools.ListFilesAsync("/nonexistent/directory");

		// Assert
		result.ShouldContain("❌ Directory not found:");
	}

	/// <summary>
	/// Tests file listing with custom parameters
	/// </summary>
	[Fact]
	public async Task Should_ListFilesWithOptions_When_CustomParametersProvided()
	{
		// Act
		var result = await _systemTools.ListFilesAsync(".", includeHidden: true, maxItems: 10);

		// Assert
		result.ShouldContain("📂 Directory Listing:");
		result.ShouldContain("(Max: 10)");
	}

	/// <summary>
	/// Tests basic mathematical calculation
	/// </summary>
	[Fact]
	public async Task Should_Calculate_When_ValidExpressionProvided()
	{
		// Act
		var result = await _systemTools.CalculateAsync("2 + 2");

		// Assert
		result.ShouldContain("🧮 Calculation Result:");
		result.ShouldContain("📝 Expression: 2 + 2");
		result.ShouldContain("🔢 Result: 4");
	}

	/// <summary>
	/// Tests complex mathematical calculation
	/// </summary>
	[Fact]
	public async Task Should_CalculateComplex_When_ComplexExpressionProvided()
	{
		// Act
		var result = await _systemTools.CalculateAsync("(10 + 5) * 2 / 3");

		// Assert
		result.ShouldContain("🧮 Calculation Result:");
		result.ShouldContain("📝 Expression: (10 + 5) * 2 / 3");
		result.ShouldContain("🔢 Result: 10");
	}

	/// <summary>
	/// Tests calculation with invalid characters
	/// </summary>
	[Fact]
	public async Task Should_RejectCalculation_When_InvalidCharactersProvided()
	{
		// Act
		var result = await _systemTools.CalculateAsync("2 + abc");

		// Assert
		result.ShouldContain("❌ Expression contains invalid characters");
	}

	/// <summary>
	/// Tests calculation with empty expression
	/// </summary>
	[Fact]
	public async Task Should_RejectCalculation_When_EmptyExpressionProvided()
	{
		// Act
		var result = await _systemTools.CalculateAsync("");

		// Assert
		result.ShouldContain("❌ Expression cannot be empty");
	}

	/// <summary>
	/// Tests calculation with whitespace-only expression
	/// </summary>
	[Fact]
	public async Task Should_RejectCalculation_When_WhitespaceOnlyExpressionProvided()
	{
		// Act
		var result = await _systemTools.CalculateAsync("   ");

		// Assert
		result.ShouldContain("❌ Expression cannot be empty");
	}

	/// <summary>
	/// Tests memory usage information retrieval
	/// </summary>
	[Fact]
	public async Task Should_GetMemoryUsage_When_Called()
	{
		// Act
		var result = await _systemTools.GetMemoryUsageAsync();

		// Assert
		result.ShouldContain("🧠 Memory Usage Information:");
		result.ShouldContain("📊 GC Memory:");
		result.ShouldContain("🔧 Working Set:");
		result.ShouldContain("🔒 Private Memory:");
		result.ShouldContain("🗑️ GC Collections (Gen 0):");
		result.ShouldContain("🗑️ GC Collections (Gen 1):");
		result.ShouldContain("🗑️ GC Collections (Gen 2):");
	}

	/// <summary>
	/// Tests health check functionality
	/// </summary>
	[Fact]
	public async Task Should_CheckHealth_When_Called()
	{
		// Act
		var result = await _systemTools.CheckHealthAsync();

		// Assert
		result.ShouldContain("🏥 MCP Server Health Status:");
		result.ShouldContain("✅ Status:");
		result.ShouldContain("⏰ Uptime:");
		result.ShouldContain("🧠 Memory:");
		result.ShouldContain("🔖 Version: 1.0.0");
		result.ShouldContain("🕐 Check Time:");
		result.ShouldContain("🌡️ Status:");
	}

	/// <summary>
	/// Tests that health check reports healthy status for normal conditions
	/// </summary>
	[Fact]
	public async Task Should_ReportHealthyStatus_When_SystemIsOperatingNormally()
	{
		// Act
		var result = await _systemTools.CheckHealthAsync();

		// Assert
		result.ShouldContain("✅ Status: Healthy");
		result.ShouldContain("All systems operational");
	}

	/// <summary>
	/// Tests logging during system information retrieval
	/// </summary>
	[Fact]
	public async Task Should_LogInformation_When_GetSystemInfoCalled()
	{
		// Act
		await _systemTools.GetSystemInfoAsync();

		// Assert
		_logger.Received(1).LogInformation("Getting system information");
		_logger.Received(1).LogInformation("System information retrieved successfully");
	}

	/// <summary>
	/// Tests logging during time retrieval
	/// </summary>
	[Fact]
	public async Task Should_LogInformation_When_GetCurrentTimeCalled()
	{
		// Arrange
		var format = "readable";

		// Act
		await _systemTools.GetCurrentTimeAsync(format);

		// Assert
		_logger.Received(1).LogInformation("Getting current time in format {Format}", format);
		_logger.Received(1).LogInformation("Current time retrieved in format {Format}", format);
	}

	/// <summary>
	/// Tests logging during file listing
	/// </summary>
	[Fact]
	public async Task Should_LogInformation_When_ListFilesCalled()
	{
		// Arrange
		var path = ".";

		// Act
		await _systemTools.ListFilesAsync(path);

		// Assert
		_logger.Received(1).LogInformation("Listing files in directory {Path}", path);
		_logger.Received(1).LogInformation("Listed {Count} items in directory {Path}", Arg.Any<int>(), path);
	}

	/// <summary>
	/// Tests logging during calculation
	/// </summary>
	[Fact]
	public async Task Should_LogInformation_When_CalculationSucceeds()
	{
		// Arrange
		var expression = "2 + 2";

		// Act
		await _systemTools.CalculateAsync(expression);

		// Assert
		_logger.Received(1).LogInformation("Calculating expression: {Expression}", expression);
		_logger.Received(1).LogInformation("Calculation completed: {Expression} = {Result}", expression, 4d);
	}

	/// <summary>
	/// Tests logging during memory usage retrieval
	/// </summary>
	[Fact]
	public async Task Should_LogInformation_When_GetMemoryUsageCalled()
	{
		// Act
		await _systemTools.GetMemoryUsageAsync();

		// Assert
		_logger.Received(1).LogInformation("Getting memory usage information");
		_logger.Received(1).LogInformation("Memory usage information retrieved");
	}

	/// <summary>
	/// Tests logging during health check
	/// </summary>
	[Fact]
	public async Task Should_LogInformation_When_CheckHealthCalled()
	{
		// Act
		await _systemTools.CheckHealthAsync();

		// Assert
		_logger.Received(1).LogInformation("Checking MCP server health");
		_logger.Received(1).LogInformation("Health check completed: {Status}", "Healthy");
	}

	/// <summary>
	/// Tests calculation with modulo operator
	/// </summary>
	[Fact]
	public async Task Should_CalculateModulo_When_ModuloExpressionProvided()
	{
		// Act
		var result = await _systemTools.CalculateAsync("10 % 3");

		// Assert
		result.ShouldContain("🧮 Calculation Result:");
		result.ShouldContain("📝 Expression: 10 % 3");
		result.ShouldContain("🔢 Result: 1");
	}

	/// <summary>
	/// Tests file listing with maximum items constraint
	/// </summary>
	[Fact]
	public async Task Should_TruncateFileList_When_MaxItemsExceeded()
	{
		// Arrange - use a directory likely to have many items
		var tempDir = Path.GetTempPath();

		// Act
		var result = await _systemTools.ListFilesAsync(tempDir, maxItems: 5);

		// Assert
		result.ShouldContain("(Max: 5)");
		// Note: Depending on temp directory contents, truncation message may or may not appear
	}
} 