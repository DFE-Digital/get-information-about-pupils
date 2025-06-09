namespace DfE.GIAP.Core.Content.Application.Model;
public sealed class ContentKey : IEquatable<ContentKey>
{
    public string Value { get; }

    private ContentKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Content key cannot be null or empty.", nameof(value));
        }

        Value = value.Trim();
    }

    public static ContentKey Create(string key) => new(key);
    public override string ToString() => Value;
    public override bool Equals(object? obj) => Equals(obj as ContentKey);
    public bool Equals(ContentKey? other)
        => other is not null &&
            Value == other.Value;
    public override int GetHashCode() => Value.GetHashCode();
}
