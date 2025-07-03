using System.Reflection;
using ExxerAI.Domain.Enums;
using NetArchTest.Rules;

namespace ExxerAI.Architecture.Tests;

public class EnumModelTests
{
    [Fact]
    public void All_Enumerations_Should_Not_Have_Duplicate_Ids()
    {
        var types = Types.InAssembly(Assembly.Load("IndTrace.Domain"))
            .That()
            .ResideInNamespace("IndTrace.Domain.Enum")
            .And()
            .Inherit(typeof(EnumModel))
            .GetTypes();

        foreach (var type in types)
        {
            var instances = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.FieldType == type)
                .Select(f => f.GetValue(null))
                .Cast<EnumModel>()
                .ToList();

            var ids = instances.Select(e => e.Value).ToList();
            ids.Distinct().Count().ShouldBe(ids.Count, $"because {type.Name} should not have duplicate Ids");
        }
    }

    [Fact]
    public void All_Enumerations_Should_Not_Have_Duplicate_Names()
    {
        var types = Types.InAssembly(Assembly.Load("IndTrace.Domain"))
            .That()
            .ResideInNamespace("IndTrace.Domain.Enum")
            .And()
            .Inherit(typeof(EnumModel))
            .GetTypes();

        foreach (var type in types)
        {
            var instances = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.FieldType == type)
                .Select(f => f.GetValue(null))
                .Cast<EnumModel>()
                .ToList();

            var names = instances.Select(e => e.Name).ToList();
            names.Distinct().Count().ShouldBe(names.Count, $"because {type.Name} should not have duplicate Names");
        }
    }
}