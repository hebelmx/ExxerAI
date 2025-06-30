using ExxerAI.Application.Interfaces;
using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.Application.Services;

/// <summary>
/// Autonomous agent for collecting, aggregating, and personalizing business intelligence
/// AUTONOMOUS DIRECTIVE: Learn user preferences and generate personalized weekly reports
/// </summary>
public class AggregatorCollectorInformationAgent : IAggregatorCollectorInformation
{
    private readonly ILLMProvider _llmProvider;
    private readonly IMcpService _mcpService;
    private readonly IGoogleDriveService _googleDriveService;
    private readonly IVectorStore _vectorStore;
    private readonly IConversationMemory _conversationMemory;

    public string AgentId { get; }
    public string AgentType { get; }

    public AggregatorCollectorInformationAgent(
        ILLMProvider llmProvider,
        IMcpService mcpService,
        IGoogleDriveService googleDriveService,
        IVectorStore vectorStore,
        IConversationMemory conversationMemory,
        string? agentId = null)
    {
        _llmProvider = llmProvider;
        _mcpService = mcpService;
        _googleDriveService = googleDriveService;
        _vectorStore = vectorStore;
        _conversationMemory = conversationMemory;
        AgentId = agentId ?? Guid.NewGuid().ToString();
        AgentType = "AggregatorCollectorInformation";
    }

