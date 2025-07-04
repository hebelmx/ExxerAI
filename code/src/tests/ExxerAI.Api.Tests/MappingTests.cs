namespace ExxerAI.Api.Tests;

/// <summary>
/// Comprehensive tests for mapping between domain models and DTOs
/// </summary>
public class MappingTests
{
	/// <summary>
	/// Test fixture for Agent to DTO mapping scenarios
	/// </summary>
	public class AgentMappingTests
	{
		[Fact]
		public void Should_MapAgentToResponse_When_ValidAgentProvided()
		{
			// Arrange
			var agent = new Agent
			{
				Id = Guid.NewGuid(),
				Name = "Test Agent",
				Description = "Test Description",
				Status = AgentStatus.Active,
				CreatedAt = DateTime.UtcNow.AddDays(-1),
				UpdatedAt = DateTime.UtcNow,
				Capabilities = new AgentCapabilities
				{
					CanProcessNaturalLanguage = true,
					CanGenerateCode = false,
					CanAnalyzeData = true,
					CanCallExternalAPIs = false,
					MaxConcurrentTasks = 3,
					SupportedTaskTypes = { "DataAnalysis", "TextProcessing" }
				},
				Configuration = new AgentConfiguration
				{
					TaskTimeoutSeconds = 600,
					MaxRetries = 5,
					Priority = 8,
					CustomProperties = { ["environment"] = "test", ["version"] = "1.0" }
				}
			};

			// Act
			var response = agent.ToResponse();

			// Assert
			response.ShouldNotBeNull();
			response.Id.ShouldBe(agent.Id);
			response.Name.ShouldBe(agent.Name);
			response.Description.ShouldBe(agent.Description);
			response.Status.ShouldBe(agent.Status);
			response.CreatedAt.ShouldBe(agent.CreatedAt);
			response.UpdatedAt.ShouldBe(agent.UpdatedAt);

			// Verify capabilities mapping
			response.Capabilities.ShouldNotBeNull();
			response.Capabilities.CanProcessNaturalLanguage.ShouldBe(true);
			response.Capabilities.CanGenerateCode.ShouldBe(false);
			response.Capabilities.CanAnalyzeData.ShouldBe(true);
			response.Capabilities.CanCallExternalAPIs.ShouldBe(false);
			response.Capabilities.MaxConcurrentTasks.ShouldBe(3);
			response.Capabilities.SupportedTaskTypes.Count.ShouldBe(2);
			response.Capabilities.SupportedTaskTypes.ShouldContain("DataAnalysis");
			response.Capabilities.SupportedTaskTypes.ShouldContain("TextProcessing");

			// Verify configuration mapping
			response.Configuration.ShouldNotBeNull();
			response.Configuration.TaskTimeoutSeconds.ShouldBe(600);
			response.Configuration.MaxRetries.ShouldBe(5);
			response.Configuration.Priority.ShouldBe(8);
			response.Configuration.CustomProperties.Count.ShouldBe(2);
			response.Configuration.CustomProperties["environment"].ShouldBe("test");
			response.Configuration.CustomProperties["version"].ShouldBe("1.0");
		}

		[Fact]
		public void Should_HandleEmptyCapabilities_When_AgentHasMinimalData()
		{
			// Arrange
			var agent = new Agent
			{
				Id = Guid.NewGuid(),
				Name = "Minimal Agent",
				Description = "",
				Status = AgentStatus.Inactive,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow,
				Capabilities = new AgentCapabilities(), // Default values
				Configuration = new AgentConfiguration() // Default values
			};

			// Act
			var response = agent.ToResponse();

			// Assert
			response.ShouldNotBeNull();
			response.Capabilities.ShouldNotBeNull();
			response.Capabilities.CanProcessNaturalLanguage.ShouldBe(true); // Default
			response.Capabilities.CanGenerateCode.ShouldBe(false); // Default
			response.Capabilities.MaxConcurrentTasks.ShouldBe(1); // Default
			response.Capabilities.SupportedTaskTypes.ShouldBeEmpty();

			response.Configuration.ShouldNotBeNull();
			response.Configuration.TaskTimeoutSeconds.ShouldBe(300); // Default
			response.Configuration.MaxRetries.ShouldBe(3); // Default
			response.Configuration.Priority.ShouldBe(1); // Default
			response.Configuration.CustomProperties.ShouldBeEmpty();
		}

