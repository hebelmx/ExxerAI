using IndTrace.Domain.Enum.Attributes;

namespace IndTrace.Domain.Enum.LookUpTable;

[EnumLookup]
public class WorkFlowTypeEntity : EnumLookUpTable
{
    public WorkFlowTypeEntity(int id, string name, string displayName) : base(id, name, displayName)
    {
    }

    public WorkFlowTypeEntity()
    {
    }
}