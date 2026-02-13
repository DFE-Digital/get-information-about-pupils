using System.Linq.Expressions;
using System.Reflection;

namespace DfE.GIAP.Core.Downloads.Application.Ctf.Builders;

public class CachePropertyValueAccessor : IPropertyValueAccessor
{
    private static readonly Dictionary<(Type, string), Func<object, string?>> _cache
        = new();

    public string? GetValue(object instance, string fieldName)
    {
        if (instance is null || string.IsNullOrWhiteSpace(fieldName))
            return null;

        Type type = instance.GetType();
        Func<object, string?>? accessor = GetOrCreateAccessor(type, fieldName);

        return accessor?.Invoke(instance);
    }

    private static Func<object, string?>? GetOrCreateAccessor(Type type, string fieldName)
    {
        (Type, string) key = (type, fieldName);

        if (_cache.TryGetValue(key, out Func<object, string?>? cached))
            return cached;

        Func<object, string?>? created = CreateAccessor(type, fieldName);
        if (created is not null)
            _cache[key] = created;

        return created;
    }

    private static Func<object, string?>? CreateAccessor(Type type, string fieldName)
    {
        PropertyInfo? property = type.GetProperty(
            fieldName,
            BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (property is null)
            return null;

        ParameterExpression param = Expression.Parameter(typeof(object), "obj");
        UnaryExpression cast = Expression.Convert(param, type);
        MemberExpression access = Expression.Property(cast, property);

        Expression body = access.Type == typeof(string)
            ? access
            : Expression.Call(access, nameof(ToString), Type.EmptyTypes);

        return Expression.Lambda<Func<object, string?>>(body, param).Compile();
    }
}