		[Fact]
		public void Should_PreserveComplexCustomProperties_When_MappingConfiguration()
		{
			// Arrange
			var agent = new Agent
			{
				Id = Guid.NewGuid(),
				Name = "Complex Agent",
				Configuration = new AgentConfiguration
				{
					CustomProperties = 
					{
						["string"] = "text value",
						["number"] = 42,
						["boolean"] = true,
						["decimal"] = 3.14159,
						["date"] = DateTime.UtcNow,
						["guid"] = Guid.NewGuid(),
						["array"] = new[] { 1, 2, 3 },
						["null"] = null!
					}
				}
			};

			// Act
			var response = agent.ToResponse();

			// Assert
			response.Configuration.CustomProperties.Count.ShouldBe(8);
			response.Configuration.CustomProperties["string"].ShouldBe("text value");
			response.Configuration.CustomProperties["number"].ShouldBe(42);
			response.Configuration.CustomProperties["boolean"].ShouldBe(true);
			response.Configuration.CustomProperties["decimal"].ShouldBe(3.14159);
			response.Configuration.CustomProperties["date"].ShouldBeOfType<DateTime>();
			response.Configuration.CustomProperties["guid"].ShouldBeOfType<Guid>();
			response.Configuration.CustomProperties["array"].ShouldBeOfType<int[]>();
			response.Configuration.CustomProperties["null"].ShouldBeNull();
		}
	}

	/// <summary>
	/// Test fixture for Capabilities DTO mapping scenarios
	/// </summary>
	public class CapabilitiesMappingTests
	{
		[Fact]
		public void Should_MapCapabilitiesToDto_When_AllCapabilitiesSet()
		{
			// Arrange
			var capabilities = new AgentCapabilities
			{
				CanProcessNaturalLanguage = true,
				CanGenerateCode = true,
				CanAnalyzeData = false,
				CanCallExternalAPIs = true,
				MaxConcurrentTasks = 10,
				SupportedTaskTypes = { "Web", "Value", "Code", "Analysis" }
			};

			// Act
			var dto = capabilities.ToDto();

			// Assert
			dto.ShouldNotBeNull();
			dto.CanProcessNaturalLanguage.ShouldBe(true);
			dto.CanGenerateCode.ShouldBe(true);
			dto.CanAnalyzeData.ShouldBe(false);
			dto.CanCallExternalAPIs.ShouldBe(true);
			dto.MaxConcurrentTasks.ShouldBe(10);
			dto.SupportedTaskTypes.Count.ShouldBe(4);
			dto.SupportedTaskTypes.ShouldContain("Web");
			dto.SupportedTaskTypes.ShouldContain("Value");
			dto.SupportedTaskTypes.ShouldContain("Code");
			dto.SupportedTaskTypes.ShouldContain("Analysis");
		}

		[Fact]
		public void Should_MapDtoToCapabilities_When_ValidDtoProvided()
		{
			// Arrange
			var dto = new AgentCapabilitiesDto
			{
				CanProcessNaturalLanguage = false,
				CanGenerateCode = true,
				CanAnalyzeData = true,
				CanCallExternalAPIs = false,
				MaxConcurrentTasks = 5,
				SupportedTaskTypes = { "AI", "ML", "NLP" }
			};

			// Act
			var capabilities = dto.ToDomain();

			// Assert
			capabilities.ShouldNotBeNull();
			capabilities.CanProcessNaturalLanguage.ShouldBe(false);
			capabilities.CanGenerateCode.ShouldBe(true);
			capabilities.CanAnalyzeData.ShouldBe(true);
			capabilities.CanCallExternalAPIs.ShouldBe(false);
			capabilities.MaxConcurrentTasks.ShouldBe(5);
			capabilities.SupportedTaskTypes.Count.ShouldBe(3);
			capabilities.SupportedTaskTypes.ShouldContain("AI");
			capabilities.SupportedTaskTypes.ShouldContain("ML");
			capabilities.SupportedTaskTypes.ShouldContain("NLP");
		}

		[Fact]
		public void Should_HandleEmptyTaskTypes_When_NoTaskTypesProvided()
		{
			// Arrange
			var capabilities = new AgentCapabilities
			{
				MaxConcurrentTasks = 1
				// SupportedTaskTypes empty by default
			};

			// Act
			var dto = capabilities.ToDto();
			var backToDomain = dto.ToDomain();

			// Assert
			dto.SupportedTaskTypes.ShouldBeEmpty();
			backToDomain.SupportedTaskTypes.ShouldNotBeNull();
			backToDomain.SupportedTaskTypes.ShouldBeEmpty();
		}

