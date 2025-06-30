using ExxerAI.Application.Interfaces;
using ExxerAI.Application.Services;
using ExxerAI.Domain.ValueObjects;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ExxerAI.Application.Tests;

/// <summary>
/// Comprehensive tests for AggregatorCollectorInformationAgent
/// Testing autonomous business intelligence capabilities
/// </summary>
public class AggregatorCollectorInformationAgentTests
{
    private readonly ILLMProvider _mockLlmProvider;
    private readonly IMcpService _mockMcpService;
    private readonly IGoogleDriveService _mockGoogleDriveService;
    private readonly IVectorStore _mockVectorStore;
    private readonly IConversationMemory _mockConversationMemory;
    private readonly AggregatorCollectorInformationAgent _agent;

    public AggregatorCollectorInformationAgentTests()
    {
        _mockLlmProvider = Substitute.For<ILLMProvider>();
        _mockMcpService = Substitute.For<IMcpService>();
        _mockGoogleDriveService = Substitute.For<IGoogleDriveService>();
        _mockVectorStore = Substitute.For<IVectorStore>();
        _mockConversationMemory = Substitute.For<IConversationMemory>();

        _agent = new AggregatorCollectorInformationAgent(
            _mockLlmProvider,
            _mockMcpService,
            _mockGoogleDriveService,
            _mockVectorStore,
            _mockConversationMemory);
    }

    [Fact]
    public void Should_InitializeAgent_When_Created()
    {
        // Arrange & Act
        var agent = new AggregatorCollectorInformationAgent(
            _mockLlmProvider,
            _mockMcpService,
            _mockGoogleDriveService,
            _mockVectorStore,
            _mockConversationMemory);

        // Assert
        agent.AgentId.ShouldNotBeNullOrEmpty();
        agent.AgentType.ShouldBe("AggregatorCollectorInformation");
    }

    [Theory]
    [InlineData("collect information about Siemens", true)]
    [InlineData("aggregate market intelligence", true)]
    [InlineData("generate weekly report", true)]
    [InlineData("gather news updates", true)]
    [InlineData("hello world", false)]
    [InlineData("perform calculation", false)]
    public async Task Should_HandleRelevantTasks_When_ContextProvided(string input, bool expectedCanHandle)
    {
        // Arrange
        var context = new AgentContext { Input = input };

        // Act
        var canHandle = await _agent.CanHandleAsync(context);

        // Assert
        canHandle.ShouldBe(expectedCanHandle);
    }

    [Fact]
    public async Task Should_LearnUserPreferences_When_InteractionHistoryProvided()
    {
        // Arrange
        var userId = "test-user-001";
        var interactions = new List<UserInteraction>
        {
            new()
            {
                UserId = userId,
                Type = UserInteractionType.Query,
                Content = "Show me Siemens automation updates",
                SatisfactionScore = 4.5f,
                Topics = new List<string> { "Siemens", "Automation", "Technology" }
            },
            new()
            {
                UserId = userId,
                Type = UserInteractionType.Feedback,
                Content = "I want more automotive industry news",
                SatisfactionScore = 3.0f,
                Topics = new List<string> { "Automotive", "Industry" }
            }
        };

        _mockLlmProvider.GenerateAgentResponseAsync(Arg.Any<AgentContext>(), Arg.Any<CancellationToken>())
            .Returns(AgentResult.CreateSuccess("User shows high interest in Siemens and automotive content"));

        // Act
        var preferences = await _agent.LearnUserPreferencesAsync(userId, interactions);

        // Assert
        preferences.ShouldNotBeNull();
        preferences.UserId.ShouldBe(userId);
        preferences.CategoryPreferences.ShouldContainKey("Technology");
        preferences.CategoryPreferences.ShouldContainKey("Automotive");
        preferences.CompaniesOfInterest.ShouldContain(c => c.CompanyName == "Siemens");
    }

