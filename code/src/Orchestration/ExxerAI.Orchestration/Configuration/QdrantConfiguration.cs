namespace ExxerAI.Orchestration.Configuration;

public class QdrantConfiguration
{
    public int Port { get; set; } = 6333;
    public int GrpcPort { get; set; } = 6334;
    public string ApiKey { get; set; } = "";
    public string StoragePath { get; set; } = "./qdrant_storage";
}