		[Fact]
		public void Should_PreserveTaskTypeOrder_When_MappingBackAndForth()
		{
			// Arrange
			var originalTaskTypes = new[] { "First", "Second", "Third", "Fourth" };
			var capabilities = new AgentCapabilities
			{
				SupportedTaskTypes = originalTaskTypes.ToList()
			};

			// Act
			var dto = capabilities.ToDto();
			var backToDomain = dto.ToDomain();

			// Assert
			dto.SupportedTaskTypes.ShouldBe(originalTaskTypes);
			backToDomain.SupportedTaskTypes.ShouldBe(originalTaskTypes);
		}
	}

	/// <summary>
	/// Test fixture for Configuration DTO mapping scenarios
	/// </summary>
	public class ConfigurationMappingTests
	{
		[Fact]
		public void Should_MapConfigurationToDto_When_AllPropertiesSet()
		{
			// Arrange
			var configuration = new AgentConfiguration
			{
				TaskTimeoutSeconds = 1800,
				MaxRetries = 10,
				Priority = 5,
				CustomProperties = 
				{
					["feature1"] = true,
					["threshold"] = 0.95,
					["category"] = "production"
				}
			};

			// Act
			var dto = configuration.ToDto();

			// Assert
			dto.ShouldNotBeNull();
			dto.TaskTimeoutSeconds.ShouldBe(1800);
			dto.MaxRetries.ShouldBe(10);
			dto.Priority.ShouldBe(5);
			dto.CustomProperties.Count.ShouldBe(3);
			dto.CustomProperties["feature1"].ShouldBe(true);
			dto.CustomProperties["threshold"].ShouldBe(0.95);
			dto.CustomProperties["category"].ShouldBe("production");
		}

		[Fact]
		public void Should_MapRequestToDomain_When_ValidUpdateRequestProvided()
		{
			// Arrange
			var request = new UpdateAgentConfigurationRequest
			{
				TaskTimeoutSeconds = 900,
				MaxRetries = 7,
				Priority = 3,
				CustomProperties = 
				{
					["debug"] = false,
					["retryDelay"] = 5000,
					["mode"] = "batch"
				}
			};

			// Act
			var configuration = request.ToDomain();

			// Assert
			configuration.ShouldNotBeNull();
			configuration.TaskTimeoutSeconds.ShouldBe(900);
			configuration.MaxRetries.ShouldBe(7);
			configuration.Priority.ShouldBe(3);
			configuration.CustomProperties.Count.ShouldBe(3);
			configuration.CustomProperties["debug"].ShouldBe(false);
			configuration.CustomProperties["retryDelay"].ShouldBe(5000);
			configuration.CustomProperties["mode"].ShouldBe("batch");
		}

		[Fact]
		public void Should_HandleEmptyCustomProperties_When_NoPropertiesProvided()
		{
			// Arrange
			var configuration = new AgentConfiguration
			{
				TaskTimeoutSeconds = 300,
				MaxRetries = 3,
				Priority = 1
				// CustomProperties empty by default
			};

			// Act
			var dto = configuration.ToDto();

			// Assert
			dto.CustomProperties.ShouldNotBeNull();
			dto.CustomProperties.ShouldBeEmpty();
		}
	}

