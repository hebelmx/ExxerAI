using Shouldly;
using Xunit;

namespace ExxerAI.Domain.Tests;

/// <summary>
/// Comprehensive tests for LLM Integration domain classes
/// Targets: LanguageModel, ModelCapabilities, ModelConfiguration, Conversation,
/// ConversationMessage, ConversationMetadata, MessageMetadata, and enums
/// </summary>
public class LLMIntegrationTests
{
    public class LanguageModelTests
    {
        [Fact]
        public void Constructor_Should_InitializeWithDefaults()
        {
            // Act
            var model = new LanguageModel();

            // Assert
            model.Id.ShouldNotBe(Guid.Empty);
            model.Name.ShouldBe(string.Empty);
            model.Provider.ShouldBe(string.Empty);
            model.Version.ShouldBe(string.Empty);
            model.Capabilities.ShouldNotBeNull();
            model.Capabilities.ShouldBeOfType<ModelCapabilities>();
            model.Configuration.ShouldNotBeNull();
            model.Configuration.ShouldBeOfType<ModelConfiguration>();
            model.IsAvailable.ShouldBeTrue();
            model.ContextWindowSize.ShouldBe(4096);
            model.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
            model.UpdatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        }

        [Fact]
        public void Properties_Should_SetAndGetCorrectly()
        {
            // Arrange
            var model = new LanguageModel();
            var testId = Guid.NewGuid();
            var testName = "GPT-4";
            var testProvider = "OpenAI";
            var testVersion = "4.0";
            var testCapabilities = new ModelCapabilities();
            var testConfiguration = new ModelConfiguration();
            var testContextWindow = 8192;
            var testCreatedAt = DateTime.UtcNow.AddDays(-1);
            var testUpdatedAt = DateTime.UtcNow.AddHours(-1);

            // Act & Assert
            model.Id = testId;
            model.Id.ShouldBe(testId);

            model.Name = testName;
            model.Name.ShouldBe(testName);

            model.Provider = testProvider;
            model.Provider.ShouldBe(testProvider);

            model.Version = testVersion;
            model.Version.ShouldBe(testVersion);

            model.Capabilities = testCapabilities;
            model.Capabilities.ShouldBe(testCapabilities);

            model.Configuration = testConfiguration;
            model.Configuration.ShouldBe(testConfiguration);

            model.IsAvailable = false;
            model.IsAvailable.ShouldBeFalse();

            model.ContextWindowSize = testContextWindow;
            model.ContextWindowSize.ShouldBe(testContextWindow);

            model.CreatedAt = testCreatedAt;
            model.CreatedAt.ShouldBe(testCreatedAt);

            model.UpdatedAt = testUpdatedAt;
            model.UpdatedAt.ShouldBe(testUpdatedAt);
        }

        [Theory]
        [InlineData(512)]
        [InlineData(4096)]
        [InlineData(8192)]
        [InlineData(32768)]
        [InlineData(128000)]
        public void ContextWindowSize_Should_AcceptValidValues(int contextWindow)
        {
            // Arrange
            var model = new LanguageModel();

            // Act
            model.ContextWindowSize = contextWindow;

            // Assert
            model.ContextWindowSize.ShouldBe(contextWindow);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsAvailable_Should_AcceptBooleanValues(bool isAvailable)
        {
            // Arrange
            var model = new LanguageModel();

            // Act
            model.IsAvailable = isAvailable;

            // Assert
            model.IsAvailable.ShouldBe(isAvailable);
        }
    }

    public class ModelCapabilitiesTests
    {
        [Fact]
        public void Constructor_Should_InitializeWithDefaults()
        {
            // Act
            var capabilities = new ModelCapabilities();

            // Assert
            capabilities.SupportsTextGeneration.ShouldBeTrue();
            capabilities.SupportsCodeGeneration.ShouldBeFalse();
            capabilities.SupportsImageAnalysis.ShouldBeFalse();
            capabilities.SupportsFunctionCalling.ShouldBeFalse();
            capabilities.SupportsStreaming.ShouldBeTrue();
            capabilities.MaxOutputTokens.ShouldBe(2048);
            capabilities.SupportedFormats.ShouldNotBeNull();
            capabilities.SupportedFormats.Count.ShouldBe(1);
            capabilities.SupportedFormats.ShouldContain("text");
        }

