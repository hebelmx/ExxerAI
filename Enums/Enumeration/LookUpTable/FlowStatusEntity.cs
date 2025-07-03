using IndTrace.Domain.Enum.Attributes;

namespace IndTrace.Domain.Enum.LookUpTable;

[EnumLookup]
public class FlowStatusEntity : EnumLookUpTable
{
    public FlowStatusEntity()
    {
    }

    public FlowStatusEntity(int id, string name, string displayName) : base(id, name, displayName)
    {
    }
}