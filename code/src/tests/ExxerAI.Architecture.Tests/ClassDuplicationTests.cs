using System.Reflection;
using NetArchTest.Rules;
using Shouldly;
using Meziantou.Extensions.Logging.Xunit.v3;
using Microsoft.Extensions.Logging;
using Xunit;

namespace ExxerAI.Architecture.Tests;

public class ClassDuplicationTests
{
    private readonly ILogger<ClassDuplicationTests> _logger;

    public ClassDuplicationTests(ITestOutputHelper testOutputHelper)
    {
        _logger = XUnitLogger.CreateLogger<ClassDuplicationTests>(testOutputHelper);
    }

    [Theory]
    [MemberData(nameof(ProductionAssemblies))]
    public void All_Classes_Should_Not_Have_Duplicate_Names_In_Different_Namespaces(string assemblyName)
    {
        _logger.LogInformation($"Checking assembly: {assemblyName}");
        // Load the assembly to be tested
        var assembly = Assembly.Load(assemblyName);

        // Find all types in the assembly, excluding Blazor legitimate patterns
        var types = Types.InAssembly(assembly)
            .That()
            .AreClasses()
            .And()
            .DoNotHaveNameMatching("TypeInference") // Blazor auto-generated
            .And()
            .DoNotHaveNameMatching("_Imports") // Blazor _Imports.razor files
            .And()
            .DoNotHaveNameMatching("InputModel") // Blazor page-specific form models
            .And()
            .DoNotResideInNamespaceMatching("__Blazor.*") // Blazor internal namespaces
            .And()
            .DoNotResideInNamespace("ExxerAI")
            .GetTypes();

        // Group by class name and check for duplicates in different namespaces
        var duplicates = types.GroupBy(t => t.Name)
            .Where(g => g.Count() > 1)
            .Select(g => new { ClassName = g.Key, Namespaces = g.Select(t => t.Namespace).Distinct().ToList() })
            .Where(g => g.Namespaces.Count > 1)
            .ToList();

        bool any = false;
        foreach (var dup in duplicates)
        {
            any = true;
            _logger.LogError("  DUPLICATE: {ClassName} in {Namespaces}", dup.ClassName, dup.Namespaces);
        }

        if (any)
        {
            _logger.LogError("FAILED: {assemblyName} has duplicate class names in different namespaces:", assemblyName);
        }
        else
        {
            _logger.LogInformation("PASSED: {assemblyName} has no duplicate class", assemblyName);
        }

        // Use Shouldly to assert that there are no duplicated class names across different namespaces
        duplicates.ShouldBeEmpty($"Duplicate class names found in assembly '{assemblyName}': {string.Join(", ", duplicates.Select(d => $"{d.ClassName} in [{string.Join(", ", d.Namespaces)}]"))}");
    }

    // List all production assemblies with their full root namespace
    public static IEnumerable<object[]> ProductionAssemblies => new[]
    {
        new object[] { "ExxerAI.Domain" },
        new object[] { "ExxerAI.Infrastructure" },
        new object[] { "ExxerAI.Application" },
        new object[] { "ExxerAI.Orchestration" },
        new object[] { "ExxerAI.CLI" },
        new object[] { "ExxerAI.Api" },
        new object[] { "ExxerAI.UI" },
        new object[] { "ExxerAI.UI.Library" },
        new object[] { "ExxerAI.Aspire.Dashboard" },
        new object[] { "ExxerAi.MCPServer" }
    };
}