namespace ExxerAI.Application.DTOs;

/// <summary>
/// Region bounds for OCR region patterns
/// </summary>
public class RegionBounds
{
    /// <summary>
    /// Gets or sets the X offset from reference point
    /// </summary>
    public int XOffset { get; set; }

    /// <summary>
    /// Gets or sets the Y offset from reference point
    /// </summary>
    public int YOffset { get; set; }

    /// <summary>
    /// Gets or sets the width of the region
    /// </summary>
    public int Width { get; set; } = 200;

    /// <summary>
    /// Gets or sets the height of the region
    /// </summary>
    public int Height { get; set; } = 50;
} 