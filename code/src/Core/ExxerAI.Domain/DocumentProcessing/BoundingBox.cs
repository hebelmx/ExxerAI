namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents a bounding box for OCR regions
/// </summary>
public class BoundingBox
{
    /// <summary>
    /// Gets or sets the X coordinate
    /// </summary>
    public int X { get; set; }

    /// <summary>
    /// Gets or sets the Y coordinate
    /// </summary>
    public int Y { get; set; }

    /// <summary>
    /// Gets or sets the width
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Gets or sets the height
    /// </summary>
    public int Height { get; set; }
}