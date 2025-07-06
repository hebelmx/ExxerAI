using System.Diagnostics;

namespace ExxerAII.Aspire.AppHost.Services;

public static class StartupValidationService
{
    public static async Task<bool> ValidateAndPrepareEnvironmentAsync(bool cleanBuild = false)
    {
        Console.WriteLine("🚀 Starting LocalAI Aspire Orchestrator🚀 ");
        Console.WriteLine("================================================");

        var isValid = true;

        // Check .NET SDK (not just runtime)
        if (!ValidateDotNetSdk())
            isValid = false;

        // Check Docker
        if (!await ValidateDockerAsync())
            isValid = false;

        // Check required directories
        if (!ValidateDirectories())
            isValid = false;

        if (!isValid)
            return false;

        // Perform build process
        if (!await PerformBuildProcessAsync(cleanBuild))
            return false;

        // Display service information
        Console.WriteLine();
        Console.WriteLine("✅ Environment validation and build completed successfully!");
        return true;
    }

    private static bool ValidateDotNetSdk()
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                Console.WriteLine("❌ Failed to start dotnet process");
                return false;
            }

            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd().Trim();

            if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
            {
                Console.WriteLine($"✅ .NET SDK Version: {output}");
                return true;
            }
            else
            {
                Console.WriteLine("❌ .NET SDK not found. Please install .NET 8.0 or later.");
                Console.WriteLine("   Download from: https://dotnet.microsoft.com/download");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ .NET SDK validation failed: {ex.Message}");
            Console.WriteLine("   Please ensure .NET 8.0 SDK is installed");
            return false;
        }
    }

    private static async Task<bool> PerformBuildProcessAsync(bool cleanBuild)
    {
        try
        {
            // Clean if requested
            if (cleanBuild)
            {
                Console.WriteLine("🧹 Cleaning build artifacts...");
                if (!await RunDotNetCommandAsync("clean"))
                {
                    Console.WriteLine("❌ Clean failed!");
                    return false;
                }
            }

            // Restore packages
            Console.WriteLine("📦 Restoring NuGet packages...");
            if (!await RunDotNetCommandAsync("restore"))
            {
                Console.WriteLine("❌ Restore failed!");
                return false;
            }

            // Build the solution
            Console.WriteLine("🔨 Building solution...");
            if (!await RunDotNetCommandAsync("build"))
            {
                Console.WriteLine("❌ Build failed!");
                return false;
            }

            Console.WriteLine("✅ Build successful!");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Build process failed: {ex.Message}");
            return false;
        }
    }

    private static async Task<bool> RunDotNetCommandAsync(string arguments)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
                return false;

            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<bool> ValidateDockerAsync()
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "info",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                Console.WriteLine("❌ Failed to start Docker process");
                return false;
            }

            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                Console.WriteLine("✅ Docker is running");
                return true;
            }
            else
            {
                Console.WriteLine("❌ Docker is not running. Please start Docker Desktop.");
                Console.WriteLine("   Make sure Docker Desktop is installed and running");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Docker validation failed: {ex.Message}");
            Console.WriteLine("   Please ensure Docker Desktop is installed and running");
            return false;
        }
    }

    private static bool ValidateDirectories()
    {
        var requiredDirs = new[]
        {
            "monitoring",
            "nginx"
        };

        var optionalDirs = new[]
        {
            "searxng"
        };

        var basePath = Directory.GetCurrentDirectory();
        var allValid = true;

        foreach (var dir in requiredDirs)
        {
            var fullPath = Path.Combine(basePath, dir);
            if (!Directory.Exists(fullPath))
            {
                Console.WriteLine($"❌ Required directory not found: {fullPath}");
                allValid = false;
            }
            else
            {
                Console.WriteLine($"✅ Found required directory: {dir}");
            }
        }

        foreach (var dir in optionalDirs)
        {
            var fullPath = Path.Combine(basePath, dir);
            if (!Directory.Exists(fullPath))
            {
                Console.WriteLine($"⚠️ Optional directory not found: {fullPath}");
                Console.WriteLine($"   This may affect {dir} functionality");
            }
            else
            {
                Console.WriteLine($"✅ Found optional directory: {dir}");
            }
        }

        // Check required configuration files
        var requiredFiles = new[]
        {
            Path.Combine("monitoring", "prometheus.yml"),
            Path.Combine("nginx", "nginx.conf")
        };

        foreach (var file in requiredFiles)
        {
            var fullPath = Path.Combine(basePath, file);
            if (!File.Exists(fullPath))
            {
                Console.WriteLine($"❌ Required configuration file not found: {fullPath}");
                allValid = false;
            }
            else
            {
                Console.WriteLine($"✅ Found configuration file: {file}");
            }
        }

        return allValid;
    }

    public static void DisplayStartupBanner()
    {
        var banner = """
            ╔══════════════════════════════════════════════════════════════╗
            ║                LocalAI Aspire Orchestrator                  ║
            ║              Self-Contained AI Stack Manager                ║
            ║                    🚀 Ready to Launch! 🚀                   ║
            ╚══════════════════════════════════════════════════════════════╝
            """;

        Console.WriteLine(banner);
        Console.WriteLine("🎯 Orchestrating: Supabase • LocalAI • SearXNG • Vector DBs • Monitoring");
        Console.WriteLine();
    }

    public static void DisplayUsageInformation()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  LocalAI.Aspire.AppHost.exe [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  --clean               Clean build artifacts before starting");
        Console.WriteLine("  --no-browser         Don't open browser automatically");
        Console.WriteLine("  --environment <env>   Set environment (Development, Production)");
        Console.WriteLine("  --urls <urls>         Set URLs to listen on");
        Console.WriteLine("  --help               Show help information");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  LocalAI.Aspire.AppHost.exe");
        Console.WriteLine("  LocalAI.Aspire.AppHost.exe --clean");
        Console.WriteLine("  LocalAI.Aspire.AppHost.exe --no-browser");
        Console.WriteLine("  LocalAI.Aspire.AppHost.exe --environment Production");
        Console.WriteLine();
        Console.WriteLine("Description:");
        Console.WriteLine("  Self-contained .NET Aspire orchestrator for the LocalAI AI stack.");
        Console.WriteLine("  Manages Supabase, LocalAI, SearXNG, vector databases, and monitoring.");
        Console.WriteLine("  Eliminates the need for Docker Compose and Python dependencies.");
        Console.WriteLine();
    }
}
