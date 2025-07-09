namespace ExxerAI.Application.Interfaces;

/// <summary>
/// Version comparison result
/// </summary>
public enum VersionComparisonResult
{
    /// <summary>
    /// Versions are identical
    /// </summary>
    Identical,
    
    /// <summary>
    /// First version is newer
    /// </summary>
    FirstNewer,
    
    /// <summary>
    /// Second version is newer
    /// </summary>
    SecondNewer,
    
    /// <summary>
    /// Versions are incomparable
    /// </summary>
    Incomparable
}