using System.Reflection;
using ExxerAI.Domain.Enums;

namespace ExxerAI.Architecture.Tests;

public class ValidationTests
{
    [Fact]
    public void FlowStatus_Should_Not_Have_Duplicate_Names()
    {
        var flowStatusFields = typeof(FlowStatus)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.FieldType == typeof(FlowStatus))
            .Select(f => (FlowStatus)f.GetValue(null)!);

        var names = flowStatusFields.Select(fs => fs.Name).ToList();

        names.Distinct().Count().ShouldBe(names.Count(), "because Names should be unique in FlowStatus");
    }
}