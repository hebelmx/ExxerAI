using System.Reflection;
using NetArchTest.Rules;
using FluentAssertions; // Add FluentAssertions for assertions
using System.Diagnostics; // For logging

namespace ExxerAI.Architecture.Tests;

public class ClassDuplicationTests
{
    [Theory]
    [MemberData(nameof(ProductionAssemblies))]
    public void All_Classes_Should_Not_Have_Duplicate_Names_In_Different_Namespaces(string assemblyName)
    {
        // Load the assembly to be tested
        var assembly = Assembly.Load(assemblyName); // Use the provided assembly name

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

        // Log duplicated classes if any
        if (duplicates.Any())
        {
            foreach (var dup in duplicates)
            {
                Debug.WriteLine($"DUPLICATE: {dup.ClassName} in [{string.Join(", ", dup.Namespaces)}]");
                Console.WriteLine($"DUPLICATE: {dup.ClassName} in [{string.Join(", ", dup.Namespaces)}]");
            }
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