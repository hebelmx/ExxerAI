namespace ExxerAI.Orchestration.Configuration;

public class MilvusConfiguration
{
    public int Port { get; set; } = 19530;
    public int WebPort { get; set; } = 9091;
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string StoragePath { get; set; } = "./milvus_data";
}