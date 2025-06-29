//using ExxerAI.Application.Interfaces;
//using ExxerAI.Domain.ValueObjects;
//using ExxerAI.Infrastructure.Services;
//using Microsoft.Extensions.DependencyInjection;
////using Microsoft.Extensions.Hosting;
////using Microsoft.Extensions.Logging;
////using Serilog;
////using System.CommandLine;

////// ===============================================================================
////// ExxerAI.CLI - Command Line Interface 💻
////// ===============================================================================
////// Purpose: Powerful CLI for AI agent orchestration and management
////// Architecture: Console application with System.CommandLine
////// Features: Interactive commands, structured logging, dependency injection
////// ===============================================================================

////// Configure Serilog for CLI logging
////Log.Logger = new LoggerConfiguration()
////    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
////    .WriteTo.File("logs/cli-.log", rollingInterval: RollingInterval.Day)
////    .CreateLogger();

////try
////{
////    Log.Information("🤖 ExxerAI CLI Starting...");

////    // ===== HOST BUILDER =====
////    var host = Host.CreateDefaultBuilder(args)
////        .UseSerilog()
////        .ConfigureServices((context, services) =>
////        {
////            // ===== EXXER AI SERVICES =====
////            services.AddScoped<IAgent, GeneralPurposeAgent>();
////            services.AddScoped<IAgentOrchestrator, AgentOrchestrator>();

////            // ===== CLI SERVICES =====
////            services.AddScoped<CliApplication>();
////        })
////        .Build();

////    // ===== COMMAND LINE SETUP =====
////    var rootCommand = new RootCommand("🤖 ExxerAI - AI Agent Orchestration CLI");

////    // Agent Execute Command
////    var executeCommand = new Command("execute", "Execute an AI agent with a prompt")
////    {
////        new Argument<string>("prompt", "The prompt to send to the agent"),
////        new Option<string>("--agent", "Agent type to use") { IsRequired = false },
////        new Option<bool>("--verbose", "Enable verbose logging") { IsRequired = false }
////    };

////    executeCommand.SetHandler(async (string prompt, string agentType, bool verbose) =>
////    {
////        using var scope = host.Services.CreateScope();
////        var cliApp = scope.ServiceProvider.GetRequiredService<CliApplication>();
////        await cliApp.ExecuteAgentAsync(prompt, agentType, verbose);
////    });

////    // Agent Status Command
////    var statusCommand = new Command("status", "Get agent status and information");

////    statusCommand.SetHandler(async () =>
////    {
////        using var scope = host.Services.CreateScope();
////        var cliApp = scope.ServiceProvider.GetRequiredService<CliApplication>();
////        await cliApp.GetStatusAsync();
////    });

////    // Interactive Mode Command
////    var interactiveCommand = new Command("interactive", "Start interactive mode");

////    interactiveCommand.SetHandler(async () =>
////    {
////        using var scope = host.Services.CreateScope();
////        var cliApp = scope.ServiceProvider.GetRequiredService<CliApplication>();
////        await cliApp.RunInteractiveModeAsync();
////    });

////    // Add commands to root
////    rootCommand.AddCommand(executeCommand);
////    rootCommand.AddCommand(statusCommand);
////    rootCommand.AddCommand(interactiveCommand);

////    // ===== EXECUTION =====
////    var result = await rootCommand.InvokeAsync(args);

////    Log.Information("✅ ExxerAI CLI completed with exit code: {ExitCode}", result);
////    return result;
////}
////catch (Exception ex)
////{
////    Log.Fatal(ex, "💥 CLI terminated unexpectedly");
////    return 1;
////}
////finally
////{
////    await Log.CloseAndFlushAsync();
////}

///// <summary>
///// CLI application service that handles command execution
///// </summary>
//public class CliApplication
//{
//    private readonly IAgent _agent;
//    private readonly ILogger<CliApplication> _logger;

//    /// <summary>
//    /// Initializes a new instance of the CliApplication class
//    /// </summary>
//    /// <param name="agent">The agent service</param>
//    /// <param name="logger">The logger instance</param>
//    public CliApplication(IAgent agent, ILogger<CliApplication> logger)
//    {
//        _agent = agent;
//        _logger = logger;
//    }

//    /// <summary>
//    /// Execute an agent with the provided prompt
//    /// </summary>
//    /// <param name="prompt">The prompt to execute</param>
//    /// <param name="agentType">Optional agent type</param>
//    /// <param name="verbose">Enable verbose logging</param>
//    public async Task ExecuteAgentAsync(string prompt, string? agentType, bool verbose)
//    {
//        try
//        {
//            _logger.LogInformation("🚀 Executing agent with prompt: {Prompt}", prompt);

//            if (verbose)
//            {
//                Console.WriteLine($"Agent Type: {agentType ?? "Default"}");
//                Console.WriteLine($"Prompt: {prompt}");
//                Console.WriteLine("Processing...");
//            }

//            var context = new AgentContext(
//                Prompt: prompt,
//                MaxTokens: 1000,
//                Temperature: 0.7f
//            );

//            var result = await _agent.ExecuteAsync(context, CancellationToken.None);

//            if (result.IsSuccessful)
//            {
//                Console.WriteLine("✅ Success!");
//                Console.WriteLine(result.Output);
//            }
//            else
//            {
//                Console.WriteLine("❌ Error:");
//                Console.WriteLine(result.ErrorMessage);
//            }
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Failed to execute agent");
//            Console.WriteLine($"❌ Error: {ex.Message}");
//        }
//    }

//    /// <summary>
//    /// Get and display agent status
//    /// </summary>
//    public async Task GetStatusAsync()
//    {
//        try
//        {
//            _logger.LogInformation("📊 Getting agent status");

//            Console.WriteLine("🤖 ExxerAI Agent Status");
//            Console.WriteLine("=======================");
//            Console.WriteLine($"Agent Available: ✅ Yes");
//            Console.WriteLine($"Status: Ready");
//            Console.WriteLine($"Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

//            await Task.CompletedTask; // Placeholder for actual status check
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Failed to get status");
//            Console.WriteLine($"❌ Error getting status: {ex.Message}");
//        }
//    }

//    /// <summary>
//    /// Run interactive mode for continuous agent interaction
//    /// </summary>
//    public async Task RunInteractiveModeAsync()
//    {
//        Console.WriteLine("🤖 ExxerAI Interactive Mode");
//        Console.WriteLine("Type 'exit' or 'quit' to leave interactive mode");
//        Console.WriteLine("==========================================");

//        while (true)
//        {
//            Console.Write("ExxerAI> ");
//            var input = Console.ReadLine()?.Trim();

//            if (string.IsNullOrEmpty(input))
//                continue;

//            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
//                input.Equals("quit", StringComparison.OrdinalIgnoreCase))
//            {
//                Console.WriteLine("👋 Goodbye!");
//                break;
//            }

//            if (input.Equals("help", StringComparison.OrdinalIgnoreCase))
//            {
//                ShowInteractiveHelp();
//                continue;
//            }

//            await ExecuteAgentAsync(input, null, false);
//            Console.WriteLine();
//        }
//    }

//    /// <summary>
//    /// Show help information for interactive mode
//    /// </summary>
//    private static void ShowInteractiveHelp()
//    {
//        Console.WriteLine("Available commands:");
//        Console.WriteLine("  help  - Show this help message");
//        Console.WriteLine("  exit  - Exit interactive mode");
//        Console.WriteLine("  quit  - Exit interactive mode");
//        Console.WriteLine("  Any other text will be sent as a prompt to the agent");
//    }
//}

Console.WriteLine("  exit  - Exit interactive mode");