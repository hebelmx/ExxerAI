using System.Reflection;
using NetArchTest.Rules;
using FluentAssertions;
using Meziantou.Extensions.Logging.Xunit.v3;
using Microsoft.Extensions.Logging;

namespace ExxerAI.Architecture.Tests;

public class ClassDuplicationTests
{
    private readonly ILogger<ClassDuplicationTests> _logger;

    public ClassDuplicationTests(ILogger<ClassDuplicationTests> logger)
    {
        _logger = logger;
    }

    [Theory]
    [MemberData(nameof(ProductionAssemblies))]
    public void All_Classes_Should_Not_Have_Duplicate_Names_In_Different_Namespaces(string assemblyName)
    {
        _logger.LogInformation($"Checking assembly: {assemblyName}");
        // Load the assembly to be tested
        var assembly = Assembly.Load(assemblyName);

        // Find all types in the assembly
        var types = Types.InAssembly(assembly)
            .That()
            .AreClasses()
            .GetTypes();

        // Group by class name and check for duplicates in different namespaces
        var duplicates = types.GroupBy(t => t.Name)
            .Where(g => g.Count() > 1)
            .Select(g => new { ClassName = g.Key, Namespaces = g.Select(t => t.Namespace).Distinct().ToList() })
            .Where(g => g.Namespaces.Count > 1)
            .ToList();

        if (duplicates.Any())
        {
            _logger.LogError($"FAILED: {assemblyName} has duplicate class names in different namespaces:");
            foreach (var dup in duplicates)
            {
                _logger.LogError($"  DUPLICATE: {dup.ClassName} in [{string.Join(", ", dup.Namespaces)}]");
            }
        }
        else
        {
            _logger.LogInformation($"PASSED: {assemblyName} has no duplicate class names in different namespaces.");
        }

        // Use FluentAssertions to assert that there are no duplicated class names across different namespaces
        duplicates.Should().BeEmpty($"Duplicate class names found in assembly '{assemblyName}': {{0}}",
            string.Join(", ", duplicates.Select(d => $"{d.ClassName} in [{string.Join(", ", d.Namespaces)}]")));
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
        new object[] { "ExxerAi.MCPServer" }
    };
}