        [Fact]
        public void Properties_Should_SetAndGetCorrectly()
        {
            // Arrange
            var capabilities = new ModelCapabilities();
            var testMaxTokens = 4096;

            // Act & Assert
            capabilities.SupportsTextGeneration = false;
            capabilities.SupportsTextGeneration.ShouldBeFalse();

            capabilities.SupportsCodeGeneration = true;
            capabilities.SupportsCodeGeneration.ShouldBeTrue();

            capabilities.SupportsImageAnalysis = true;
            capabilities.SupportsImageAnalysis.ShouldBeTrue();

            capabilities.SupportsFunctionCalling = true;
            capabilities.SupportsFunctionCalling.ShouldBeTrue();

            capabilities.SupportsStreaming = false;
            capabilities.SupportsStreaming.ShouldBeFalse();

            capabilities.MaxOutputTokens = testMaxTokens;
            capabilities.MaxOutputTokens.ShouldBe(testMaxTokens);
        }

        [Fact]
        public void SupportedFormats_Should_BeModifiableCollection()
        {
            // Arrange
            var capabilities = new ModelCapabilities();

            // Act
            capabilities.SupportedFormats.Add("json");
            capabilities.SupportedFormats.Add("markdown");

            // Assert
            capabilities.SupportedFormats.Count.ShouldBe(3); // "text" + 2 new ones
            capabilities.SupportedFormats.ShouldContain("text");
            capabilities.SupportedFormats.ShouldContain("json");
            capabilities.SupportedFormats.ShouldContain("markdown");
        }

        [Theory]
        [InlineData(512)]
        [InlineData(1024)]
        [InlineData(2048)]
        [InlineData(4096)]
        [InlineData(8192)]
        public void MaxOutputTokens_Should_AcceptValidValues(int maxTokens)
        {
            // Arrange
            var capabilities = new ModelCapabilities();

            // Act
            capabilities.MaxOutputTokens = maxTokens;

            // Assert
            capabilities.MaxOutputTokens.ShouldBe(maxTokens);
        }
    }

    public class ModelConfigurationTests
    {
        [Fact]
        public void Constructor_Should_InitializeWithDefaults()
        {
            // Act
            var configuration = new ModelConfiguration();

            // Assert
            configuration.EndpointUrl.ShouldBe(string.Empty);
            configuration.ApiKey.ShouldBe(string.Empty);
            configuration.DefaultTemperature.ShouldBe(0.7);
            configuration.MaxTokensPerRequest.ShouldBe(1000);
            configuration.RequestTimeoutSeconds.ShouldBe(30);
            configuration.RateLimitPerMinute.ShouldBe(60);
            configuration.CustomProperties.ShouldNotBeNull();
            configuration.CustomProperties.ShouldBeEmpty();
        }

        [Fact]
        public void Properties_Should_SetAndGetCorrectly()
        {
            // Arrange
            var configuration = new ModelConfiguration();
            var testEndpoint = "https://api.openai.com/v1";
            var testApiKey = "test-api-key";
            var testTemperature = 0.5;
            var testMaxTokens = 2000;
            var testTimeout = 60;
            var testRateLimit = 100;

            // Act & Assert
            configuration.EndpointUrl = testEndpoint;
            configuration.EndpointUrl.ShouldBe(testEndpoint);

            configuration.ApiKey = testApiKey;
            configuration.ApiKey.ShouldBe(testApiKey);

            configuration.DefaultTemperature = testTemperature;
            configuration.DefaultTemperature.ShouldBe(testTemperature);

            configuration.MaxTokensPerRequest = testMaxTokens;
            configuration.MaxTokensPerRequest.ShouldBe(testMaxTokens);

            configuration.RequestTimeoutSeconds = testTimeout;
            configuration.RequestTimeoutSeconds.ShouldBe(testTimeout);

            configuration.RateLimitPerMinute = testRateLimit;
            configuration.RateLimitPerMinute.ShouldBe(testRateLimit);
        }

        [Fact]
        public void CustomProperties_Should_BeModifiableDictionary()
        {
            // Arrange
            var configuration = new ModelConfiguration();

            // Act
            configuration.CustomProperties["organization"] = "test-org";
            configuration.CustomProperties["region"] = "us-west-2";

            // Assert
            configuration.CustomProperties.Count.ShouldBe(2);
            configuration.CustomProperties["organization"].ShouldBe("test-org");
            configuration.CustomProperties["region"].ShouldBe("us-west-2");
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(0.3)]
        [InlineData(0.7)]
        [InlineData(1.0)]
        [InlineData(1.5)]
        public void DefaultTemperature_Should_AcceptValidValues(double temperature)
        {
            // Arrange
            var configuration = new ModelConfiguration();

            // Act
            configuration.DefaultTemperature = temperature;

            // Assert
            configuration.DefaultTemperature.ShouldBe(temperature);
        }
    }

