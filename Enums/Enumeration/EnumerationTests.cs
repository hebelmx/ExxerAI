using NetArchTest.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Shouldly;

namespace Architecture.Tests.Enumeration;

public class EnumModelTests
{
    [Fact]
    public void All_Enumerations_Should_Not_Have_Duplicate_Ids()
    {
        var types = Types.InAssembly(Assembly.Load("IndTrace.Domain"))
            .That()
            .ResideInNamespace("IndTrace.Domain.Enum")
            .And()
            .Inherit(typeof(IndTrace.Domain.Enum.EnumModel))
            .GetTypes();

        foreach (var type in types)
        {
            var instances = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.FieldType == type)
                .Select(f => f.GetValue(null))
                .Cast<IndTrace.Domain.Enum.EnumModel>()
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
            .Inherit(typeof(IndTrace.Domain.Enum.EnumModel))
            .GetTypes();

        foreach (var type in types)
        {
            var instances = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.FieldType == type)
                .Select(f => f.GetValue(null))
                .Cast<IndTrace.Domain.Enum.EnumModel>()
                .ToList();

            var names = instances.Select(e => e.Name).ToList();
            names.Distinct().Count().ShouldBe(names.Count, $"because {type.Name} should not have duplicate Names");
        }
    }
}
