using IndTrace.Domain.Enum.Attributes;

namespace IndTrace.Domain.Enum.LookUpTable;

[EnumLookup]
public class TagsGroupEntity(int id, string name, string displayName) : EnumLookUpTable(id, name, displayName);