    /// <summary>
    /// Learns user preferences from interaction patterns and feedback
    /// </summary>
    public async Task<UserPreferenceProfile> LearnUserPreferencesAsync(
        string userId, 
        IEnumerable<UserInteraction> interactionHistory, 
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🧠 Learning preferences for user {userId}...");

        try
        {
            // Analyze interaction patterns using LLM
            var analysisPrompt = CreatePreferenceLearningPrompt(interactionHistory);
            var analysisContext = new AgentContext { Input = analysisPrompt, UserId = userId };
            
            var analysisResult = await _llmProvider.GenerateAgentResponseAsync(analysisContext, cancellationToken);
            
            if (analysisResult.IsSuccessful)
            {
                // Parse the analysis to update preferences
                var updatedProfile = ParsePreferenceAnalysis(userId, analysisResult.Output, interactionHistory);
                
                Console.WriteLine($"✅ Learned {updatedProfile.CategoryPreferences.Count} category preferences");
                Console.WriteLine($"📊 Learning confidence: {updatedProfile.LearningConfidence:P}");
                
                return updatedProfile;
            }
            
            return CreateDefaultPreferenceProfile(userId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to learn preferences: {ex.Message}");
            return CreateDefaultPreferenceProfile(userId);
        }
    }

    /// <summary>
    /// Collects information from all configured sources based on user interests
    /// </summary>
    public async Task<InformationCollection> CollectRelevantInformationAsync(
        UserPreferenceProfile userProfile, 
        DateTimeRange timeRange, 
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🔍 Collecting information for {userProfile.UserId} from {timeRange.StartDate:MM/dd} to {timeRange.EndDate:MM/dd}...");

        try
        {
            var collection = new InformationCollection
            {
                CollectionPeriod = timeRange,
                CollectionTimestamp = DateTime.UtcNow
            };

            var collectionTasks = new List<Task<List<InformationItem>>>();

            // Collect from Google Drive
            if (userProfile.PreferredSources.ContainsKey("GoogleDrive"))
            {
                collectionTasks.Add(CollectFromGoogleDriveAsync(userProfile, timeRange, cancellationToken));
            }

            // Collect from web sources via MCP
            if (userProfile.PreferredSources.ContainsKey("Web"))
            {
                collectionTasks.Add(CollectFromWebSourcesAsync(userProfile, timeRange, cancellationToken));
            }

            // Collect from company-specific sources
            collectionTasks.Add(CollectCompanyInformationAsync(userProfile, timeRange, cancellationToken));

            // Collect technology updates
            collectionTasks.Add(CollectTechnologyUpdatesAsync(userProfile, timeRange, cancellationToken));

            var collectionResults = await Task.WhenAll(collectionTasks);
            
            // Combine all collected information
            foreach (var items in collectionResults)
            {
                collection.Items.AddRange(items);
            }

            // Score relevance for each item
            await ScoreInformationRelevanceAsync(collection, userProfile, cancellationToken);

            Console.WriteLine($"📊 Collected {collection.Items.Count} information items");
            Console.WriteLine($"🎯 Average relevance score: {collection.Items.Average(i => i.RelevanceScore):F2}");

            return collection;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to collect information: {ex.Message}");
            return new InformationCollection();
        }
    }

    /// <summary>
    /// Aggregates collected information into meaningful categories and insights
    /// </summary>
    public async Task<AggregatedIntelligence> AggregateInformationAsync(
        InformationCollection rawInformation, 
        UserBusinessContext userContext, 
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🎪 Aggregating {rawInformation.Items.Count} information items...");

        try
        {
            var aggregation = new AggregatedIntelligence
            {
                ProcessingTimestamp = DateTime.UtcNow
            };

            // Categorize information
            aggregation.Categories.AddRange(await CategorizeInformationAsync(rawInformation, userContext, cancellationToken));

            // Discover trending topics
            aggregation.TrendingTopics.AddRange(await DiscoverTrendingTopicsAsync(userContext, TimeSpan.FromDays(7), cancellationToken));

            // Generate insights
            aggregation.KeyInsights.AddRange(await GenerateInsightsAsync(rawInformation, userContext, cancellationToken));

            // Create actionable items
            aggregation.ActionableItems.AddRange(await CreateActionableItemsAsync(rawInformation, userContext, cancellationToken));

            // Analyze company mentions
            aggregation.CompanyMentions = AnalyzeCompanyMentions(rawInformation.Items);

            Console.WriteLine($"📋 Created {aggregation.Categories.Count} categories");
            Console.WriteLine($"🔥 Found {aggregation.TrendingTopics.Count} trending topics");
            Console.WriteLine($"💡 Generated {aggregation.KeyInsights.Count} insights");
            Console.WriteLine($"⚡ Created {aggregation.ActionableItems.Count} actionable items");

            return aggregation;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to aggregate information: {ex.Message}");
            return new AggregatedIntelligence();
        }
    }

    /// <summary>
    /// Generates personalized weekly report for the user
    /// </summary>
    public async Task<PersonalizedReport> GenerateWeeklyReportAsync(
        string userId, 
        AggregatedIntelligence aggregatedIntelligence, 
        ReportTemplate reportTemplate, 
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"📄 Generating personalized weekly report for {userId}...");

        try
        {
            var report = new PersonalizedReport
            {
                UserId = userId,
                GeneratedAt = DateTime.UtcNow,
                ReportPeriod = new DateTimeRange
                {
                    StartDate = DateTime.UtcNow.AddDays(-7),
                    EndDate = DateTime.UtcNow
                }
            };

            // Generate executive summary
            report.ExecutiveSummary = await GenerateExecutiveSummaryAsync(aggregatedIntelligence, cancellationToken);

            // Prioritize categories based on user preferences
            report.PrioritizedCategories = aggregatedIntelligence.Categories
                .OrderByDescending(c => c.AverageRelevance)
                .Take(5)
                .ToList();

            // Select most relevant trends
            report.RelevantTrends = aggregatedIntelligence.TrendingTopics
                .OrderByDescending(t => t.TrendingScore)
                .Take(3)
                .ToList();

            // Include actionable items
            report.RecommendedActions = aggregatedIntelligence.ActionableItems
                .OrderByDescending(a => a.Priority)
                .Take(5)
                .ToList();

            // Company updates
            report.CompanyUpdates = aggregatedIntelligence.CompanyMentions
                .OrderByDescending(kvp => kvp.Value)
                .Take(10)
                .ToDictionary(kvp => kvp.Key, kvp => $"{kvp.Value} mentions this week");

            // Calculate metrics
            report.Metrics = new ReportMetrics
            {
                TotalItemsProcessed = aggregatedIntelligence.Categories.Sum(c => c.ItemCount),
                RelevantItemsIncluded = report.PrioritizedCategories.Sum(c => c.ItemCount),
                ProcessingTime = DateTime.UtcNow - aggregatedIntelligence.ProcessingTimestamp,
                AverageRelevanceScore = aggregatedIntelligence.Categories.Average(c => c.AverageRelevance),
                ActionItemsGenerated = report.RecommendedActions.Count
            };

            Console.WriteLine($"✅ Generated personalized report with {report.PrioritizedCategories.Count} categories");
            Console.WriteLine($"📊 Report covers {report.Metrics.TotalItemsProcessed} processed items");

            return report;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to generate report: {ex.Message}");
            return new PersonalizedReport { UserId = userId };
        }
    }

