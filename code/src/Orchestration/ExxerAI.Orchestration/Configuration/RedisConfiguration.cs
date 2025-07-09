namespace ExxerAI.Orchestration.Configuration;

public class RedisConfiguration
{
    public int Port { get; set; } = 6379;
    public string Password { get; set; } = "";
    public int Database { get; set; } = 0;
}