    public class ConversationTests
    {
        [Fact]
        public void Constructor_Should_InitializeWithDefaults()
        {
            // Act
            var conversation = new Conversation();

            // Assert
            conversation.Id.ShouldNotBe(Guid.Empty);
            conversation.Title.ShouldBe(string.Empty);
            conversation.SystemPrompt.ShouldBe(string.Empty);
            conversation.AgentId.ShouldBe(Guid.Empty);
            conversation.Agent.ShouldBeNull();
            conversation.LanguageModelId.ShouldBe(Guid.Empty);
            conversation.LanguageModel.ShouldBeNull();
            conversation.Status.ShouldBe(ConversationStatus.Active);
            conversation.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
            conversation.UpdatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
            conversation.Messages.ShouldNotBeNull();
            conversation.Messages.ShouldBeEmpty();
            conversation.Metadata.ShouldNotBeNull();
            conversation.Metadata.ShouldBeOfType<ConversationMetadata>();
        }

        [Fact]
        public void Properties_Should_SetAndGetCorrectly()
        {
            // Arrange
            var conversation = new Conversation();
            var testId = Guid.NewGuid();
            var testTitle = "Test Conversation";
            var testSystemPrompt = "You are a helpful assistant";
            var testAgentId = Guid.NewGuid();
            var testAgent = new Agent();
            var testLanguageModelId = Guid.NewGuid();
            var testLanguageModel = new LanguageModel();
            var testStatus = ConversationStatus.Paused;
            var testCreatedAt = DateTime.UtcNow.AddDays(-1);
            var testUpdatedAt = DateTime.UtcNow.AddHours(-1);
            var testMetadata = new ConversationMetadata();

            // Act & Assert
            conversation.Id = testId;
            conversation.Id.ShouldBe(testId);

            conversation.Title = testTitle;
            conversation.Title.ShouldBe(testTitle);

            conversation.SystemPrompt = testSystemPrompt;
            conversation.SystemPrompt.ShouldBe(testSystemPrompt);

            conversation.AgentId = testAgentId;
            conversation.AgentId.ShouldBe(testAgentId);

            conversation.Agent = testAgent;
            conversation.Agent.ShouldBe(testAgent);

            conversation.LanguageModelId = testLanguageModelId;
            conversation.LanguageModelId.ShouldBe(testLanguageModelId);

            conversation.LanguageModel = testLanguageModel;
            conversation.LanguageModel.ShouldBe(testLanguageModel);

            conversation.Status = testStatus;
            conversation.Status.ShouldBe(testStatus);

            conversation.CreatedAt = testCreatedAt;
            conversation.CreatedAt.ShouldBe(testCreatedAt);

            conversation.UpdatedAt = testUpdatedAt;
            conversation.UpdatedAt.ShouldBe(testUpdatedAt);

            conversation.Metadata = testMetadata;
            conversation.Metadata.ShouldBe(testMetadata);
        }

        [Theory]
        [InlineData(ConversationStatus.Active)]
        [InlineData(ConversationStatus.Paused)]
        [InlineData(ConversationStatus.Completed)]
        [InlineData(ConversationStatus.Archived)]
        public void Status_Should_AcceptAllValidValues(ConversationStatus status)
        {
            // Arrange
            var conversation = new Conversation();

            // Act
            conversation.Status = status;

            // Assert
            conversation.Status.ShouldBe(status);
        }

        [Fact]
        public void Messages_Should_BeModifiableCollection()
        {
            // Arrange
            var conversation = new Conversation();
            var message1 = new ConversationMessage { Content = "Hello" };
            var message2 = new ConversationMessage { Content = "Hi there" };

            // Act
            conversation.Messages.Add(message1);
            conversation.Messages.Add(message2);

            // Assert
            conversation.Messages.Count.ShouldBe(2);
            conversation.Messages.ShouldContain(message1);
            conversation.Messages.ShouldContain(message2);
        }
    }

