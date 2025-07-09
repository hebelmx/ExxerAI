namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Version agentStatus for document processing decisions
/// </summary>
public enum VersionStatus
{
    /// <summary>
    /// Document is new and should be processed
    /// </summary>
    New,
    
    /// <summary>
    /// Document has been updated and should be reprocessed
    /// </summary>
    Update,
    
    /// <summary>
    /// Document is unchanged and can be skipped
    /// </summary>
    Skip,
    
    /// <summary>
    /// Version determination failed or is uncertain
    /// </summary>
    Uncertain
}