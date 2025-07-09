namespace ExxerAI.Domain.DocumentProcessing;

/// <summary>
/// Represents document fingerprint information for similarity detection.
/// </summary>
public class DocumentFingerprint
{
    /// <summary>
    /// Gets or sets the file size in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Gets or sets the document creation date.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets the MIME type.
    /// </summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the title pattern for similarity matching.
    /// </summary>
    public string TitlePattern { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content structure hash (layout, headings, etc.).
    /// </summary>
    public string StructureHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional fingerprint metadata.
    /// </summary>
    public Dictionary<string, string> Properties { get; set; } = [];

    /// <summary>
    /// Calculates similarity score with another fingerprint.
    /// </summary>
    /// <param name="other">The other fingerprint to compare with.</param>
    /// <returns>Similarity score from 0.0 to 1.0.</returns>
    public float CalculateSimilarity(DocumentFingerprint other)
    {
        if (other == null) return 0.0f;

        var score = 0.0f;
        var factors = 0;

        // File size similarity (weight: 0.2)
        if (FileSize > 0 && other.FileSize > 0)
        {
            var sizeRatio = Math.Min(FileSize, other.FileSize) / (float)Math.Max(FileSize, other.FileSize);
            score += sizeRatio * 0.2f;
            factors++;
        }

        // MIME type match (weight: 0.3)
        if (!string.IsNullOrEmpty(MimeType) && !string.IsNullOrEmpty(other.MimeType))
        {
            score += (MimeType.Equals(other.MimeType, StringComparison.OrdinalIgnoreCase) ? 1.0f : 0.0f) * 0.3f;
            factors++;
        }

        // Title pattern similarity (weight: 0.3)
        if (!string.IsNullOrEmpty(TitlePattern) && !string.IsNullOrEmpty(other.TitlePattern))
        {
            var titleSimilarity = CalculateStringSimilarity(TitlePattern, other.TitlePattern);
            score += titleSimilarity * 0.3f;
            factors++;
        }

        // Structure hash match (weight: 0.2)
        if (!string.IsNullOrEmpty(StructureHash) && !string.IsNullOrEmpty(other.StructureHash))
        {
            score += (StructureHash.Equals(other.StructureHash, StringComparison.Ordinal) ? 1.0f : 0.0f) * 0.2f;
            factors++;
        }

        return factors > 0 ? score / factors : 0.0f;
    }

    private static float CalculateStringSimilarity(string str1, string str2)
    {
        if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2)) return 0.0f;
        if (str1.Equals(str2, StringComparison.OrdinalIgnoreCase)) return 1.0f;

        // Simple Levenshtein-based similarity
        var maxLength = Math.Max(str1.Length, str2.Length);
        var distance = LevenshteinDistance(str1.ToLowerInvariant(), str2.ToLowerInvariant());
        return 1.0f - (distance / (float)maxLength);
    }

    private static int LevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source)) return target?.Length ?? 0;
        if (string.IsNullOrEmpty(target)) return source.Length;

        var matrix = new int[source.Length + 1, target.Length + 1];

        for (var i = 0; i <= source.Length; i++) matrix[i, 0] = i;
        for (var j = 0; j <= target.Length; j++) matrix[0, j] = j;

        for (var i = 1; i <= source.Length; i++)
        {
            for (var j = 1; j <= target.Length; j++)
            {
                var cost = source[i - 1] == target[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1), matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[source.Length, target.Length];
    }
}