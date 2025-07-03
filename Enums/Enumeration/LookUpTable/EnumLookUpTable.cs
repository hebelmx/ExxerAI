namespace IndTrace.Domain.Enum.LookUpTable;

/// <summary>
/// Represents a lookup table entry with an ID, name, and display name.
/// </summary>
public class EnumLookUpTable : ILookUpTable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnumLookUpTable"/> class.
    /// </summary>
    public EnumLookUpTable()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumLookUpTable"/> class with specified values.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="name">The name.</param>
    /// <param name="displayName">The display name.</param>
    public EnumLookUpTable(int id, string name, string displayName)
    {
        Id = id;
        Name = name;
        DisplayName = displayName;
    }

    /// <summary>
    /// Deconstructs the lookup table into its components.
    /// </summary>
    /// <param name="value">The ID value.</param>
    /// <param name="name">The name.</param>
    /// <param name="displayName">The display name.</param>
    public void Deconstruct(out int value, out string name, out string displayName)
    {
        value = Id;
        name = Name;
        displayName = DisplayName;
    }

    /// <summary>
    /// Converts this lookup table to a specified upper class type.
    /// </summary>
    /// <typeparam name="TLookUpTable">The type of the upper class.</typeparam>
    /// <param name="lookUpTable">The lookup table to convert.</param>
    /// <returns>A new instance of <typeparamref name="TLookUpTable"/>.</returns>
    public TLookUpTable ToUpperClass<TLookUpTable>(ILookUpTable lookUpTable) where TLookUpTable : EnumLookUpTable, ILookUpTable, new()
    {
        var (id, name, displayName) = lookUpTable;
        return new TLookUpTable { Id = id, Name = name, DisplayName = displayName };
    }

    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the display name.
    /// </summary>
    public string DisplayName { get; set; }
}