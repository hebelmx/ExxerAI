using ExxerAI.Domain.ValueObjects;

namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Autonomous agent for collecting, aggregating, and personalizing business intelligence
/// DIRECTIVE: Learn user preferences and generate personalized weekly reports
/// </summary>
public interface IAggregatorCollectorInformation : IAgent
{
    /// <summary>
    /// Learns user preferences from interaction patterns and feedback
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="interactionHistory">User's past interactions and feedback</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated user preference profile</returns>
    Task<UserPreferenceProfile> LearnUserPreferencesAsync(
        string userId, 
        IEnumerable<UserInteraction> interactionHistory, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Collects information from all configured sources based on user interests
    /// </summary>
    /// <param name="userProfile">User preference profile</param>
    /// <param name="timeRange">Information collection time range</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collected and categorized information</returns>
    Task<InformationCollection> CollectRelevantInformationAsync(
        UserPreferenceProfile userProfile, 
        DateTimeRange timeRange, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Aggregates collected information into meaningful categories and insights
    /// </summary>
    /// <param name="rawInformation">Raw collected information</param>
    /// <param name="userContext">User's business context and role</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Aggregated and structured information</returns>
    Task<AggregatedIntelligence> AggregateInformationAsync(
        InformationCollection rawInformation, 
        UserBusinessContext userContext, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Suggests new information sources based on emerging patterns and user interests
    /// </summary>
    /// <param name="userProfile">Current user preferences</param>
    /// <param name="recentInformation">Recently collected information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Suggested new information sources and topics</returns>
    Task<IEnumerable<InformationSourceSuggestion>> SuggestNewSourcesAsync(
        UserPreferenceProfile userProfile, 
        AggregatedIntelligence recentInformation, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates personalized weekly report for the user
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="aggregatedIntelligence">Processed information</param>
    /// <param name="reportTemplate">User's preferred report format</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Personalized intelligence report</returns>
    Task<PersonalizedReport> GenerateWeeklyReportAsync(
        string userId, 
        AggregatedIntelligence aggregatedIntelligence, 
        ReportTemplate reportTemplate, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes user feedback to improve future recommendations
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="feedback">User feedback on report relevance</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Learning adjustment result</returns>
    Task<LearningAdjustment> ProcessUserFeedbackAsync(
        string userId, 
        ReportFeedback feedback, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Automatically discovers trending topics relevant to user's business context
    /// </summary>
    /// <param name="businessContext">User's business environment</param>
    /// <param name="timeWindow">Trend analysis window</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Discovered trending topics</returns>
    Task<IEnumerable<TrendingTopic>> DiscoverTrendingTopicsAsync(
        UserBusinessContext businessContext, 
        TimeSpan timeWindow, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Monitors information sources for changes and new content
    /// </summary>
    /// <param name="informationSources">Configured information sources</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Source monitoring results</returns>
    Task<IEnumerable<SourceMonitoringResult>> MonitorInformationSourcesAsync(
        IEnumerable<InformationSource> informationSources, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs autonomous information collection and processing cycle
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Autonomous cycle execution result</returns>
    Task<AutonomousCycleResult> ExecuteAutonomousCycleAsync(
        string userId, 
        CancellationToken cancellationToken = default);
} 