namespace ExxerAI.Orchestration.Configuration;

public class NginxConfiguration
{
    public int HttpPort { get; set; } = 80;
    public int HttpsPort { get; set; } = 443;
    public string ConfigPath { get; set; } = "./nginx/nginx.conf";
    public bool EnableSsl { get; set; } = false;
}