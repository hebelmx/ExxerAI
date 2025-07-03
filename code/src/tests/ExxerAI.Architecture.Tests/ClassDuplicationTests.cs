using System.Reflection;
using NetArchTest.Rules;

namespace ExxerAI.Architecture.Tests;

public class ClassDuplicationTests
{
    [Fact]
    public void All_Classes_Should_Not_Have_Duplicate_Names_In_Different_Namespaces()
    {
        // Load the assembly to be tested
        var assembly = Assembly.Load("IndTrace.Domain"); // Replace with your specific assembly name

        // Find all types in the assembly
        var types = Types.InAssembly(assembly)
            .That()
            .AreClasses()
            .GetTypes();

        // Group by class name and check for duplicates in different namespaces
        var duplicates = types.GroupBy(t => t.Name)
            .Where(g => g.Count() > 1) // Find groups with more than one class with the same name
            .Select(g => new { ClassName = g.Key, Namespaces = g.Select(t => t.Namespace).Distinct() })
            .Where(g => g.Namespaces.Count() > 1) // Check if the same class name exists in more than one namespace
            .ToList();

        // Assert that there are no duplicated class names across different namespaces
        duplicates.ShouldBeEmpty("because no class should have the same name in different namespaces");
    }
}