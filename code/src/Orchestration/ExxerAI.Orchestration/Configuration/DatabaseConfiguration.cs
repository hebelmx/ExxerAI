using System.ComponentModel.DataAnnotations;

namespace ExxerAI.Orchestration.Configuration;

/// <summary>
/// Database (Supabase/PostgreSQL) configuration
/// Note: Username and Password are stored securely in KeyStore
/// </summary>
public class DatabaseConfiguration
{
    [Required]
    public string DatabaseName { get; set; } = "localai_db";
    
    [Required]
    public string Username { get; set; } = "postgres"; // Default fallback, actual value from KeyStore
    
    [Required]
    public string Password { get; set; } = "postgres"; // Default fallback, actual value from KeyStore
    
    public int Port { get; set; } = 5432;
    
    public string Host { get; set; } = "localhost";
    
    // Supabase specific settings
    public SupabaseConfiguration Supabase { get; set; } = new();
}