	/// <summary>
	/// Test fixture for round-trip mapping scenarios
	/// </summary>
	public class RoundTripMappingTests
	{
		[Fact]
		public void Should_PreserveAllData_When_MappingAgentToResponseAndBack()
		{
			// Note: This test simulates what would happen if we had a reverse mapping
			// In the current API, we only map from domain to DTO, not back

			// Arrange
			var originalAgent = new Agent
			{
				Id = Guid.NewGuid(),
				Name = "Round Trip Agent",
				Description = "Testing round trip mapping",
				Status = AgentStatus.Busy,
				CreatedAt = DateTime.UtcNow.AddHours(-5),
				UpdatedAt = DateTime.UtcNow.AddMinutes(-10),
				Capabilities = new AgentCapabilities
				{
					CanProcessNaturalLanguage = true,
					CanGenerateCode = false,
					CanAnalyzeData = true,
					CanCallExternalAPIs = true,
					MaxConcurrentTasks = 7,
					SupportedTaskTypes = { "Analysis", "Web", "API" }
				},
				Configuration = new AgentConfiguration
				{
					TaskTimeoutSeconds = 1200,
					MaxRetries = 4,
					Priority = 6,
					CustomProperties = { ["test"] = "value", ["number"] = 123 }
				}
			};

			// Act - Map to response DTO
			var response = originalAgent.ToResponse();

			// Assert - Verify all data is preserved in DTO
			response.Id.ShouldBe(originalAgent.Id);
			response.Name.ShouldBe(originalAgent.Name);
			response.Description.ShouldBe(originalAgent.Description);
			response.Status.ShouldBe(originalAgent.Status);
			response.CreatedAt.ShouldBe(originalAgent.CreatedAt);
			response.UpdatedAt.ShouldBe(originalAgent.UpdatedAt);

			// Capabilities
			response.Capabilities.CanProcessNaturalLanguage.ShouldBe(originalAgent.Capabilities.CanProcessNaturalLanguage);
			response.Capabilities.CanGenerateCode.ShouldBe(originalAgent.Capabilities.CanGenerateCode);
			response.Capabilities.CanAnalyzeData.ShouldBe(originalAgent.Capabilities.CanAnalyzeData);
			response.Capabilities.CanCallExternalAPIs.ShouldBe(originalAgent.Capabilities.CanCallExternalAPIs);
			response.Capabilities.MaxConcurrentTasks.ShouldBe(originalAgent.Capabilities.MaxConcurrentTasks);
			response.Capabilities.SupportedTaskTypes.ShouldBe(originalAgent.Capabilities.SupportedTaskTypes.ToList());

			// Configuration
			response.Configuration.TaskTimeoutSeconds.ShouldBe(originalAgent.Configuration.TaskTimeoutSeconds);
			response.Configuration.MaxRetries.ShouldBe(originalAgent.Configuration.MaxRetries);
			response.Configuration.Priority.ShouldBe(originalAgent.Configuration.Priority);
			response.Configuration.CustomProperties.ShouldBe(originalAgent.Configuration.CustomProperties);
		}

		[Fact]
		public void Should_PreserveCapabilities_When_MappingToAndFromDto()
		{
			// Arrange
			var originalCapabilities = new AgentCapabilities
			{
				CanProcessNaturalLanguage = false,
				CanGenerateCode = true,
				CanAnalyzeData = false,
				CanCallExternalAPIs = true,
				MaxConcurrentTasks = 15,
				SupportedTaskTypes = { "Type1", "Type2", "Type3", "Type4", "Type5" }
			};

			// Act - Round trip mapping
			var dto = originalCapabilities.ToDto();
			var mappedBack = dto.ToDomain();

			// Assert - All properties preserved
			mappedBack.CanProcessNaturalLanguage.ShouldBe(originalCapabilities.CanProcessNaturalLanguage);
			mappedBack.CanGenerateCode.ShouldBe(originalCapabilities.CanGenerateCode);
			mappedBack.CanAnalyzeData.ShouldBe(originalCapabilities.CanAnalyzeData);
			mappedBack.CanCallExternalAPIs.ShouldBe(originalCapabilities.CanCallExternalAPIs);
			mappedBack.MaxConcurrentTasks.ShouldBe(originalCapabilities.MaxConcurrentTasks);
			mappedBack.SupportedTaskTypes.ShouldBe(originalCapabilities.SupportedTaskTypes);
		}
	}

	/// <summary>
	/// Test fixture for edge cases and boundary conditions in mapping
	/// </summary>
	public class EdgeCaseMappingTests
	{
		[Fact]
		public void Should_HandleNullCustomProperties_When_MappingConfiguration()
		{
			// Arrange
			var request = new UpdateAgentConfigurationRequest
			{
				TaskTimeoutSeconds = 300,
				MaxRetries = 3,
				Priority = 1,
				CustomProperties = null! // This might happen in real scenarios
			};

			// Act
			var configuration = request.ToDomain();

			// Assert
			configuration.ShouldNotBeNull();
			configuration.CustomProperties.ShouldNotBeNull();
			configuration.CustomProperties.ShouldBeEmpty();
		}