    public class ConversationMessageTests
    {
        [Fact]
        public void Constructor_Should_InitializeWithDefaults()
        {
            // Act
            var message = new ConversationMessage();

            // Assert
            message.Id.ShouldNotBe(Guid.Empty);
            message.ConversationId.ShouldBe(Guid.Empty);
            message.Conversation.ShouldBeNull();
            message.Role.ShouldBe(MessageRole.System); // Default enum value
            message.Content.ShouldBe(string.Empty);
            message.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
            message.TokenCount.ShouldBe(0);
            message.Metadata.ShouldNotBeNull();
            message.Metadata.ShouldBeOfType<MessageMetadata>();
        }

        [Fact]
        public void Properties_Should_SetAndGetCorrectly()
        {
            // Arrange
            var message = new ConversationMessage();
            var testId = Guid.NewGuid();
            var testConversationId = Guid.NewGuid();
            var testConversation = new Conversation();
            var testRole = MessageRole.User;
            var testContent = "Hello, how are you?";
            var testCreatedAt = DateTime.UtcNow.AddHours(-1);
            var testTokenCount = 15;
            var testMetadata = new MessageMetadata();

            // Act & Assert
            message.Id = testId;
            message.Id.ShouldBe(testId);

            message.ConversationId = testConversationId;
            message.ConversationId.ShouldBe(testConversationId);

            message.Conversation = testConversation;
            message.Conversation.ShouldBe(testConversation);

            message.Role = testRole;
            message.Role.ShouldBe(testRole);

            message.Content = testContent;
            message.Content.ShouldBe(testContent);

            message.CreatedAt = testCreatedAt;
            message.CreatedAt.ShouldBe(testCreatedAt);

            message.TokenCount = testTokenCount;
            message.TokenCount.ShouldBe(testTokenCount);

            message.Metadata = testMetadata;
            message.Metadata.ShouldBe(testMetadata);
        }

        [Fact]
        public void Timestamp_Should_AliasCreatedAt()
        {
            // Arrange
            var message = new ConversationMessage();
            var testTimestamp = DateTime.UtcNow.AddHours(-2);

            // Act
            message.Timestamp = testTimestamp;

            // Assert
            message.Timestamp.ShouldBe(testTimestamp);
            message.CreatedAt.ShouldBe(testTimestamp);

            // Test reverse
            var newTimestamp = DateTime.UtcNow.AddHours(-1);
            message.CreatedAt = newTimestamp;
            message.Timestamp.ShouldBe(newTimestamp);
        }

        [Theory]
        [InlineData(MessageRole.System)]
        [InlineData(MessageRole.User)]
        [InlineData(MessageRole.Assistant)]
        [InlineData(MessageRole.Function)]
        public void Role_Should_AcceptAllValidValues(MessageRole role)
        {
            // Arrange
            var message = new ConversationMessage();

            // Act
            message.Role = role;

            // Assert
            message.Role.ShouldBe(role);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(4096)]
        public void TokenCount_Should_AcceptValidValues(int tokenCount)
        {
            // Arrange
            var message = new ConversationMessage();

            // Act
            message.TokenCount = tokenCount;

            // Assert
            message.TokenCount.ShouldBe(tokenCount);
        }
    }

    public class ConversationMetadataTests
    {
        [Fact]
        public void Constructor_Should_InitializeWithDefaults()
        {
            // Act
            var metadata = new ConversationMetadata();

            // Assert
            metadata.TotalTokens.ShouldBe(0);
            metadata.EstimatedCost.ShouldBe(0);
            metadata.Properties.ShouldNotBeNull();
            metadata.Properties.ShouldBeEmpty();
        }

        [Fact]
        public void Properties_Should_SetAndGetCorrectly()
        {
            // Arrange
            var metadata = new ConversationMetadata();
            var testTotalTokens = 1500;
            var testEstimatedCost = 0.05m;

            // Act & Assert
            metadata.TotalTokens = testTotalTokens;
            metadata.TotalTokens.ShouldBe(testTotalTokens);

            metadata.EstimatedCost = testEstimatedCost;
            metadata.EstimatedCost.ShouldBe(testEstimatedCost);
        }

