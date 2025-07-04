namespace ExxerAI.Domain.Enums;

/// <summary>
/// Provides a base class for creating strongly-typed enumerations.
/// </summary>
public class EnumModel : IComparable, IEnumModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnumModel"/> class.
    /// </summary>
    public EnumModel()
    {
    }

    IEnumModel IEnumModel.Invalid => Invalid;

    /// <summary>
    /// Gets the invalid enumeration instance.
    /// </summary>
    public static readonly EnumModel Invalid
      = new(1, "Invalid Value");

    /// <summary>
    /// Initializes a new instance of the <see cref="EnumModel"/> class with specified values.
    /// </summary>
    /// <param name="value">The integer value.</param>
    /// <param name="name">The name.</param>
    /// <param name="displayName">The display name.</param>
    protected EnumModel(int value, string name, string displayName = "")
    {
        Value = value;
        Name = name;
        DisplayName = !string.IsNullOrWhiteSpace(displayName) ? DisplayName! : Name;
    }

    /// <summary>
    /// Deconstructs the enumeration into its components.
    /// </summary>
    /// <param name="value">The integer value.</param>
    /// <param name="name">The name.</param>
    /// <param name="displayName">The display name.</param>
    public void Deconstruct(out int value, out string name, out string displayName)
    {
        value = Value;
        name = Name;
        displayName = DisplayName ?? Name;
    }

    /// <summary>
    /// Gets the integer value of the enumeration.
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Gets or sets the name of the enumeration.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets the display name of the enumeration.
    /// </summary>
    public string DisplayName { get; }

    int IEnumModel.Value { get; set; }
    string IEnumModel.DisplayName { get; set; }

    /// <summary>
    /// Implicitly converts an enumeration to its integer value.
    /// </summary>
    /// <param name="d">The enumeration to convert.</param>
    public static implicit operator int(EnumModel d) => d.Value;

    /// <summary>
    /// Returns a string representation of the enumeration.
    /// </summary>
    /// <returns>The display name or name of the enumeration.</returns>
    public override string ToString()
    {
        return DisplayName ?? Name;
    }

    /// <summary>
    /// Checks if a given integer value corresponds to any of the defined enumerations.
    /// </summary>
    /// <typeparam name="TEnum">The derived EnumModel type.</typeparam>
    /// <param name="value">The integer value to check.</param>
    /// <returns>True if the value corresponds to a defined enumeration, otherwise False.</returns>
    public static bool Exists<TEnum>(int value) where TEnum : EnumModel
    {
        // Using reflection to get all static readonly fields of the derived type
        var definedValues = typeof(TEnum)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(f => f.GetValue(null))
            .Cast<EnumModel>()
            .Select(e => e.Value);

        // Check if the given value exists in the definedValues
        return definedValues.Contains(value);
    }

    /// <summary>
    /// Converts an integer value to a FlowStatus enumeration.
    /// </summary>
    /// <param name="enumeration">The enumeration instance.</param>
    /// <param name="value">The integer value to convert.</param>
    /// <returns>The corresponding FlowStatus enumeration.</returns>
    public string ToSting()
    {
        return Name;
    }

    /// <summary>
    /// Retrieves all instances of a particular EnumModel-derived type.
    /// </summary>
    /// <typeparam name="TEnumeration">Type of the EnumModel</typeparam>
    /// <returns>An IEnumerable containing all instances of the EnumModel-derived type.</returns>
    //TODO [PERFORMANCE][CURSOR][20/JUNE/2025] - Reflection is used in GetAll<TEnumeration> and Parse methods. Consider caching reflection results for performance. See .NET best practices for reflection.
    public static IEnumerable<TEnumeration> GetAll<TEnumeration>() where TEnumeration : EnumModel, new()
    {
        var type = typeof(TEnumeration);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        foreach (var fieldInfo in fields)
        {
            if (fieldInfo.GetValue(null) is TEnumeration instance)
            {
                yield return instance;
            }
        }
    }

    public static IList<TLookUpTable> ToLookUpTable<TLookUpTable, TEnumeration>()
        where TLookUpTable : EnumLookUpTable, ILookUpTable, new()
        where TEnumeration : EnumModel, new()
    {
        var type = typeof(TEnumeration);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        return (from info in fields
                let instance = new TEnumeration()
                select info.GetValue(null)
            into enumeration
                where enumeration != null
                let value = enumeration.GetType().GetProperty("Value")?.GetValue(enumeration, null)
                let name = enumeration.GetType().GetProperty("Name")?.GetValue(enumeration, null)
                let displayName = enumeration.GetType().GetProperty("DisplayName")?.GetValue(enumeration, null)
                where value != null && (int)value >= 0
                select new EnumLookUpTable((int)value, (string)name, (string)displayName) into lookUpTable
                select lookUpTable.ToUpperClass<TLookUpTable>(lookUpTable)).ToList();
    }

    public static IList<EnumLookUpTable> ToLookUpTable<TEnumeration>() where TEnumeration : EnumModel, new()
    {
        var type = typeof(TEnumeration);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

        return (from info in fields
                let instance = new TEnumeration()
                select info.GetValue(null)
            into enumeration
                where enumeration != null
                let value = enumeration.GetType().GetProperty("Value")?.GetValue(enumeration, null)
                let name = enumeration.GetType().GetProperty("Name")?.GetValue(enumeration, null)
                let displayName = enumeration.GetType().GetProperty("DisplayName")?.GetValue(enumeration, null)
                where value != null && (int)value >= 0
                select new EnumLookUpTable((int)value, (string)name, (string)displayName)).ToList();
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current enumeration.
    /// </summary>
    /// <param name="obj">The object to compare with the current enumeration.</param>
    /// <returns>True if equal, otherwise false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not EnumModel otherValue)
        {
            return false;
        }

        var typeMatches = GetType() == obj.GetType();
        var valueMatches = Value.Equals(otherValue.Value);

        return typeMatches && valueMatches;
    }

    /// <summary>
    /// Returns a hash code for the enumeration.
    /// </summary>
    /// <returns>A hash code for the enumeration.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Value);
    }

    /// <summary>
    /// Calculates the absolute difference between two enumeration values.
    /// </summary>
    /// <param name="firstValue">The first enumeration value.</param>
    /// <param name="secondValue">The second enumeration value.</param>
    /// <returns>The absolute difference between the two values.</returns>
    public static int AbsoluteDifference(EnumModel firstValue, EnumModel secondValue)
    {
        var absoluteDifference = Math.Abs(firstValue.Value - secondValue.Value);
        return absoluteDifference;
    }

    /// <summary>
    /// Creates an enumeration instance from an integer value.
    /// </summary>
    /// <typeparam name="TEnumeration">The type of enumeration to create.</typeparam>
    /// <param name="value">The integer value.</param>
    /// <returns>A new enumeration instance.</returns>
    public static TEnumeration FromValue<TEnumeration>(int value) where TEnumeration : EnumModel, new()
    {
        try
        {
            var matchingItem = Parse<TEnumeration, int>(value, "value", item => item.Value == value);

            return matchingItem;
        }
        catch (Exception)
        {
            return InvalidValue<TEnumeration>();
        }
    }

    /// <summary>
    /// Creates an enumeration instance from a display name.
    /// </summary>
    /// <typeparam name="TEnumeration">The type of enumeration to create.</typeparam>
    /// <param name="displayName">The display name.</param>
    /// <returns>A new enumeration instance.</returns>
    public static TEnumeration FromDisplayName<TEnumeration>(string displayName) where TEnumeration : EnumModel, new()
    {
        try
        {
            var matchingItem = Parse<TEnumeration, string>(displayName, "display name", item => item.DisplayName == displayName);
            return matchingItem;
        }
        catch (Exception)
        {
            return InvalidValue<TEnumeration>();
        }
    }

    /// <summary>
    /// Creates an enumeration instance from a nullable integer value.
    /// </summary>
    /// <typeparam name="TEnumeration">The type of enumeration to create.</typeparam>
    /// <param name="value">The nullable integer value.</param>
    /// <returns>A new enumeration instance.</returns>
    public static TEnumeration FromValue<TEnumeration>(int? value) where TEnumeration : EnumModel, new()
    {
        try
        {
            var matchingItem = Parse<TEnumeration, int?>(value, "value", item => item.Value == value);
            return matchingItem;
        }
        catch (Exception)
        {
            return InvalidValue<TEnumeration>();
        }
    }

    /// <summary>
    /// Creates an enumeration instance from a name.
    /// </summary>
    /// <typeparam name="TEnumeration">The type of enumeration to create.</typeparam>
    /// <param name="name">The name.</param>
    /// <returns>A new enumeration instance.</returns>
    public static TEnumeration FromName<TEnumeration>(string name) where TEnumeration : EnumModel, new()
    {
        try
        {
            var matchingItem = Parse<TEnumeration, string>(name, "name", item => item.Name == name);
            return matchingItem;
        }
        catch (Exception)
        {
            return InvalidValue<TEnumeration>();
        }
    }

    private static TEnumeration Parse<TEnumeration, TU>(TU value, string description, Func<TEnumeration, bool> predicate)
        where TEnumeration : EnumModel, new()
    {
        try
        {
            var matchingItem = GetAll<TEnumeration>().FirstOrDefault(predicate);

            if (matchingItem != null)
            {
                return matchingItem;
            }
            else
            {
                // return Invalid TEnumeration();

                return InvalidValue<TEnumeration>();
            }
        }
        catch (Exception)
        {
            return InvalidValue<TEnumeration>();
        }
    }

    /// <summary>
    /// Creates an invalid enumeration instance.
    /// </summary>
    /// <typeparam name="TEnumeration">The type of enumeration to create.</typeparam>
    /// <returns>An invalid enumeration instance.</returns>
    public static TEnumeration InvalidValue<TEnumeration>() where TEnumeration : EnumModel, new()
    {
        // Create a new instance of the derived class
        var newEnumeration = new TEnumeration();

        // Return the static Invalid instance from the derived class if it exists.
        var invalidField = typeof(TEnumeration).GetField("Invalid", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

        if (invalidField != null && invalidField.GetValue(null) is TEnumeration invalidInstance)
        {
            return invalidInstance;
        }

        // If the derived class does not have an Invalid field, create a default instance.
        return new TEnumeration { Name = "Invalid Value" };
    }

    /// <summary>
    /// Compares the current enumeration with another object.
    /// </summary>
    /// <param name="other">The object to compare with.</param>
    /// <returns>A value indicating the relative order of the objects.</returns>
    public int CompareTo(object? other)
    {
        return other is EnumModel enumeration ? Value.CompareTo(enumeration.Value) : default;
    }
}