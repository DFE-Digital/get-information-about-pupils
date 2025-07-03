namespace DfE.GIAP.Core.Common.Domain;

/// <summary>
/// Abstract Value Object base class which is used as the basis of any domain concept when the
/// model's identity is not a key design consideration, rather equality is determined by assessing
/// whether all attributes are the same. The key aspects of a domain value object are as follows:
/// <list type="bullet">
/// <item>
/// <term>Equals</term>
/// <description>
/// Used to determine whether all attributes of one value object are
/// equivalent to all attributes of another comparable value object.
/// </description>
/// </item>
/// <item>
/// <term>GetHashCode</term>
/// <description>
/// Ensures consistent hashing based on the object's attributes, enabling
/// correct behavior in hash-based collections.
/// </description>
/// </item>
/// </list>
/// </summary>
/// <typeparam name="TValueObject">The concrete value object type.</typeparam>
public abstract class ValueObject<TValueObject> where TValueObject : ValueObject<TValueObject>
{
    /// <summary>
    /// Method used to check referential equality of all attributes
    /// associated between two value objects.
    /// </summary>
    /// <param name="obj">The value object to compare against.</param>
    /// <returns>A result predicated on equivalence, or otherwise (i.e. true or false).</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not TValueObject other)
            return false;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Gets the hash code for the value object based on its properties.
    /// </summary>
    /// <returns>
    /// A hash code that represents the value object, calculated from its properties.
    /// </returns>
    public override int GetHashCode()
    {
        int hash = 17;

        foreach (object component in GetEqualityComponents())
            hash = (hash * 31) + (component?.GetHashCode() ?? 0);

        return hash;
    }

    /// <summary>
    /// Exposes all attributes made available for equality checking in a collection of objects.
    /// </summary>
    /// <returns>A collection of objects representing available attributes to compare.</returns>
    protected abstract IEnumerable<object> GetEqualityComponents();
}
