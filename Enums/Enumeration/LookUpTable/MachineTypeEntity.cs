using IndTrace.Domain.Enum.Attributes;

namespace IndTrace.Domain.Enum.LookUpTable;

[EnumLookup]
public class MachineTypeEntity : EnumLookUpTable
{
    public MachineTypeEntity(int id, string name, string displayName) : base(id, name, displayName)
    {
    }

    public MachineTypeEntity()
    {
    }
}