		[Fact]
		public void Should_HandleNullSupportedTaskTypes_When_MappingCapabilities()
		{
			// Arrange
			var dto = new AgentCapabilitiesDto
			{
				CanProcessNaturalLanguage = true,
				MaxConcurrentTasks = 1,
				SupportedTaskTypes = null! // This might happen in real scenarios
			};

			// Act
			var capabilities = dto.ToDomain();

			// Assert
			capabilities.ShouldNotBeNull();
			capabilities.SupportedTaskTypes.ShouldNotBeNull();
			capabilities.SupportedTaskTypes.ShouldBeEmpty();
		}

		[Fact]
		public void Should_HandleExtremeValues_When_MappingConfiguration()
		{
			// Arrange
			var configuration = new AgentConfiguration
			{
				TaskTimeoutSeconds = int.MaxValue,
				MaxRetries = int.MaxValue,
				Priority = int.MaxValue,
				CustomProperties = { ["maxValue"] = int.MaxValue, ["minValue"] = int.MinValue }
			};

			// Act
			var dto = configuration.ToDto();

			// Assert
			dto.TaskTimeoutSeconds.ShouldBe(int.MaxValue);
			dto.MaxRetries.ShouldBe(int.MaxValue);
			dto.Priority.ShouldBe(int.MaxValue);
			dto.CustomProperties["maxValue"].ShouldBe(int.MaxValue);
			dto.CustomProperties["minValue"].ShouldBe(int.MinValue);
		}

		[Fact]
		public void Should_HandleVeryLongTaskTypeNames_When_MappingCapabilities()
		{
			// Arrange
			var longTaskType = new string('A', 1000);
			var capabilities = new AgentCapabilities
			{
				SupportedTaskTypes = { longTaskType, "Normal", "AnotherNormal" }
			};

			// Act
			var dto = capabilities.ToDto();
			var mappedBack = dto.ToDomain();

			// Assert
			dto.SupportedTaskTypes.ShouldContain(longTaskType);
			mappedBack.SupportedTaskTypes.ShouldContain(longTaskType);
		}

		[Fact]
		public void Should_HandleManyTaskTypes_When_MappingCapabilities()
		{
			// Arrange
			var manyTaskTypes = Enumerable.Range(1, 1000)
				.Select(i => $"TaskType{i}")
				.ToList();

			var capabilities = new AgentCapabilities
			{
				SupportedTaskTypes = manyTaskTypes
			};

			// Act
			var dto = capabilities.ToDto();
			var mappedBack = dto.ToDomain();

			// Assert
			dto.SupportedTaskTypes.Count.ShouldBe(1000);
			mappedBack.SupportedTaskTypes.Count.ShouldBe(1000);
			dto.SupportedTaskTypes.ShouldBe(manyTaskTypes);
			mappedBack.SupportedTaskTypes.ShouldBe(manyTaskTypes);
		}
	}

	/// <summary>
	/// Test fixture for performance of mapping operations
	/// </summary>
	public class MappingPerformanceTests
	{
		[Fact]
		public void Should_MapQuickly_When_MappingManyAgents()
		{
			// Arrange
			var agents = Enumerable.Range(1, 10000)
				.Select(i => new Agent
				{
					Id = Guid.NewGuid(),
					Name = $"Performance Agent {i}",
					Description = $"Agent {i} for performance testing",
					Status = (AgentStatus)(i % 5),
					CreatedAt = DateTime.UtcNow.AddDays(-i),
					UpdatedAt = DateTime.UtcNow.AddHours(-i),
					Capabilities = new AgentCapabilities
					{
						MaxConcurrentTasks = i % 10 + 1,
						SupportedTaskTypes = { $"Type{i % 5}", $"Type{(i + 1) % 5}" }
					},
					Configuration = new AgentConfiguration
					{
						TaskTimeoutSeconds = 300 + i,
						MaxRetries = i % 5,
						Priority = i % 10 + 1,
						CustomProperties = { [$"prop{i}"] = $"value{i}" }
					}
				})
				.ToArray();

			// Act
			var stopwatch = System.Diagnostics.Stopwatch.StartNew();
			
			var responses = agents.Select(a => a.ToResponse()).ToArray();
			
			stopwatch.Stop();

			// Assert
			responses.Length.ShouldBe(10000);
			stopwatch.ElapsedMilliseconds.ShouldBeLessThan(1000); // Should complete under 1 second

			// Verify a few samples
			responses[0].Name.ShouldBe("Performance Agent 1");
			responses[9999].Name.ShouldBe("Performance Agent 10000");
			responses.All(r => r.Id != Guid.Empty).ShouldBeTrue();
		}
	}
} 