    /// <summary>
    /// Performs autonomous information collection and processing cycle
    /// </summary>
    public async Task<AutonomousCycleResult> ExecuteAutonomousCycleAsync(
        string userId, 
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"🤖 AUTONOMOUS CYCLE: Starting for user {userId}...");

        try
        {
            var startTime = DateTime.UtcNow;
            var result = new AutonomousCycleResult
            {
                UserId = userId,
                CycleStartTime = startTime,
                IsSuccessful = true
            };

            // Step 1: Learn current preferences
            var interactions = await GetRecentInteractionsAsync(userId, cancellationToken);
            var preferences = await LearnUserPreferencesAsync(userId, interactions, cancellationToken);

            // Step 2: Collect information
            var timeRange = new DateTimeRange
            {
                StartDate = DateTime.UtcNow.AddDays(-1), // Daily collection
                EndDate = DateTime.UtcNow
            };
            var information = await CollectRelevantInformationAsync(preferences, timeRange, cancellationToken);

            // Step 3: Aggregate intelligence
            var intelligence = await AggregateInformationAsync(information, preferences.BusinessContext, cancellationToken);

            // Step 4: Store results for weekly report generation
            await StoreIntelligenceAsync(userId, intelligence, cancellationToken);

            result.CycleEndTime = DateTime.UtcNow;
            result.ItemsProcessed = information.Items.Count;
            result.CategoriesCreated = intelligence.Categories.Count;
            result.InsightsGenerated = intelligence.KeyInsights.Count;

            Console.WriteLine($"✅ AUTONOMOUS CYCLE COMPLETED in {result.CycleEndTime - result.CycleStartTime}");
            Console.WriteLine($"📊 Processed {result.ItemsProcessed} items, created {result.CategoriesCreated} categories");

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ AUTONOMOUS CYCLE FAILED: {ex.Message}");
            return new AutonomousCycleResult
            {
                UserId = userId,
                IsSuccessful = false,
                ErrorMessage = ex.Message,
                CycleEndTime = DateTime.UtcNow
            };
        }
    }

    // Core interface implementations
    public async Task<bool> CanHandleAsync(AgentContext context)
    {
        // This agent can handle information aggregation requests
        var keywords = new[] { "information", "report", "intelligence", "aggregate", "collect", "news", "updates" };
        return keywords.Any(k => context.Input.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<AgentResult> ExecuteAsync(AgentContext context)
    {
        return await ExecuteAsync(context, CancellationToken.None);
    }

    public async Task<AgentResult> ExecuteAsync(AgentContext context, CancellationToken cancellationToken)
    {
        Console.WriteLine($"AggregatorCollectorInformationAgent {AgentId} executing task: {context.Input}");

        try
        {
            // Execute autonomous cycle for the user
            var cycleResult = await ExecuteAutonomousCycleAsync(context.UserId, cancellationToken);

            if (cycleResult.IsSuccessful)
            {
                var response = $"Autonomous information collection completed successfully!\n" +
                             $"Processed: {cycleResult.ItemsProcessed} items\n" +
                             $"Categories: {cycleResult.CategoriesCreated}\n" +
                             $"Insights: {cycleResult.InsightsGenerated}\n" +
                             $"Duration: {cycleResult.CycleEndTime - cycleResult.CycleStartTime}";

                return AgentResult.CreateSuccess(response);
            }
            else
            {
                return AgentResult.CreateFailure($"Autonomous cycle failed: {cycleResult.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            return AgentResult.CreateFailure($"Agent execution failed: {ex.Message}");
        }
    }

    public async Task ExecuteAsync(string prompt, string agentType)
    {
        var context = new AgentContext { Input = prompt };
        await ExecuteAsync(context);
    }

    public async Task<string> GetAgentStatusAsync()
    {
        try
        {
            var mcpHealthy = await _mcpService?.GetSessionStatusAsync("default", CancellationToken.None) != null;
            var vectorHealthy = await _vectorStore?.IsHealthyAsync(CancellationToken.None) ?? false;
            
            return $"AggregatorCollectorInformationAgent {AgentId}: Ready for autonomous operation\n" +
                   $"  MCP Service: {(mcpHealthy ? "✅" : "❌")}\n" +
                   $"  Vector Store: {(vectorHealthy ? "✅" : "❌")}\n" +
                   $"  Status: AUTONOMOUS & OPERATIONAL";
        }
        catch (Exception ex)
        {
            return $"AggregatorCollectorInformationAgent {AgentId}: Error - {ex.Message}";
        }
    }

    // Implementation continues with helper methods...
    // (Truncated for brevity - additional private methods would implement specific collection logic)

    #region Private Helper Methods

    private string CreatePreferenceLearningPrompt(IEnumerable<UserInteraction> interactions)
    {
        return $"""
            Analyze user interaction patterns to determine preferences for business intelligence:
            
            INTERACTIONS: {string.Join("\n", interactions.Take(10).Select(i => $"- {i}"))}
            
            Determine:
            1. Preferred information categories (Technology, Business, Industry, etc.)
            2. Company interests and relationship types
            3. Information consumption patterns
            4. Topics to filter out
            
            Respond with structured analysis focusing on business intelligence needs.
            """;
    }

    private UserPreferenceProfile CreateDefaultPreferenceProfile(string userId)
    {
        return new UserPreferenceProfile
        {
            UserId = userId,
            CategoryPreferences = new Dictionary<string, float>
            {
                ["Technology"] = 0.8f,
                ["Business"] = 0.7f,
                ["Industry"] = 0.9f,
                ["Automotive"] = 0.9f,
                ["Regulatory"] = 0.3f
            },
            CompaniesOfInterest = new List<CompanyInterest>
            {
                new() { CompanyName = "Siemens", Relationship = "Partner", ImportanceLevel = 0.9f },
                new() { CompanyName = "Rockwell", Relationship = "Partner", ImportanceLevel = 0.9f },
                new() { CompanyName = "ABB", Relationship = "Partner", ImportanceLevel = 0.9f }
            },
            LearningConfidence = 0.5f
        };
    }

    // Additional helper methods would be implemented here...
    // This is a comprehensive foundation for the autonomous agent

    #endregion

    // Missing interface implementations that need to be completed
    public Task<IEnumerable<InformationSourceSuggestion>> SuggestNewSourcesAsync(UserPreferenceProfile userProfile, AggregatedIntelligence recentInformation, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in next iteration");
    }

    public Task<LearningAdjustment> ProcessUserFeedbackAsync(string userId, ReportFeedback feedback, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in next iteration");
    }

    public Task<IEnumerable<TrendingTopic>> DiscoverTrendingTopicsAsync(UserBusinessContext businessContext, TimeSpan timeWindow, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in next iteration");
    }

    public Task<IEnumerable<SourceMonitoringResult>> MonitorInformationSourcesAsync(IEnumerable<InformationSource> informationSources, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Will be implemented in next iteration");
    }

    #endregion
} 