    [Fact]
    public async Task Should_CollectRelevantInformation_When_UserProfileProvided()
    {
        // Arrange
        var userProfile = CreateTestUserProfile();
        var timeRange = new DateTimeRange
        {
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow
        };

        var mockDriveDocuments = new List<DriveDocument>
        {
            new()
            {
                Id = "doc1",
                Name = "Siemens_Q4_Report.pdf",
                ModifiedTime = DateTime.UtcNow.AddDays(-2)
            }
        };

        _mockGoogleDriveService.ListDocumentsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(mockDriveDocuments);

        // Act
        var collection = await _agent.CollectRelevantInformationAsync(userProfile, timeRange);

        // Assert
        collection.ShouldNotBeNull();
        collection.Items.ShouldNotBeEmpty();
        collection.CollectionPeriod.ShouldBe(timeRange);
        await _mockGoogleDriveService.Received(1).ListDocumentsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_ExecuteAutonomousCycle_When_UserIdProvided()
    {
        // Arrange
        var userId = "test-user-autonomous";
        
        _mockLlmProvider.GenerateAgentResponseAsync(Arg.Any<AgentContext>(), Arg.Any<CancellationToken>())
            .Returns(AgentResult.CreateSuccess("Analysis complete"));

        _mockGoogleDriveService.ListDocumentsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new List<DriveDocument>());

        _mockVectorStore.IsHealthyAsync(Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _agent.ExecuteAutonomousCycleAsync(userId);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccessful.ShouldBeTrue();
        result.UserId.ShouldBe(userId);
        result.CycleEndTime.ShouldBeGreaterThan(result.CycleStartTime);
    }

    [Fact]
    public async Task Should_GeneratePersonalizedReport_When_IntelligenceProvided()
    {
        // Arrange
        var userId = "test-user-report";
        var intelligence = CreateTestAggregatedIntelligence();
        var template = new ReportTemplate
        {
            Name = "Executive Summary",
            Format = ReportFormat.ExecutiveSummary,
            MaxItems = 10
        };

        _mockLlmProvider.GenerateAgentResponseAsync(Arg.Any<AgentContext>(), Arg.Any<CancellationToken>())
            .Returns(AgentResult.CreateSuccess("Executive summary of key business intelligence"));

        // Act
        var report = await _agent.GenerateWeeklyReportAsync(userId, intelligence, template);

        // Assert
        report.ShouldNotBeNull();
        report.UserId.ShouldBe(userId);
        report.ExecutiveSummary.ShouldNotBeNullOrEmpty();
        report.PrioritizedCategories.ShouldNotBeEmpty();
        report.Metrics.ShouldNotBeNull();
    }

    [Fact]
    public async Task Should_ReturnHealthyStatus_When_ServicesAreOperational()
    {
        // Arrange
        _mockVectorStore.IsHealthyAsync(Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var status = await _agent.GetAgentStatusAsync();

        // Assert
        status.ShouldContain("Ready for autonomous operation");
        status.ShouldContain("✅");
    }

    [Fact]
    public async Task Should_HandleFailure_When_ServiceUnavailable()
    {
        // Arrange
        _mockLlmProvider.GenerateAgentResponseAsync(Arg.Any<AgentContext>(), Arg.Any<CancellationToken>())
            .Returns(AgentResult.CreateFailure("LLM service unavailable"));

        var context = new AgentContext 
        { 
            Input = "Generate intelligence report",
            UserId = "test-user"
        };

        // Act
        var result = await _agent.ExecuteAsync(context);

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccessful.ShouldBeFalse();
        result.ErrorMessage.ShouldContain("failed");
    }

    #region Test Data Helpers

    private UserPreferenceProfile CreateTestUserProfile()
    {
        return new UserPreferenceProfile
        {
            UserId = "test-user",
            BusinessContext = new UserBusinessContext
            {
                JobTitle = "Business Intelligence Manager",
                Industry = "Industrial Automation",
                Responsibilities = new List<string> { "Market Analysis", "Partner Relations" }
            },
            CategoryPreferences = new Dictionary<string, float>
            {
                ["Technology"] = 0.9f,
                ["Automotive"] = 0.8f,
                ["Industrial"] = 0.9f,
                ["Regulatory"] = 0.3f
            },
            CompaniesOfInterest = new List<CompanyInterest>
            {
                new() { CompanyName = "Siemens", Relationship = "Partner", ImportanceLevel = 0.9f },
                new() { CompanyName = "Rockwell", Relationship = "Partner", ImportanceLevel = 0.8f },
                new() { CompanyName = "General Motors", Relationship = "Client", ImportanceLevel = 0.7f }
            },
            PreferredSources = new Dictionary<string, float>
            {
                ["GoogleDrive"] = 0.9f,
                ["Web"] = 0.7f,
                ["HackerNews"] = 0.8f
            },
            LearningConfidence = 0.75f
        };
    }

    private AggregatedIntelligence CreateTestAggregatedIntelligence()
    {
        return new AggregatedIntelligence
        {
            Categories = new List<IntelligenceCategory>
            {
                new()
                {
                    CategoryName = "Technology Updates",
                    Items = new List<InformationItem>
                    {
                        new()
                        {
                            Title = "Siemens Launches New IoT Platform",
                            Content = "Siemens announces enhanced industrial IoT capabilities",
                            RelevanceScore = 0.9f,
                            RelatedCompanies = new List<string> { "Siemens" }
                        }
                    },
                    ItemCount = 1,
                    AverageRelevance = 0.9f
                }
            },
            TrendingTopics = new List<TrendingTopic>
            {
                new()
                {
                    TopicName = "Industrial AI",
                    TrendingScore = 0.8f,
                    MentionCount = 15,
                    Direction = TrendDirection.Rising
                }
            },
            KeyInsights = new List<IntelligenceInsight>
            {
                new()
                {
                    Title = "Automation Investment Trend",
                    Description = "Automotive sector increasing automation investments",
                    Type = InsightType.Observation,
                    ConfidenceLevel = 0.8f
                }
            },
            ActionableItems = new List<ActionableItem>
            {
                new()
                {
                    Title = "Follow up on Siemens partnership",
                    Description = "Reach out to discuss new IoT platform opportunities",
                    Priority = ActionPriority.High
                }
            },
            CompanyMentions = new Dictionary<string, int>
            {
                ["Siemens"] = 25,
                ["Rockwell"] = 18,
                ["General Motors"] = 12
            }
        };
    }

    #endregion
} 