using System;

namespace IndTrace.Domain.Enum.Attributes
{
    /// <summary>
    /// Indicates that the EnumModel should have a generated lookup table provider.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class EnumLookupAttribute : Attribute
    {
    }
}