namespace IndTrace.Domain.Enum.LookUpTable;

using IndTrace.Domain.Enum.Attributes;

[EnumLookup]
public class PartStatusEntity : EnumLookUpTable
{
    public PartStatusEntity(int id, string name, string displayName) : base(id, name, displayName)
    {
    }

    public PartStatusEntity()
    {
    }
}