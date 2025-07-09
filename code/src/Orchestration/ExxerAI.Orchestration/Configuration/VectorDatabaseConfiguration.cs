namespace ExxerAI.Orchestration.Configuration;

/// <summary>
/// Vector databases configuration
/// </summary>
public class VectorDatabaseConfiguration
{
    public QdrantConfiguration Qdrant { get; set; } = new();
    public MilvusConfiguration Milvus { get; set; } = new();
}