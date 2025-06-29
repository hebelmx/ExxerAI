using System.Runtime.CompilerServices;
using Xunit.v3;

namespace ExxerAI.Application.Tests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize()
    {
        // This method is required for xUnit v3 to discover and run tests
        AssertHelper.RecordExistence();
    }
}