        [Fact]
        public void Properties_Should_BeModifiableDictionary()
        {
            // Arrange
            var metadata = new ConversationMetadata();

            // Act
            metadata.Properties["model_version"] = "4.0";
            metadata.Properties["temperature"] = 0.7;

            // Assert
            metadata.Properties.Count.ShouldBe(2);
            metadata.Properties["model_version"].ShouldBe("4.0");
            metadata.Properties["temperature"].ShouldBe(0.7);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(1000)]
        [InlineData(10000)]
        public void TotalTokens_Should_AcceptValidValues(int totalTokens)
        {
            // Arrange
            var metadata = new ConversationMetadata();

            // Act
            metadata.TotalTokens = totalTokens;

            // Assert
            metadata.TotalTokens.ShouldBe(totalTokens);
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(0.01)]
        [InlineData(1.50)]
        [InlineData(100.75)]
        public void EstimatedCost_Should_AcceptValidValues(decimal estimatedCost)
        {
            // Arrange
            var metadata = new ConversationMetadata();

            // Act
            metadata.EstimatedCost = estimatedCost;

            // Assert
            metadata.EstimatedCost.ShouldBe(estimatedCost);
        }
    }

    public class MessageMetadataTests
    {
        [Fact]
        public void Constructor_Should_InitializeWithDefaults()
        {
            // Act
            var metadata = new MessageMetadata();

            // Assert
            metadata.ResponseTimeMs.ShouldBe(0);
            metadata.ModelParameters.ShouldNotBeNull();
            metadata.ModelParameters.ShouldBeEmpty();
            metadata.Properties.ShouldNotBeNull();
            metadata.Properties.ShouldBeEmpty();
        }

        [Fact]
        public void Properties_Should_SetAndGetCorrectly()
        {
            // Arrange
            var metadata = new MessageMetadata();
            var testResponseTime = 250;

            // Act & Assert
            metadata.ResponseTimeMs = testResponseTime;
            metadata.ResponseTimeMs.ShouldBe(testResponseTime);
        }

        [Fact]
        public void ModelParameters_Should_BeModifiableDictionary()
        {
            // Arrange
            var metadata = new MessageMetadata();

            // Act
            metadata.ModelParameters["temperature"] = 0.8;
            metadata.ModelParameters["max_tokens"] = 150;

            // Assert
            metadata.ModelParameters.Count.ShouldBe(2);
            metadata.ModelParameters["temperature"].ShouldBe(0.8);
            metadata.ModelParameters["max_tokens"].ShouldBe(150);
        }

        [Fact]
        public void Properties_Should_BeModifiableDictionary()
        {
            // Arrange
            var metadata = new MessageMetadata();

            // Act
            metadata.Properties["user_id"] = "user123";
            metadata.Properties["session_id"] = "session456";

            // Assert
            metadata.Properties.Count.ShouldBe(2);
            metadata.Properties["user_id"].ShouldBe("user123");
            metadata.Properties["session_id"].ShouldBe("session456");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(50)]
        [InlineData(250)]
        [InlineData(1000)]
        [InlineData(5000)]
        public void ResponseTimeMs_Should_AcceptValidValues(int responseTime)
        {
            // Arrange
            var metadata = new MessageMetadata();

            // Act
            metadata.ResponseTimeMs = responseTime;

            // Assert
            metadata.ResponseTimeMs.ShouldBe(responseTime);
        }
    }

    public class ConversationStatusTests
    {
        [Theory]
        [InlineData(ConversationStatus.Active)]
        [InlineData(ConversationStatus.Paused)]
        [InlineData(ConversationStatus.Completed)]
        [InlineData(ConversationStatus.Archived)]
        public void ConversationStatus_Should_HaveAllExpectedValues(ConversationStatus status)
        {
            // Assert
            Enum.IsDefined(typeof(ConversationStatus), status).ShouldBeTrue();
        }

        [Fact]
        public void ConversationStatus_Should_HaveCorrectCount()
        {
            // Act
            var statusCount = Enum.GetValues<ConversationStatus>().Length;

            // Assert
            statusCount.ShouldBe(4);
        }
    }

    public class MessageRoleTests
    {
        [Theory]
        [InlineData(MessageRole.System)]
        [InlineData(MessageRole.User)]
        [InlineData(MessageRole.Assistant)]
        [InlineData(MessageRole.Function)]
        public void MessageRole_Should_HaveAllExpectedValues(MessageRole role)
        {
            // Assert
            Enum.IsDefined(typeof(MessageRole), role).ShouldBeTrue();
        }

        [Fact]
        public void MessageRole_Should_HaveCorrectCount()
        {
            // Act
            var roleCount = Enum.GetValues<MessageRole>().Length;

            // Assert
            roleCount.ShouldBe(4);
        }
    }
}