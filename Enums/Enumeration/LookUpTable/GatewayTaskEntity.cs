using IndTrace.Domain.Enum.Attributes;

namespace IndTrace.Domain.Enum.LookUpTable;

[EnumLookup]
public class GatewayTaskEntity : EnumLookUpTable
{
    public GatewayTaskEntity(int id, string name, string displayName) : base(id, name, displayName)
    {
    }

    public GatewayTaskEntity()
    {
    }
}