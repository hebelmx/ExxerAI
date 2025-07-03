using IndTrace.Domain.Enum.Attributes;

namespace IndTrace.Domain.Enum.LookUpTable;

[EnumLookup]
public class CycleStatusEntity : EnumLookUpTable
{
    public CycleStatusEntity(int id, string name, string displayName) : base(id, name, displayName)
    {
    }

    public CycleStatusEntity()
    {
    }
}