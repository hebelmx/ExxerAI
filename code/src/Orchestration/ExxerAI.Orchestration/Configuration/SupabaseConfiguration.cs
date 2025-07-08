namespace ExxerAI.Orchestration.Configuration;

/// <summary>
/// Supabase service configuration
/// Note: All secrets (JWT, API keys) are managed by KeyStore
/// </summary>
public class SupabaseConfiguration
{
    // These are fallback values; actual secrets stored in KeyStore
    public string JwtSecret { get; set; } = "your-super-secret-jwt-token-with-at-least-32-characters-long";
    public string AnonKey { get; set; } = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZS1kZW1vIiwicm9sZSI6ImFub24iLCJleHAiOjE5ODM4MTI5OTZ9.CRXP1A7WOeoJeXxjNni43kdQwgnWNReilDMblYTn_I0";
    public string ServiceRoleKey { get; set; } = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZS1kZW1vIiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImV4cCI6MTk4MzgxMjk5Nn0.EGIM96RAZx35lJzdJsyH-qQwv8Hdp7fsn3W0YpN81IU";
    public int RestPort { get; set; } = 3000;
    public int AuthPort { get; set; } = 9999;
    public int RealtimePort { get; set; } = 4000;
    public int StoragePort { get; set; } = 5000;
}