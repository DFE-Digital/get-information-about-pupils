namespace DfE.GIAP.Core.Contents.Application.Models;

/// <summary>
/// Represents a strongly-typed key used to identify content.
/// </summary>
public sealed class ContentKey : IEquatable<ContentKey>
{
    /// <summary>
    /// Gets the string value of the content key.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentKey"/> class.
    /// </summary>
    /// <param name="value">The string value of the content key.</param>
    /// <exception cref="ArgumentException">Thrown when the key is null or whitespace.</exception>
    private ContentKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Content key cannot be null or empty.", nameof(value));
        }

        Value = value.Trim();
    }

    /// <summary>
    /// Creates a new <see cref="ContentKey"/> instance from the specified string.
    /// </summary>
    /// <param name="key">The string key to wrap.</param>
    /// <returns>A new <see cref="ContentKey"/> instance.</returns>
    public static ContentKey Create(string key) => new(key);

    /// <inheritdoc/>
    public override string ToString() => Value;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as ContentKey);

    /// <inheritdoc/>
    public bool Equals(ContentKey? other)
        => other is not null && Value == other.Value;

    /// <inheritdoc/>
    public override int GetHashCode() => Value.GetHashCode();
}
